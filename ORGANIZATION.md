# Mod structure

Hi! This is how I want to structure mods in this repo internally. It's a weird mix between DB/EE, SpiritR, SLR, TOverhaul, and Fables's organization based purely on vibes and aesthetics and whatnot.

Goals:
- Be consistent and intuitive to read at a glance.
- Group similar content close together.
- Be easy to organize as it expands.

## Folder naming and namespaces

Directory should match namespace, unless the directory's name is prefixed with an underscore. This allows us to minimize namespace elements while allowing more granular organization of systems and content at the folder level without increasing namespace complexity.

Example: a source file at `Content/CellularGrowth/NPCs` should have a namespace of `Content.CellularGrowth.NPCs`, however a source file located at `Content/CellularGrowth/NPCs/_Droplings` would still have the previous namespace.

**Limit namespace element count to 4** - `Dropling` in `SpaceEventMod.Content.CellularGrowth.NPCs.Dropling` is illegal and I will cry.

## Directories

`Assets`: Contains assets, such as sounds, images, shaders, 3d models, structures, localization, etc.

`Common`: Contains gameplay systems, base types, mechanics, mod players, worldgen, etc. Each sub-folder should be split into the content it functions alongside the most. Rule of thumb is this is for behavior implementations specific to the game.

`Content`: Contains all game content, as in all entities or ModTypes. This includes content specific ModPlayers and ModSystems. Files within should appear in the form of `Content/[ContentPiece/Theme]/[ContentType]/{SubType.}[MainClassName]{.PartialDescriptor}`, with the exception of when there is ONE major ModSystem, ModPlayer, or so on for the given content piece.

`Core`: Contains infrastructure, such as graphics and physics systems, commands, low-level helpers, audio and camera systems, etc. "Engine-ish" functionality that will power most of your mod. Rule of thumb is if you could reuse it outside of terraria then it belongs here.

`Properties`: launchSettings.json

`Utilities`: Contains utilities split by what references they utilize (/Xna/, /Terraria/, /TModLoader/).

## Content file structure

All content files must contain one cohesive piece of content. Since a lot of content is just boilerplate though, you should condense a lot of content into singular files (i.e. you should group an item with its held projectile, or a tile with its placed item). The goal is to avoid having to reference other namespaces for singular bits of content. Organize types from small to big, so that none are lost at the bottom. Consider splitting a class into a partial class if it exceeds 500 lines of code.

As mentioned earlier, content files should be named like `{SubType.}[MainClassName]{.PartialDescriptor}.cs`, where PartialDescriptor is only present if the main class is a partial and SubType elaborates a bit more on what type of content it is. (i.e. an accessory's file might be named Accessory.TheThing.cs). 

### Various subtypes

`Misc.*.cs` - cannot be sorted into any category cleanly (i.e. dialogue or an item with a very unique use).<br>
`LootPool.*.cs` - chest loot table.<br>
`Rarity.*.cs` - a `ModRarity`.<br>

`Biomes.*.cs` - a `ModBiome`.<br>
`Dusts.*.cs` - a `ModDust`.<br>
`Gores.*.cs` - a `ModGore`.<br>

`Buffs.*.cs` - a `ModBuff`.<br>
`Debuffs.*.cs` - a buff that is a debuff.<br>

`Items.*.cs` - a `ModItem`.<br>
`Accessories.*.cs` - an item thats an accessory<br>
`Ammo.*.cs` - any form of ranged ammunition.<br>
`Armors.*.cs` - an armor set containing 3 `ModItem`s and usually a `ModPlayer`.<br>
`Consumables.*.cs` - item that can be consumed.<br>
`Dyes.*.cs` - an item that is a dye.<br>
`Food.*.cs` - item that is a food.<br>
`Materials.*.cs` - item that is primarily a crafting material.<br>
`Mounts.*.cs` - item which is a mount.<br>
`Pets.*.cs` - a pet or a light pet.<br>
`Potions.*.cs` - item is a potion that gives a potion effect.<br>
`Tools.*.cs` - a tool (i.e. a pickaxe or something).<br>
`Vanity.*.cs` - a vanity item (i.e. a vanity accessory).<br>
`Weapons.*.cs` - a weapon.<br>

`NPCs.*.cs` - a `ModNPC`.<br>
`Critters.*.cs` - an npc thats a non-hostile creature.<br>
`Enemy.*.cs` - a hostile creature thats not a boss.<br>
`Town.*.cs` - town npc or town pet.<br>

`Projectiles.*.cs` - a `ModProjectile`.<br>
`Minions.*.cs` - a projectile thats a summon minion/an item thats a summon weapon.<br>

`Tiles.*.cs` - a `ModTile` or an item that places a tile.<br>
`Trees.*.cs` - a tile thats a `ModTree` or is just in general a tree.<br>

`Visuals.*.cs` - a visual effect of some kind (i.e. a `ScreenShaderData`).<br>
`Backgrounds.*.cs` - a background visual.<br>
`Foregrounds.*.cs` - a foreground visual.<br>
`Particles.*.cs` - represents a particle effect (not dust).<br>

(you are free to come up with your own btw)

## Member naming

Private fields should be named `_likeThis` (prefixed with an underscore, camel case) and constants should be named `LIKE_THIS`.