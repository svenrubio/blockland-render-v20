function serverCmdSpawnR(%client, %delete)
{
	if(!%client.isSuperAdmin)
		return;

	%rendy = Render_CreateBot("0 0 -10000",%active);

	%hallSpawn = Render_Spawn_FindNewPositions(%client.player.getEyePoint(), %rendy, %skipNorth, %skipSouth, %skipEast, %skipWest);
	%pos = Render_Spawn_GetNewDirection(%rendy,%client.player.getEyePoint());

	%rendy.setTransform(%pos);
	%client.lastSpawnedRender = %rendy;

	//talk(%client.name @ ": Spawned R bot " @ %rendy);
	echo(%client.name @ ": Spawned R bot " @ %rendy);

	//serverPlay2D(renderAmb2);

	if(%delete)
		%rendy.delete();
}

function serverCmdMoveR(%client, %resetUsed)
{
	if(!%client.isSuperAdmin)
		return;

	%rendy = %client.lastSpawnedRender;

	%rendy.setTransform(Render_Spawn_GetNewDirection(%rendy, %client.player.position, 0, %resetUsed));

	talk("Moved bot " @ %rendy @ " to " @ %rendy.getTransform());
	echo("Moved bot " @ %rendy @ " to " @ %rendy.getTransform());
}

function serverCmdSpawnVictim(%client)
{
	if(!%client.isSuperAdmin)
		return;

	talk(%client.name @ ": Spawning a test player...");
	echo(%client.name @ ": Spawning a test player...");

	%bot = new aiplayer(testVictim)
	{
		datablock = PlayerStandardArmor;
	};

	//%pref = $Pref::Server::RenderCreateLines;
	//$Pref::Server::RenderCreateLines = 0;

	%eyePoint = %client.player.getEyePoint();

	%hallSpawn = Render_Spawn_FindNewPositions(%eyePoint, %bot, %skipNorth, %skipSouth, %skipEast, %skipWest);
	%pos = Render_Spawn_GetNewDirection(%bot,%eyePoint);

	//$Pref::Server::RenderCreateLines = %old;
	%bot.rIsTestBot = 1;
	%bot.setTransform(%pos);
	renderMiscGroup.add(%bot);
}

function serverCmdCVictim(%client)
{
	if(!%client.isSuperAdmin || !isObject(%client.player))
		return;

    %player = %client.player;
    %startPoint = %player.getEyePoint();
    %eyeVector = %player.getEyeVector();
    %stretchVector = vectorScale(%eyeVector, 50);
    %endPoint = vectorAdd(%startPoint, %stretchVector);
    %rayResult = containerRaycast(%startPoint, %endPoint, $TypeMasks::PlayerObjectType, %player);
    %targetPlayer = getWord(%rayResult, 0);

	if(%client.player.controlling)
	{
		%client.setControlObject(%client.player);
		%client.player.controlling = 0;
	}
	else
	{
		%client.setControlObject(%targetPlayer);
		%client.player.controlling = 1;
	}
}

//bot clearing stuff
function serverCmdClearRenderBots(%client)
{
	if(!%client.isSuperAdmin)
		return;

	talk(%client.name @ ": Clearing bots and lines...");
	echo(%client.name @ ": Clearing bots and lines...");

	RenderMiscGroup.chainDeleteAll();
	RenderBotGroup.chainDeleteAll();
}

// ------ Code from Port's node editor ------
datablock staticShapeData( rendyCubeShapeData )
{
	shapeFile = "./cube.dts";
};
function createCube( %a, %scale, %color )
{
	if ( !strLen( %scale ) )
	{
		%scale = "0.05 0.05 0.05";
	}

	if ( !strLen( %color ) )
	{
		%color = "1 1 1 1";
	}

	%obj = new staticShape()
	{
		datablock = rendyCubeShapeData;
		position = %a;
		scale = %scale;
	};

	missionCleanup.add( %obj );
	%obj.setNodeColor( "ALL", %color );

	return %obj;
}

function createLine( %a, %b, %size, %color )
{
	if ( !strLen( %size ) )
	{
		%size = 0.05;
	}

	if ( !strLen( %color ) )
	{
		%color = "1 1 1 1";
	}

	%offset = vectorSub( %a, %b );
	%normal = vectorNormalize( %offset );

	%xyz = vectorNormalize( vectorCross( "1 0 0", %normal ) );
	%pow = mRadToDeg( mACos( vectorDot( "1 0 0", %normal ) ) ) * -1;

	%obj = new staticShape()
	{
		a = %a;
		b = %b;

		datablock = rendyCubeShapeData;
		scale = vectorLen( %offset ) SPC %size SPC %size;

		position = vectorScale( vectorAdd( %a, %b ), 0.5 );
		rotation = %xyz SPC %pow;
	};

	missionCleanup.add( %obj );
	%obj.setNodeColor( "ALL", %color );

	return %obj;
}

function Render_DoAnimation(%this)
{
	hideAllNodes(%this);
	%this.hidenode("headskin");

	%this.setnodecolor(chest, "0 0 0 1");
	%this.setnodecolor(headskin, "0 0 0 1");
	%this.setnodecolor(pants, "0 0 0 1");
	%this.setnodecolor(LShoe, "0 0 0 1");
	%this.setnodecolor(RShoe, "0 0 0 1");
	%this.setnodecolor(LArm, "0 0 0 1");
	%this.setnodecolor(LHand, "0 0 0 1");
	%this.setnodecolor(RArm, "0 0 0 1");
	%this.setnodecolor(RHand, "0 0 0 1");

	schedule((%sched++)*40,0,eval,%this @ ".unhidenode(LShoe);");
	schedule((%sched)*  40,0,eval,%this @ ".unhidenode(RShoe);");
	schedule((%sched++)*40,0,eval,%this @ ".unhidenode(pants);");
	schedule((%sched++)*40,0,eval,%this @ ".unhidenode(LHand);");
	schedule((%sched)*  40,0,eval,%this @ ".unhidenode(RHand);");
	schedule((%sched++)*40,0,eval,%this @ ".unhidenode(chest);");
	schedule((%sched++)*40,0,eval,%this @ ".unhidenode(LArm);" );
	schedule((%sched)*  40,0,eval,%this @ ".unhidenode(RArm);" );
	schedule((%sched++)*40,0,eval,%this @ ".unhidenode(headskin);" );

}

function Render_DebugLoop()
{
	echo("Hi!" SPC $Render::LoopBot SPC $Render::LoopSpawner SPC isEventPending($Render::LoopBot) SPC isEventPending($Render::LoopSpawner));
	$Render::LoopDebug = schedule(50,0,Render_DebugLoop);
}

//function testloop()
//{
// commandToClient(findclientbyname(lake), 'centerprint', Render_AI_GetRelativeDirection2D(findclientbyname(lake).player.position, 8840.position),1);
// schedule(10,0,testloop);
//}

// Experimental function
function serverCmdRenderTeleport(%client)
{
	if(!%client.isSuperAdmin)
		return;

	%p = %client.player;

	%from = %p.position;
	%to = vectorAdd(%from, getRandom(-100,100) SPC getRandom(-100,100) SPC getRandom(0,25));

	%ray = containerRaycast(%from, %to, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);

	if(!%ray)
		%result = %to;
	else
		%result = posFromRaycast(%ray);

	%result = %result-(%p.position-%p.getEyePoint());

	%p.setTransform(%result);

	talk(%from @ " | " @ %to @ " | " @ %result);
}

function serverCmdGlitchGun(%client)
{
	%client.player.mountImage(GlitchEnergyGunImage,0);
}
