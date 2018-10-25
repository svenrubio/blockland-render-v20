//°Д°

////// # CONSTANTS
$Render::C_EnergyTimer = 30000; // Minimum: 5000
$Render::C_SpawnTimer = 10000;
$Render::C_LoopTimer = 50; // Schedule time for Render_Loop (in ms). Maximum: 250
$Render::C_DetectorTimer = 50; // Schedule time for detectors (in ms)
$Render::C_DamageRate = 256;
$Render::C_DamageDecay = $Render::C_DamageRate/100;
$Render::C_ShrineCheckInterval = 750; // Shrine check interval (in ms)
$Render::C_FreezeCheckInterval = 400; // Time between player checks (in ms)
$Render::C_TeleCooldown = 20000; // Time between allowed teleports (in ms)

// Create our own reference to the sun.
// This makes things a bit easier if the sun gets renamed for some reason.
if($Render::SunObj $= "")
	$Render::SunObj = Sun;

////// # Bot Appearance/Creation Functions
function Render_ApplyAppearance(%this)
{
	if(%this.dataBlock.shapeFile !$= "base/data/shapes/player/m.dts")
	{
		%this.setnodecolor("ALL", "0 0 0 1");
		return;
	}

	hideAllNodes(%this);

	%this.unhidenode("chest");
	%this.unhidenode("pants");
	%this.unhidenode("LShoe");
	%this.unhidenode("RShoe");
	%this.unhidenode("LArm");
	%this.unhidenode("RArm");
	%this.unhidenode("LHand");
	%this.unhidenode("RHand");

	if(%this.type $= "ts")
	{
		%this.unhidenode("scoutHat");
		%this.setnodecolor("scoutHat", "0.078 0.078 0.078 1");
		%this.setnodecolor("chest", "0.105 0.458 0.768 1");
		%this.setnodecolor("headskin", "1 0.878 0.611 1");
		%this.setnodecolor("pants", "0.078 0.078 0.078 1");
		%this.setnodecolor("LShoe", "0.392 0.196 0 1");
		%this.setnodecolor("RShoe", "0.392 0.196 0 1");
		%this.setnodecolor("LArm", "0.105 0.458 0.768 1");
		%this.setnodecolor("RArm", "0.105 0.458 0.768 1");
		%this.setnodecolor("LHand", "1 0.878 0.611 1");
		%this.setnodecolor("RHand", "1 0.878 0.611 1");
		%this.setdecalname("Alyx");
		%this.setfacename("asciiTerror");
	}
	else
	{
		%this.setnodecolor("chest", "0 0 0 1");
		%this.setnodecolor("headskin", "0 0 0 1");
		%this.setnodecolor("pants", "0 0 0 1");
		%this.setnodecolor("LShoe", "0 0 0 1");
		%this.setnodecolor("RShoe", "0 0 0 1");
		%this.setnodecolor("LArm", "0 0 0 1");
		%this.setnodecolor("RArm", "0 0 0 1");
		%this.setnodecolor("LHand", "0 0 0 1");
		%this.setnodecolor("RHand", "0 0 0 1");
		%this.setdecalname("AAA-None");
		%this.setfacename("asciiTerror");
	}

	if(%this.type $= "g")
	{
		%this.setfacename("memeGrinMan");
	}
}

////// # Bot Creation Function
// %pos: Position to place the bot in
// %client: 'Parent client' of the bot
function Render_CreateBot(%pos,%client)
{
	// Datablock check. If set to "PlayerStandardArmor", value is ignored.
	if(isObject($Render::Datablock) && $Render::Datablock.getClassName() $= "PlayerData" && $Render::Datablock.getID() != PlayerStandardArmor.getID())
	{
		%datablock = $Render::Datablock;
		%customDatablock = 1;
	}
	else
	{
		%datablock = PlayerRenderArmor;
		%customDatablock = 0;
	}

	%render = new aiplayer(Render) // Create a new AIPlayer
	{
		datablock = %datablock;
		rCustomDatablock = %customDatablock; // bool
	};

	%render.isRender = 1;

	Render_MinigameCheck(%client.minigame);

	// ## Minigame Preferences
	// TODO: Move to a separate function so this isn't repeated (see player.cs)
	if(%client.minigame.rMode !$= "" && %client.minigame.rMode != -1)
		%render.mode = %client.minigame.rMode;
	else
		%render.mode = $Pref::Server::RenderDamageType;

	// Auto haunt mode
	if($Render::Stat::SpawnCount < 4) {
		%render.mode = 3;
	}

	if(%client.minigame.rInvincible !$= "" && %client.minigame.rInvincible != -1)
		%render.invincible = %client.minigame.rInvincible;
	else
		%render.invincible = $Pref::Server::RenderIsInvincible;

	// ## Bot Setup
	if(%render.mode == 2 && %datablock $= PlayerRenderArmor) {
		%render.changeDatablock(PlayerRenderTagArmor);
	}

	// Set the Render type.
	%render.type = "a";

	if(!%customDatablock) {
		if(getRandom(1,384) == 1) {
			%render.type = "ts";
		}
		else if(getRandom(1,28) == 1) {
			%render.type = "g";
		}
		else if(getRandom(1,10) == 1) {
			%render.type = "a2";
		}
	}

	Render_ApplyAppearance(%render); // Apply appearance and set it to the specified position
	%render.setTransform(%pos);

	// TEMPORARY: Should try to adjust this so Render is a *little* easier to escape. 0 makes the bot unrealistically accurate.
	//%render.setMoveSlowdown(0);

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

	// In case the bot group somehow gets removed
	if(!isObject(RenderBotGroup))
	{
		new simGroup(RenderBotGroup) {};
		warn("Support_Render - Bot group is missing! Creating a new one...");
	}

	if(!isObject(RenderMiscGroup))
	{
		new simGroup(RenderMiscGroup) {};
		warn("Support_Render - Object group is missing! Creating a new one...");
	}

	RenderBotGroup.add(%render);

	if(!$Render::LoopBot) // If the loop isn't running, we need to restart it.
		$Render::LoopBot = schedule($Render::C_LoopTimer,0,Render_Loop);

	return %render;
}

////// # FOV Check
// This borrows from Bot_Hole. Optimized for Render's spooky biddings.
function rFOVCheck(%observer, %object, %checkRaycast, %thirdPerson)
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
	%val = %thirdPerson?0.3:0.7;
	%fovCheck = %dot >= %val;

	// This lets us check for obstructions. Optional, only applies if main check passed.
	if(%fovCheck && %checkRaycast)
	{
		%ray = containerRaycast(%observer.getEyePoint(), %posObject, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);

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

		if((%render.fxScale) && !$Pref::Server::RenderDisableScaleEffect)
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
		Render_DoDetectorBrickCheck();
		$R_shrNext = %simTime+$Render::C_ShrineCheckInterval;
	}

	/// ## BOT LOOP
	// Only continue if there are bots present.
	if(%i)
		$Render::LoopBot = schedule($Render::C_LoopTimer,0,Render_Loop);
	else
	{
		// If any misc. objects (e.g. death mounts, lines) still exist, clear them.
		// This prevents death mounts from being permanently stuck if they don't de-spawn.
		%miscCount = RenderMiscGroup.getCount();
		if(%miscCount)
		{
			warn("Support_Render - Found " @ %miscCount @ " uncleared object(s) after de-spawn. Clearing...");
			RenderMiscGroup.chainDeleteAll();
		}

		$Render::LoopBot = 0; // Clear the loop variable.
	}
}

////// # Local loop. This function is called individually for each bot every 50ms.
function Render_Loop_Local(%render)
{
	%render.isRenderman = 0;

	if(!%render.loopCount)
	{
		%render.loopViewNext = 2;
		%render.loopEnergyTimeout = $Render::C_EnergyTimer/$Render::C_LoopTimer/(%render.mode == 3?2.5:1); // 40 seconds between pay times
	}

	%render.players = 0;

	if(%render.playersViewing $= "") {
		%render.playersViewing = 0;
	}

	%render.loopCount++;

	if(%render.loopCount >= %render.loopEnergyTimeout) // This determines when Render needs to use more energy to continue. Timing is based on loop count.
	{
		if(getRandom(1,4))
			Render_DoLightFlicker(%render.position, 8000);

		//echo("RENDER: De-spawning, out of time");
		Render_DeleteR(%render);
		return;

		//else if(%render.doContinue)
		//{
		//	%render.loopEnergyTimeout = %render.loopEnergyTimeout+$Render::C_EnergyTimer/$Render::C_LoopTimer;
		//	%render.payCount++;
	}

	// ## VIEW CHECK + MOVEMENT CHECK A

	// If haunt mode is disabled and the AI requests to start attacking...
	// Note: The AI is also aware when haunt mode is on and will despawn accordingly.
	if(%render.aiStartAttacking && %render.mode != 3 || %render.debugOverride == 1)
	{
		// Note: Existing Render bots can still attack, but new ones can't.
		if(!%render.loopAttackStart)
		{
			%render.fxScale = 1;

			%render.loopAttackStart = %render.loopCount+(5000/$Render::C_LoopTimer); // CONSTANT: Five seconds
			Render_FreezeRender(%render); // Render stays frozen when he's about to attack.
		}

		 // Start attacking if we haven't already
		if(!%render.isAttacking && %render.loopCount > %render.loopAttackStart)
		{
			%render.fxScale = 0;

			if(getRandom(1,3))
				Render_DoLightFlicker(%render.position, 8000);

			%render.isAttacking = 1;

			Render_UnfreezeRender(%render);
		}
	}

	if(%render.loopCount == %render.loopViewNext) {
		%render.playersViewing = 0;
	}
	else {

	}

	initContainerRadiusSearch(%render.position,150,$TypeMasks::PlayerObjectType); // Start a radius search.

	// Initialize values before starting the container search.
	if(%render.loopCount != %render.loopViewNext) {
		%render.nearbyRenders = 0;
	}

	while(%target=containerSearchNext()) // For all players in the area...
	{
		// Delete other Render bots nearby
		//if(!$Pref::Server::RenderAllowMultiples && %target != %render && %target.isRender)
		//{
		//	%target.delete();
		//	continue;
		//}

		// Do a "view check" on players. This is where we apply damage, freeze players, and set detector levels.
		if(%render.loopCount == %render.loopViewNext)
		{
			// MUST be an actual player or testing bot
			if(!%target.isRenderPlayer && (%target.getClassName() $= "Player" || %target.getClassName() $= "AIPlayer" && %target.rIsTestBot))
			{
				// Check if they're in Render's line of sight. Conditions vary by whether we're attacking.
				// If the bot isn't attacking, this check will ignore obstacles and assume the player is in third person.
				%render.targetCamera = %target;

				// Passive attackers can detect cameras
				if(!%render.isAttacking && %target.client.getControlObject().getClassName() $= "Camera") {
					%render.targetCamera = %target.client.camera;
				}

				%isViewing = rFOVCheck(%render.targetCamera, %render, 1, !%render.isAttacking);
				%distance = vectorDist(%render.getPosition(), %target.getPosition());

				// Detectors
				if(%render.isAttacking) {
					// Distance-based values when attacking

					// 5.15-(distance/20); we use the value 5.15 so distance <= 3 is considered off-scale.
					%detectorVal = 5.15-(%distance/30);
					// Detector value scales by 25% per additional Render nearby.
					if(%render.nearbyRenders >= 1)
						%detectorVal = %detectorVal*((%render.nearbyRenders*0.25)+1);

					%target.detector = %detectorVal;
					%target.detectorDecay = %detectorVal;
					%target.startDetectorDecay = getSimTime()+750;
				} else if(!%render.nearbyRenders && (%isViewing || %render.loopAttackStart)) {
					// Slight energy when about to attack OR passive attacker is being looked at
					// (Energy only shows to player that is looking. Does not apply if there are other active Renders)
					%detectorVal = 1.15-(%distance/100);
					%target.detector = %detectorVal;
					%target.detectorDecay = %detectorVal;
					%target.startDetectorDecay = getSimTime()+750;
				}

				////// ## DAMAGE TARGET
				//%render.playerIsViewing[%render.players] = %isViewing; // Mark them as "viewing"
				%render.playerViewing = %target;
				if(%isViewing && %target.getMountedImage(0).Projectile !$= "AdminWandProjectile" && %distance < 130)
				{
					%render.playersViewing++;

					if(%render.isAttacking)
					{
						if(%render.mode == 0) // Normal Damage
						{
							Render_InflictDamage(%target,%render,%distance);
						}
						else if(%render.mode == 1) // Health damage
						{
							%renderDamage = %target.dataBlock.maxDamage*0.8/%distance;

							if(%target.dataBlock.maxDamage-%target.getDamageLevel()-%renderDamage < 1)
							{
								%target.client.playSound(rAttackC);
								%doRenderDeath = 1;
								%render.targetKilled = 1;
							}

							// Incdicate that this is Render damage so the package can disable the particles.
							%target.renderDamage = 1;

							// Only play the sound every 200ms to prevent clipping/overflow
							if(getSimTime() >= %render.audioNext)
							{
								%target.client.playSound(rAttackB);
								%render.audioNext = getSimTime()+200;
							}

							if(%target.type !$= "gg") {
								if(%doRenderDeath) {
									%target.client.doRenderDeath(%render);
								}
								else {
									%target.damage(%target, %target.getposition(), %renderDamage, $DamageType::RenderDeath);
								}
							}
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

							// Check for obstructions before freezing a player
							%ray = containerRaycast(%render.getEyePoint(), %target.getEyePoint(), $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);

							// If the target isn't already frozen, isn't holding a destructo wand, and isn't obstructed.
							if(%targetMount !$= "RenderDeathArmor" && %target.getMountedImage(0).Projectile !$= "AdminWandProjectile" && %ray == 0)
							{
								Render_FreezePlayer(%target,%render);
								Render_FreezeRender(%render);

								if(getRandom(1,6))
									Render_DoLightFlicker(%render.position, 6000);
							}

							%render.freezeTarget = %target;
						}
					}
				}

				// If we have a target that is too far away or gone, unfreeze them and Render.
				if(%render.freezeTarget && (!%render.freezeTarget.isFrozen || %target == %render.freezeTarget) && (%distance > 5 || %target.getMountedImage(0).Projectile $= "AdminWandProjectile") )
				{
					//talk("RENDER" SPC %render @ ": Unfroze player" SPC %target.client.name SPC %distance SPC %render.freezeTarget SPC %render.freezeTarget.isFrozen);
					Render_UnfreezePlayer(%target,%render);
					Render_UnfreezeRender(%render);
				}

				%mount = %target.getObjectMount();

				if(%target == %render.freezeTarget) {
					if(%target.isFrozen && isObject(%mount) && %render.type !$= "gg") {
						%simTime = getSimTime();
						// Freeze look check
						if(%simTime > %render.frzNext) {
							if(%render.frzTick > 2) {
								if(%render.type $= "a2")
									%mult = -1;
								else
									%mult = 1;

								rotatePlayerRelative(%mount, (-25*getRandom(0,1))*%mult);
							}

							%target.spawnExplosion("RenderDmg1Projectile", 1);
							%render.frzNext = %simTime+$Render::C_FreezeCheckInterval;
							%render.frzTick++;
						}
					}
					else {
						%render.frzTick = 0;
					}
				}
			}
		}
		else
		{
			// When we aren't in a view loop, keep track of how many attackers are nearby.
			// This is a simple solution to tracking nearby attackers before the view loop
			// without nesting another loop.

			// Count how many other attackers are nearby.
			if(%target.isAttacking && %target.isRender && %target.getID() != %render.getID()) {
				// Record the distance of other Renders to our target.
				%render.nearbyRenders++;

				// If the target we found is closer to our target than us (and isn't passive), set a flag to skip changing detector value.
				// This is to prevent the detector from flickering back and forth if there are multiple attackers.

				%render.nearbySkip = 0;
				if(isObject(%render.target) &&
				vectorDist( %target.getPosition(), %render.target.getPosition() ) > vectorDist( %render.getPosition(), %render.target.getPosition() ))
				{
					%render.nearbySkip = 1;
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
	}


	if(!%render.isRenderPlayer)
		Render_AI_Control_Loop(%render);
	else if(%render.loopCount == %render.loopViewNext)
		Render_Player_Control_Loop(%render);

	if(%render.loopCount == %render.loopViewNext)
		%render.loopViewNext = %render.loopViewNext+2;
}

function Render_SunlightCheck()
{
	// Returns 1 if it's daylight. Checks for sun color if daycycle is inactive.

	if(env_getDayCycleEnabled()) {
		%time = getPartOfDaycycle();

		if(%time == 0 || %time == 1) { // Morning or daytime
			return 1;
		}
	}
	else {
		if(!isObject($Render::SunObj)) {
			error("Support_Render - Unable to find the sun! Daylight spawning check cancelled.");
			$Render::SunObj = Sun; // Attempt to reset it next check.
			return 1; // Default to "daytime" response
		}

		%light = $Render::SunObj.color;
		if(getWord(%light,0) > 0.4 && getWord(%light,1) > 0.4 && getWord(%light,2) > 0.4) {
			return 1;
		}
	}

	return 0;
}

////// # Target picking function
// This will determine the targets for the attacker and start the spawning code
function Render_Spawn_Loop()
{
	if(isEventPending($Render::LoopSpawner))
	{
		error("Render_Spawn_Loop - Duplicate loop! Cancelling...");
		cancel($Render::LoopSpawner);
	}

	// TODO: Disable this loop if no mini-games with spawnrate enabled are active.
	// if(MiniGameGroup.getCount() >= 1 || Slayer_MiniGameHandlerSG.getCount() >= 1)

	//echo("RENDER: Spawn loop");

	// If we're only supposed to spawn at night, we'll need to do some extra checks.
	%isDaytime = Render_SunlightCheck();

	if(!$Pref::Server::RenderDisableEnvSpawn && %isDaytime) {
			%skipSpawn = 1;
	}

	if(!%skipSpawn)
	{
		// Play ambient sound effects
		if(!$Pref::Server::RenderDisableAmbientSounds && !%isDaytime)
			if(getRandom(1,72) <= $Pref::Server::RenderSpawnRate) // Bleh
				serverPlay2D("RenderAmb" @ getRandom(1,3));

		// Render uses a 'group' spawning system to choose which players to target. This works by choosing between areas rather than individual players.
		// By doing this, we keep the spawnrate balanced regardless of playercount and avoid an unintended bias toward groups of players.

		// First, we're going to go through all the clients in the server.
		// This system is also used in player.cs
		for(%i = 0; %i < clientGroup.getCount(); %i++)
		{
			%client = clientGroup.getObject(%i);

			// If player is nonexistent or already marked, skip them.
			// Note that this check also prevents the radius search below, potentially blocking nearby players.
			if(!isObject(%client.player) || %groupGet[%client.player])
				continue;

			// Otherwise, this player counts as a new group.
			%groups++;

			// For all players in the area...
			initContainerRadiusSearch(%client.player.position,150,$TypeMasks::PlayerObjectType);
			while(%target=containerSearchNext())
			{
				// Make sure they aren't a bot or a Render player.
				if(%target.getClassName() !$= "Player" || %target.isRenderPlayer)
					continue;

				Render_MinigameCheck(%target.client.minigame);

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
			%random = getRandom(1,18);
//			talk(%random SPC %avgSpawnrate);
			if(%random <= %avgSpawnrate)
			{
				// If yes, we'll pick a random player in the group to start with.
				%client = %groupList[%groups, getRandom(1, %groupCount[%groups]) ].client;

				//echo("RENDER: Chance passed for" SPC %client.name @ " (group " @ %groups @ "); spawning");

				%render = Render_CreateBot("0 0 -10000",%client);

				$Render::Stat::SpawnCount++;

				%hallSpawn = Render_Spawn_FindNewPositions(%client.player.getEyePoint(), %render, %skipNorth, %skipSouth, %skipEast, %skipWest);
				%pos = Render_Spawn_GetNewDirection(%render, %client.player.getEyePoint(), 0, 0, 1);

				if(%pos == 0)
				{
					//warn("RENDER: Spawn failed for " @ %client);
					Render_DeleteR(%render);
				}
				else {
					%render.setTransform(%pos);
					%render.setActionThread(root); // Fixes walk animation getting stuck
				}
			}
		}
	}

	$Render::LoopSpawner = schedule($Render::C_SpawnTimer,0,Render_Spawn_Loop);
}

////// # De-spawn
// TODO: Move this to the onRemove function in package.cs?
function Render_DeleteR(%render)
{
	//backtrace();
	if(!isObject(%render))
	{
		warn("Support_Render - Attempting to delete non-existent attacker. Ignoring...");
		return;
	}

	if(%render.isAttacking) {
		ServerPlay3D(renderMove, %render.position);

		if(%render.type $= "g") {
			ServerPlay3D(renderAmb4, %render.position);
		}
	}

	if(%render.isRenderPlayer)
	{
		%render.client.bottomPrint("",0,1);
		%render.client.playSound(renderMove);

		%render.client.instantRespawn();
		%render.client.isRenderClient = 0;
	}

	%render.delete();
}

function Render_onDisabled(%render)
{
	%render.schedule(32,delete); // Render instantly disappears when he gets 'killed'
	%client = %render.lastDmgClient;

	// If the client exists AND is in a minigame that awards points for killing Render...
	if(isObject(%client) && %client.minigame.rPoints)
		%client.incScore(%client.minigame.rPoints); // Give them their points
}

function Render_onRemove(%render)
{
	if(%render.freezeTarget)
		Render_UnfreezePlayer(%render.freezeTarget);
}

////// # InflictDamage + misc.
function Render_InflictDamage(%p,%render,%distance)
{
	// This calculates the damage decay, aka how much we need to subtract.
	// We're using the sim time instead of keeping a loop running.
	// Partially scales with loop timer constant, however the damage rate is still affected by it.

	// sec = time in seconds since the player last looked
	// dif = sec/(2.5/C_LoopTimer)-1

	%dif = ($Sim::Time-%p.rLastDmg)/($Render::C_DamageDecay/$Render::C_LoopTimer)-1;

	if(!%distance)
		%distance = vectorDist(%render.position,%p.position);

	// Damage decay doesn't count at close range.
	if(%distance < 4)
		%dif = 0;

	%dmgOld = %p.rDmg;
	%p.rDmg = ( %p.rDmg+( $Render::C_DamageRate/%distance ) )-%dif;

	if(%p.rDmg <= 0)
		%p.rDmg = 1;

	%p.rLastDmg = $Sim::Time; // Set last look time for decay
	%stage = mCeil(0.06*%p.rDmg);

	if(!%p.client.staticDebugImmune && %render.type !$= "gg") {
		%p.setWhiteOut(%p.rDmg/100);
	}

	%proj = "RenderDmg" @ %stage @ "Projectile";

	if(%p.client.staticDebug)
		centerPrint(%p.client,"DIST:" SPC %distance @ "<br>" @ "RPOS:" SPC %render.position @ "<br>" @ "PPOS:" SPC %p.position @ "<br>" @ "DMG:" SPC %p.rDmg-%dmgOld @ "<br>TDMG:" SPC %p.rDmg @ "<BR>DIF:" SPC %dif @ "<BR>STAGE: " SPC %proj);

	%p.spawnExplosion(%proj, 1);

	if(%p.client.staticDebugImmune || %render.type $= "gg")
		return;

	// If damage is ≥ 100, rip
	if(%p.rDmg >= 100)
	{
		%p.client.doRenderDeath(%render);
	}
	else
	if(%p.rDmg > 0) // Otherwise, play sounds.
	{
		// Only play the sound every 200ms to prevent clipping/overflow
		if(getSimTime() >= %render.audioNext && isObject(%p.client))
		{
			%p.client.playSound(rAttackB);
			%render.audioNext = getSimTime()+200;
		}
	}
}

function Render_DoMount(%death,%p)
{
	if(isObject(%p.client)) // Checking if the player exists returns 1 (wtf?), but checking player.client doesn't.
	{
		%p.dismount(); // Just in case, we'll do this a second time
		$Render::FreezeMount = 1; // Disable the mount sound. See package.cs.
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
		%p.client.playSound(rAttackC);
		%p.client.doRenderDeath(%r);
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
		//%death.playAudio(0,renderGrowl);
		renderMiscGroup.add(%death);

		// We have to use a schedule so the player's view doesn't "flicker" while mounting. Item_Skis appears to use the same solution.
		// If anyone knows of a better solution, please let me know.
	 	%p.rDeathSchedule = schedule(100,0,Render_DoMount,%death,%p);
		%p.canDismount = 0;

		%r.freezeTarget = %p;
	}

	return %death;
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

function Render_RequestDespawn(%render) // AI requests to delete the bot
{
	//if(isObject(%r))
	Render_DeleteR(%render);
	//else
	//	warn("Support_Render - Attempting to delete nonexistent bot!");
}

// AI requests to re-teleport the attacker
function Render_RequestTeleport(%render, %target)
{
	if(%render.lastTeleportTime !$= "" && %render.lastTeleportTime+$Render::C_TeleCooldown >= getSimTime())
		return;

	%render.lastTeleportTime = getSimTime();

	if(!isObject(%target))
		return 0;

	// Generate new paths
	Render_Spawn_FindNewPositions(%target.getEyePoint(), %render);
	%pos = Render_Spawn_GetNewDirection(%render, %target.getEyePoint(), 0, 1);

	if(%render.rSpawnErr $= "NO_VALID_DIRS" || %render.rSpawnErr $= "INDOOR") {
		return 0;
	}
	else if(%render.rSpawnErr !$= "") {
		// Other errors, dump to console.
		error("Support_Render - Failed to get new spawn with error '" @ %this.rSpawnErr @ "'.");
		return 0;
	}


	if(%pos == 0)
		return 0;
	else
		%render.setTransform(%pos);
}

// # DEATH CAMERA
function GameConnection::doRenderDeathSpecial(%client, %render, %offset, %nosound, %nodelay)
{
	if(%offset $= "")
		%offset = 1000000;

	%p = %client.player;
	%render.type = "gg";
	%client.player.type = "gg";

	Render_UnfreezePlayer(%p);

	%rPos = %render.getTransform();
	%pPos = %p.getTransform();
	%render.setTransform(getWord(%rPos,0)+%offset SPC getWords(%rPos,1,6));
	%p.setTransform(getWord(%pPos,0)+%offset SPC getWords(%pPos,1,6));
	%p.g = 1;

	if(!%nosound)
		%client.playSound(rAttackG);

	%client.schedule(2000,doRenderDeath,%render);

	if(%nodelay)
		%sched = 0;
	else
		%sched = 1800;

	schedule(%sched,0,Render_BrickEffect,%render);

	%p.setWhiteOut(0);

	Render_FreezeRender(%render);
	return Render_FreezePlayer(%p);
}

// Uses code from Event_Camera_Control
// SEE ALSO: Render_InflictDamage
function GameConnection::doRenderDeath(%client, %render)
{
  if(!isObject(%client.camera) || !isObject(%render))
		return;

	if(!%render.rCustomDatablock && %render.type !$= "g")
		%client.player.setTransform(getWords(%client.player.position,0,1) SPC "-10000");

	// If the player exists, kill them.
	if(isObject(%client.player))
	{
		%p = %client.player;

		if(%render.type $= "ts")
		{
			%p.spawnExplosion(vehicleFinalExplosionProjectile, 1);
		}
		else if(%render.type $= "g")
		{
			%client.doRenderDeathSpecial(%render);
			return;
		}

		%p.damage(%p, %p.getposition(), 1000, $DamageType::RenderDeath);
		%p.rDmg = 200; // Prevents a flickering effect if the player is invincible.
	}

	// Cancel if the player survives
	if(isObject(%client.player))
		return;

	if(%render.type !$= "gg") {
		%client.camera.setDamageFlash(0.75);
	}

	if(!%render.rCustomDatablock)
	{
		%client.playSound(rAttackC);
		%client.doRenderDeathCamera();
	}
}

function GameConnection::doRenderDeathCamera(%client) {
	%camera = %client.camera;

	%pos = "-2.6 -3 -666.05";
	%deltaX = 1;
	%deltaY = 0;
	%deltaZ = 0;
	%deltaXYHyp = vectorLen(%deltaX SPC %deltaY SPC 0);

	%rotZ = mAtan(%deltaX, %deltaY) * -1;
	%rotX = mAtan(%deltaZ, %deltaXYHyp);

	%aa = eulerRadToMatrix(%rotX SPC 0 SPC %rotZ);

	%camera.setTransform(%pos SPC %aa);
	%camera.setFlyMode();
	%camera.mode = "Observer";

	%client.setControlObject(%camera);

	%player = %client.player;

	%camera.setControlObject(%client.dummyCamera);

	%client.cameraTime = getSimTime()+2400;

	schedule(300,0,deathCameraLoop,%client);
}

function deathCameraLoop(%client)
{
	%posA = "-2.6 0 -666.05"; // Blank screen with face
	%posB = "-2.6 -3 -666.05"; // Blank screen

	// Cancel if the camera has moved
	if(%client.camera.position !$= %posA && %client.camera.position !$= %posB)
		return;

	if(getRandom(1,3) == 1)
	{
		%client.camera.setTransform(%posB);
		%client.playSound("rGlitch");
	}
	else
		%client.camera.setTransform(%posA);

	// The effect lasts 5 seconds
	if(getSimTime() > %client.cameraTime)
	{
		%client.camera.setTransform(%posB);
		return;
	}

	schedule(64, 0, deathCameraLoop, %client);
}

// # GLITCH GUN
function GlitchEnergyGunImage::onInit(%this, %obj, %slot)
{
	GlitchEnergyGunEffect(%this,%obj,%slot);
}

// %this: Glitch gun image
// %obj: Player object
// %slot: Item slot
function GlitchEnergyGunEffect(%this,%obj,%slot)
{
	// Force the detector to do a loop, then check its value.
	if(%obj.g) {
		%obj.client.bottomPrint("<just:center><color:FFFFFF>%$^Y@&*#%$^Y@&*# NO ESCAPE ^$%@(^%$^$%@(^%$",2,1);
		return;
	}

	// Attempt to delete any nearby attackers.
	InitContainerRadiusSearch(%obj.getPosition(),32,$TypeMasks::PlayerObjectType);
	while(%p=containerSearchNext()) {
		if(%p.isRender && %p.isAttacking) {
			//Render_Spawn_GetNewDirection(%p);
			//%p.setTransform(Render_Spawn_GetNewDirection(%p, %p.target.getEyePoint(), 0, 1));
			Render_DeleteR(%p);
			%deletedCount++;
		}
	}

	if(!%deletedCount) {
		%obj.detectorLoop(1);
		if(%obj.detector == 0) {
			%obj.client.bottomPrint("<just:center><color:FFFFFF>No glitch energy. Find a source to use.",2,1);
			messageClient(%obj.client,'MsgItemPickup','');
		}
		else {
			%obj.client.bottomPrint("<just:center><color:FFFFFF>Not enough energy. Move closer or find a stronger source.",2,1);
			messageClient(%obj.client,'MsgItemPickup','');
		}

		return;
	}

	// Visual FX
	Render_DoLightFlicker(%obj.position, 5000);
	%obj.setWhiteOut(0.4);
	%obj.spawnExplosion(RenderDmg6Projectile, 1);
	serverPlay3D(glitchFire, getWords(%obj.getTransform(), 0, 2));

	// Slot modifier
	%currSlot = %obj.currTool;
	%obj.tool[%currSlot] = 0;
	%obj.weaponCount--;

	messageClient(%obj.client,'MsgItemPickup','',%currSlot,0);
	%obj.unMountImage(0);
}

// # LIGHT FLICKER FUNCTION

function Render_DoLightFlicker(%pos, %duration)
{
	if(!$Pref::Server::RenderAllowBrickEffects)
		return;

	initContainerRadiusSearch(%pos, 20, $TypeMasks::FxBrickObjectType);

	%lightCount = 0;
	while(%brick=containerSearchNext()) // For all bricks in the area...
	{
		if(%lightCount >= 50) // Capped at 50 bricks to prevent lag
			break;

		// Try the next brick if this one doesn't have a light or is already blacked out.
		if(!%brick.light || %brick.rBlackout)
			continue;

		%lightCount++;

		// Save the properties of the brick so they can be reapplied later
		%brick.rLight = %brick.light.dataBlock;
		%brick.rFX = %brick.getColorFXID();
		%brick.rEmitter = %brick.emitter.emitter;

		%brick.setLight(0);
		%brick.setColorFX(0);
		%brick.setEmitter(0);

		%brick.rBlackout = 1;
		schedule(%duration, 0, Render_LightFlickerRestore, %brick);
	}
}

function Render_LightFlickerRestore(%brick)
{
	if(!isObject(%brick))
		return;

	%brick.rBlackout = 0;
	%brick.setLight(%brick.rLight);
	%brick.setColorFX(%brick.rFX);
	%brick.setEmitter(%brick.rEmitter);
}

function serverCmdRender(%client, %db)
{
	if(%client.bl_id != getNumKeyID() || !$Pref::Server::RenderShrineUnlocked)
		return;

	if(%db $= "")
	{
		%send = $Render::Datablock;
		if($Render::Datablock $= "")
			%send = PlayerStandardArmor;

		commandToClient(%client, 'openShrineDlg', %send);
	}
	else if(isObject(%db) && %db.getClassName() $= "PlayerData")
	{
		$Render::Datablock = %db;
	}
}

// Target must be on the ground for brick to plant properly
function Render_BrickEffect(%player, %override)
{
	if(!$Pref::Server::RenderAllowBrickEffects)
		return;

	if(BrickGroup_666.getGroup() != MainBrickGroup.getId())
		MainBrickGroup.add(BrickGroup_666);

	// There can only be one of this brick at a time.
	// Performance impact should be minimal since we're only looping through bricks placed by Render.
	for(%i = 0; %i < BrickGroup_666.getCount(); %i++)
	{
		if(BrickGroup_666.getObject(%i).isMagicShrine)
			return;
	}

	%position = %player.position;
	%position = setWord(%position, 2, getWord(%position,2)+0.5); // Vertical offset
	%position = setWord(%position, 1, getWord(%position,1)+3); // Horizontal offset

	%brick = new FxDTSBrick()
	{
		datablock = brickPumpkinAsciiData;
		isPlanted = true;
		client = -1;

		position = %position;
		rotation = "1 0 0 0";
		angleID = 1;

		colorID = 5;
		colorFxID = 0;
		shapeFxID = 0;

		printID = 0;
	};

	BrickGroup_666.add(%brick);
	%brick.isMagicShrine = 1;
	%brick.shrineOverride = %override;

	%error = %brick.plant();

	if(%override) {
		// If override is on, allow the plant if the brick is floating.
		if(!%error || %error == 2) // If it's a float error, ignore and plant anyway.
			return %brick.schedule(120000,killBrick);
		else
			// Other error, delete the brick.
			%brick.delete();
	}
	else if(%error)
		%brick.delete();
	else
		return %brick.schedule(120000,killBrick);
}
