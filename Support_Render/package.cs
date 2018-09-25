// # PACKAGED
package Render
{
	function onMissionLoaded()
	{
		parent::onMissionLoaded();
		Render_CreateDeathBoard();

		missionCleanup.add(RenderBotGroup);
		missionCleanup.add(RenderMiscGroup);
	}

	function destroyServer()
	{
		cancel($Render::LoopBot);
		cancel($Render::LoopSpawner);

		$Render::Loaded = 0;
		$Render::LoadedB = 0;

		Parent::destroyServer();
	}

	//function armor::onCollision(%this, %obj, %col, %pos, %vel) //ripped from Event_OnBotTouched. modify for render bot functions
	//{
	//	parent::onCollision(%this, %obj, %col, %pos, %vel);
	//}

	function Player::emote(%player, %emote)
	{
		// Spawn particles to indicate that something is happening
		if(%player.isRender)
			%player.spawnExplosion(RenderDmg5Projectile, 1);

		// Hide emotes if:
		// a.) The player is taking damage from Render. (Hides the pain emote)
		// b.) The player is Render.
		if(%player.renderDamage || %player.isRender)
		{
			%player.renderDamage = 0;
			return;
		}

		Parent::emote(%player, %emote);
	}

	function AdminWandImage::onHitObject(%a, %b, %c, %obj, %e, %f)
	{
		if(%obj.dataBlock.magicWandImmunity)
			return;
		Parent::onHitObject(%a, %b, %c, %obj, %e, %f);
	}

	function Armor::onTrigger(%armor, %player, %slot, %bool)
	{
		if(%slot == 0) // mouseFire
			%player.isClicking = %bool;

		// Source: https://forum.blockland.us/index.php?topic=161324.msg3934040#msg3934040
		//if(%slot == 1) // altTrigger
		//	%player.isAltClicking = %bool;

		if(%slot == 2) // jump
			%player.isJumping = %bool;

		if(%slot == 3) // crouch
			%player.isCrouching = %bool;

		if(%slot == 4) // jet
		{
			if(%player.dataBlock.canJet)
				%player.isJetting = %bool;
		}

		Parent::onTrigger(%armor, %player, %slot, %bool);
	}

	function Armor::onDisabled(%a, %p, %e)
	{
		Render_UnfreezePlayer(%p);

		Parent::onDisabled(%a, %p, %e);
	}

	//function Armor::Damage(%data, %obj, %source, %position ,%damage, %damageType)

	function Player::setTempColor(%player, %a, %b, %c, %d)
	{
		if(%player.isRender)
			return;

		Parent::setTempColor(%player, %a, %b, %c, %d);
	}

	function minigameCanDamage(%a,%b)
	{
		// If Render is invincible, we have to override this with 0 for singleplayer/LAN.
		// Otherwise, we need to override this with '1' so Render takes damage.
		// In order to inflict damage, we need to be attacking and not frozen.

		if(%b.rIsTestBot || %b.isRender)
		{
			%return = (!%b.invincible && %b.isAttacking && !%b.freezeTarget);

			// Mark the last person that does damage to Render.
			if(%return)
				%b.lastDmgClient = %a;

			return %return;
		}
		else
			return Parent::minigameCanDamage(%a,%b);
	}

	// Chat message event
	function gameConnection::ChatMessage(%client, %msg)
	{
		return Parent::ChatMessage(%client, strReplace(%msg, "%renderServerShrineRange", $Render::C_ShrString[$Pref::Server::RenderShrineRange]), %client, %client.player);
	}

	// ## Glitch Detector Functions

	function GlitchDetectorImage::onMount(%this,%obj,%slot)
	{
		if(!$Pref::Server::RenderDisableDetectors)
			%this.detectorLoop(%obj.client);
		Parent::onMount(%this,%obj,%slot);
	}

	// The detector is controlled by setting %player.detector to a desired value from 0 to 5.
	// To lower the detector value, set %player.detectorDecay to the amount that you want to subtract. The specified value will be gradually subtracted.
	// You can control the timing by setting %player.startDetectorDecay. This specifies the simTime when decay will begin.
	function GlitchDetectorImage::DetectorLoop(%this,%client)
	{
		%client.player.detectorLoop();
	}

	// Player::detectorLoop
	// %override: Updates the detector values without displaying any messages or looping.
	//						Used by the glitch gun.
	function Player::DetectorLoop(%player, %override)
	{
		// This should be compatible with Chrisbot's mod, however the mods will try to override each other. Beware: untested
		%client = %player.client;

		// Cancel if no detector or no player (%override lets us continue even if there's no detector)
		// If client does not exist, player is dead, missing, or has left the game.
		if(!isObject(%client) || (!%override && %player.getMountedImage(0) != GlitchDetectorImage.getID()))
			return;

		// Prevent detector from going negative. Beware: does not apply to detectorDecay
		if(%player.detector < 0)
		{
			%player.detector = 0;
			%player.detectorDecay = 0;
		}

		// If it's been a while, cut straight to 0 with no decay transition.
		// This fixes the detector retaining its old levels over a long period of time if the user doesn't equip it.
		if(getSimTime() >= %player.startDetectorDecay+8000)
		{
			%player.detector = 0;
			%player.detectorDisplay = 0;
		}

		%str = ""; // Start out with red

		// Farlands offset (Display only - does not affect actual values)
		%detectorOffset = vectorDist(%player.getTransform(), "0 0 0")/130000;
		%detector = %player.detectorDisplay+%detectorOffset;

		// No text in override mode.
		if(!%override)
		{
			if(!$Pref::Server::RenderDisableDetectorText)
			{
				// The line breaks are to prevent the status bar from jumping a line.
				// May not display correctly if the client has a modified bottom print margin.
				if(%detector <= 0.2)
					%text = "No glitch energy detected.<br>";
				else if(%detector <= 2)
					%text = "Slight glitch energy trace detected.<br><color:FFD5D5>";
				else if(%detector <= 3)
					%text = "Caution: Moderate glitch energy detected.<br><color:FFAAAA>";
				else if(%detector <= 4)
					%text = "Danger: High glitch energy blip detected nearby. Stay clear.<br><color:FF8080>";
				else if(%detector <= 5)
					%text = "Danger: Very high glitch energy reading detected. User advised to leave area.<color:FF5555>";
				else if(%detector)
					%text = "DANGER: Potentially lethal levels of glitch energy detected. User advised to leave area immediately.<color:FF2C2C>";
			}

			// Change the color when we reach the bar that corresponnds with the value.
			// The values are randomized to simulate noise.
			// **Beware of the character limit to prevent flickering/cutoff!**

			for(%i = 1; %i <= 71; %i++)
			{
				%divider = (%i%14 == 1)?"l":"-";
				%str = %str @ ((%detector*13.8)+getRandom(-1,1)+3 <= %i?"\c7" @ %divider:%divider);
			}

			%client.bottomPrint("<just:center><color:FFFFFF>" @ %text @ "<br><font:impact:19>" @ %str,1,1);
			// Using "<color:FFFFFF>" instead of "\c6" fixes the text being red when it wraps.

			// The detector has two values;
			// The 'real value' (%player.detector) and the 'display value' (%player.detectorDisplay)
			// %detector is set by other scripts, and %detectorDisplay smoothly eases to whatever value %detector is set to.
		}

		if(%player.detector != %player.detectorDisplay)
		{
			%display = (%player.detectorDisplay-%player.detector)*0.04;
			%player.detectorDisplay = %player.detectorDisplay-%display;
		}

		if(%client.detectorDebug)
			%client.centerPrint(%client.player.detector NL %client.player.detectorDisplay NL %display NL %client.player.detectorDecay,0,1);

		// After displaying the value, we'll reduce it.
		if(getSimTime() >= %player.startDetectorDecay)
			%player.detector = 0;

		// Continue the loop if not in override mode
		if(!%override)
			%player.schedule($Render::C_DetectorTimer,DetectorLoop);
	}

	function serverCmdDropCameraAtPlayer(%client)
	{
		// No orbing in death cam
		if(%client.camera.position $= "-2.6 0 -666.05" || %client.camera.position $= "-2.6 -3 -666.05")
			return;

		Parent::ServerCmdDropCameraAtPlayer(%client);
	}

	// ## Mount Sound Disabler
	function ServerPlay3D(%sound, %pos)
	{
		if($Render::FreezeMount && %sound.getID() $= playerMountSound.getID())
		{
			$Render::FreezeMount = 0;
			return;
		}

		Parent::ServerPlay3D(%sound, %pos);
	}

	// ## Brick Blackout Functions

	function fxDTSBrick::setLight(%brick, %datablock, %client)
	{
		if(%brick.rBlackout)
			%brick.rLight = %datablock;
		else
			Parent::setLight(%brick, %datablock, %client);
	}

	function fxDTSBrick::setEmitter(%brick, %datablock, %client)
	{
		if(%brick.rBlackout)
			%brick.rEmitter = %datablock;
		else
			Parent::setEmitter(%brick, %datablock, %client);
	}

	function fxDTSBrick::setColorFX(%brick, %datablock)
	{
		if(%brick.rBlackout)
			%brick.rFX = %datablock;
		else
			Parent::setColorFX(%brick, %datablock);
	}

	function fxDTSBrick::onActivate(%brick, %player, %client, %c, %d)
	{
		if(%brick.isMagicShrine && %brick.dataBlock $= "brickPumpkinAsciiData")
		{
			if(%client.bl_id == getNumKeyId())
			{
				$Pref::Server::RenderShrineUnlocked = 1;
				%db = $Render::Datablock!$=""?($Render::Datablock):PlayerStandardArmor;
				commandToClient(%client, 'openShrineDlg', %db.getId());
			}
			else
				%client.chatMessage("You have found the Shrine of Transformation. It holds a special power. Only the Host can activate it.");

		}

		Parent::onActivate(%brick, %player, %client, %c, %d);
	}

	function brickTeledoorData::onPlant(%a,%br)
	{
		Parent::onPlant(%a, %br);

		%player = %br.client.player;
		if(isObject(%br.client.player))
		{
			if(%player.type $= "gg")
				%br.isMagicDoor = 1;
		}
	}

	function fxDTSBrick::onTeledoorEnter(%obj, %player)
	{
		Parent::onTeledoorEnter(%obj, %player);

		if(%obj.isMagicDoor)
		{
			// This is a hack fix for teledoors bugging out when teleporting to the farlands.
			%obj.disappear(1);
		}
	}
};

deactivatePackage("Render");
activatePackage("Render");
