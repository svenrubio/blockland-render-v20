// # Dependencies, Scripts

// Some parts of the AI utilize functions from Bot_Hole.
if(forceRequiredAddOn("Bot_Hole") == $Error::AddOn_Disabled)
	exec("Add-Ons/Bot_Hole/server.cs");

// Shrines require the pumpkin model.
if(forceRequiredAddOn("Brick_Halloween") == $Error::AddOn_Disabled)
	exec("Add-Ons/Brick_Halloween/server.cs");

// The DayCycle pref utilizes a script from Lugnut's Event_DayCycles.
// https://forum.blockland.us/index.php?topic=204001.0

// We'll see if the function already exists first.
if(!isFunction(getPartOfDayCycle))
{
	if(isFile("Add-Ons/Event_Daycycles/timeSupport.cs")) // If not, we'll try to run it from the add-on.
		exec("Add-Ons/Event_DayCycles/timeSupport.cs");
	else
		exec("./modules/timeSupport.cs"); // If all else fails, execute our own copy.
}

// 12/1 - 12/31
%date = getDateTime();
if(getSubStr(%date, 0, 2) == 12) {
  $Render::C_HolidayCheer = 1;
}

if(isFile("./debug.cs"))
	exec("./debug.cs");

exec("./modules/envServer.cs");
exec("./modules/lastEvent.cs");
exec("./modules/eulerToAxis.cs");

exec("./ai_main.cs");
exec("./ai_spawn.cs");

exec("./bricks.cs");

exec("./events.cs");

if(!$Render::Loaded)
{
	exec("./init.cs");
	$Render::Loaded = 1;
	$Render::LoadedB = 1;
}

// Note: This also has parts in compat/slayer.cs.
exec("./player.cs");

exec("./package.cs");
exec("./render.cs");

if($Version < 21)
	exec("./compat/v20.cs");
