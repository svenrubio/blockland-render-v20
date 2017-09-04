//%date = getDateTime();
%date = "10/31/17 00:00:00";

if(getSubStr(%date, 0, 2) == 10 && getSubStr(%date, 3, 2) >= 24 || getSubStr(%date, 0, 2) == 11 && getSubStr(%date, 3, 2) <= 07)
{
  exec("./ui/ui.cs");
}
