// TODO: Fix attackers being unable to freeze players (may be specitic to victims)
// TODO: Jet mechanic
// TODO: Client interface
// TODO: Choosing spawn locations
// TODO: Slayer integration
// TODO: Minigame events integration

// # CreateRenderUser
function createRenderUser(%client)
{
  // ## Properties
  %render = %client.player;
  %render.isRenderPlayer = 1;
  %render.isRender = 1;
  %render.changeDatablock(PlayerRenderArmor);

  // ## Minigame Preferences
  // TODO: Move to a separate function so this isn't repeated (see render.cs)
  if(%client.minigame.rMode !$= "" && %client.minigame.rMode != -1)
		%render.mode = %client.minigame.rMode;
	else
		%render.mode = $Pref::Server::RenderDamageType;

	if(%client.minigame.rInvincible !$= "" && %client.minigame.rInvincible != -1)
		%render.invincible = %client.minigame.rInvincible;
	else
		%render.invincible = $Pref::Server::RenderIsInvincible;

  RenderBotGroup.add(%render);

  // ## Loop
  if(!isEventPending($Render::LoopBot))
    $Render::LoopBot = schedule($Render::C_LoopTimer,0,Render_Loop);
}

///// # Player control loop
function Render_Player_Control_Loop(%render)
{
  %string = "\c7[\c6light\c7] leave    ";
  if(!%render.attackInit && %render.mode != 3)
    %string = %string @ "\c7[\c6plant\c7] attack ";

  %render.client.bottomPrint("<font:impact:38>\c7" @ mCeil((%render.loopPayNext-%render.loopCount)*$Render::C_LoopTimer/1000) @ "<just:right>" @ %string ,1,1);
}
