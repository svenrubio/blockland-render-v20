$Render::C_StuckTimer = 500;
$Render::C_TargetTimer = 800; // (NOT IMPLEMENTED) Time between target checks (in ms)

//to do: add bot aggression level (if the bot has done a lot of damage (excluding full kills), it will have a higher chance of not leaving. this level wears off over time)

///// # AI control loop
function Render_AI_Control_Loop(%render)
{
	if(!%render.attackInit)
	{
		%render.aiStartAttacking = getRandom(0,1); // The AI sends a request to start attacking.
		%render.aiWillAttack = %render.aiStartAttacking; // AI flag; are we planning to attack? For now, this is always determined immediately.
		%render.attackInit = 1;
	}

	///// # TARGET CHECK
	// A few ways we could optimize this:
	//	1.) Delay this check. We don't really need to test for targets this frequently.
	//	2.) Use the raycast results from the whiteout damage function rather than calling containerRaycast a second time

	// If we don't have a target or our old one is gone...
	//%render.target.client is a method of checking if the player is alive without creating errors if they don't exist.
	if(%render.needsNewTarget || (!isObject(%render.target.client) && !%render.target.rIsTestBot) )
	{
		%render.needsNewTarget = 0;

		for(%i = 0; %i <= %render.players-1; %i++)
		{
			%player = %render.player[%i];

			// If target is non-existent or dead (Having destructo wand counts as dead)
			if( (!isObject(%player.client) && !%player.rIsTestBot) ||  %render.playerskip[%render.player[%i]] )
			{
				echo("AI Main: Skipping target '" @ %player @ "' (missing)");
				%render.playerskip[%render.player[%i]] = 1;
				%actualPlayers--;
				continue;
			}

			// Now that we know they exist, we'll check if they're in view.
			%rayCheck[%i] = containerRaycast(%render.getEyePoint(), %render.player[%i].getEyePoint(), $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);
			%render.playerRayDirect[%render.player[%i]] = %rayCheck[%i]; // Set the direct raycast

			if(%rayCheck[%i] !$= 0) // If the target is out of view, mark them as such.
			{
				if(%render.player[%i].getObjectMount() != getWord(%raycheck[%i],0)) // Make sure we aren't hitting their vehicle (if we are, mark as valid)
				{
					echo("AI Main: Skipping target '" @ %player @ "' (out of view)");
					%render.targetHidden[%i] = 1;
					%actualPlayers--;
					continue;
				}
			}

			// We've made it this far, the target must be valid!
			if(!%i)
				%targets = -1; // Start at 0

			%render.playerInView[%render.player[%i]] = 1; // Mark the player as "in view"

			%target[%targets++] = %render.player[%i]; // Count our valid targets

			//echo(%render.player[%i] SPC %render.target);
		}

		if(!%targets && %render.movingToPlayer) // Nobody's there. If we aren't on a path, we'll just assume everyone's gone.
		{
			echo("AI Main: no targets, despawning");

			Render_RequestDespawn(%render);
			return;
		}
		else
		{
			%newtarget = getRandom(1,%targets); // Pick a new target
			%render.target = %target[%newtarget];
		}
	}

	////// ENERGY CHECK //////
	// Determine whether we should continue attacking.
	// INCOMPLETE: Does not account for observe mode.

	if(%render.freezeTarget) // If we're currently freezing someone, we *probably* don't want to
		%continueChance += 4;

	if(%render.loopCount >= %render.loopPayNext-5000)
		%render.doContinue = mRound( getRandom(0, 10+%continueChance)/10 );

	////// # MOVEMENT

	if(!%render.freezeTarget && !%render.movingToPlayer) // If we're not moving to someone...
	{
		if(%render.playerInView[%render.target]) // There you are! °Д°
		{
			echo("AI Main: Found player, switching movement");

			%render.movingToPlayer = 1;
		}
		else // Not yet...
		{
			// We can't see the target, so we'll keep following the path until we spot them.
			%node = %render.hit[%render.currentDirection];
			%dist = vectorDist(%render.position,%node);

			//if(%dist < 3)
			//{
			//	%node = %render.from;
			//}

			if(%render.getMoveDestination() != %node)
			{
				%render.setMoveDestination(%node);
				%render.setMoveY(1);
			}
		}

	}

	////// # OBSERVER DESPAWN
	// If we aren't planning on attacking, we want to do a timed de-spawn when the player looks at us.
	if(!%render.aiWillAttack && %render.playersViewing && !%render.aiLoopObserverDespawn)
		%render.aiLoopObserverDespawn = %render.loopCount+(getRandom(250,3000)/$Render::C_LoopTimer); // 0.25-3 sec. Should be based on how close the player(s) are
	else if(%render.aiLoopObserverDespawn !$= "" && %render.loopCount >= %render.aiLoopObserverDespawn)
	{
		echo("AI Main: Despawning, timed");
		Render_RequestDespawn(%render);
		return;
	}
	Render_AI_Movement_Loop(%render);
}

function Render_AI_Movement_Loop(%render)
{
	if(!%render.movingToPlayer)
		return;

	///// ## NORMAL MOVEMENT
	if(!%render.freezeTarget)
	{
		%render.setMoveTolerance($Render::C_MoveTolerance);

		if(%render.getMoveObject() != %render.target && isObject(%render.target))
		{
			%render.setMoveObject(%render.target);
			%render.setMoveY(1);
		}
		//echo("target: " @ %render.target @ "; aim: " @ %render.getAimObject());

		// Basic "stuck" detection. This is a very simple solution to Render spawning stuck and not being able to navigate simple obstacles.
		// This is a very simple fix and it will likely be improved later.
		// Ensure that: at least ($Render::C_StuckTimer)ms has passed since last stuck check; our current position is close to the position form the last stuck check; we aren't freezing a player; we aren't in the spot where we froze a player; we are allowed to move
		if(%render.loopCount > %render.nextStuckCheck)
		{
			%render.nextStuckCheck = %render.loopCount+$Render::C_StuckTimer/$Render::C_LoopTimer;

			%dist = vectorDist(%render.lastPosition, %render.getPosition());
			if(!%render.rIsFrozen && %dist <= 0.1 && !%render.freezeTarget && %render.stuckEnd != %render.getPosition())
			{
				//talk("AI Main: Bot is stuck!" SPC %dist SPC "||" SPC %render.lastPosition SPC %render.freezeTarget SPC %render.lastFrozenCheckPos);

				%render.stuckEnd = ""; // This value means "if we're still stuck at this position, give up." We want this blank for now.
				%render.stuckConsecutive++;

				if(%render.stuckConsecutive == 2)
					%render.setCrouching(1);
				else
					%render.setCrouching(0);

				if(%render.stuckConsecutive >= 4)
					%render.setMoveY(0); // Stop moving forward if we've been stuck for too long.
				else
					%render.setMoveY(1);

				if(%render.stuckConsecutive >= 8)
					%render.stuckEnd = %render.getPosition(); // Give up if we remain stuck in the exact same spot for too long. The stuck check will resume if the bot's position changes at all.

				%render.hAvoidObstacles = 1;
				%render.hAvoidObstacle(0,0,1); // SO APPARENTLY THIS LETS RENDER OPEN DOORS AND IT SCARED THE PISS OUT OF ME. I'M BLAMING ROTONDO FOR THAT ONE.
				if(getRandom(0,1)) // Very simple solution, should do for now. Render will switch between the two until he gets unstuck.
					%render.setMoveX(1);
				else
					%render.setMoveX(-1);

				//%render.setJumping(1);
				//%render.schedCrouch = %render.schedule(500,0,setCrouching,0);
				//%render.schedJump = %render.schedule(500,0,setJumping,0);

				%render.lastFrozenCheckPos = %render.getPosition();
			}
			else
			{
				//talk("AI Main: Not stuck;" SPC %dist SPC "||" SPC %render.lastPosition SPC %render.freezeTarget SPC %render.lastFrozenCheckPos);
				%render.stuckConsecutive = 0;
				%render.hAvoidObstacles = 0;

				%render.setMoveY(1); // Restart movement
				%render.setMoveX(0);
				%render.setCrouching(0);
				%render.setJumping(0);
			}
		}

		if(%render.rIsFrozen) // If we're frozen, reset the stuck check so we don't freak out.
			%render.lastPosition = "0 0 -9999";
		else
			%render.lastPosition = %render.getPosition();
	}
	else ////// ## FREEZING A TARGET/MOVEMENT DISABLED
	{
		%render.setMoveTolerance(50); // Disable movement.
		%render.clearMoveX();
		%render.clearMoveY();
	}

	if(%render.target.isJetting && !%render.freezeTarget && !%render.rIsFrozen)
		%render.setJetting(1);
	else if(%render.isJetting())
		%render.setJetting(0);
}

// This checks the player's velocity for changes in direction.
// Note: Jetting causes the third value to change rapidly.
function Render_AI_TrackPlayer(%player)
{
	//%vel = %player.getVelocity();
	//
	//if(%player.direction $= "")
	//	%player.direction = "0 0 0";
	//
	//for(%i = 0; %i <= 2; %i++)
	//{
	//	if(getWord(%vel, %i) > 0)
	//		%player.direction = setWord(%player.direction, %i, 1);
	//	else if(getWord(%vel, %i) < 0)
	//		%player.direction = setWord(%player.direction, %i, -1);
	//}
	//
	//talk(%player.direction);
	//return %player.direction;
}
