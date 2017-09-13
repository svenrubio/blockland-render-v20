// TODO: Fix attackers being unable to freeze players (may be specitic to victims)
// TODO: Jet mechanic
// TODO: Client interface
// TODO: Choosing spawn locations
// TODO: Slayer integration
// TODO: Minigame events integration

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
function Render_Player_Control_Loop(%render)
{
  if(!%render.attackInit)
    %string = "\c7[\c4light\c7] to attack";
  else
    %string = "\c7[\c4light\c7] to leave";


  %render.client.bottomPrint("<font:impact:38>\c7" @ mCeil((%render.loopPayNext-%render.loopCount)*$Render::C_LoopTimer/1000) @ "<just:right>" @ %string ,1,1);
}
