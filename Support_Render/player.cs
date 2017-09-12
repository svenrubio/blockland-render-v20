function createRenderUser(%client)
{
  %player = %client.player;
  %player.isRenderPlayer = 1;
  %player.isRender = 1;
  %player.changeDatablock(PlayerRenderArmor);

  RenderBotGroup.add(%client.player);

  if(!isEventPending($Render::LoopBot))
    $Render::LoopBot = schedule($Render::C_LoopTimer,0,Render_Loop);

  %render.aiStartAttacking = 1;
}

///// # Player control loop
// Currently a copy of the AI loop with misc. tweaks.
function Render_Player_Control_Loop(%render)
{
	if(!%render.attackInit)
	{
		// The AI sends a request to start attacking, but only if not in haunt mode.
		// Mode 3 = Passive only; Debug override = always attack; normal 50/50 chance
		%render.aiStartAttacking = 1; // By default, always attack if possible.
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

	////// ENERGY CHECK //////
	// Determine whether we should continue attacking.

	if(%render.freezeTarget) // If we're currently freezing someone, we *probably* don't want to
		%continueChance += 4;

	if(%render.loopCount >= %render.loopPayNext-5000)
		%render.doContinue = mFloatLength( getRandom(0, 10+%continueChance)/10, 0);

	////// # OBSERVER DESPAWN
	// If we aren't planning on attacking, we want to do a timed de-spawn when the player looks at us.
	if(!%render.aiWillAttack && %render.playersViewing && !%render.aiLoopObserverDespawn)
		%render.aiLoopObserverDespawn = %render.loopCount+(getRandom(250,3000)/$Render::C_LoopTimer); // 0.25-3 sec. Should be based on how close the player(s) are
	else if(%render.aiLoopObserverDespawn !$= "" && %render.loopCount >= %render.aiLoopObserverDespawn)
	{
		Render_RequestDespawn(%render);
		return;
	}
}
