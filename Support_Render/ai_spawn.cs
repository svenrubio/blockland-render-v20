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
		%rayNorth = containerRaycast(%from, %toNorth, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);
	}
	if(!%skipSouth)
	{
		%toSouth = vectorAdd(%from, "0 -100 0");
		%raySouth = containerRaycast(%from, %toSouth, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);
	}
	if(!%skipEast)
	{
		%toEast = vectorAdd(%from, "100 0 0");
		%rayEast = containerRaycast(%from, %toEast, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);
	}
	if(!%skipWest)
	{
		%toWest = vectorAdd(%from, "-100 0 0");
		%rayWest = containerRaycast(%from, %toWest, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);
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

		%ray[%sA] = containerRaycast(%hit[%dir], vectorAdd(%hit[%dir], %sToA), $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);
		%ray[%sB] = containerRaycast(%hit[%dir], vectorAdd(%hit[%dir], %sToB), $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);

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

// Simple method of checking if a player is outdoors or not.
// This check is not very accurate on its own.
function Render_PosIsOutdoors(%pos)
{
	%up = setWord(%pos, 2, getWord(%pos, 2)+100);
	%ray = containerRaycast(%pos, %up, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);

	if(%ray == 0)
		return 1;
	else
		return 0;
}

// Here's another crucial spawning function. It picks a direction and randomizes the bot's position. Returns current position if out of directions.
// %sameDirection: Find a spot in the same direction (***NOT IMPLEMENTED***)
// %disableUsedMark: Don't mark directions as "used"
// %useDistanceChance: Use the direction's distance as a factor in the choice, rather than keeping it purely random.
// This relies on the outline that the previous function gives us.
// Errors:
// INDOOR
// BAD_POS (Unexpected)
// POS_TOO_FAR (Unexpected)
// NO_VALID_DIRS
function Render_Spawn_GetNewDirection(%this, %plpos, %sameDirection, %disableUsedMark, %useDistanceChance)
{
	if(!%this.validDirections)
		return %this.getTransform();

	if(!%useDistanceChance)
		%choice = getRandom(1, %this.validDirections);
	else
	{
		// Get a random value between 0 and the total distance of all lines.
		%distTotal = %this.dist["North"]+%this.dist["South"]+%this.dist["East"]+%this.dist["West"];
		%choice = getRandom(0, %distTotal);
	}

	if($Pref::Server::RenderDisableIndoorSpawn)
	{
		%avg = (%this.distNorth+%this.distSouth+%this.distEast+%this.distWest)/4;

		if(%avg < 30 && !Render_PosIsOutdoors(%plpos))
		{
			%this.rSpawnErr = "INDOOR";
			return 0;
		}
	}

	for(%i = 1; %i <= 4; %i++)
	{
		if(%i == 1) { %dir = "North"; %sA = "NorthToEast"; %sB = "NorthToWest"; %add = -1; }
		if(%i == 2) { %dir = "South"; %sA = "SouthToEast"; %sB = "SouthToWest"; %add = 1; }
		if(%i == 3) { %dir = "East"; %sA = "EastToNorth"; %sB = "EastToSouth"; %add = -1; }
		if(%i == 4) { %dir = "West"; %sA = "WestToNorth"; %sB = "WestToSouth"; %add = 1; }

		%used[%dir] = %this.used[%dir];

		if(!%this.valid[%dir] || %used[%dir])
			continue;

		%distCheck += %this.dist[%dir];
		%choices++;

		// If this is the chosen direction... (Differs based on what %useDistanceChance is set to)
		if((%useDistanceChance && %choice <= %distCheck) || %choices == %choice)
		{
			%pos1 = %this.pos1[%dir];
			%pos2 = %this.pos2[%dir];

			// Multiplier; this lets us randomize the position
			// Things start getting really messy at distances of 10,000,000 and higher.
			if(vectorDist(%plpos, "0 0 0")  > 10000) {
				%mult = 0.1; // Special handling for high distances. (Not perfect but it works)
			}
			else {
				%mult = 100;
			}

			%randA = getRandom(%pos1*%mult, %pos2*%mult);
			%randB = %randA/%mult;

			if(%i == 1 || %i == 2)
				%pos3 = %randB SPC getWord(%this.hit[%dir], 1)+%add SPC getWord(%plpos, 2);

			if(%i == 3 || %i == 4)
				%pos3 = getWord(%this.hit[%dir], 0)+%add SPC %randB SPC getWord(%plpos, 2);

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

			// ## rayForward ## //
			// We're going to do another check to randomize our position.
			%rayForward = containerRaycast(%from, %to, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);

			if(!%rayForward)
				%hit = %to;
			else
				%hit = posFromRaycast(%rayForward);

			if(%i == 1) //north
				%randC = getRandom(getWord(%pos3,1)*%mult,(getWord(%hit,1)+1)*%mult);
			if(%i == 2) //south
				%randC = getRandom(getWord(%pos3,1)*%mult,(getWord(%hit,1)-1)*%mult);
			if(%i == 3) //east
				%randC = getRandom(getWord(%pos3,0)*%mult,(getWord(%hit,0)+1)*%mult);
			if(%i == 4) //west
				%randC = getRandom(getWord(%pos3,0)*%mult,(getWord(%hit,0)-1)*%mult);

			if(%i == 1 || %i == 2) {
				%pos = %randB SPC %randC/%mult SPC getWord(%plpos,2)-1;
			}
			if(%i == 3 || %i == 4) {
				%pos = %randC/%mult SPC %randB SPC getWord(%plpos,2)-1;
			}

			if($Pref::Server::RenderCreateLines == 1)
			{
				%lineC = createLine(%from,%hit,"","1 0 0 1");
				renderMiscGroup.add(%lineC);
			}

			if(getWord(%pos,0) == %this)
			{
				error("Render_Spawn_GetNewDirection - Something happened.");
				%this.rSpawnErr = "BAD_POS";
				return 0;
			}

			// ## rayDownward ## //
			// Check below the chosen position to make sure we're on the ground.
			// This is less complicated than rayForward because we don't have to mess with the horizontal position.
			%toDownward = setWord(%pos, 2, getWord(%pos, 2)-150);
			%rayDownward = containerRaycast(%pos, %toDownward, $TypeMasks::StaticShapeObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::FxBrickObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType);

			if(!%rayDownward)
				%hitDownward = %to;
			else
				%hitDownward = posFromRaycast(%rayDownward);

			if($Pref::Server::RenderCreateLines == 1)
			{
				%lineC = createLine(%pos,%hitDownward,"","1 0 0 1");
				renderMiscGroup.add(%lineC);
			}

			%pos = %hitDownward;

			// ## Finalize ## //
			if(!%disableUsedMark) // Mark position as "used" unless disabled
			{
				%this.used[%dir] = 1;
				%this.validDirections--;
			}

			// Final check. We need to make sure the position is near the player.
			// If it's more than 300 units away, it's almost guaranteed that something went wrong.
			if(vectorDist(%plpos, %pos) > 300)
			{
				%this.rSpawnErr = "POS_TOO_FAR";
				return 0;
			}

			%this.currentDirection = %dir;
			return %pos;
		}
	}

	%this.rSpawnErr = "NO_VALID_DIRS";
	return 0;
}
