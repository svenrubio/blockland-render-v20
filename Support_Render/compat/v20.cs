// This script allows Support_Render to be partially compatible with older versions of Blockland.

package RenderCompat
{
  function RenderDeathArmor::onAdd(%datablock,%obj)
  {
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

  // Overwrite this function from bot holes so there aren't any console errors.
  // Note that the without this function, the AI will not be able to move around obstacles as easily.
  function AIPlayer::hAvoidObstacle() {
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
    new simGroup(RenderBotGroup) {}; // Render bot group
    new simGroup(RenderMiscGroup) {}; // Render object group

    parent::onMissionLoaded();
    RenderCompatInit();
  }

  function GameConnection::doRenderDeathSpecial(%client, %render, %offset, %nosound, %nodelay)
  {
    // The sound bugs out in v20, so it is disabled for now.
    // The brick placement delay is also disabled.
    %death = Parent::doRenderDeathSpecial(%client, %render, %offset, 1, 1);

    // For reasons unknown, we want players to be able to build while frozen during the effect.
    // Due to the "giant players" patch, this isn't possible unless we make some tweaks to the scaling.
    if(isObject(%death)) {
      %death.setScale("0.75 0.75 0.75");
      %render.setScale("1.5 1.5 1.5"); // Re-scale the attacker as well so this fix isn't as noticeable.
    }
    // Because of wacky bugs in v20, these two scales will look the same when in-game.
  }

  // Patch for the special brick effect.
  // In order to accomodate v20 maps (Slate included), we have to use an invisible baseplate.
  function Render_BrickEffectFix(%player)
  {
  	%position = %player.position;
  	%position = setWord(%position, 2, getWord(%position,2)-0.3); // Vertical offset
  	%position = setWord(%position, 1, getWord(%position,1)+3); // Horizontal offset

  	%brick = new FxDTSBrick()
  	{
  		datablock = brick64x64fData;
  		isPlanted = true;
  		client = -1;

  		position = %position;
  		rotation = "1 0 0 0";
  		angleID = 1;

  		colorID = 5;
  		colorFxID = 0;
  		shapeFxID = 0;

  		printID = 0;
  	};

  	BrickGroup_666.add(%brick);

    %brick.setRendering(0);

  	%error = %brick.plant();

		if(!%error || %error == 2) // If it's a float error, ignore and plant anyway.
			%brick.schedule(120000,killBrick);
		else
			// Other error, delete thr brick.
			%brick.delete();
  }

  function Render_BrickEffect(%player, %override) {
    // Set %override to tell the function to ignore float errors.
    %schedule = Parent::Render_BrickEffect(%player, 1);

    // If the schedule is running, we know the first brick planted successfully.
    // This means we can proceed with the plate.
    if(isEventPending(%schedule)) {
      Render_BrickEffectFix(%player);
    }
  }
};

deactivatePackage(RenderCompat);
activatePackage(RenderCompat);
