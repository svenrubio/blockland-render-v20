%date = getDateTime();

exec("./ui/shrineGui.cs");

// 10/21 - 11/10
if(getRandom(1,1000) == 1 || getSubStr(%date, 0, 2) == 10 && getSubStr(%date, 3, 2) >= 21 || getSubStr(%date, 0, 2) == 11 && getSubStr(%date, 3, 2) <= 11)
  exec("./ui/ui.cs");
