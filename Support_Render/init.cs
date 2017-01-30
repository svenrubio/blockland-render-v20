//This contains datablocks, packages, and other run-once things (for convenience of re-executing the add-on)

$Render::C_ShrineLimit = 32;

//////# PREFERENCES

//$Pref::Server::RenderMinSpawnDistance = 32; //huge open spaces/outdoors
//$Pref::Server::RenderMinSpawnDistance = 16; //large rooms/outdoors
//$Pref::Server::RenderMinSpawnDistance = 8; //medium rooms
//$Pref::Server::RenderMinSpawnDistance = 4; //super small spaces.

//0; Disabled
//1/1; High
//3/4; Above Normal
//2/3; Normal
//1/2; Below Normal
//1/6; Low

$Pref::Server::RenderMinSpawnDistance = 8;
$Pref::Server::RenderAllowMultiples = 0;

if(isFunction("RTB_registerPref"))
{
	$Pref::Server::RenderDifficulty = 100;
	//RTB_registerPref("Difficulty", "Render", "$Pref::Server::RenderDifficulty", "list Passive 0 Easy 25 Normal 100 Hard 400 Insane 1600", "GameMode_Renderman_Haunting", $Pref::Server::RenderDifficulty, 0, 0);
	RTB_registerPref("Attack Type", "Render", "$Pref::Server::RenderDamageType", "list Static 0 Health 1", "GameMode_Renderman_Haunting", $Pref::Server::RenderDamageType, 0, 0);
	RTB_registerPref("Spawn Rate", "Render", "$Pref::Server::RenderSpawnRate", "list Disabled 0 Low 6 Below_Normal 4 Normal 3 Above_Normal 2 High 1", "GameMode_Renderman_Haunting", $Pref::Server::RenderSpawnRate, 0, 0);
	RTB_registerPref("Shrine Range", "Render", "$Pref::Server::RenderShrineRange", "list 64x 28 48x 20 32x 12 16x 4 Disabled -1", "GameMode_Renderman_Haunting", $Pref::Server::RenderShrineRange, 0, 0);
	RTB_registerPref("Only spawn at night", "Render", "$Pref::Server::RenderDayCycleSpawn", "bool", "GameMode_Renderman_Haunting", $Pref::Server::RenderDayCycleSpawn, 0, 0);
	//RTB_registerPref("Hard mode (Allows multiple Renders at once)", "Render", "$Pref::Server::RenderAllowMultiples", "bool", "GameMode_Renderman_Haunting", $Pref::Server::RenderAllowMultiples, 0, 0);

	//RTB_registerPref("Minimum Spawning Distance", "Render", "$Pref::Server::RenderMinSpawnDistance", "int 4 64", "GameMode_Renderman_Haunting", "4", 0, 0); //check this
}
else
{
	$Pref::Server::RenderDifficulty = 100;
	$Pref::Server::RenderDamageType = 0;
	$Pref::Server::RenderSpawnRate = 6;
	$Pref::Server::RenderShrineRange = 20;
	$Pref::Server::RenderDayCycleSpawn = 0;
}

if(!$RTB::RTBR_ServerControl_Hook)
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");

//////# SOUNDS
// (To do: compress these)
datablock AudioProfile(renderGrowl)
{
   filename    = "./sound/indoorgrowl.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(renderAmb1)
{
   filename    = "./sound/rendercycle1.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(renderAmb2)
{
   filename    = "./sound/rendercycle2.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(renderMove)
{
   filename    = "./sound/entityMove.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(renderForm)
{
   filename    = "./sound/entityForm.wav";
   description = AudioClose3d;
   preload = true;
};

//datablock AudioProfile(glitchFire)
//{
//   filename    = "./sound/glitchFire.wav";
//   description = AudioClose3d;
//   preload = true;
//};

datablock AudioProfile(rStatic)
{
   filename    = "./sound/glimpse.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(rAttackB)
{
   filename    = "./sound/attackB.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(rAttackC)
{
   filename    = "./sound/attackC.wav";
   description = AudioClose3d;
   preload = true;
};

//////# PARTICLES
datablock ParticleData(RendermanCameraParticleA : CameraParticleA)
{
	animTexName[0] = "./dotB";
	animTexName[1] = "base/data/particles/dot";
	colors[0] = "0.266667 0.000000 0.266667 1.000000";
	colors[1] = "0.200000 0.000000 0.200000 1.000000";
	colors[2] = "0.000000 0.000000 0.000000 0.000000";
	colors[3] = "1.000000 1.000000 1.000000 1.000000";
	useInvAlpha = "1";
};

datablock ParticleEmitterData(RendermanCameraEmitterA : CameraEmitterA)
{
	particles = "RendermanCameraParticleA";
	uiName = "Camera Glow Evil";
};

//////# BRICKS
datablock fxDtsBrickData(brickGlitchShrineData)
{
	brickFile = "Add-Ons/Brick_Halloween/pumpkin_ascii.blb";
	uiName = "Glitch Shrine";
	category = "Special";
	subCategory = "Interactive";
	iconName = "Add-Ons/Support_Render/Glitch Shrine";
	indestructable = 1;
};

////// # Shrines

function brickGlitchShrineData::onPlant(%a,%br) // Planted
{
	Render_ShrinePlant(%br);
	Parent::onPlant(%a,%br);
}

function brickGlitchShrineData::onLoadPlant(%a,%br) // Planted (load)
{
	Render_ShrinePlant(%br);
	Parent::onLoadPlant(%br);
}

//## Shrine Functions

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

// ## Shrine Functions B

// We need to "register" the shrine by setting some variables. This is so we can easily access it later and use it to perform radius searches.
// We also want to make sure the brickgroup stays within the shrine limit.
// Render_Shrine[total_shrines++] = brick_id
function Render_ShrinePlant(%br)
{
	%group = %br.getGroup();

	if(%group.rShrines >= 32) // If there are too many shrines, set this one as dormant permanently.
	{
		%client = Brickgroup_14128.client;

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
	}
}

// This handles removing shrines from the list. We fill the empty spot by replacing it with the most recent shrine, then decreasing the count.
// If %id is specified, we will force-remove the id from the list.
function Render_ShrineRemove(%br,%id)
{
	if(!%id) // If ID isn't specified... (We don't need to use $= "" because shrine counts start at 1)
		%id = %br.shrineId;

	if(%id)
	{
		$R_Shr[%id] = $R_Shr[$R_Shr_t]; // To fill our 'empty' slot, we'll just take the latest shrine and move it to this one.
		$R_Shr_t--; // Decrease the count. The latest shrine's original slot will be filled as if it doesn't exist.

		$R_Shr_G[%id].rShrines--; // Now we'll subtract one from the brickgroup's shrine count.
	}

	%br.isGlitchShrine = 0;
}

//////# FACE PROJECTILE
//datablock ParticleData(RenderAttackParticle)
//{
//	dragCoefficient      = 5.0;
//	gravityCoefficient   = 0.0;
//	inheritedVelFactor   = 1.0;
//	windCoefficient      = 0;
//	constantAcceleration = 0.0;
//	lifetimeMS           = 800;
//	lifetimeVarianceMS   = 0;
//	useInvAlpha          = false;
//	textureName          = "Add-Ons/Brick_Halloween/RenderAttack";
//	colors[0]     = "1 1 1 0";
//	colors[1]     = "1 1 1 1";
//	colors[2]     = "1 1 1 0";
//	sizes[0]      = 1;
//	sizes[1]      = 1.5;
//	sizes[2]      = 1.3;
//	times[0]      = 0;
//	times[1]      = 0.5;
//	times[2]      = 1.0;
//};
//
//datablock ParticleEmitterData(RenderAttackEmitter)
//{
//	ejectionPeriodMS = 100;
//	periodVarianceMS = 0;
//	ejectionVelocity = 0.0;
//	ejectionOffset   = 1.8;
//	velocityVariance = 0.0;
//	thetaMin         = 0;
//	thetaMax         = 0;
//	phiReferenceVel  = 0;
//	phiVariance      = 0;
//	overrideAdvance = false;
//	lifeTimeMS = 100;
//	particles = "RenderAttackParticle";
//
//	doFalloff = true; //if we do fall off with this emitter it ends up flickering, for most emitters you want this TRUE
//
//	emitterNode = GenericEmitterNode;        //used when placed on a brick
//	pointEmitterNode = TenthEmitterNode; //used when placed on a 1x1 brick
//};
//
//datablock ExplosionData(RenderAttackExplosion)
//{
//	lifeTimeMS = 2000;
//	emitter[0] = RenderAttackEmitter;
//	//soundProfile = RenderAttackSound;
//};
//
//datablock ProjectileData(RenderAttackProjectile)
//{
//	explosion           = "";
//
//	armingDelay         = 0;
//	lifetime            = 10;
//	explodeOnDeath		= false;
//};

//////# PLAYERTYPE
datablock PlayerData(PlayerRenderArmor : PlayerStandardArmor)
{
	isInvincible = 1;
	magicWandImmunity = 1;

	//maxBackwardSpeed = 40;
	//maxForwardSpeed = 70;
	//maxSideSpeed = 60;
};

function PlayerRenderArmor::onRemove(%a, %render)
{
	if(%render.freezeTarget)
		Render_UnfreezePlayer(%render.freezeTarget);

	Parent::onRemove(%a, %render);
}

//////# FUNCTIONS
// Death vehicle from Item_Skis was used as a reference for this
datablock PlayerData(RenderDeathArmor : PlayerStandardArmor)
{
	airControl = 0;

	canRide = 0;

	cameraMaxDist = 20;
	cameraVerticalOffset = 15;

	drag = 2;

	isInvincible = 1;

	jumpForce = 0;
	runForce = 0;

	uiName = "";
};

function RenderDeathArmor::onAdd(%datablock,%obj)
{
	%obj.hideNode("ALL");
}

function RenderDeathArmor::onRemove(%this, %obj)
{
	%player = %obj.getMountedObject(0);

	if(isObject(%player))
		%player.canDismount = 1;
}

//////# ITEMS
//datablock ItemData(GlitchEnergyGunItem)
//{
//	category = "Weapon";  // Mission editor category
//	className = "Weapon"; // For inventory system
//
//	// Basic Item Properties
//	shapeFile = "base/data/shapes/printGun.dts";
//	rotate = false;
//	mass = 1;
//	density = 0.2;
//	elasticity = 0.2;
//	friction = 0.6;
//	emap = true;
//
//	uiName = "Glitch Gun";
//	iconName = " ";
//	doColorShift = true;
//	colorShiftColor = "0.0 1.0 1.0 1.0";
//
//	// Dynamic properties defined by the scripts
//	image = GlitchEnergyGunImage;
//	canDrop = true;
//};

//datablock ShapeBaseImageData(GlitchEnergyGunImage)
//{
//	// Basic Item properties
//	shapeFile = "base/data/shapes/printGun.dts";
//	emap = true;
//
//	// Specify mount point & offset for 3rd person, and eye offset
//	// for first person rendering.
//	mountPoint = 0;
//	offset = "0 0 0";
//	eyeoffset = "0.7 1.2 -1";
//	rotation = eulerToMatrix( "0 0 0" );
//
//	// When firing from a point offset from the eye, muzzle correction
//	// will adjust the muzzle vector to point to the eye LOS point.
//	// Since this weapon doesn't actually fire from the muzzle point,
//	// we need to turn this off.
//	correctMuzzleVector = true;
//
//	// Add the WeaponImage namespace as a parent, WeaponImage namespace
//	// provides some hooks into the inventory system.
//	className = "WeaponImage";
//
//	// Projectile && Ammo.
//	item = GlitchEnergyGunItem;
//	//ammo = " ";
//	//projectile = wrenchProjectile; // The item doesn't actually fire a projectile.
//	//projectileType = Projectile;
//	//
//	//casing = gunShellDebris;
//	//shellExitDir        = "0.0 0.0 0.0";
//	//shellExitOffset     = "0 0 0";
//	//shellExitVariance   = 0.0;
//	//shellVelocity       = 0.0;
//
//	armReady = true;
//
//	doColorShift = true;
//	colorShiftColor = GlitchEnergyGunItem.colorShiftColor; //"0.400 0.196 0 1.000";
//
//	//casing = " ";
//
//	// Images have a state system which controls how the animations
//	// are run, which sounds are played, script callbacks, etc. This
//	// state system is downloaded to the client so that clients can
//	// predict state changes and animate accordingly.  The following
//	// system supports basic ready->fire->reload transitions as
//	// well as a no-ammo->dryfire idle state.
//
//	// Initial start up state
//	stateName[0]                     = "Activate";
//	stateTimeoutValue[0]             = 0.0;
//	stateTransitionOnTimeout[0]       = "Ready";
//	stateSound[0]					= weaponSwitchSound;
//
//	stateName[1]                    = "Ready";
//	stateTransitionOnTriggerDown[1] = "initiate";
//	stateAllowImageChange[1]        = true;
//
//	stateName[2]                = "initiate";
//	stateScript[2]              = "onInit";
//	stateTimeoutValue[2]        = 1;
//	stateTransitionOnTimeout[2] = "Ready";
//	stateAllowImageChange[2]    = false;
//};

//////# MISC
new simGroup(RenderBotGroup) {}; //Render bot group
//missionCleanup.add(RenderBotGroup);

new simGroup(RenderMiscGroup) {}; //Render object group
//missionCleanup.add(RenderMiscGroup);

$Render::LoopBot = schedule(50,0,Render_Loop);
$Render::LoopSpawner = schedule(30000,0,Render_Spawn_Loop);
