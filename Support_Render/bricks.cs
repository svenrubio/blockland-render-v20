////// # Glitch Shrine
// See init.cs for shrine datablocks and prefs

//// ## onPlant

function brickGlitchShrineData::onPlant(%a,%br) // Planted
{
	Parent::onPlant(%a,%br);
	%br.shrineSched = schedule(15, 0, Render_ShrinePlant, %br);
	// Using a schedule prevents us from returning the host's brick group instead of the actual owner's group
	// (This may only be necessary for onLoadPlant)

   %enabled     = 1;
   %delay       = 0;
   %inputEvent  = "OnActivate";
   %target      = "client";
   %outputEvent = "ChatMessage";
   %par1        = "<color:FFFFFF>This is a Glitch Shrine. It creates a safe zone from Render. %renderServerShrineRange";
   %br.addEvent(%enabled, %delay, %inputEvent, %target, %outputEvent, %par1);
}

function brickGlitchShrineData::onLoadPlant(%a,%br) // Planted (load)
{
	Parent::onLoadPlant(%br);
	%br.shrineSched = schedule(15, 0, Render_ShrinePlant, %br, 1);
	// Using a schedule prevents us from returning the host's brick group instead of the actual owner's group
}

// ## onRemove/onDeath

function brickGlitchShrineData::onDeath(%a,%br) // Brick deleted
{
	Render_ShrineRemove(%br);
	Parent::onDeath(%br);
}

function brickGlitchShrineData::onRemove(%a,%br) // Brick removed (in case onDeath isn't called)
{
	if(%br.isPlanted && %br.isGlitchShrine)
		Render_ShrineRemove(%br);

	Parent::onRemove(%br);
}

// ## Plant/Remove B

// We need to "register" the shrine by setting some variables. This is so we can easily access it later and use it to perform radius searches.
// We also want to make sure the brickgroup stays within the shrine limit.
// Render_Shrine[total_shrines++] = brick_id
function Render_ShrinePlant(%br, %loadPlant)
{
	if(!%br.isPlanted)
		return;

	%group = %br.getGroup();

	if(%group.rShrines >= 32) // If there are too many shrines, set this one as dormant permanently.
	{
		%client = %group.client;

		if(isObject(%client))
			%client.centerPrint("\c6You can't have more than 32 shrines!",3);

		%br.setDatablock(brickPumpkinBaseData);


		if(!%loadPlant)
			%br.clearEvents();
	}
	else
	{
		%group.rShrines++;

		%br.isGlitchShrine = 1;

		//%chunk = mFloor( getWord(%pos,0)/15 ) @ "_" @ mFloor( getWord(%pos,1)/15 ) @ "_" @ mFloor( getWord(%pos,2)/15 );

		$R_Shr[$R_Shr_t++] = %br;
		$R_Shr_G[$R_Shr_t] = %br.getGroup(); // This is an extra precaution in case a shrine mysteriously vanishes.
		%br.shrineId = $R_Shr_t;

		//echo("Registered shrine " @ %br @ " to group " @ %group @ " (total: " @ $r_shr_t @ ")");

		// Do a shrine check on this brick when we place it for the first time.
		// This fixes the shrine check threshold being noticeable when you place a shrine by an attacker.
		Render_DoShrineCheck(%br);
	}
}

// This handles removing shrines from the list. We fill the empty spot by replacing it with the most recent shrine, then decreasing the count.
// If %id is specified, we will force-remove the id from the list.
function Render_ShrineRemove(%br,%id)
{
	if(!%id) // If ID isn't specified... (We don't need to use $= "" because shrine counts start at 1)
		%id = %br.shrineId;

	cancel(%br.shrineSched); // Just in case.

	if(%id)
	{
		$R_Shr[%id] = $R_Shr[$R_Shr_t]; // To fill our 'empty' slot, we'll just take the latest shrine and move it to this one.
		$R_Shr_t--; // Decrease the count. The latest shrine's original slot will be filled as if it doesn't exist.

		$R_Shr_G[%id].rShrines--; // Now we'll subtract one from the brickgroup's shrine count.
	}

	%br.isGlitchShrine = 0;
	//echo("Unregistered shrine " @ %br @ " (total: " @ $r_shr_t @ ")");
}

//// ## Shrine Check
// %br: if specified, shrine check only applies to brick %br.
// Otherwise runs a global shrine check.
function Render_DoShrineCheck(%br)
{

	// If no brick specified, apply to all bricks on server.
	if(!%br)
		%total = $R_Shr_t;
	else
	{
		%total = 1; // If working with an individual brick, our total is, obviously, one.
		%manual = 1; // Specify that we're manually checking a single brick.
	}

	for(%i = 1; %i <= %total; %i++)
	{
		if(!%manual) // If a brick is specified, ignore this.
			%br = $R_Shr[%i];

		%r = $Pref::Server::RenderShrineRange;

		if(%br.position $= "") // Error if one is missing.
		{
			error("Support_Render - Shrine " @ $R_Shr[%i] @ " (" @ %i @ ") does not exist! Shrine will be force-removed.");
			Render_ShrineRemove(%br, $R_Shr[%i]);
		}
		else if(%r != -1 && %br.isRayCasting())
		{
			// Start a box search. If a Render bot is nearby, delete it immediately.
			for(%iB = 0; %iB < RenderBotGroup.getCount(); %iB++)
			{
				%target = RenderBotGroup.getObject(%iB);

				// A seperate function on a schedule is used to reduce performance impacts.
				schedule(0, 0, Render_DoShrineEffect, %target, %br, %r);
			}
		}
	}
}

function Render_DoShrineEffect(%target, %br, %r)
{
	if(%target.isRender && vectorDist(%target.position,%br.position) <= %r)
	{
		//echo("RENDER (global): Force de-spawning " @ %target @ " due to shrine");
		Render_DeleteR(%target);

		// Flicker to indicate that the shrine did something.
		%br.setDatablock(brickPumpkinBaseData);
		%br.schedule(128,setDatablock,brickGlitchShrineData);
	}
}


////// # Detector Brick
// This is mostly a copy of the above code.

function brickGlitchDetectorData::onPlant(%a,%br) // Planted
{
	Parent::onPlant(%a, %br);
	%br.detSched = schedule(15, 0, Render_DetectorBrickPlant, %br);

	%br.addEvent(1, 0, "OnRelay", "self", "setColorFX", 3);
	%br.addEvent(1, 256, "OnRelay", "self", "setColorFX", 0);
}

function brickGlitchDetectorData::onLoadPlant(%a,%br) // Planted (load)
{
	Parent::onLoadPlant(%br);
	%br.detSched = schedule(15, 0, Render_DetectorBrickPlant, %br, 1);
	// Using a schedule prevents us from returning the host's brick group instead of the actual owner's group
}

// ## onRemove/onDeath

function brickGlitchDetectorData::onDeath(%a,%br) // Brick deleted
{
	Render_DetectorBrickRemove(%br);
	Parent::onDeath(%br);
}

function brickGlitchDetectorData::onRemove(%a,%br) // Brick removed (in case onDeath isn't called)
{
	if(%br.isPlanted && %br.isGlitchDetector)
		Render_DetectorBrickRemove(%br);

	Parent::onRemove(%br);
}

// ## Plant/Remove B

function Render_DetectorBrickPlant(%br, %loadPlant)
{
	if(!%br.isPlanted)
		return;

	%group = %br.getGroup();

	if(%group.rDetectorBricks >= 48) // If there are too many detectors, set this to a regular 1x1F.
	{
		%client = %group.client;

		if(isObject(%client))
			%client.centerPrint("\c6You can't have more than 48 detectors!",3);

		%br.setDatablock(brick1x1fData);

		if(!%loadPlant)
			%br.clearEvents();
	}
	else
	{
		%group.rDetectorBricks++;

		%br.isGlitchDetector = 1;

		$R_Det[$R_Det_t++] = %br;
		$R_Det_G[$R_Det_t] = %br.getGroup();
		%br.detId = $R_Det_t;

		//echo("Registered detector " @ %br @ " to group " @ %group @ " (total: " @ $r_det_t @ ")");

		Render_DoDetectorBrickCheck(%br);
	}
}

function Render_DetectorBrickRemove(%br,%id)
{
	if(!%id)
		%id = %br.detId;

	cancel(%br.detSched);

	if(%id)
	{
		$R_Det[%id] = $R_Det[$R_Shr_t];
		$R_Det_t--;

		$R_Det_G[%id].rDetectorBricks--;
	}

	%br.isGlitchDetector = 0;
	//echo("Unregistered detector " @ %br @ " (total: " @ $r_det_t @ ")");
}

//// ## Detector Brick Check
function Render_DoDetectorBrickCheck(%br)
{
	// If no brick specified, apply to all bricks on server.
	if(!%br)
		%total = $R_Det_t;
	else
	{
		%total = 1;
		%manual = 1;
	}

	for(%i = 1; %i <= %total; %i++)
	{
		if(!%manual) // If a brick is specified, ignore this.
			%br = $R_Det[%i];

		%r = 60; // Distance for "moderate" energy.

		if(%br.position $= "") // Error if one is missing.
		{
			error("Support_Render - Detector brick " @ $R_Det[%i] @ " (" @ %i @ ") does not exist! Brick will be force-removed.");
			Render_DetectorBrickRemove(%br, $R_Det[%i]);
		}
		else
		{
			for(%iB = 0; %iB < RenderBotGroup.getCount(); %iB++)
			{
				%target = RenderBotGroup.getObject(%iB);

				schedule(0, 0, Render_DoDetectorBrickEffect, %target, %br, %r);
			}
		}
	}
}

function Render_DoDetectorBrickEffect(%target, %br, %r)
{
	// Detected! Fire relay on the brick.
	if(%target.isRender && %target.isAttacking && vectorDist(%target.position,%br.position) <= %r)
		%br.fireRelay();
}
