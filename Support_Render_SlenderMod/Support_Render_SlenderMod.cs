package Support_Render_Slender
{
  function Render_onApplyAppearance(%obj)
  {
    // You can get the desired values for any appearance by doing the following:
    // 1. Start a game and place down a bot hole spawner
    // 2. Wrench it, and add the event: onBotSpawn -> Bot -> SetAppearance -> Custom. Leave the boxes blank and hit send
    // 3. Wrench it again and check the boxes
    // 4. Copy and paste the vales from the three boxes here, in order
    // Alternatively, if you know what you're doing, you can use node functions (hidenode, setnodecolor, etc.) as desired.

    %obj.setAppearance(6, "1 0 0 0 0 0 0 0 0 0 0 0 0 asciiTerror Mod-Suit",
                          "1.0 1.0 1.0 0.2 0.2 0.2 0.2 0.2 0.2 0.2 0.2 0.2 0.1 0.1 0.1 0.1 0.1 0.1 0.2 0.2 0.2",
                          "0.1 0.1 0.1 0.1 0.1 0.1 0.1 0.1 0.1 0.1 0.1 0.1 1.0 1.0 1.0 1.0 1.0 1.0");

    // Adjust the bot's scale. This line can be removed if you do not wish to do this.
    %obj.setPlayerScale("1 1 1.3");

    // IMPORTANT: Return true to let the mod know that we're customizing it.
    return true;

    // We don't need a parent call since only one of these modifiers should ever be enabled at once.
    // If more than one is enabled, the last one that loads will override the others.
  }
};

activatePackage("Support_Render_Slender");
