%date = getDateTime();

exec("./ui/shrineGui.cs");

// 10/29 - 11/4
if(getSubStr(%date, 0, 2) == 10 && getSubStr(%date, 3, 2) >= 29 || getSubStr(%date, 0, 4) == 11 && getSubStr(%date, 3, 2) <= 3) {
  %title = MainMenuButtonsGui.getObject(0);

  if(%title.bitmap $= "base/client/ui/title.png")
    %title.bitmap = "Add-Ons/Support_Render/ui/title.png";

  // °Д°
}

if(getRandom(1,1600) == 1)
  exec("./ui/ui.cs");
