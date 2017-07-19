//This contains datablocks, packages, and other run-once things (for convenience of re-executing the add-on)
$Render::C_ShrineLimit = 32;

//////# PREFERENCES

//$Pref::Server::RenderMinSpawnDistance = 32; //huge open spaces/outdoors
//$Pref::Server::RenderMinSpawnDistance = 16; //large rooms/outdoors
//$Pref::Server::RenderMinSpawnDistance = 8; //medium rooms
//$Pref::Server::RenderMinSpawnDistance = 4; //super small spaces.

//0; Disabled
//5/5; 100%; High
//4/5; 80%; Above Normal
//3/5; 60%; Normal
//2/5; 40%; Below Normal
//1/5; 20%; Low

$Pref::Server::RenderMinSpawnDistance = 2;
$Pref::Server::RenderAllowMultiples = 0;

if(!$RTB::RTBR_ServerControl_Hook && isFile("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs"))
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");

if(isFunction("RTB_registerPref"))
{
	$Pref::Server::RenderDifficulty = 100;
	//RTB_registerPref("Difficulty", "Render", "$Pref::Server::RenderDifficulty", "list Passive 0 Easy 25 Normal 100 Hard 400 Insane 1600", "GameMode_Renderman_Haunting", $Pref::Server::RenderDifficulty, 0, 0);
	RTB_registerPref("Mode", "Render", "$Pref::Server::RenderDamageType", "list Normal 0 Health 1 Tag 2 Haunt 3", "GameMode_Renderman_Haunting", $Pref::Server::RenderDamageType, 0, 0);
	RTB_registerPref("Spawn Rate", "Render", "$Pref::Server::RenderSpawnRate", "list Disabled 0 Low 2 Below_Normal 3 Normal 4 Above_Normal 5 High 6", "GameMode_Renderman_Haunting", $Pref::Server::RenderSpawnRate, 0, 0);
	RTB_registerPref("Shrine Range", "Render", "$Pref::Server::RenderShrineRange", "list 64x 28 48x 20 32x 12 16x 4 Disabled -1", "GameMode_Renderman_Haunting", $Pref::Server::RenderShrineRange, 0, 0);
	RTB_registerPref("Only spawn at night (Day cycle)", "Render", "$Pref::Server::RenderDayCycleSpawn", "bool", "GameMode_Renderman_Haunting", $Pref::Server::RenderDayCycleSpawn, 0, 0);
	RTB_registerPref("Disable ambient sounds", "Render", "$Pref::Server::RenderDisableAmbientSounds", "bool", "GameMode_Renderman_Haunting", $Pref::Server::RenderDisableAmbientSounds, 0, 0);
	RTB_registerPref("Disable lights", "Render", "$Pref::Server::RenderDisableLights", "bool", "GameMode_Renderman_Haunting", $Pref::Server::RenderDisableLights, 0, 0);
	RTB_registerPref("Invincible", "Render", "$Pref::Server::RenderIsInvincible", "bool", "GameMode_Renderman_Haunting", $Pref::Server::RenderIsInvincible, 0, 0);

	//RTB_registerPref("Minimum Spawning Distance", "Render", "$Pref::Server::RenderMinSpawnDistance", "int 4 64", "GameMode_Renderman_Haunting", "4", 0, 0); //check this
}
else
{
	$Pref::Server::RenderDifficulty = 100;
	$Pref::Server::RenderDamageType = 0;
	$Pref::Server::RenderSpawnRate = 6;
	$Pref::Server::RenderShrineRange = 20;
}

// Extra load check; these are the things that REALLY shouldn't run twice.
if(!$Render::LoadedB)
{
	// NOTE: If including Support_Render and Slayer in a game-mode, make sure to have Slayer load first.
	// Console errors may occur in game-modes.
	if(isFile("Add-Ons/GameMode_Slayer/server.cs") && $AddOn__GameMode_Slayer)
		exec("./compat/slayer.cs");
}

//////# SOUNDS
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

datablock AudioProfile(glitchFire)
{
   filename    = "./sound/glitchFire.wav";
   description = AudioClose3d;
   preload = true;
};

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

datablock fxDtsBrickData(brickGlitchDetectorData)
{
	brickFile = "base/data/bricks/flats/1x1F.blb";
	uiName = "Glitch Detector";
	category = "Special";
	subCategory = "Interactive";
	iconName = "base/client/ui/brickIcons/1x1F";
	indestructable = 1;
};

//// ## Shrine String
$Render::C_ShrString[-1] = "NOTE: Shrines are disabled.";
$Render::C_ShrString[4] = "Shrine range: 16x";
$Render::C_ShrString[12] = "Shrine range: 32x";
$Render::C_ShrString[20] = "Shrine range: 48x";
$Render::C_ShrString[28] = "Shrine range: 64x";

//////# PLAYERTYPE
datablock PlayerData(PlayerRenderArmor : PlayerStandardArmor)
{
	magicWandImmunity = 1;
	maxDamage = 300; // Max health

	//maxBackwardSpeed = 40;
	//maxForwardSpeed = 70;
	//maxSideSpeed = 60;
};

datablock PlayerData(PlayerRenderTagArmor : PlayerRenderArmor)
{
	maxDamage = 600;
};

function PlayerRenderArmor::onDisabled(%a, %render, %str)
{
	%render.schedule(32,delete); // Render instantly disappears when he gets 'killed'
	%client = %render.lastDmgClient;

	// If the client exists AND is in a minigame that awards points for killing Render...
	if(isObject(%client) && %client.minigame.rPoints)
		%client.incScore(%client.minigame.rPoints); // Give them their points

	Parent::onDisabled(%a, %render, %str);
}

function PlayerRenderArmor::onRemove(%a, %render)
{
	if(%render.freezeTarget)
		Render_UnfreezePlayer(%render.freezeTarget);

	Parent::onRemove(%a, %render);
}

// Same as above but with tag mode armor
function PlayerRenderTagArmor::onDisabled(%a, %render, %str)
{
	%render.schedule(32,delete); // Render instantly disappears when he gets 'killed'
	Parent::onDisabled(%a, %render, %str);
}

function PlayerRenderTagArmor::onRemove(%a, %render)
{
	if(%render.freezeTarget)
		Render_UnfreezePlayer(%render.freezeTarget);

	Parent::onRemove(%a, %render);
}

////// # DAMAGE TYPE
AddDamageType("RenderDeath", '<bitmap:Add-Ons/Support_Render/CI_Render> %1', '%2 <bitmap:Add-Ons/Support_Render/CI_Render> %1', 0.5, 1);

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
//## Glitch Gun
datablock ItemData(GlitchEnergyGunItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	// Basic Item Properties
	shapeFile = "base/data/shapes/printGun.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Glitch Gun";
	iconName = " ";
	doColorShift = true;
	colorShiftColor = "0.0 1.0 1.0 1.0";

	// Dynamic properties defined by the scripts
	image = GlitchEnergyGunImage;
	canDrop = true;
};

datablock ShapeBaseImageData(GlitchEnergyGunImage)
{
	// Basic Item properties
	shapeFile = "base/data/shapes/printGun.dts";
	emap = true;

	// Specify mount point & offset for 3rd person, and eye offset
	// for first person rendering.
	mountPoint = 0;
	offset = "0 0 0";
	eyeoffset = "0.7 1.2 -1";
	rotation = eulerToMatrix( "0 0 0" );

	// When firing from a point offset from the eye, muzzle correction
	// will adjust the muzzle vector to point to the eye LOS point.
	// Since this weapon doesn't actually fire from the muzzle point,
	// we need to turn this off.
	correctMuzzleVector = true;

	// Add the WeaponImage namespace as a parent, WeaponImage namespace
	// provides some hooks into the inventory system.
	className = "WeaponImage";

	// Projectile && Ammo.
	item = GlitchEnergyGunItem;
	//ammo = " ";
	//projectile = wrenchProjectile; // The item doesn't actually fire a projectile.
	//projectileType = Projectile;
	//
	//casing = gunShellDebris;
	//shellExitDir        = "0.0 0.0 0.0";
	//shellExitOffset     = "0 0 0";
	//shellExitVariance   = 0.0;
	//shellVelocity       = 0.0;

	armReady = true;

	doColorShift = true;
	colorShiftColor = GlitchEnergyGunItem.colorShiftColor; //"0.400 0.196 0 1.000";

	//casing = " ";

	// Images have a state system which controls how the animations
	// are run, which sounds are played, script callbacks, etc. This
	// state system is downloaded to the client so that clients can
	// predict state changes and animate accordingly.  The following
	// system supports basic ready->fire->reload transitions as
	// well as a no-ammo->dryfire idle state.

	// Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.0;
	stateTransitionOnTimeout[0]       = "Ready";
	stateSound[0]					= weaponSwitchSound;

	stateName[1]                    = "Ready";
	stateTransitionOnTriggerDown[1] = "initiate";
	stateAllowImageChange[1]        = true;

	stateName[2]                = "initiate";
	stateScript[2]              = "onInit";
	stateTimeoutValue[2]        = 1;
	stateTransitionOnTimeout[2] = "Ready";
	stateAllowImageChange[2]    = false;
};

//## Detector
datablock ItemData(GlitchDetector)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	shapeFile = "./models/detector.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Glitch Detector";
	iconName = "./models/Icon_detector";
	doColorShift = false;
	colorShiftColor = "1.0 1.0 1.0 1.0";

	image = GlitchDetectorImage;
	canDrop = true;
};

datablock ShapeBaseImageData(GlitchDetectorImage)
{
   shapeFile = "./models/detector.dts";
   emap = true;

   mountPoint = 0;
   offset = "0 0 0";
   eyeoffset = "0.7 1.2 -1";
   rotation = eulerToMatrix( "0 0 0" );

   correctMuzzleVector = true;

   className = "WeaponImage";
   item = GlitchDetector;

   armReady = true;

   doColorShift = false;
   colorShiftColor = GlitchDetector.colorShiftColor;//"0.400 0.196 0 1.000";
};

//////# DEATH BOARD
datablock staticShapeData(renderDeathBoardData)
{
	shapeFile = "./models/cube.dts";
};

function Render_CreateDeathBoard()
{
	// Create the background
	%obj = new staticShape(RenderBoard)
	{
		datablock = renderDeathBoardData;
		position = "0 0 -666";
		scale = "0.01 16 16";
	};

	missionCleanup.add(%obj);
	%obj.setNodeColor("ALL", "0 0 0 1");

	$Render::DeathBoard = %obj;

	// Create the emitter
	%obj2 = new ParticleEmitterNode(RenderBoardNode)
	{
		datablock = GenericEmitterNode;
		emitter = RenderBoardEmitter;
		position = "-2 0 -666";
		scale = "0.05 0.05 0.05";
	};
	missionCleanup.add(%obj2);
}

//////# PARTICLES

// # Face
datablock ParticleData(RenderBoardParticle)
{
	dragCoefficient		= -32.0;
	windCoefficient		= 0.0;
	gravityCoefficient	= 0.0;
	inheritedVelFactor	= 0.0;
	constantAcceleration	= 0.0;
	lifetimeMS		= 200;
	lifetimeVarianceMS	= 0;
	spinSpeed		= 10.0;
	spinRandomMin		= -50.0;
	spinRandomMax		= 50.0;
	useInvAlpha		= false;
	animateTexture		= false;

	textureName		= "./render.png";

	colors[0]	= "1 1 1 0.2";
	colors[1]	= "1 1 1 0.0";
	sizes[0]	= 2.0;
	sizes[1]	= 0.1;
	times[0]	= 0.0;
	times[1]	= 1.0;
};

datablock ParticleEmitterData(RenderBoardEmitter)
{
   ejectionPeriodMS = 2;
   periodVarianceMS = 0;

   ejectionVelocity = 0.0;
   velocityVariance = 0.0;

   ejectionOffset = 0;

   thetaMin         = 0.0;
   thetaMax         = 90.0;

   particles = RenderBoardParticle;

	 uiName = "RenderBoardEmitter";
};

// # Damage
datablock ParticleData(RenderDmgExplosionParticle)
{
	dragCoefficient      = 10;
	gravityCoefficient   = 0.0;
	inheritedVelFactor   = 0.2;
	constantAcceleration = 0.0;
	useInvAlpha = 1;

	lifetimeMS           = 300;
	lifetimeVarianceMS   = 290;

	spinSpeed		= 0.0;
	spinRandomMin		= -150.0;
	spinRandomMax		= 150.0;
	textureName          = "base/data/particles/dot";

	colors[0]	= "0.0 0.0 0.0 0.5";
	colors[1]	= "0.0 0.0 0.0 0.5";
	colors[2]	= "0.0 0.0 0.0 0.5";
	colors[3]	= "0.0 0.0 0.0 0.0";

	sizes[0]	= 0.2;
	sizes[1]	= 0.2;
	sizes[2]	= 0.1;
	sizes[3]	= 0.0;

	times[0] = 0.0;
	times[1] = 0.9;
	times[2] = 1.0;
	times[3] = 2;
};

datablock ParticleEmitterData(RenderDmgExplosionEmitter)
{
	lifeTimeMS = 50;

	ejectionPeriodMS = 10;
	periodVarianceMS = 0;
	ejectionVelocity = 2;
	velocityVariance = 4.0;
	ejectionOffset   = 1.25;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance  = false;

	particles = "RenderDmgExplosionParticle";

	uiName = "RenderDmgExplosionEmitter";
};

//////# EVENTS
registerOutputEvent(Minigame, "setRenderMode", "list UseServerPreference -1 Normal 0 Health 1 Tag 2 Haunt 3", 1);
registerOutputEvent(Minigame, "setRenderSpawnRate", "list UseServerPreference -1 Disabled 0 Low 2 BelowNormal 3 Normal 4 AboveNormal 5 High 6", 1);
registerOutputEvent(Minigame, "setRenderInvincibility", "list UseServerPreference -1 Disabled 0 Enabled 1", 1);

//////# MISC
new simGroup(RenderBotGroup) {}; //Render bot group
//missionCleanup.add(RenderBotGroup);

new simGroup(RenderMiscGroup) {}; //Render object group
//missionCleanup.add(RenderMiscGroup);

Render_CreateDeathBoard();

$Render::LoopBot = schedule(50,0,Render_Loop);
$Render::LoopSpawner = schedule(30000,0,Render_Spawn_Loop);
