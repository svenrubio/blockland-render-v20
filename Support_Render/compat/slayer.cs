forceRequiredAddOn("GameMode_Slayer");

new ScriptObject(Slayer_PrefSO : Slayer_DefaultPrefSO)
{
  category = "Render";
  title = "Mode";
  defaultValue = -1;
  permissionLevel = $Slayer::PermissionLevel["Any"];
  variable = "%mini.rMode";
  type = "list";
  list_items = "-1 Use Server Preference" NL "0 Static" NL "1 Damage" NL "2 Tag" NL "3 Haunt";
  guiTag = "Advanced";
};

new ScriptObject(Slayer_PrefSO : Slayer_DefaultPrefSO)
{
  category = "Render";
  title = "Spawn Rate";
  defaultValue = -1;
  permissionLevel = $Slayer::PermissionLevel["Any"];
  variable = "%mini.rSpawnRate";
  type = "list";
  list_items = "-1 Use Server Preference" NL "0 Disabled" NL "2 Low" NL "3 Below Normal" NL "4 Normal" NL "5 Above Normal" NL "6 High";
  guiTag = "Advanced";
};

new ScriptObject(Slayer_PrefSO : Slayer_DefaultPrefSO)
{
  category = "Render";
  title = "Invinciblity";
  defaultValue = -1;
  permissionLevel = $Slayer::PermissionLevel["Any"];
  variable = "%mini.rInvincible";
  type = "list";
  list_items = "-1 Use Server Preference" NL "1 Enabled" NL "0 Disabled";
  guiTag = "Advanced";
};

new ScriptObject(Slayer_PrefSO : Slayer_DefaultPrefSO)
{
  category = "Render";
  title = "Points - Kill Render";
  defaultValue = 0;
  permissionLevel = $Slayer::PermissionLevel["Any"];
  variable = "%mini.rPoints";
  type = "int";
  int_maxValue = 999;
  int_minValue = -999;
  guiTag = "Advanced";
};
