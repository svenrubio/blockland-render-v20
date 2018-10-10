// This contains datablocks, packages, and other run-once things (for convenience of re-executing the add-on)
$Render::C_ShrineLimit = 32;

////// # Stats
$Render::Stat::SpawnCount = 0;

////// # Preferences # //////

//$Pref::Server::RenderMinSpawnDistance = 32; //huge open spaces/outdoors
//$Pref::Server::RenderMinSpawnDistance = 16; //large rooms/outdoors
//$Pref::Server::RenderMinSpawnDistance = 8; //medium rooms
//$Pref::Server::RenderMinSpawnDistance = 4; //super small spaces.

$Pref::Server::RenderMinSpawnDistance = 2;
$Pref::Server::RenderAllowMultiples = 0;

if(!$RTB::RTBR_ServerControl_Hook && isFile("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs"))
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");

if(isFunction("RTB_registerPref"))
{
	// We're still using the RTB_registerPref function for the sake reverse-compatibility with v20.
	// Normally you wouldn't want to register prefs this way, please avoid using this as an example.
	// See here for the proper way to do this: https://forum.blockland.us/index.php?topic=320521

	//RTB_registerPref("Difficulty", "Render", "$Pref::Server::RenderDifficulty", "list Passive 0 Easy 25 Normal 100 Hard 400 Insane 1600", "Support_Render", $Pref::Server::RenderDifficulty, 0, 0);
	RTB_registerPref("Mode", "Render", "$Pref::Server::RenderDamageType", "list Normal 0 Damage 1 Tag 2 Haunt 3", "Support_Render", 0, 0, 0);
	// For spawnrate values, see also: registerOutputEvent (below), compat/slayer.cs
	RTB_registerPref("Spawn Rate", "Render", "$Pref::Server::RenderSpawnRate", "list Disabled 0 Low 1 Below_Normal 5 Normal 7 Above_Normal 12 High 14", "Support_Render", 7, 0, 0);
	RTB_registerPref("Shrine Range", "Render", "$Pref::Server::RenderShrineRange", "list 64x 28 48x 20 32x 12 16x 4 Disabled -1", "Support_Render", 28, 0, 0);
	RTB_registerPref("Shrines admin only", "Render", "$Pref::Server::RenderAdminShrines", "bool", "Support_Render", 0, 0, 0);
	RTB_registerPref("Affect bricks and lights", "Render", "$Pref::Server::RenderAllowBrickEffects", "bool", "Support_Render", 1, 0, 0);
	RTB_registerPref("Daytime spawning", "Render", "$Pref::Server::RenderDisableEnvSpawn", "bool", "Support_Render", 0, 0, 0);
	RTB_registerPref("Only spawn outdoors", "Render", "$Pref::Server::RenderDisableIndoorSpawn", "bool", "Support_Render", 0, 0, 0);
	RTB_registerPref("Disable ambient sounds", "Render", "$Pref::Server::RenderDisableAmbientSounds", "bool", "Support_Render", 0, 0, 0);
	RTB_registerPref("Disable lights", "Render", "$Pref::Server::RenderDisableLights", "bool", "Support_Render", 0, 0, 0);
	RTB_registerPref("Invincible", "Render", "$Pref::Server::RenderIsInvincible", "bool", "Support_Render", 0, 0, 0);
	// Blockland Glass/Support_Preferences hook
	// Bit of a hack. This re-registers the category so we can use a custom icon.
	if(isFunction("registerPreferenceAddon")) {
		registerPreferenceAddon("Support_Render", "Render", "skull_old");
	}
}
else
{
	//$Pref::Server::RenderDifficulty = 100;
	$Pref::Server::RenderDamageType = 0;
	$Pref::Server::RenderSpawnRate = 7;
	$Pref::Server::RenderShrineRange = 28;
	$Pref::Server::RenderAllowBrickEffects = 1;
}

// Spawnrate pref reverse compatibility
// This prevents the pref from working incorrectly if updating from an old version.
%rate = $Pref::Server::RenderSpawnRate;
if(%rate == 2 || %rate == 3 || %rate == 4 || %rate == 6)
{
	$Pref::Server::RenderSpawnRate = 7;
}

// Extra load check; these are the things that REALLY shouldn't run twice.
if(!$Render::LoadedB)
{
	// NOTE: If including Support_Render and Slayer in a game-mode, make sure to have Slayer load first.
	// Console errors may occur in game-modes.

	if(isObject(Slayer_PrefSO) || ( isFile("Add-Ons/GameMode_Slayer/server.cs") && $AddOn__GameMode_Slayer == 1 ))
		exec("./compat/slayer.cs");
}

////// # Particles # //////

// ## Detector Blink FX ## //
//datablock ParticleData(GlitchDetectorParticle)
//{
//	dragCoefficient		= 0.0;
//	windCoefficient		= 0.0;
//	gravityCoefficient	= 0.0;
//	inheritedVelFactor	= 0.0;
//	constantAcceleration	= 0.0;
//	lifetimeMS		= 200;
//	lifetimeVarianceMS	= 0;
//	spinSpeed		= 0.0;
//	spinRandomMin		= 0.0;
//	spinRandomMax		= 0.0;
//	useInvAlpha		= false;
//	animateTexture		= false;
//
//	textureName		= "base/data/particles/thinRing.png";
//
//	colors[0]	= "1 1 1 1";
//	colors[1]	= "1 1 1 0.0";
//
//	sizes[0]	= 0;
//	sizes[1]	= 0.5;
//
//	times[0]	= 0.0;
//	times[1]	= 1.0;
//};
//
//datablock ParticleEmitterData(GlitchDetectorEmitter)
//{
//   ejectionPeriodMS = 500;
//   periodVarianceMS = 0;
//
//   ejectionVelocity = 0.2;
//   velocityVariance = 0;
//
//   ejectionOffset = 0;
//
//   thetaMin         = 0.0;
//   thetaMax         = 0.0;
//
//   particles = GlitchDetectorParticle;
//};

// ## Face ## //
datablock ParticleData(RenderBoardParticle)
{
	dragCoefficient		= 0.0;
	windCoefficient		= 0.0;
	gravityCoefficient	= 0.0;
	inheritedVelFactor	= 0.0;
	constantAcceleration	= 0.0;
	lifetimeMS		= 200;
	lifetimeVarianceMS	= 0;
	spinSpeed		= 0.0;
	spinRandomMin		= 0.0;
	spinRandomMax		= 0.0;
	useInvAlpha		= true;
	animateTexture		= false;

	textureName		= "./render.png";

	colors[0]	= "1 1 1 0.5";
	colors[1]	= "1 1 1 0.0";

	sizes[0]	= 1;
	sizes[1]	= 1.5;

	times[0]	= 0.0;
	times[1]	= 1.0;
};

datablock ParticleEmitterData(RenderBoardEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 0;

   ejectionVelocity = 0.2;
   velocityVariance = 0.2;

   ejectionOffset = 0;

   thetaMin         = 0.0;
   thetaMax         = 90.0;

   particles = RenderBoardParticle;
};

// ## Damage ## //
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

// ### Level 1 (Base)
datablock ParticleEmitterData(RenderDmg1ExplosionEmitter)
{
	lifeTimeMS = 50;

	ejectionPeriodMS = 48;
	periodVarianceMS = 0;
	ejectionVelocity = 4;
	velocityVariance = 4;
	ejectionOffset   = 1.25;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance  = false;

	particles = "RenderDmgExplosionParticle";
};

datablock ExplosionData(RenderDmg1Explosion)
{
   //explosionShape = "";
   lifeTimeMS = 500;

   emitter[0] = RenderDmg1ExplosionEmitter;

   faceViewer     = true;
   explosionScale = "1 1 1";

   shakeCamera = true;
   camShakeFreq = "0.4 0.4 0.4";
   camShakeAmp = "0.4 0.4 0.4";
   camShakeDuration = 0.13;
   camShakeRadius = 1;

   // Dynamic light
   lightStartRadius = 0;
   lightEndRadius = 0;
   lightStartColor = "0 0 0";
   lightEndColor = "0 0 0";
};

datablock ProjectileData(RenderDmg1Projectile)
{
   projectileShapeName = "base/data/shapes/empty.dts";
   directDamage        = 0;
   directDamageType    = $DamageType::Default;
   radiusDamageType    = $DamageType::Default;

   brickExplosionImpact = false;

   impactImpulse	     = 000;
   verticalImpulse	  = 100;
   explosion           = RenderDmg1Explosion;
   bloodExplosion        = RenderDmg1Explosion;
   explodeOnPlayerImpact = true;
   explodeOnDeath        = true;

   muzzleVelocity      = 90;
   velInheritFactor    = 1;

   armingDelay         = 3000;
   lifetime            = 3000;
   fadeDelay           = 3500;
   bounceElasticity    = 0.99;
   bounceFriction      = 0.20;
   isBallistic         = true;
   gravityMod = 0.0;

   hasLight    = false;
   lightRadius = 3.0;
   lightColor  = "0 0 0.5";
};

// ### Level 2
datablock ParticleEmitterData(RenderDmg2ExplosionEmitter : RenderDmg1ExplosionEmitter)
{
		ejectionPeriodMS = 24;
};
datablock ExplosionData(RenderDmg2Explosion : RenderDmg1Explosion)
{
	emitter[0] = RenderDmg2ExplosionEmitter;
	camShakeAmp = "0.8 0.8 0.8";
};
datablock ProjectileData(RenderDmg2Projectile : RenderDmg1Projectile)
{
   explosion			=	RenderDmg2Explosion;
   bloodExplosion	=	RenderDmg2Explosion;
};

// ### Level 3
datablock ParticleEmitterData(RenderDmg3ExplosionEmitter : RenderDmg1ExplosionEmitter)
{
		ejectionPeriodMS = 12;
};
datablock ExplosionData(RenderDmg3Explosion : RenderDmg1Explosion)
{
	emitter[0] = RenderDmg3ExplosionEmitter;
	camShakeAmp = "1 1 1";
};
datablock ProjectileData(RenderDmg3Projectile : RenderDmg1Projectile)
{
   explosion			=	RenderDmg3Explosion;
   bloodExplosion	=	RenderDmg3Explosion;
};

// ### Level 4
datablock ParticleEmitterData(RenderDmg4ExplosionEmitter : RenderDmg1ExplosionEmitter)
{
		ejectionPeriodMS = 6;
};
datablock ExplosionData(RenderDmg4Explosion : RenderDmg1Explosion)
{
	emitter[0] = RenderDmg4ExplosionEmitter;
	camShakeAmp = "2 2 2";
};
datablock ProjectileData(RenderDmg4Projectile : RenderDmg1Projectile)
{
   explosion			=	RenderDmg4Explosion;
   bloodExplosion	=	RenderDmg4Explosion;
};

// ### Level 5
datablock ParticleEmitterData(RenderDmg5ExplosionEmitter : RenderDmg1ExplosionEmitter)
{
		ejectionPeriodMS = 3;
};
datablock ExplosionData(RenderDmg5Explosion : RenderDmg1Explosion)
{
	emitter[0] = RenderDmg5ExplosionEmitter;
	camShakeAmp = "3 3 3";
};
datablock ProjectileData(RenderDmg5Projectile : RenderDmg1Projectile)
{
   explosion			=	RenderDmg5Explosion;
   bloodExplosion	=	RenderDmg5Explosion;
};

// ### Level 6
datablock ParticleEmitterData(RenderDmg6ExplosionEmitter : RenderDmg1ExplosionEmitter)
{
		ejectionPeriodMS = 1;
};
datablock ExplosionData(RenderDmg6Explosion : RenderDmg1Explosion)
{
	emitter[0] = RenderDmg6ExplosionEmitter;
	camShakeAmp = "4 4 4";
};
datablock ProjectileData(RenderDmg6Projectile : RenderDmg1Projectile)
{
   explosion			=	RenderDmg6Explosion;
   bloodExplosion	=	RenderDmg6Explosion;
};

////// # Sounds # //////
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

datablock AudioProfile(renderAmb3)
{
   filename    = "./sound/rendercycle3.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(renderAmb4)
{
   filename    = "./sound/distant.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(renderMove)
{
   filename    = "./sound/entityMove.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(glitchFire)
{
   filename    = "./sound/glitchFire.wav";
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

datablock AudioProfile(rAttackG)
{
   filename    = "./sound/g.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(rGlitch)
{
   filename    = "./sound/glitch.wav";
   description = AudioClose3d;
   preload = true;
};

////// # Bricks # //////
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

//// ## Shrine String ## ////
$Render::C_ShrString[-1] = "NOTE: Shrines are disabled.";
$Render::C_ShrString[4] = "Shrine range: 16x";
$Render::C_ShrString[12] = "Shrine range: 32x";
$Render::C_ShrString[20] = "Shrine range: 48x";
$Render::C_ShrString[28] = "Shrine range: 64x";

////// # Playertype # //////
datablock PlayerData(PlayerRenderArmor : PlayerStandardArmor)
{
	magicWandImmunity = 1;
	maxDamage = 300; // Max health

	canJet = 0;

	//maxBackwardSpeed = 40;
	//maxForwardSpeed = 70;
	//maxSideSpeed = 60;

	uiName = "";
};

datablock PlayerData(PlayerRenderTagArmor : PlayerRenderArmor)
{
	maxDamage = 600;

	uiName = "";
};

function PlayerRenderArmor::onDisabled(%a, %render, %str)
{
	Render_OnDisabled(%render);

	Parent::onDisabled(%a, %render, %str);
}

// Same as above but with tag mode armor
function PlayerRenderTagArmor::onDisabled(%a, %render, %str)
{
	Render_OnDisabled(%render);
	Parent::onDisabled(%a, %render, %str);
}

function PlayerRenderTagArmor::onRemove(%a, %render)
{
	if(%render.freezeTarget)
		Render_UnfreezePlayer(%render.freezeTarget);

	Parent::onRemove(%a, %render);
}

function PlayerRenderArmor::onRemove(%a, %render)
{
	if(%render.freezeTarget)
		Render_UnfreezePlayer(%render.freezeTarget);

	Parent::onRemove(%a, %render);
}

////// # Damage Type # //////
AddDamageType("RenderDeath", '<bitmap:Add-Ons/Support_Render/CI_Render> %1', '%2 <bitmap:Add-Ons/Support_Render/CI_Render> %1', 0.5, 0);

////// # Functions # //////
// Death vehicle from Item_Skis was used as a reference for this
datablock PlayerData(RenderDeathArmor : PlayerStandardArmor)
{
	airControl = 0;

	canRide = 0;

	cameraMaxDist = 20;
	cameraVerticalOffset = 15;

	// Setting drag higher than 0.3 seems to cause the mount to go crazy in water
	drag = 0.3;

	isInvincible = 1;

	runForce = 10000;

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

////// # ITEMS //////
// ## Glitch Gun ## //
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
	eyeoffset = "0.7 1.2 -0.55";
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

// ## Detector ## //
datablock ItemData(GlitchDetector)
{
	category				= "Weapon";  // Mission editor category
	className				= "Weapon"; // For inventory system

	shapeFile				= "./models/detector.dts";
	rotate					= false;
	mass						= 1;
	density					= 0.2;
	elasticity			= 0.2;
	friction				= 0.6;
	emap						= true;

	uiName					= "Glitch Detector";
	iconName				= "./models/Icon_detector";
	doColorShift		= false;
	colorShiftColor	= "1.0 1.0 1.0 1.0";

	image						= GlitchDetectorImage;
	canDrop					= true;
};

datablock ShapeBaseImageData(GlitchDetectorImage)
{
   shapeFile = "./models/detector.dts";
   emap = true;

   mountPoint = 0;
   offset = "0 0 0";
   eyeoffset = "0.7 1.2 -0.75";
   rotation = eulerToMatrix( "0 0 0" );

   correctMuzzleVector = true;

   className = "WeaponImage";
   item = GlitchDetector;

   armReady = true;

   doColorShift = false;
   colorShiftColor = GlitchDetector.colorShiftColor;//"0.400 0.196 0 1.000";
};

////// # Death Board # //////
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
		scale = "0.05 24 16";
	};

	%light = new fxlight()
	{
		dataBlock = playerLight;
		enable = 1;
		iconsize = 1;
		position = "-4 0 -666";
	};

	missionCleanup.add(%obj);
	missionCleanup.add(%light);
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

////// # Events # //////
registerOutputEvent(Minigame, "setRenderMode", "list UseServerPreference -1 Normal 0 Damage 1 Tag 2 Haunt 3", 1);
registerOutputEvent(Minigame, "setRenderSpawnRate", "list UseServerPreference -1 Disabled 0 Low 1 Below_Normal 5 Normal 7 Above_Normal 12 High 14", 1);
registerOutputEvent(Minigame, "setRenderInvincibility", "list UseServerPreference -1 Disabled 0 Enabled 1", 1);
registerOutputEvent(fxDTSBrick, "setRDetectorLevel", "int 0 10 6", 1);
registerOutputEvent(fxDTSBrick, "incRDetectorLevel", "int 0 10 1", 1);
registerOutputEvent(fxDTSBrick, "decRDetectorLevel", "int 0 10 1", 1);

////// # Groups and Loops # //////
new simGroup(RenderBotGroup) {}; // Render bot group

new simGroup(RenderMiscGroup) {}; // Render object group

if(!isObject(BrickGroup_666))
{
	new SimGroup(BrickGroup_666)
	{
		client = -1;
		name = "BL_ID: 666";
		bl_id = 666;
	};
}

$Render::LoopBot = schedule(50,0,Render_Loop);
$Render::LoopSpawner = schedule(30000,0,Render_Spawn_Loop);
