$Render::C_StuckTimer = 250;
$Render::C_TargetTimer = 800; // (NOT IMPLEMENTED) Time between target checks (in ms)

////// # AI control loop //////
function Render_AI_Control_Loop(%render)
{
	if(!%render.attackInit)
	{
		// The AI sends a request to start attacking, but only if not in haunt mode.
		// Mode 3 = Passive only; Debug override = always attack; normal 50/50 chance
		%render.aiStartAttacking = %render.mode==3?0:%render.debugOverride==1?1:getRandom(0,1);
		%render.aiWillAttack = %render.aiStartAttacking; // AI flag; are we planning to attack? For now, this is always determined immediately.
		%render.attackInit = 1;
	}

	////// # TARGET CHECK # //////
	// TODO: A few ways we could optimize this:
	//	1.) Delay this check. We don't really need to test for targets this frequently.
	//	2.) Use the raycast results from the whiteout damage function rather than calling containerRaycast a second time

	// If we don't have a target or our old one is gone...
	// %render.target.client is a method of checking if the player is alive without creating errors if they don't exist.
	if(%render.needsNewTarget || (!isObject(%render.target.client) && !%render.target.rIsTestBot) )
	{
		%render.needsNewTarget = 0;

		for(%i = 0; %i <= %render.players-1; %i++)
		{
			%player = %render.player[%i];

			// If target is non-existent or dead (Having destructo wand counts as dead)
			if( (!isObject(%player.client) && !%player.rIsTestBot) ||  %render.playerskip[%render.player[%i]] )
			{
				//echo("AI Main: Skipping target '" @ %player @ "' (missing)");
				%render.playerskip[%render.player[%i]] = 1;
				%actualPlayers--;
				continue;
			}

			// Now that we know they exist, we'll check if they're in view.
			%rayCheck[%i] = containerRaycast(%render.getEyePoint(), %render.player[%i].getEyePoint(), $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType);
			%render.playerRayDirect[%render.player[%i]] = %rayCheck[%i]; // Set the direct raycast

			if(%rayCheck[%i] !$= 0) // If the target is out of view, mark them as such.
			{
				if(%render.player[%i].getObjectMount() != getWord(%raycheck[%i],0)) // Make sure we aren't hitting their vehicle (if we are, mark as valid)
				{
					////echo("AI Main: Skipping target '" @ %player @ "' (out of view)");
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
		}

		if(!%targets && %render.movingToPlayer) // Nobody's there. If we aren't on a path, we'll just assume everyone's gone.
		{
			//echo("AI Main: no targets, despawning");

			Render_RequestDespawn(%render);
			return;
		}
		else
		{
			%newtarget = getRandom(1,%targets); // Pick a new target
			%render.target = %target[%newtarget];
		}
	}

	////// # ENERGY CHECK # //////
	// Determine whether we should continue attacking.

	if(%render.freezeTarget) // If we're currently freezing someone, we *probably* don't want to
		%continueChance += 4;

	if(%render.loopCount >= %render.loopPayNext-5000)
		%render.doContinue = mFloatLength( getRandom(0, 10+%continueChance)/10, 0);

	////// # MOVEMENT # //////

	if(!%render.freezeTarget && !%render.movingToPlayer) // If we're not moving to someone...
	{
		if(%render.playerInView[%render.target]) // There you are! °Д°
		{
			//echo("AI Main: Found player, switching movement");

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
				%render.setMoveDestination(%node);
		}
	}

	////// # OBSERVER DESPAWN # //////
	// If we aren't planning on attacking, we want to do a timed de-spawn when the player looks at us.
	if(!%render.aiWillAttack && %render.playersViewing && !%render.aiLoopObserverDespawn)
		%render.aiLoopObserverDespawn = %render.loopCount+(getRandom(250,3000)/$Render::C_LoopTimer); // 0.25-3 sec. Should be based on how close the player(s) are
	else if(%render.aiLoopObserverDespawn !$= "" && %render.loopCount >= %render.aiLoopObserverDespawn)
	{
		Render_RequestDespawn(%render);
		return;
	}
	if(!%render.isRenderPlayer)
		Render_AI_Movement_Loop(%render);
}

function Render_AI_Movement_Loop(%render)
{
	if(!%render.movingToPlayer)
		return;

	///// ## NORMAL MOVEMENT ## /////
	if(!%render.freezeTarget)
	{
		%render.setMoveTolerance($Render::C_MoveTolerance);

		%moveDist = vectorDist(getWords(%render.target.position, 0, 1), getWords(%render.position, 0, 1));
		if(%render.getMoveObject() != %render.target && isObject(%render.target))
		{
			%render.clearMoveY();
			%render.setMoveObject(%render.target);
		}

		// Basic "stuck" detection. This is a very simple solution to Render spawning stuck and not being able to navigate simple obstacles.
		// Ensure that: at least ($Render::C_StuckTimer)ms has passed since last stuck check; our current position is close to the position form the last stuck check; we aren't freezing a player; we aren't in the spot where we froze a player; we are allowed to move
		if(%render.loopCount > %render.nextStuckCheck)
		{
			%render.nextStuckCheck = %render.loopCount+$Render::C_StuckTimer/$Render::C_LoopTimer;

			// We'll want to skip the stuck check if we're within freezing distance.
			%distPlayer = vectorDist(%render.position, %render.target.position);
			if(%distPlayer > 2.8)
			{
				// Increase distance after the first check to prevent a stuck-unstuck-stuck-unstuck pattern.
				if(%render.stuckConsecutive)
					%distCheck = 0.3;
				else
					%distCheck = 0.1;

				%dist = vectorDist(%render.lastPosition, %render.getPosition());
				if(!%render.rIsFrozen && %dist <= %distCheck && !%render.freezeTarget && %render.stuckEnd != %render.getPosition())
				{
					talk("AI Main: Bot is stuck!" SPC %dist SPC "||" SPC %render.lastPosition SPC %render.freezeTarget SPC %render.lastFrozenCheckPos);

					%render.stuckEnd = ""; // This value means "if we're still stuck at this position, give up." We want this blank for now.
					%render.stuckConsecutive++;

					if(%render.stuckConsecutive == 6)
						%render.setCrouching(1);
					else
						%render.setCrouching(0);

					if(%render.stuckConsecutive == 8)
					{
						%render.hAvoidObstacles = 1;
						%render.hAvoidObstacle(0,0,1); // SO APPARENTLY THIS LETS RENDER OPEN DOORS AND IT SCARED THE PISS OUT OF ME. I'M BLAMING ROTONDO FOR THAT ONE.
					}

					if(%render.stuckConsecutive >= 16)
						%render.stuckEnd = %render.getPosition(); // Give up if we remain stuck in the exact same spot for too long. The stuck check will resume if the bot's position changes at all.

					%render.setMoveX(Render_AI_GetRelativeDirection2D(%render.position,%render.target.position));

					//%render.setJumping(1);
					//%render.schedCrouch = %render.schedule(500,0,setCrouching,0);
					//%render.schedJump = %render.schedule(500,0,setJumping,0);

					%render.lastFrozenCheckPos = %render.getPosition();
				}
				else
				{
					talk("AI Main: Not stuck;" SPC %dist SPC "||" SPC %render.lastPosition SPC %render.freezeTarget SPC %render.lastFrozenCheckPos);
					%render.stuckConsecutive = 0;
					%render.hAvoidObstacles = 0;

					%render.clearMoveX();
					%render.clearMoveY();
					%render.setCrouching(0);
					%render.setJumping(0);
				}
			}

			if(%render.rIsFrozen) // If we're frozen, reset the stuck check so we don't freak out.
				%render.lastPosition = "0 0 -9999";
			else
				%render.lastPosition = %render.getPosition();
		}
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

// EXPERIMENTAL: This checks the player's velocity for changes in direction.
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

// GetRelativeDirection2D( pos1, pos2)
// Determines whether pos2 is to the right or left of pos1, assuming pos1 is facing toward pos2.
// Returns -1 for left, 1 for right
function Render_AI_GetRelativeDirection2D(%pos1,%pos2)
{
	%dir1 = vectorDist( getWord(%pos1,0), getWord(%pos2,0)) < vectorDist(getWord(%pos1,1), getWord(%pos2,1))?0:1;

	if(%dir1)
		%dir2 = getWord(%pos1,!%dir1) > getWord(%pos2, !%dir1)?0:1;
	else
		%dir2 = getWord(%pos1,!%dir1) < getWord(%pos2, !%dir1)?0:1;

	%dir3 = (getWord(%pos1,%dir1) > getWord(%pos2,%dir1))?1:0;
	%dir4 = (%dir2?%dir3:!%dir3);

	if(%dir4)
		return 1;
	else
		return -1;

	//if(%dir4)
	//		return "right(" @ %dir1 SPC %dir2 SPC %dir3 SPC %dir4 @ ")<br>" @ %pos1 @ "<br>" @ %pos2;
	//	else
	//		return "left(" @ %dir1 SPC %dir2 SPC %dir3 SPC %dir4 @ ")<br>" @ %pos1 @ "<br>" @ %pos2;
}
