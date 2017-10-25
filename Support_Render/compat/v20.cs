// This script allows Support_Render to be partially compatible with older versions of Blockland.

package RenderCompatPackage
{
  function Render_DoMount(%death,%p)
  {
    %death.setScale("1 1 1"); // This fixes frozen players becoming giants.
    Parent::Render_DoMount(%death,%p);
  }
};

deactivatePackage(RenderCompatPackage);
activatePackage(RenderCompatPackage);
