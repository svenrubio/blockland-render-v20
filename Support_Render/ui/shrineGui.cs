exec("./shrineGui.gui");

function shrineGui::loadDatablocks()
{
  %oldDB = shrineGui_Datablock.getText();
  shrineGui_Datablock.clear();

  %dbCount = getDataBlockGroupSize();
  for(%i = 0; %i < %dbCount; %i++)
  {
    %db = getDataBlock(%i);
    if(%db.uiName !$= "" && %db.getClassName() $= "PlayerData")
    {
      shrineGui_Datablock.add(%db.uiName, %db);
    }
  }
}

function shrineGui::send(%this)
{
  %db = shrineGui_Datablock.getSelected();
  commandToServer('Render', %db);

  canvas.popDialog(shrineGui);
}

function clientCmdOpenShrineDlg(%db)
{
  if(%db !$= "")
    shrineGui_Datablock.setSelected(%db.getId());

  canvas.pushDialog(shrineGui);
}

package renderClientPackage
{
  function clientCmdwrench_LoadMenus()
  {
    parent::clientCmdwrench_LoadMenus();
    shrineGui.loadDatablocks();
  }
};

activatePackage("renderClientPackage");
