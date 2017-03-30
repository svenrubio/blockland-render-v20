// # Dependencies, Scripts

// Some parts of the AI utilize functions from Bot_Hole.
if(forceRequiredAddOn("Bot_Hole") == $Error::AddOn_Disabled)
	exec("Add-Ons/Bot_Hole/server.cs");

// Shrines require the pumpkin model.
if(forceRequiredAddOn("Brick_Halloween") == $Error::AddOn_Disabled)
	exec("Add-Ons/Brick_Halloween/server.cs");

// The DayCycle pref utilizes a script from Lugnut's Event_DayCycles. https://forum.blockland.us/index.php?topic=204001.0
if(!isFunction(getPartOfDayCycle)) // We'll see if the function already exists first.
{
	if(isFile("Add-Ons/Event_Daycycles/timeSupport.cs")) // If not, we'll try to run it from the add-on.
		exec("Add-Ons/Event_DayCycles/timeSupport.cs");
	else
		exec("./modules/timeSupport.cs"); // If all else fails, execute our own copy.
}

exec("./modules/envServer.cs");

exec("./ai_main.cs");
exec("./ai_spawn.cs");

exec("./brick_shrine.cs");

exec("./render.cs");

if(isFile("./debug.cs"))
	exec("./debug.cs");

if(!$Render::Loaded)
{
	exec("./init.cs");
	$Render::Loaded = 1;
}
