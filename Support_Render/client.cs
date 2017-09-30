//%date = getDateTime();
%date = "10/31/17 00:00:00";
// TODO: Change to getDateTime() before release

// 10/24 - 11/07
if(getSubStr(%date, 0, 2) == 10 && getSubStr(%date, 3, 2) >= 24 || getSubStr(%date, 0, 2) == 11 && getSubStr(%date, 3, 2) <= 07)
  exec("./ui/ui.cs");
