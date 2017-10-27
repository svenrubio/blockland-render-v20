![Face](https://github.com/LakeYS/blockland-render/blob/master/asciiTerror.png)

# Render
Renderman is a mysterious entity that haunts Blockland. Over the years, this entity has become increasingly intelligent... and increasingly deadly.

## What's New in the Halloween Update
- Visual effects have been overhauled
- New and improved sound effects
- (Experimental) You can now play as Render. Controllable Render spawns can be enabled in preferences.
- Partial compatibility with v20
- Tons of minor tweaks and bugfixes

## The Glitch Detector
![DetectorImg](https://raw.githubusercontent.com/LakeYS/blockland-render/master/Support_Render/models/Icon_detector.png)
This is the Glitch Detector. This tool will alert you to the presence of Render by measuring glitch energy. It has one weakness, however; Render can passively observe you without being detected.

## The Glitch Gun
This state-of-the-art device was invented during an experiment with glitch energy. Upon use, a glitch gun will repel Render, shattering the gun in the process. Not much else is known about this device or if there are other uses of the technology.

## Glitch Shrines
![Shrine Brick Image](https://raw.githubusercontent.com/LakeYS/blockland-render/master/Support_Render/Glitch%20Shrine.png)
The Glitch Shrine is a brick that you can place in your builds to passively repel Render. Glitch Shrines have the appearance of a Jack-o-lantern.

You can turn a shrine off by hitting it with the wrench and unchecking "Raycasting" in the menu. You can also do this automatically with events!

## Detector Bricks
![Detector Brick Image](https://raw.githubusercontent.com/LakeYS/blockland-render/master/1x1F.png)
These are special bricks that trigger **onRelay** when Render is nearby. You can use these to make automated alarm systems and other things for your builds.

## Preferences
- **Mode** - Changes Render's behavior. There are four modes to choose from: *Normal* (Render acts normal as always), *Damage* (Render drains players' health), *Tag* (Render kills on touch rather than on sight), and *Haunt* (Render is passive and does not attack.
- **Spawn Rate** - Adjust this to change how often Render spawns. Setting this to 'Disabled' will turn Render off.
- **Shrine Range** - This changes the area of effect for Glitch Shrines. Set this to 'Disabled' to turn shrines off entirely.
- **Affect bricks and lights** - Allows Render to flicker lights and place bricks.
- **Only spawn at night (Day cycle)** - With this enabled, Render will only spawn at night in day cycle environments. If using a normal environment, Render will continue to spawn like normal.
- **Disable Ambient Sounds** - Checking this turns off ambient sound effeccts.
- **Disable lights** - When checked, Render will not use lights.
- **Invincible** - Turn this on to make Render invincible. When turned off, weapons can be used against Render.
- **Transform chance at spawn** - The rate at which players will spawn as Render.

### Slayer Support
This add-on supports Slayer mini-games. The following preferences can be changed in a Slayer mini-game:
- Invincibility - Changes whether Render is invincible or not.
- Mode - Changes Render's mode.
- Player Transformation Rate - How often players will spawn as Render.
- Points for Killing Render - How many points are awarded for defeating Render with a weapon.
- Spawn Rate - Changes Render's spawn rate.
By default, these preferences are set to 'Use Server Preference', meaning that the server's configuration will apply as usual.

### Events
You can configure Render's behavior in a mini-game using events! The events only work if you are the mini-game owner and they ONLY affect the mini-game. Slayer is not required for them to work.
- MiniGame -> setRenderInvincibility
- MiniGame -> setRenderMode
- MiniGame -> setRenderSpawnRate

# Update History
v2.0 - The Halloween 2017 Update
v1.4 - Events and Interactivity
v1.3 - The Slayer Update
v1.2.1 - Mystery Update Reveal
v1.2 - The Mystery Update
v1.1 - Glitch Guns

# See also
- Flickering Player Light (Direct download): http://blockland.us/files/Light_Flickering_Player_Light.zip
- Flashlight: https://forum.blockland.us/index.php?topic=240727.0
- Prepper Script: https://forum.blockland.us/index.php?topic=178185.0
- Support_Prepper (Old): https://forum.blockland.us/index.php?topic=174563.0

*By LakeYS - http://LakeYS.net - Join the Discord: https://discord.gg/s3vCQba*


# Project Goal
The goal of this project is to create a polished survival horror experience that conforms to Blockland's non-linear gameplay style.

# Project Accomplishments
Here are a few things that have been successfully accomplished in this project.
- A lightweight node generator for spawning and movement that can, in many cases, detect and navigate around corners of players' builds.
- Original sound effects and ambience created specifically for the mod.
- A functioning set of "horror mechanics" that do not utilize jumpscares.
- Interactive items, events (scriptable elements), configuration, and brick types that allow for unique gameplay.
- Polished visual effects.
- Partial reverse-compatibility with older versions of Blockland
