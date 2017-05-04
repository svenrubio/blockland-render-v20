// See init.cs for registration

function MiniGameSO::setRenderSpawnRate(%this, %rate, %client)
{
  %brick = %client.renderLastClick;
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
