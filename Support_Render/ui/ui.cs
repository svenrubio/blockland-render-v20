exec("./RenderBitmapGui.gui");

function r_getRandomScreenPos()
{
  %extent = canvas.getExtent();
  %extentX = getWord(%extent, 0);
  %extentY = getWord(%extent, 1);

  %x = getRandom(0, %extentX-128);
  %y = getRandom(0, %extentY-128);

  return %x SPC %y;
}

// TODO: Test for issues and make this function call at random times (but only on the main menu)
function R_Check()
{
  if(mainMenuGui.isAwake())
  {
    RenderBitmap.position = r_getRandomScreenPos();
    canvas.pushDialog(RenderBitmapGui);
    R_Loop();
  }
}

function R_Loop()
{
  if(!$R_LoopCount)
    $R_LoopCount = 0;

  if($R_LoopCount >= 59)
  {
    $R_LoopCount = 0;
    canvas.popDialog(RenderBitmapGui);
    return;
  }

  if($R_LoopCount < 10)
    RenderBitmap.setBitmap("Add-Ons/Support_Render/ui/render_00000" @ $R_LoopCount @ ".png");
  else
    RenderBitmap.setBitmap("Add-Ons/Support_Render/ui/render_0000" @ $R_LoopCount @ ".png");

  $R_LoopCount++;
  $R_UiLoop = schedule(16, 0, R_Loop);
}
