////// # Prefs and Initialization
RTB_registerPref("Transform chance at spawn", "Render|Render Players", "$Pref::Server::RenderPlSpawnChance", "list Disabled 0 Low 2 Below_Normal 3 Normal 4 Above_Normal 5 High 6 Always 24", "Support_Render", 0, 0, 0);

// Re-register to override the RTB legacy icon. (see init.cs)
if(isFunction("registerPreferenceAddon")) {
  registerPreferenceAddon("Support_Render", "Render", "skull_old");
}

////// # Do Render Transition
function Render_DoRenderTransition(%rClient, %debug)
{
  if(%debug)
  {
    %render = Render_CreateBot("0 0 -10000",%client);
    %pos = %rClient.player.position;
  }
  else
  {
    // First we need to pick a player to spawn near.
    // This uses the same grouping system as render.cs with modifications.
    for(%i = 0; %i < clientGroup.getCount(); %i++)
    {
      %client = clientGroup.getObject(%i);

      // If player is nonexistent, already marked, or matches the active client, skip them.
      // Note that this check also prevents the radius search below, potentially blocking nearby players.
      if(!isObject(%client.player) || %groupGet[%client.player] || %client == %rClient)
        continue;

      // Otherwise, this player counts as a new group.
      %groups++;

      // For all players in the area...
      initContainerRadiusSearch(%client.player.position,100,$TypeMasks::PlayerObjectType);
      while(%target=containerSearchNext())
      {
        // Make sure they aren't a bot, a Render player, or worse yet, the active Render client.
        if(%target.getClassName() !$= "Player" || %target.isRenderPlayer || %target.client == %rClient)
          continue;

        // If the Render client is in a minigame, the target player has to be in the same one.
        if(%rClient.minigame > 0 && %target.client.minigame != %rClient.minigame)
          continue;

        %groupGet[%target] = %groups; // So we can easily 'get' the group containing a target
        %groupList[%groups,%groupCount[%groups]++] = %target; // So we can list all targets for a group.

        %targets++;
      }
    }

    // If there are no available players, cancel.
    if(!%groups || !%targets)
      return;

    // Unlike render.cs, which has a set chance per group, we're going to pick a random client in a random group.
    %group = getRandom(1,%groups);
    %client = %groupList[%group, getRandom(1, %groupCount[%group]) ].client;

    %render = Render_CreateBot("0 0 -10000",%client);

    %hallSpawn = Render_Spawn_FindNewPositions(%client.player.getEyePoint(), %render, %skipNorth, %skipSouth, %skipEast, %skipWest);
    %pos = Render_Spawn_GetNewDirection(%render, %client.player.getEyePoint(), 0, 0, 1);
  }

  // TODO: Try another client rather than cancelling the check
  if(!%pos == 0)
  {
    //warn("RENDER: Spawn failed for " @ %client);
    Render_DeleteR(%render);
    return;
  }

  %render.setTransform(%pos);

  %rClient.isRenderClient = 1;
  %rClient.render = %render;

  %render.isRenderPlayer = 1;
  %render.client = %rClient;

  %rClient.player.delete();
  %rClient.setControlObject(%render);
}

////// # Player control loop
function Render_Player_Control_Loop(%render)
{
  // Cancel if the client is detached.
  if(%render.client.getControlObject() != %render)
  {
    Render_DeleteR(%render);
    return;
  }

  %string = "\c7[\c6light\c7] leave ";

  if(!%render.attackInit && %render.mode != 3 && %render.loopCount <= %render.loopEnergyTimeout-(5000/$Render::C_LoopTimer))
    %string = %string @ "\c7[\c6plant\c7] attack ";

  %render.client.bottomPrint("<font:impact:38>\c7" @ mCeil((%render.loopEnergyTimeout-%render.loopCount)*$Render::C_LoopTimer/1000) @ "<just:right>" @ %string ,1,1);
}


////// # Render Player Package # //////

package RenderPlayer
{
  function serverCmdlight(%client)
  {
    if(%client.isRenderClient && %client.render.loopCount > 500/$Render::C_LoopTimer)
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

    if(%client.doRenderTransition)
    {
      %client.doRenderTransition = 0;
      Render_DoRenderTransition(%client);
    }
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

	function Armor::onDisabled(%a, %p, %e)
	{
		if(isObject(%p.client))

    // Chance of the client becoming an attacker
    %rand = getRandom(1,8);

    // Account for both mini-game and server configuration.
    if(%p.client.minigame.rSpawnRatePlayer $= "" || %p.client.minigame.rSpawnRatePlayer == -1)
      %spawnrate = $Pref::Server::RenderPlSpawnChance;
    else
      %spawnrate = %p.client.minigame.rSpawnRatePlayer;

    if(%rand <= %spawnrate)
      %p.client.doRenderTransition = 1;

    Parent::onDisabled(%a, %p, %e);
  }
};

deactivatePackage("RenderPlayer");
activatePackage("RenderPlayer");
