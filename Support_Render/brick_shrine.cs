//// # Shrine Functions
// See init.cs for shrine datablocks and prefs

//// # onPlant

function brickGlitchShrineData::onPlant(%a,%br) // Planted
{
	Parent::onPlant(%a,%br);
	%br.shrineSched = schedule(15,0,Render_ShrinePlant,%br);
	// Using a schedule prevents us from returning the host's brick group instead of the actual owner's group
	// (This may only be necessary for onLoadPlant)
}

function brickGlitchShrineData::onLoadPlant(%a,%br) // Planted (load)
{
	Parent::onLoadPlant(%br);
	%br.shrineSched = schedule(15,0,Render_ShrinePlant,%br);
	// Using a schedule prevents us from returning the host's brick group instead of the actual owner's group
}

// # onRemove/onDeath

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

// # Plant/Remove B

// We need to "register" the shrine by setting some variables. This is so we can easily access it later and use it to perform radius searches.
// We also want to make sure the brickgroup stays within the shrine limit.
// Render_Shrine[total_shrines++] = brick_id
function Render_ShrinePlant(%br)
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

//// # Shrine Check
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

	for(%iB = 1; %iB <= %total; %iB++)
	{
		if(!%manual) // If a brick is specified, ignore this.
			%br = $R_Shr[%iB];

		%r = $Pref::Server::RenderShrineRange;

		if(%br.position $= "") // Error if one is missing.
		{
			error("Support_Render - Shrine " @ $R_Shr[%iB] @ " (" @ %iB @ ") does not exist! Shrine will be force-removed.");
			Render_ShrineRemove(%br, $R_Shr[%iB]);
		}
		else if(%r != -1 && %br.isRayCasting())
		{
			// Start a box search. If a Render bot is nearby, delete it immediately.
			initContainerBoxSearch(%br.position,%r SPC %r SPC %r,$TypeMasks::PlayerObjectType);
			while(%target=containerSearchNext())
			{
				if(%target.isRender)
				{
					//echo("RENDER (global): Force de-spawning " @ %target @ " due to shrine");
					%target.delete();

					// Flicker to indicate that the shrine did something.
					%br.setDatablock(brickPumpkinBaseData);
					%br.schedule(128,setDatablock,brickGlitchShrineData);
				}
			}
		}
	}
}
