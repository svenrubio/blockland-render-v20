// **See init.cs for registration**

function MiniGameSO::setRenderMode(%this, %rate, %client)
{
  %brick = %client.lastEventBrick;
  %minigame = %client.minigame;

  %brickOwner = %brick.client.bl_id;

  if(%minigame.owner)
    %minigameOwner = %minigame.owner.bl_id;
  else
    %minigameOwner = %minigame.creatorBLID;

  if(%brickOwner != %minigameOwner)
    return;

  %minigame.rMode = %rate;
}

function MiniGameSO::setRenderSpawnRate(%this, %rate, %client)
{
  %brick = %client.lastEventBrick;
  %minigame = %client.minigame;

  %brickOwner = %brick.client.bl_id;

  if(%minigame.owner)
    %minigameOwner = %minigame.owner.bl_id;
  else
    %minigameOwner = %minigame.creatorBLID;

  if(%brickOwner != %minigameOwner)
    return;

  %minigame.rSpawnRate = %rate;
}

function MiniGameSO::setRenderInvincibility(%this, %rate, %client)
{
  %brick = %client.lastEventBrick;
  %minigame = %client.minigame;

  %brickOwner = %brick.client.bl_id;

  if(%minigame.owner)
    %minigameOwner = %minigame.owner.bl_id;
  else
    %minigameOwner = %minigame.creatorBLID;

  if(%brickOwner != %minigameOwner)
    return;

  %minigame.rInvincible = %rate;
}

function FxDTSBrick::setRDetectorLevel(%this, %level)
{
  %this.rDetectorLevel = %level;
}
