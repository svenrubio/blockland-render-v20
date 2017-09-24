// TODO: Fix attackers being unable to freeze players (may be specitic to victims)
// TODO: Jet mechanic
// TODO: Client interface
// TODO: Choosing spawn locations
// TODO: Slayer integration
// (see compat/slayer.cs and GameMode_Slayer/server/defaults/team-preferences.cs)
// TODO: Minigame events integration
// TODO: ai_spawn integration
// TODO: Replace free movement with manual button when freezing players
// (Rather than being able to walk away, attacker should only be able to release the player by pressing a button)
// TODO: Test invincible pref

// See package.cs for button press code

////// # CreateRenderPlayer
function createRenderPlayer(%player)
{
  // ## Properties
  %player.isRenderPlayer = 1;
  %player.isRender = 1;
  %player.changeDatablock(PlayerRenderArmor);
  Render_ApplyAppearance(%player);

  // ## Minigame Preferences
  // TODO: Move to a separate function so this isn't repeated (see render.cs)
  if(%client.minigame.rMode !$= "" && %client.minigame.rMode != -1)
		%player.mode = %client.minigame.rMode;
	else
		%player.mode = $Pref::Server::RenderDamageType;

	if(%client.minigame.rInvincible !$= "" && %client.minigame.rInvincible != -1)
		%player.invincible = %client.minigame.rInvincible;
	else
		%player.invincible = $Pref::Server::RenderIsInvincible;

  RenderBotGroup.add(%player);

  // ## Loop
  if(!isEventPending($Render::LoopBot))
    $Render::LoopBot = schedule($Render::C_LoopTimer,0,Render_Loop);
}

////// # Do Render Transition
function Render_DoRenderTransition(%client)
{
  %client.isRenderClient = 1;
  %client.instantRespawn();
}

////// # Player control loop
function Render_Player_Control_Loop(%render)
{
  %string = "\c7[\c6light\c7] leave ";

  // TODO: Apply the 'five-second rule' in render.cs
  if(!%render.attackInit && %render.mode != 3 && %render.loopCount <= %render.loopEnergyTimeout-(5000/$Render::C_LoopTimer))
    %string = %string @ "\c7[\c6plant\c7] attack ";

  %render.client.bottomPrint("<font:impact:38>\c7" @ mCeil((%render.loopEnergyTimeout-%render.loopCount)*$Render::C_LoopTimer/1000) @ "<just:right>" @ %string ,1,1);
}


////// # Render Player Package # //////

package RenderPlayer
{
  function serverCmdlight(%client)
  {
    if(%client.player.isRenderPlayer)
      Render_RequestDespawn(%client.player);
    else
      Parent::serverCmdLight(%client);
  }

  function serverCmdPlantBrick(%client)
  {
    if(%client.player.isRenderPlayer)
    {
      // Two if checks are used so we only return to parent if they aren't a Render player.
      if(!%client.player.attackInit)
      {
        // The player sends a request to start attacking.
        %client.player.aiStartAttacking = 1;
        %client.player.attackInit = 1;
      }
    }
    else
      Parent::serverCmdPlantBrick(%client);
  }

  function GameConnection::createPlayer(%client, %transform)
  {
    //if(%client.isRenderClient)
    //	%transform = "0 0 -9999";

    Parent::createPlayer(%client, %transform);

    if(%client.isRenderClient)
    {
      // This is only needed on spawn, so we can set it to 0 now.
      %client.isRenderClient = 0;

      Render_ApplyAppearance(%client.player);
      createRenderPlayer(%client.player);
      %client.player.setShapeNameDistance(0);
    }
  }

  /// ## Server Command Disablers ## ///
  function serverCmdShiftBrick(%client, %a, %b, %direction)
  {
    if(%client.player.isRenderPlayer)
      return;

    Parent::ServerCmdShiftBrick(%client, %a, %b, %direction);
  }

  function serverCmdRotateBrick(%client, %direction)
  {
    if(%client.player.isRenderPlayer)
      return;

    Parent::ServerCmdRotateBrick(%client, %direction);
  }

  function serverCmdSit(%client)
  {
    if(%client.player.isRenderPlayer)
      return;

    Parent::ServerCmdSit(%client);
  }

  // TODO: Fix these on the client side so the boxes don't work.
  function serverCmdUseInventory(%client, %inv)
  {
    if(%client.player.isRenderPlayer)
      return;

    Parent::ServerCmdUseInventory(%client, %inv);
  }

  function serverCmdUseSprayCan(%client, %inv)
  {
    if(%client.player.isRenderPlayer)
      return;

    Parent::ServerCmdUseSprayCan(%client, %inv);
  }

  function serverCmdUseTool(%client, %inv)
  {
    if(%client.player.isRenderPlayer)
      return;

    Parent::ServerCmdUseTool(%client, %inv);
  }
};

deactivatePackage("RenderPlayer");
activatePackage("RenderPlayer");
