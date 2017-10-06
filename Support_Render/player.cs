// TODO: Jet mechanic
// TODO: Client interface
// TODO: Choosing spawn locations
// TODO: Slayer integration
// (see compat/slayer.cs and GameMode_Slayer/server/defaults/team-preferences.cs)
// TODO: Minigame events integration
// TODO: ai_spawn integration
// TODO: Replace free movement with manual button when freezing players
// (Rather than being able to walk away, attacker should only be able to release the player by pressing a button)
// TODO: Fix being able to jump and jet with Render_FreezeRender. This will likely require special render datablocks.
// TODO: Cancel the loop if clients get detached from bot
// TODO: Lower the bottomprint rate
// TODO: Fix tool, paint, and bricks bars on the client side so the boxes don't work.

// See package.cs for button press code

////// # Prefs and Initialization
RTB_registerPref("Transform chance at spawn", "Render|Render Players", "$Pref::Server::RenderPlSpawnChance", "list Disabled 0 Low 2 Below_Normal 3 Normal 4 Above_Normal 5 High 6 Always 24", "Support_Render", 0, 0, 0);

////// # Do Render Transition
function Render_DoRenderTransition(%client)
{
  %client.isRenderClient = 1;

  %render = Render_CreateBot(%client.player.getTransform());
  %client.render = %render;

  %render.isRenderPlayer = 1;
  %render.client = %client;

  %client.player.delete();
  %client.setControlObject(%render);
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
    if(%client.isRenderClient)
      Render_RequestDespawn(%client.render);
    else
      Parent::serverCmdLight(%client);
  }

  function serverCmdPlantBrick(%client)
  {
    if(%client.isRenderClient)
    {
      // Two if checks are used so we only return to parent if they aren't a Render player.
      if(!%client.render.attackInit)
      {
        // The player sends a request to start attacking.
        %client.render.aiStartAttacking = 1;
        %client.render.attackInit = 1;
      }
    }
    else
      Parent::serverCmdPlantBrick(%client);
  }

  function GameConnection::createPlayer(%client, %transform)
  {
    Parent::createPlayer(%client, %transform);
    if(%client.isRenderClient)
      %client.isRenderClient = 0;
  }

  /// ## Server Command Disablers ## ///

  //function serverCmdShiftBrick(%client, %a, %b, %direction)
  //function serverCmdRotateBrick(%client, %direction)
  //function serverCmdSit(%client)
  //function serverCmdFind(%client, %target)
  //function serverCmdFetch(%client, %target)

  function serverCmdDropCameraAtPlayer(%client)
  {
    if(%client.isRenderClient)
      return;

    Parent::serverCmdDropCameraAtPlayer(%client);
  }

  function serverCmdDropPlayerAtCamera(%client)
  {
    if(%client.isRenderClient)
      return;

    Parent::serverCmdDropPlayerAtCamera(%client);
  }
};

deactivatePackage("RenderPlayer");
activatePackage("RenderPlayer");
