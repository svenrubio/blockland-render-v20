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

function R_Check()
{
  cancel($R_Check);

  if(!mainMenuGui.isAwake())
    return;

  %rand = getRandom(1,5);

  if(%rand == 1)
  {
    RenderBitmap.position = r_getRandomScreenPos();
    canvas.pushDialog(RenderBitmapGui);
    R_Check_Loop();
  }

  $R_Check = schedule(30000, 0, R_Check); // 50000
}

function R_Check_Loop()
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
  $R_UiLoop = schedule(16, 0, R_Check_Loop);
}

package CheckPackage
{
  function MainMenuGui::onSleep()
  {
    Parent::onSleep();
    cancel($R_Check);
  }

  function MainMenuGui::onRender()
  {
    Parent::onRender();
    $R_Check = schedule(30000, 0, R_Check);
  }
};
activatePackage("CheckPackage");
