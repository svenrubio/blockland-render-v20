// **See init.cs for registration**

function rVerifyEvent(%client) {
  %brick = %client.lastEventBrick;
  %minigame = %client.minigame;

  %brickOwner = %brick.client.bl_id;

  if(%minigame.owner) {
    %minigameOwner = %minigame.owner.bl_id;
  }
  else {
    %minigameOwner = %minigame.creatorBLID;
  }

  // Extra precaution in case something goes wrong. This is a catch-all for a few potential exploits.
  if(%brickOwner $= "") {
    warn("Support_Render - Brick owner is blank for " @ %client @ "! Skipping event...");
    return false;
  }

  // No mini-game events if disabled by host
  if($Pref::Server::RenderAdminMinigamePrefs && !%brick.client.isAdmin) {
    if(!%brick.client.rDisabledMsg) {
      messageClient(%brick.client, '', "WARNING: Renderman minigame config is currently admin only on this server.");
      %brick.client.rDisabledMsg = 1;
    }

    return false;
  }

  if(%brickOwner == %minigameOwner) {
    return true;
  }
  else {
    return false;
  }
}

// If the host disables minigame config, we have to make sure we clear existing options.
// See init.cs and compat/slayer.cs for defaults.
function Render_MinigameCheck(%minigame) {
  if(%minigame.owner) {
    %owner = findClientByBL_ID(%minigame.owner.bl_id);
  }
  else {
    %owner = findClientByBL_ID(%minigame.creatorBLID);
  }

	if(!%owner.isAdmin && $Pref::Server::RenderAdminMinigamePrefs) {
    %minigame.rMode = -1;
    %minigame.rSpawnRate = -1;
    %minigame.rInvincible = -1;
    %minigame.rSpawnRatePlayer = -1;
	}
}

function MiniGameSO::setRenderMode(%this, %rate, %client)
{
  if(rVerifyEvent(%client)) {
    %this.rMode = %rate;
  }
}

function MiniGameSO::setRenderSpawnRate(%this, %rate, %client)
{
  if(rVerifyEvent(%client)) {
    %this.rSpawnRate = %rate;
  }
}

function MiniGameSO::setRenderInvincibility(%this, %rate, %client)
{
  if(rVerifyEvent(%client)) {
    %this.rInvincible = %rate;
  }
}

function FxDTSBrick::setRDetectorLevel(%this, %level)
{
  %this.rDetectorLevel = %level;
}

function FxDTSBrick::incRDetectorLevel(%this, %level)
{
  // If value is nonexistent, it is treated as "6" by default.
  // We'll have to initialize it as such before adding or subtracting it.
  if(%this.rDetectorLevel $= "") {
    %this.rDetectorLevel = 6;
  }

  %this.rDetectorLevel += %level;

  if(%this.rDetectorLevel > 10) {
    %this.rDetectorLevel = 10;
  }
}

function FxDTSBrick::decRDetectorLevel(%this, %level)
{
  if(%this.rDetectorLevel $= "") {
    %this.rDetectorLevel = 6;
  }

  %this.rDetectorLevel -= %level;

  if(%this.rDetectorLevel < 1) {
    %this.rDetectorLevel = 1;
  }
}
