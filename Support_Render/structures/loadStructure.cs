// This whole thing is super hacky... let's just roll with it.
function Render_LoadStructure(%position, %structure, %override)
{
  // If something's loaded in the last 2 minutes, cancel to avoid issues.
  if(($LoadingBricks_StartTime+90000 >= getSimTime() || getWord(%position, 2) > 5) && !%override)
  {
    return;
  }

  if(BrickGroup_666.getGroup() != MainBrickGroup.getId())
    MainBrickGroup.add(BrickGroup_666);

  %loadoffsetOriginal = $LoadOffset;

  $loadoffset = getWord(%position, 0) SPC getWord(%position, 1) SPC 0;

  %fakeClient = new SimObject();
  %fakeClient.brickGroup = BrickGroup_666;

  $LoadingBricks_ColorMethod = 3;
  $LoadingBricks_DirName = "";
  $LoadingBricks_Ownership = 1;
  $LoadingBricks_Client = %fakeClient;
  $LoadingBricks_BrickGroup = BrickGroup_666;
  $LoadingBricks_Silent = true;

  %fakeClient.schedule(10000, 0, delete);

  $LoadingBricks_StartTime = getSimTime();

  calcSaveOffset();
  RenderLoadSaveFile_Start("Add-Ons/Support_Render/structures/structure" @ %structure @ ".bls");

  schedule(2000, 0, Render_ResetOffset, %loadoffsetOriginal);

  BrickGroup_666.schedule(getRandom(30000,120000), chainDeleteAll);
}

// Renderman: The Onset of the Reset of the Offset
function Render_ResetOffset(%loadoffsetOriginal)
{
  $LoadOffset = %loadoffsetOriginal;
}

// This is a butchered version of ServerLoadSaveFile_Start from the base game.
// We need to do this instead of using the original to remove the console logs and the temp.bls loading.
// Source: https://github.com/Electrk/bl-decompiled/blob/5ea86a1e4f71cb92799bdc1305ba062771c5867e/server/scripts/allGameScripts/loadBricks/startLoad.cs
function RenderLoadSaveFile_Start (%filename)
{
	if ($Game::MissionCleaningUp)
	{
		return;
	}

	$Server_LoadFileObj = new FileObject();

	if(isFile(%filename))
	{
		$Server_LoadFileObj.openForRead (%filename);
	}

	if($UINameTableCreated == 0)
	{
		createUINameTable();
	}

	$LastLoadedBrick = 0;

	$Load_failureCount = 0;
	$Load_brickCount = 0;


	$Server_LoadFileObj.readLine();
	%lineCount = $Server_LoadFileObj.readLine();

	for (%i = 0;  %i < %lineCount; %i++)
	{
		$Server_LoadFileObj.readLine();
	}

	if (isEventPending($LoadSaveFile_Tick_Schedule))
	{
		cancel ($LoadSaveFile_Tick_Schedule);
	}

	ServerLoadSaveFile_ProcessColorData();
	ServerLoadSaveFile_Tick();

	stopRaytracer();
}
