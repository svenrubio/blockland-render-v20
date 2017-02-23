// Main spawn positioning function.
// It will find an indoor place for the bot to spawn (so the bot doesn't spawn and walk through walls)
//
// How it works:
// - Four rays are fired (north, south, east, and west) to detect walls.
// - Each ray will "split up" when it hits something
// (example: when the north ray hits, two rays will be fired (to east and west) from the place where the north ray hit)
// - Returns information to the object specified on %return.
//
// NOTE: For players, I strongly recommend using getEyePoint() instead of the player's regular position.
// (You may encounter issues if you do this inconsistently; make sure to always provide positions the same way)
function Render_Spawn_FindNewPositions(%from, %return, %skipNorth, %skipSouth, %skipEast, %skipWest)
{
	if(!%skipNorth)
	{
		%toNorth = vectorAdd(%from, "0 100 0");
		%rayNorth = containerRaycast(%from, %toNorth, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);
	}
	if(!%skipSouth)
	{
		%toSouth = vectorAdd(%from, "0 -100 0");
		%raySouth = containerRaycast(%from, %toSouth, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);
	}
	if(!%skipEast)
	{
		%toEast = vectorAdd(%from, "100 0 0");
		%rayEast = containerRaycast(%from, %toEast, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);
	}
	if(!%skipWest)
	{
		%toWest = vectorAdd(%from, "-100 0 0");
		%rayWest = containerRaycast(%from, %toWest, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);
	}

	for(%i = 1; %i <= 4; %i++) // For all directions; north, south, east, and west...
	{
		if(%i == 1){%dir = "North"; %sA = "NorthToEast";%sB = "NorthToWest";%sToA="100 -1 0";%sToB="-100 -1 0";}
		if(%i == 2){%dir = "South"; %sA = "SouthToEast";%sB = "SouthToWest";%sToA="100 1 0";%sToB="-100 1 0";}
		if(%i == 3){%dir = "East"; %sA = "EastToNorth";%sB = "EastToSouth";%sToA="-1 100 0";%sToB="-1 -100 0";}
		if(%i == 4){%dir = "West"; %sA = "WestToNorth";%sB = "WestToSouth";%sToA="1 100 0";%sToB="1 -100 0";}

		if(%skip[%dir]) // If we're skipping this direction, continue.
			continue;

		if(!%ray[%dir])
			%hit[%dir] = %to[%dir]; // Use %to if the ray doesn't hit something
		else
			%hit[%dir] = posFromRaycast(%ray[%dir]); // Otherwise use the ray position.

		%dist[%dir] = vectorDist(%from, %hit[%dir]);

		if(%dist[%dir] < $Pref::Server::RenderMinSpawnDistance) // If it's too close to the player (based on pref) we have to skip it.
			continue;

		%ray[%sA] = containerRaycast(%hit[%dir], vectorAdd(%hit[%dir], %sToA), $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);
		%ray[%sB] = containerRaycast(%hit[%dir], vectorAdd(%hit[%dir], %sToB), $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);

		%hit[%sA] = posFromRaycast(%ray[%sA]);
		%hit[%sB] = posFromRaycast(%ray[%sB]);

		if(!%ray[%sA]) { %hit[%sA] = vectorAdd(%hit[%dir], %sToA); }
		if(!%ray[%sB]) { %hit[%sB] = vectorAdd(%hit[%dir], %sToB); }

		if(vectorDist(%hit[%sA], %hit[%sB]) > 1.5)
		{
			%valid[%dir] = 1;
			%validDirections++;
		}
		else // Direction is un-usable (too narrow space), so we'll skip it
			continue;

		if(%i == 1 || %i == 2)
		{
			%pos1[%dir] = getWord(%hit[%sA], 0)-1;
			%pos2[%dir] = getWord(%hit[%sB], 0)+1;
		}
		else
		{
			%pos1[%dir] = getWord(%hit[%sA], 1)-1;
			%pos2[%dir] = getWord(%hit[%sB], 1)+1;
		}

		if(isObject(%return))
		{
			// There's a lot of information here, so we're going to assign it all to an object rather than returning one big string.

			%return.ray[%dir] = %ray[%dir];
			%return.hit[%dir] = %hit[%dir];
			%return.dist[%dir] = %dist[%dir];

			%return.ray[%sA] = %ray[%sA];
			%return.ray[%sB] = %ray[%sB];

			%return.hit[%sA] = %hit[%sA];
			%return.hit[%sB] = %hit[%sB];

			%return.valid[%dir] = %valid[%dir];
			%return.validDirections = %validDirections;

			%return.pos1[%dir] = %pos1[%dir];
			%return.pos2[%dir] = %pos2[%dir];

			%return.from = %from;

			//creating lines
			if($Pref::Server::RenderCreateLines == 1)
			{
				%lineA[%dir] = createLine(%from,%hit[%dir]);
				%lineB[%dir] = createLine(%hit[%sA],%hit[%sB]);

				RenderMiscGroup.add(%lineA[%dir]);
				RenderMiscGroup.add(%lineB[%dir]);
			}
		}
	}

	return %validDirections;
}

// Here's another crucial spawning function. It picks a direction and randomizes the bot's position. Returns current position if out of directions.
// %disableUsedMark: Don't mark directions as "used"
// %sameDirection: Find a spot in the same direction (***NOT IMPLEMENTED***)
// This relies on the outline that the previous function gives us.
function Render_Spawn_GetNewDirection(%this, %plpos, %sameDirection, %disableUsedMark)
{
	if(!%this.validDirections)
		return %this.getTransform();

	%choice = getRandom(1, %this.validDirections);

	for(%i = 1; %i <= 4; %i++)
	{
		if(%i == 1){%dir = "North"; %sA = "NorthToEast";%sB = "NorthToWest"; %add = -1;}
		if(%i == 2){%dir = "South"; %sA = "SouthToEast";%sB = "SouthToWest"; %add = 1;}
		if(%i == 3){%dir = "East"; %sA = "EastToNorth";%sB = "EastToSouth"; %add = -1;}
		if(%i == 4){ %dir = "West"; %sA = "WestToNorth";%sB = "WestToSouth"; %add = 1;}

		%used[%dir] = %this.used[%dir];

		if(!%this.valid[%dir] || %used[%dir])
			continue;

		%choices++;

		if(%choices == %choice) // If this is the chosen direction
		{
			%pos1 = %this.pos1[%dir];
			%pos2 = %this.pos2[%dir];
			%randA = getRandom(%pos1*100,%pos2*100);
			%randB = %randA/100;

			//%this.lineA[%dir].setNodeColor("ALL","0 0 1 1");
			//%this.lineB[%dir].setNodeColor("ALL","0 0 1 1");

			if(%i == 1 || %i == 2)
				%pos3 = %randB SPC getWord(%this.hit[%dir],1)+%add SPC getWord(%plpos,2);

			if(%i == 3 || %i == 4)
				%pos3 = getWord(%this.hit[%dir],0)+%add SPC %randB SPC getWord(%plpos,2);

			//forward vector stuff
			%from = %pos3;

			if(%i == 1) //north
				%to = getWord(%pos3, 0) SPC getWord(%plpos, 1) + $Pref::Server::RenderMinSpawnDistance SPC getWord(%pos3, 2);
			if(%i == 2) //south
				%to = getWord(%pos3, 0) SPC getWord(%plpos, 1) - $Pref::Server::RenderMinSpawnDistance SPC getWord(%pos3, 2);
			if(%i == 3) //east
				%to = getWord(%plpos, 0) + $Pref::Server::RenderMinSpawnDistance SPC getWord(%pos3, 1) SPC getWord(%pos3, 2);
			if(%i == 4) //west
				%to = getWord(%plpos, 0) - $Pref::Server::RenderMinSpawnDistance SPC getWord(%pos3, 1) SPC getWord(%pos3, 2);

			//echo("i " @ %i @ "; to " @ %to);

			// We're going to do another check to randomize our position.

			%rayForward = containerRaycast(%from, %to, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType);

			if(!%rayForward)
				%hit = %to;
			else
				%hit = posFromRaycast(%rayForward);

			if(%i == 1) //north
				%randC = getRandom(getWord(%pos3,1)*100,(getWord(%hit,1)+1)*100);
			if(%i == 2) //south
				%randC = getRandom(getWord(%pos3,1)*100,(getWord(%hit,1)-1)*100);
			if(%i == 3) //east
				%randC = getRandom(getWord(%pos3,0)*100,(getWord(%hit,0)+1)*100);
			if(%i == 4) //west
				%randC = getRandom(getWord(%pos3,0)*100,(getWord(%hit,0)-1)*100);

			if(%i == 1 || %i == 2)
				%pos = %randB SPC %randC/100 SPC getWord(%plpos,2);
			if(%i == 3 || %i == 4)
				%pos = %randC/100 SPC %randB SPC getWord(%plpos,2);

			if($Pref::Server::RenderCreateLines == 1)
			{
				%lineC = createLine(%from,%hit,"","1 0 0 1");
				renderMiscGroup.add(%lineC);
			}

			if(getWord(%pos,0) == %this)
			{
				error("Render_Spawn_GetNewDirection: Something happened.");
				return;
			}

			if(!%disableUsedMark) // Mark position as "used" unless disabled
			{
				%this.used[%dir] = 1;
				%this.validDirections--;
			}

			%this.currentDirection = %dir;
			return %pos;
		}
	}
	error("Render_Spawn_GetNewDirection - Returning blank");
	return;
}
