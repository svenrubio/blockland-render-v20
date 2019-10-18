// Support_Render is required, so we need to load it
%error = ForceRequiredAddOn("Support_Render");

if(%error == $Error::AddOn_NotFound)
{
  // If the base mod doesn't exist, we can't continue
  error("ERROR: Support_Render_SlenderMod - required add-on Support_Render not found");
}
else
{
  // Run Support_Render_SlenderMod.cs
  exec("./Support_Render_SlenderMod.cs");
}
