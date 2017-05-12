//°Д°

////// # CONSTANTS
$Render::C_MoveTolerance = 2;
$Render::C_MoveToleranceObserve = 10;
$Render::C_EnergyTimer = 25000; // Minimum: 5000
$Render::C_SpawnTimer = 30000;
$Render::C_LoopTimer = 50; // Schedule time for Render_Loop (in ms)
$Render::C_DetectorTimer = 50; // Schedule time for detectors (in ms)
$Render::C_DamageRate = 200;
$Render::C_DamageDecay = $Render::C_DamageRate/100;
$Render::C_ShrineCheckInterval = 750; // Shrine check interval (in ms)
$Render::C_PlayerCheckInterval = 1000; // (NOT IMPLEMENTED) Time between player checks (in ms)

////// # Bot Appearance/Creation Functions
function Render_ApplyAppearance(%this)
{
	hideAllNodes(%this);
	%this.unhidenode(chest);
	%this.unhidenode(pants);
	%this.unhidenode(LShoe);
	%this.unhidenode(RShoe);
	%this.unhidenode(LArm);
	%this.unhidenode(LHand);
	%this.unhidenode(RArm);
	%this.unhidenode(RHand);
	%this.setnodecolor(chest, "0 0 0 1");
	%this.setnodecolor(headskin, "0 0 0 1");
	%this.setnodecolor(pants, "0 0 0 1");
	%this.setnodecolor(LShoe, "0 0 0 1");
	%this.setnodecolor(RShoe, "0 0 0 1");
	%this.setnodecolor(LArm, "0 0 0 1");
	%this.setnodecolor(LHand, "0 0 0 1");
	%this.setnodecolor(RArm, "0 0 0 1");
	%this.setnodecolor(RHand, "0 0 0 1");
	%this.setdecalname("AAA-None");
	%this.setfacename("asciiTerror");
}

////// # Bot Creation Function
// %pos: Position to place the bot in
// %client: 'Parent client' of the bot
function Render_CreateBot(%pos,%client)
{
	%render = new aiplayer(Render) // Create a new AIPlayer
	{
		datablock = PlayerRenderArmor;
	};

	%render.isRender = 1;

	// ## Minigame Preferences
	if(%client.minigame.rMode !$= "" && %client.minigame.rMode != -1)
		%render.mode = %client.minigame.rMode;
	else
		%render.mode = $Pref::Server::RenderDamageType;

	if(%client.minigame.rInvincible !$= "" && %client.minigame.rInvincible != -1)
		%render.invincible = %client.minigame.rInvincible;
	else
		%render.invincible = $Pref::Server::RenderIsInvincible;

	// ## Bot Setup

	if(%render.mode == 2)
		%render.changeDatablock(PlayerRenderTagArmor);

	Render_ApplyAppearance(%render); // Apply appearance and set it to the specified position
	%render.setTransform(%pos);

	// TEMPORARY: Should try to adjust this so Render is a *little* easier to escape. 0 makes the bot unrealistically accurate.
	//%render.setMoveSlowdown(0);
	//%render.setMaxForwardSpeed(6); // Default: 7
	//%render.setMaxBackwardSpeed(3); // Default: 4
	//%render.setMaxSideSpeed(5); // Default: 5

	%render.setMoveTolerance($Render::C_MoveToleranceObserve);

	// Bot hole stuff
	%render.hMelee = 1;
	%render.name = "Render";

	if(!$Pref::Server::RenderDisableLights) // Add a light (bugged)
	{
		%render.light = new fxlight() // Try $r_light[%render]; or something?
		{
			dataBlock = playerLight;
			enable = 0;
			iconsize = 1;
			player = %render;
		};
		%render.light.attachToObject(%render);
		%render.light.schedule(32,setEnable,1); // Fix for lights flickering in a different location on spawn.
	}

	%render.setTransform(%pos);
	RenderBotGroup.add(%render);

	if(!$Render::LoopBot) // If the loop isn't running, we need to restart it.
		$Render::LoopBot = schedule($Render::C_LoopTimer,0,Render_Loop);

	return %render;
}

////// # FOV Check
// This borrows from Bot_Hole. Optimized for Render's spooky biddings.
function Player::rFOVCheck(%observer, %object, %checkRaycast)
{
	%posObserver = %observer.getPosition();
	%posObject = %object.getPosition();

	%observerEye = %observer.getEyeVector();

	// draw a line between us and the Object
	%line = vectorNormalize( vectorSub( %posObject, %posObserver ) );

	// compare our eye to the line
	%dot = vectorDot( %observerEye, %line );

	// return the dot product if they want it, otherwise return true or false
	//if( %returnDot )
	//	return %dot;
	//else
	//{

	// this will return 0 or 1
	%fovCheck = %dot >= 0.7;

	// This lets us check for obstructions. Optional, only applies if main check passed.
	if(%fovCheck && %checkRaycast)
	{
		%ray = containerRaycast(%observer.getEyePoint(), %posObject, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType);

		if(%ray != 0)
			%fovCheck = 0;
	}

	return %fovCheck;

	//}
}

////// # Global loop; runs every 50ms
function Render_Loop()
{
	cancel($Render::LoopBot);
	%scale = "1 1 1";
	%simTime = getSimTime();

	if(!$Pref::Server::RenderDisableScaleEffect)
		%scale = getRandom(096,104)/100 SPC getRandom(094,104)/100 SPC getRandom(099,101)/100;

	for(%i = 0; %i < RenderBotGroup.getCount(); %i++) // For each bot...
	{
		%render = renderBotGroup.getObject(%i);

		if((%render.fxScale) && !$Pref::Server::RenderDisableScaleEffect) // Scale effect applies when we're frozen or freezing a player
			%render.setPlayerScale(%scale); // Apply the scale effect

		Render_Loop_Local(%render); // Run the local loop
	}

	// ## SHRINE CHECK
	// If you place 1,024 shrines, the game may stutter slightly--just barely enough to be noticeable, even with multiple Renders.
	// Given the limit of 32 shrines per brickgroup, it would take the combined effort of 32 unique players to place this many shrines.
	// If you 'multi-client', you can cause a minor annoyance for the low price of 32 Blockland keys, or $319.68!

	// Ways to optimize this:
	// - Remove shrines from the list when they are disabled via pref. (Spamming disabled shrines still causes lag even if they don't do anything)

	// Check if it's time to do a shrine check first. Delaying these checks reduces the impact on performance if we have a lot of shrines/attackers present.
	// Check cancels if there are no bots present.
	if(%i && %simTime > $R_shrNext)
	{
		Render_DoShrineCheck();
		$R_shrNext = %simTime+$Render::C_ShrineCheckInterval;
	}

	/// ## BOT LOOP
	// Only continue if there are bots present.
	if(%i)
		$Render::LoopBot = schedule($Render::C_LoopTimer,0,Render_Loop);
	else
		$Render::LoopBot = 0;
}

////// # Local loop. This function is called individually for each bot every 50ms.
function Render_Loop_Local(%render)
{
	%render.isRenderman = 0;

	if(!%render.loopCount)
	{
		%render.loopViewNext = 2;
		%render.loopPayNext = $Render::C_EnergyTimer/$Render::C_LoopTimer; // 40 seconds between pay times
	}

	%render.players = 0;
	%render.playersViewing = 0;
	%render.loopCount++;

	if(%render.loopCount >= %render.loopPayNext) // This determines when Render needs to use more energy to continue. Timing is based on loop count.
	{
		if(!%render.doContinue) // If they aren't attacking at this point, we'll just de-spawn them.
		{
			//echo("RENDER: De-spawning, out of time");
			%render.delete();
			return;
		}

		%render.doContinue = 0; // Reset this

		%render.aiNeedsToPay = 1;
		%render.payCount++;

		%render.loopPayNext = %render.loopPayNext+$Render::C_EnergyTimer/$Render::C_LoopTimer; //
	}

	// Until energy is implemented, we will use a random chance that is influenced by how long Render has already been alive.
	// This is planned to be determined by a more sophisticated AI, considering factors like whether we're freezing a player, how many players we're pursuing, etc.
	if(%render.aiNeedsToPay && getRandom(1,2) == 1)
	{
		//echo("RENDER: De-spawning (energy check failed)");
		%render.delete();
		return;
	}

	// ## VIEW CHECK + MOVEMENT CHECK A

	// INCOMPLETE: This is to be corrected; we don't need to run a radius search this often.

	if(%render.aiStartAttacking) // If the AI requests to start attacking...
	{
		if(!%render.loopAttackStart)
		{
			%render.fxScale = 1;

			%render.loopAttackStart = %render.loopCount+(5000/$Render::C_LoopTimer); // CONSTANT: Five seconds
			Render_FreezeRender(%render); // Render stays frozen when he's about to attack.
		}

		if(%render.loopCount > %render.loopAttackStart) // Start attacking
		{
			%render.fxScale = 0;

			%render.isAttacking = 1;
			%render.setMoveTolerance($Render::C_MoveTolerance);
			Render_UnfreezeRender(%render);
		}
	}

	initContainerRadiusSearch(%render.position,150,$TypeMasks::PlayerObjectType); // Start a radius search.

	while(%target=containerSearchNext()) // For all players in the area...
	{
		// Delete other Render bots nearby
		//if(!$Pref::Server::RenderAllowMultiples && %target != %render && %target.isRender)
		//{
		//	%target.delete();
		//	continue;
		//}

		// MUST be an actual player or testing bot; ignore if they have destructo wand
		if(%target.getMountedImage(0).Projectile !$= "AdminWandProjectile" && (%target.getClassName() $= "Player" || %target.getClassName() $= "AIPlayer" && %target.rIsTestBot))
		{
			// Do a "view check" on players. This is where we apply damage, freeze players, and set detector levels.
			if(%render.loopCount == %render.loopViewNext)
			{
				%isViewing = %target.rFOVCheck(%render, 1); // Check if they're in Render's line of sight.
				%distance = vectorDist(%render.getPosition(), %target.getPosition());

				// Detectors
				if(%render.isAttacking)
				{
					%detectorVal = 5.15-(%distance/20); // 5.1-(distance/20); we use the value 5.25 so distance <= 3 is considered off-scale.
					%target.detector = %detectorVal;
					%target.detectorDecay = %detectorVal;
					%target.startDetectorDecay = getSimTime()+750; // ?????
				}

				////// ## DAMAGE TARGET
				//%render.playerIsViewing[%render.players] = %isViewing; // Mark them as "viewing"
				%render.playerViewing = %target;
				if(%isViewing)
				{
					%render.playersViewing++;

					if(%render.isAttacking)
					{
						if(%render.mode == 0) // Whiteout Damage
							Render_InflictWhiteOutDamage(%target,%render,%distance);
						else if(%render.mode == 1) // Health damage
						{
							%renderDamage = %target.dataBlock.maxDamage*0.8/%distance;

							if(%target.dataBlock.maxDamage-%target.getDamageLevel()-%renderDamage < 1)
								%render.targetKilled = 1;

							%target.renderDamage = 1;
							%target.addHealth(-%renderDamage);
						} // Damage type 2 doesn't need this.

					}
				}

				////// ## FREEZE TARGET
				if(%distance <= 2.8) // If the player is close enough, freeze them.
				{
					if(%render.isAttacking)
					{
						if(!%render.freezeTarget && %targetMount !$= "RenderDeathArmor")
						{
							//talk("RENDER" SPC %render @ ": Freezing a target!");
							if(%targetMount !$= "RenderDeathArmor" && %target.getMountedImage(0).Projectile !$= "AdminWandProjectile") // If the target isn't already frozen and isn't holding a destructo wand.
								Render_FreezePlayer(%target,%render);

							%render.freezeTarget = %target;
						}
					}
					else
					{
						Render_FreezeRender(%render);
						%froze = 1;
					}
				}

				// If we have a target that is too far away or gone, unfreeze them and Render.
				if(%render.freezeTarget && (!%render.freezeTarget.isFrozen || %target == %render.freezeTarget && %distance > 5))
				{
					//talk("RENDER" SPC %render @ ": Unfroze player" SPC %target.client.name SPC %distance SPC %render.freezeTarget SPC %render.freezeTarget.isFrozen);
					Render_UnfreezePlayer(%target,%render);
				}

				if(%target.isFrozen && %target.frozenPosition !$= "") // Extra precaution to make sure frozen targets stay in place. This will likely cause problems on ramp bricks.
				{
					%target.setTransform(getWords(%target.frozenPosition,0,1) SPC getWord(%player.position,2));
					%target.setVelocity("0 0" SPC getWord(%target.getVelocity(),2));
				}
			}

			%render.player[%render.players] = %target;
			%render.players++;
		}
	}

	////// ## MOVEMENT CHECK B

	// We aren't allowed to move if we're not attacking and players are looking at us. (First five seconds counts as "not attacking")
	if(%render.loopCount == %render.loopViewNext)
	{
		if(!%render.isAttacking && %render.playersViewing)
			Render_FreezeRender(%render);
		else if(%render.rIsFrozen && !%froze) // Necessary?
			Render_UnfreezeRender(%render);
	}

	if(%render.loopCount == %render.loopViewNext)
		%render.loopViewNext = %render.loopViewNext+2;

	Render_AI_Control_Loop(%render);
}

//////# Target picking function
//This will determine the targets for the attacker and start the spawning code
function Render_Spawn_Loop()
{
	if(isEventPending($Render::LoopSpawner))
	{
		error("Render_Spawn_Loop - Duplicate loop! Cancelling...");
		cancel($Render::LoopSpawner);
	}

	//echo("RENDER: Spawn loop");

	if($Pref::Server::RenderDayCycleSpawn && env_getDayCycleEnabled()) // If we're only supposed to spawn at night, we'll need to do some extra checks. (Only if the daycycle is actually enabled, of course.)
	{
		%time = getPartOfDaycycle();

		if(%time == 0 || %time == 1) // Morning or daytime, we won't spawn at all.
			%skipSpawn = 1;
	}

	if(!%skipSpawn && $Pref::Server::RenderSpawnRate != 0)
	{
		// Play ambient sound effects

		if(!$Pref::Server::RenderDisableAmbientSounds)
			if(getRandom(1,12) <= $Pref::Server::RenderSpawnRate) // Bleh
				serverPlay2D("RenderAmb" @ getRandom(1,2));

		// Render uses a 'group' spawning system to choose which players to target. This works by choosing between areas rather than individual players.
		// By doing this, we keep the spawnrate balanced regardless of playercount and avoid an unintended bias toward groups of players.

		// First, we're going to go through all the clients in the server.
		for(%i = 0; %i < clientGroup.getCount(); %i++)
		{
			%client = clientGroup.getObject(%i);

			// If player is nonexistent or already marked, skip them.
			if(!isObject(%client.player) || %groupGet[%client.player])
				continue;

			// Otherwise, this player counts as a new group.
			%groups++;

			// For all players in the area...
			initContainerRadiusSearch(%client.player.position,100,$TypeMasks::PlayerObjectType);
			while(%target=containerSearchNext())
			{
				if(%target.getClassName() !$= "Player") // Make sure they aren't a bot.
					continue;

				// Get each player's spawnrate.
				if(%target.client.minigame.rSpawnRate $= "" || %target.client.minigame.rSpawnRate == -1)
					%spawnrate[%target] = $Pref::Server::RenderSpawnRate;
				else
					%spawnrate[%target] = %target.client.minigame.rSpawnRate;

				%groupGet[%target] = %groups; // So we can easily 'get' the group containing a target
				%groupList[%groups,%groupCount[%groups]++] = %target; // So we can list all targets for a group.

				%spawnrateTotal[%groups] += %spawnrate[%target];
				//echo("RENDER: Adding " @ %target @ " as player #" @ %groupCount[%groups] @ " in group " @ %groups);
			}

			// Calculate the final spawnrate for this group.
			// Per-group spawnrate is determined by average.
			%avgSpawnrate = %spawnrateTotal[%groups]/%groupCount[%groups];
			//echo("RENDER: Average spawnrate is " @ %avgSpawnrate);

			// Now, we choose if we want to spawn for this group.
			%random = getRandom(1,6);
			if(%random <= %avgSpawnrate)
			{
				// If yes, we'll pick a random player in the group to start with.
				%client = %groupList[%groups, getRandom(1, %groupCount[%groups]) ].client;

				//echo("RENDER: Chance passed for" SPC %client.name @ " (group " @ %groups @ "); spawning");

				%render = Render_CreateBot("0 0 -10000",%client);

				%hallSpawn = Render_Spawn_FindNewPositions(%client.player.getEyePoint(), %render, %skipNorth, %skipSouth, %skipEast, %skipWest);
				%pos = Render_Spawn_GetNewDirection(%render, %client.player.getEyePoint(), 0, 0, 1);

				if(!%pos)
				{
					//warn("RENDER: Spawn failed for " @ %client);
					%render.delete();
				}
				else
					%render.setTransform(%pos);
			}
		}
	}

	$Render::LoopSpawner = schedule($Render::C_SpawnTimer,0,Render_Spawn_Loop);
}

// # InflictWhiteOutDamage + misc.

function Render_InflictWhiteOutDamage(%p,%render,%distance)
{
	// This calculates the damage decay, aka how much we need to subtract.
	// We're using the sim time instead of keeping a loop running.
	// Partially scales with loop timer constant, however the damage rate is still affected by it.

	// sec = time in seconds since the player last looked
	// dif = sec/(2.5/C_LoopTimer)-1

	%dif = ($Sim::Time-%p.rLastDmg)/($Render::C_DamageDecay/$Render::C_LoopTimer)-1;

	if(!%distance)
		%distance = vectorDist(%render.position,%p.position);

	%dmgOld = %p.rDmg;
	%p.rDmg = ( %p.rDmg+( $Render::C_DamageRate/%distance ) )-%dif;


	if(%p.rDmg <= 0)
		%p.rDmg = 1;

	%p.rLastDmg = $Sim::Time; // Set last look time for decay

	if(%p.client.staticDebug)
		centerPrint(%p.client,"DIST:" SPC %distance @ "<br>" @ "RPOS:" SPC %render.position @ "<br>" @ "PPOS:" SPC %p.position @ "<br>" @ "DMG:" SPC %p.rDmg-%dmgOld @ "<br>TDMG:" SPC %p.rDmg @ "<BR>DIF:" SPC %dif @ "<BR>SND:" SPC %p.rDmg/50,1);

	//if(%p.client.staticDebugImmune)
	//	return;

	%p.setWhiteOut(%p.rDmg/100);

	if(%p.rDmg >= 100) // If damage is ≥ 100, rip
	{
		%p.rDmg = 1;
		%p.setWhiteOut(1);
		%p.client.playSound(rAttackC);
		%p.kill();
	}
	else
	if(%p.rDmg > 0) // Otherwise, play sounds.
	{
		if(%p.rDmg >= 25)
			%p.client.playSound(rAttackB);
		else
			%p.client.playSound(rStatic);
	}
}

function Render_DoMount(%death,%p)
{
	if(isObject(%p.client)) // Checking if the player exists returns 1 (wtf?), but checking player.client doesn't.
	{
		%p.dismount(); // Just in case, we'll do this a second time
		%death.mountObject(%p,8);
	}
	else
		%death.delete();
}

// Freeze player function. This creates a "death mount" and forces the player on it, allowing us to freeze them without changing their datablock.
function Render_FreezePlayer(%p,%r)
{
	// If attack mode is 2, rip
	if(%r.mode == 2)
	{
		%p.kill();
		return;
	}

	if(isObject(%p))
	{
		%p.isFrozen = 1;
		%death = new AIPlayer()
		{
			dataBlock = RenderDeathArmor;
			scale = "0.2 0.2 0.2";
			position = "0 0 -999";
		};
		%death.render = %r;

		// Note: This might not work well with larger vehicles where the dismount point is far away from the actual seat.

		%p.dismount(); // We have to do this before we set the mount's position, otherwise it'll end up inside the vehicle.
		%death.setTransform(%p.getTransform());
		%death.playAudio(0,renderGrowl);
		MissionCleanup.add(%death);

		// We have to use a schedule so the player's view doesn't "flicker" while mounting. Item_Skis appears to use the same solution.
		// If anyone knows of a better solution, please let me know.
	 	%p.rDeathSchedule = schedule(100,0,Render_DoMount,%death,%p);
		%p.canDismount = 0;

		%r.freezeTarget = %p;
	}
}

function Render_UnfreezePlayer(%p,%r)
{
	if(isObject(%p))
	{
		cancel(%p.rDeathSchedule); // Cancel the 'death schedule' if there is one. This keeps us from freezing a player if Render just de-spawned.

		%p.isFrozen = 0;
		%mount = %p.getObjectMount();

		%pos = %mount.position; // Get the mount's position.

		if(%mount.dataBlock $= "RenderDeathArmor") // If we're in a death vehicle, dismount and delete it.
		{
			%p.canDismount = 1; // This is so the player can dismount if they get in a vehicle afterwards.
			%p.dismount();
			%mount.delete();
		}
	}

	%p.setTransform(%pos); // Set the player's position so they don't "jump" when dismounted.

	%r.freezeTarget = 0;
}

function Render_FreezeRender(%p)
{
	%p.dismount();

	%p.setMaxForwardSpeed(0);
	%p.setMaxBackwardSpeed(0);
	%p.setMaxSideSpeed(0);

	%p.setMaxCrouchForwardSpeed(0);
	%p.setMaxCrouchBackwardSpeed(0);
	%p.setMaxCrouchSideSpeed(0);

	%p.setMaxUnderwaterForwardSpeed(0);
	%p.setMaxUnderwaterBackwardSpeed(0);
	%p.setMaxUnderwaterSideSpeed(0);

	%p.rIsFrozen = 1;
}

function Render_UnfreezeRender(%p)
{
	%d = %p.dataBlock;

	%p.setMaxForwardSpeed(%d.maxForwardSpeed);
	%p.setMaxBackwardSpeed(%d.maxBackwardSpeed);
	%p.setMaxSideSpeed(%d.maxSideSpeed);

	%p.setMaxCrouchForwardSpeed(%d.maxForwardCrouchSpeed);
	%p.setMaxCrouchBackwardSpeed(%d.maxBackwardCrouchSpeed);
	%p.setMaxCrouchSideSpeed(%d.maxSideCrouchSpeed);

	%p.setMaxUnderwaterForwardSpeed(%d.maxUnderwaterForwardSpeed);
	%p.setMaxUnderwaterBackwardSpeed(%d.maxUnderwaterBackwardSpeed);
	%p.setMaxUnderwaterSideSpeed(%d.maxUnderwaterSideSpeed);

	%p.rIsFrozen = 0;
}

function Render_RequestDespawn(%r) // AI requests to delete the bot
{
	//if(isObject(%r))
	%r.delete();
	//else
	//	warn("Support_Render - Attempting to delete nonexistent bot!");
}

// # GLITCH GUN
function GlitchEnergyGunImage::onInit(%this, %obj, %slot)
{
	GlitchEnergyGunEffect(%this,%obj,%slot);
	return serverPlay3D(glitchFire, getWords(%obj.getTransform(), 0, 2));
}

function GlitchEnergyGunEffect(%this,%obj,%slot)
{
	%obj.setWhiteOut(1);
	InitContainerRadiusSearch(%obj.getPosition(),20,$TypeMasks::PlayerObjectType);
	while(%p=containerSearchNext())
	{
		//%p.setWhiteOut(1);
		if(%p.isRender)
		{
			//Render_Spawn_GetNewDirection(%p);
			//%p.setTransform(Render_Spawn_GetNewDirection(%p, %p.target.getEyePoint(), 0, 1));
			%p.delete();
		}
	}

	%currSlot = %obj.currTool;
	%obj.tool[%currSlot] = 0;
	%obj.weaponCount--;

	messageClient(%obj.client,'MsgItemPickup','',%currSlot,0);
	%obj.unMountImage(0);
}
