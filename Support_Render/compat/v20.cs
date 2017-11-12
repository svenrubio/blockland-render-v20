// This script allows Support_Render to be partially compatible with older versions of Blockland.

package RenderCompatPackage
{
  function Render_DoMount(%death,%p)
  {
    // TODO: Fix the 'giant' bug still being briefly visible on mount
    %death.setScale("1 1 1"); // This fixes frozen players becoming giants.
    Parent::Render_DoMount(%death,%p);
  }

  // Death board is not compatible with maps currently.
  // It is disabled to prevent errors.
  function GameConnection::doRenderDeath(%client)
  {
    reurn -1;
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
};

deactivatePackage(RenderCompatPackage);
activatePackage(RenderCompatPackage);
