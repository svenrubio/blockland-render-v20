// **See init.cs for registration**

function rVerifyEvent() {
  %brick = %client.lastEventBrick;
  %minigame = %client.minigame;

  %brickOwner = %brick.client.bl_id;

  if(%minigame.owner) {
    %minigameOwner = %minigame.owner.bl_id;
  }
  else {
    %minigameOwner = %minigame.creatorBLID;
  }

  if(%brickOwner == %minigameOwner) {
    return true;
  }
  else {
    return false;
  }
}

function MiniGameSO::setRenderMode(%this, %rate, %client)
{
  if(rVerifyEvent()) {
    %minigame.rMode = %rate;
  }
}

function MiniGameSO::setRenderSpawnRate(%this, %rate, %client)
{
  if(rVerifyEvent()) {
    %minigame.rSpawnRate = %rate;
  }
}

function MiniGameSO::setRenderInvincibility(%this, %rate, %client)
{
  if(rVerifyEvent()) {
    %minigame.rInvincible = %rate;
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
