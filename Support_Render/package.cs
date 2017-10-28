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
		if(isObject(%p.client))
		{
			if(%p.client.doRenderDeath)
			{
				// Note: Invincible players are still marked as 'dead' by Render.
				// If they later die by other means, they will see the Render death screen.
				%p.client.doRenderDeath();
				%p.client.doRenderDeath = 0;
			}
		}

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

	// The detetector is controlled by setting %player.detector to a desired value from 0 to 5.
	// To lower the detector value, set %player.detectorDecay to the amount that you want to subtract. The specified value will be gradually subtracted.
	// You can control the timing by setting %player.startDetectorDecay. This specifies the simTime when decay will begin.
	function GlitchDetectorImage::DetectorLoop(%this,%client)
	{
		// This should be compatible with Chrisbot's mod, however the mods will try to override each other. Beware: untested
		%player = %client.player;

		// Cancel if no detector or no player
		if(!isObject(%player) || %player.getMountedImage(0) != GlitchDetectorImage.getID())
			return;

		// Prevent detector from going negative. Beware: does not apply to detectorDecay
		if(%player.detector < 0)
		{
			%player.detector = 0;
			%player.detectorDecay = 0;
		}

		%str = ""; // Start out with red

		if(!$Pref::Server::RenderDisableDetectorText)
		{
			// The line breaks are to prevent the status bar from jumping a line.
			// May not display correctly if the client has a modified bottom print margin.
			if(%player.detector < 0.2)
				%text = "No glitch energy detected.<br>";
			else if(%player.detector < 2)
				%text = "Slight glitch energy trace detected.<br><color:FFD5D5>";
			else if(%player.detector < 3)
				%text = "Caution: Moderate glitch energy detected.<br><color:FFAAAA>";
			else if(%player.detector < 4)
				%text = "Danger: High glitch energy blip detected nearby. Stay clear.<br><color:FF8080>";
			else if(%player.detector < 5)
				%text = "Danger: Very high glitch energy reading detected. User advised to leave area.<color:FF5555>";
			else if(%player.detector)
				%text = "DANGER: Potentially lethal levels of glitch energy detected. User advised to leave area immediately.<color:FF2C2C>";
		}

		// Change the color when we reach the bar that corresponnds with the value.
		// The values are randomized to simulate noise.
		// **Beware of the character limit!**
		for(%i = 1; %i <= 80; %i++)
	    %str = %str @ ((%client.player.detector*15.6)+getRandom(-1,1)+3 <= %i?"\c7-":"-");

		%client.bottomPrint("<just:center><color:FFFFFF>" @ %text @ "<br><font:arial black:14>I" @ %str @ "I",1,1);
		// Using "<color:FFFFFF>" instead of "\c6" fixes the text being red when it wraps.

		// After displaying the value, we'll reduce it. (Only applies to values set via detectorDecay)
		if(getSimTime() >= %player.startDetectorDecay)
		{
			%decay = %player.detectorDecay/20;

			if(%decay < 0.01)
				%decay = %player.detectorDecay; // Get rid of the rest so the value stops at zero.

			%player.detectorDecay = %player.detectorDecay-%decay;
			%player.detector = %player.detector-%decay;
		}

		%this.schedule($Render::C_DetectorTimer,DetectorLoop,%client);
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

};

deactivatePackage("Render");
activatePackage("Render");
