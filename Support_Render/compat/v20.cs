// This script allows Support_Render to be partially compatible with older versions of Blockland.

package RenderCompat
{
  function RenderDeathArmor::onAdd(%datablock,%obj)
  {
    // TODO: Fix the 'giant' bug still being briefly visible on mount
    %obj.setScale("1 1 1"); // This fixes frozen players becoming giants.
    Parent::onAdd(%datablock,%obj);
  }

  // Death board is not compatible with maps currently.
  // It is disabled to prevent errors.
  function GameConnection::doRenderDeathCamera(%client)
  {
    return -1;
  }

  // TODO: See if setJumping and setCrouching can be fixed?
  function Player::SetJumping(%x)
  {
    return -1;
  }

  function Player::SetCrouching(%x)
  {
    return -1;
  }

  function Player::clearMoveDestination()
  {
    return -1;
  }

  // Completely overwrite freezeRender functions as they rely entirely on functions that aren't in v20.
  function Render_FreezeRender(%p)
  {
    return -1;
  }

  function Render_UnfreezeRender(%p)
  {
    return -1;
  }

  // TODO: See if setJumping and setCrouching can be fixed?
  function Player::SetJumping(%x)
  {
    return -1;
  }

  function Player::SetCrouching(%x)
  {
    return -1;
  }

  function Player::clearMoveDestination()
  {
    return -1;
  }

  // TODO: If speed is set to 0, freeze the player
  function Player::SetMaxForwardSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxBackwardSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxSideSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxCrouchForwardSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxCrouchBackwardSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxCrouchSideSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxUnderwaterForwardSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxUnderwaterBackwardSpeed(%x)
  {
    return -1;
  }

  function Player::SetMaxUnderwaterSideSpeed(%x)
  {
    return -1;
  }

  function RenderCompatInit() {
    // Sunlight check compatibility
    // The sun is not named in many v20 maps by default, so we'll need to define the sun.
    for(%i = 0; %i < 10; %i++) {
      if(isObject(MissionGroup)) {
        %obj = MissionGroup.getObject(%i);

        if(%obj.getClassName() $= "Sun") {
          $Render::SunObj = %obj;
          break;
        }
      }
    }
  }

  function onMissionLoaded()
  {
    parent::onMissionLoaded();
    RenderCompatInit();
  }

  function GameConnection::doRenderDeathSpecial(%client, %render, %offset, %nosound)
  {
    // The sound bugs out in v20, so it is disabled for now.
    Parent::doRenderDeathSpecial(%client, %render, %offset, 1);
  }
};

deactivatePackage(RenderCompat);
activatePackage(RenderCompat);
