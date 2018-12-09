%date = getDateTime();

exec("./ui/shrineGui.cs");

if(getRandom(1,1000) == 1)
  exec("./ui/ui.cs");
