# Randomizer 3

Randomizer 3 expands on previous versions of the Hollow Knight randomizer by allowing you to randomize more items than ever before and, for the first time, to randomize area or room transitions.
- Randomizer 3 requires SeanprCore.dll and Modding Api version 51 or greater to run. These are both automatically downloaded by the ModInstaller.

Details on all of the various settings follow:

## Restrictions

These settings control which difficult skips the randomizer may require. If you are not familiar with these skips, especially as they are used in speedrunning, you are advised to turn them off.

With these skips allowed, the player is advised to take care to not lock out of certain required pogos. Obtain:
- No more than 1 nail upgrade before claw or wings
- No more than 3 nail upgrades before claw

## Quality of Life

- Salubra Notches: automatically gives you each Salubra charm notch upon acquiring the required number of charms
- Lemm Sell All: sell all relics at once to Lemm by talking to him from the right
- Early Geo: start the game with 300 geo.
- Jiji Hints: trade a rancid egg for information on which areas contain which items. Note that this shop uses a broader area definition than that used for area rando
- Quirrel Hints: talk to Quirrel at various locations to receive hints. You may only receive hints from up to 3 locations per playthrough. Specifically:
	- Black Egg Temple: area locations for Dream Nail, Dream Gate, and Awoken Dream Nail
	- Lake of Unn: area location for first vertical movement item
	- Queen's Station: area location for first dash upgrade
	- Mantis Village: area location for Lumafly Lantern
	- City of Tears: area locations for Desolate Dive and Descending Dark
	- Deepnest: area location for second dash upgrade
	- Crystal Peak: area location for Crystal Heart
	- Fog Canyon: area location for Isma's Tear
	- Teacher's Archive:  encounter removed in randomizer
	- Blue Lake: area location for second vertical movement item
- 1.2.2.1 Levers: Restores the larger hitboxes of levers from past patches, allowing them to sometimes be hit from the other side. Note that not all levers were fixable. Lever skips are never required in logic.
## Randomization

These settings control which items are randomized. The first four settings are always on.
- Dreamers: Lurien, Monomon, Herrah, and World Sense. World Sense is the Black Egg Temple pickup to view your completion percentage
- Skills: all spells, nail arts, and movement abilities, excluding the Awoken Dream Nail, which is never randomized
- Charms
- Keys: all key objects, as well as King's Brand, Godtuner, and Collector's Map
- Mask Shards
- Vessel Fragments
- Pale Ore
- Charm Notches: all charm notches, except those sold by Salubra
- Geo Chests: all geo chests, except the one above Baldur Shell and those in the Junk Pit
- Rancid Eggs
- Relics: all wanderers journals, hallownest seals, king's idols, and arcane eggs.
- Maps
- Stags
- Grubs
- Whispering Roots

- Duplicate Major Items: adds second copies of several important items such as Mantis Claw, Mothwing Cloak etc. Collecting a second copy after the first has no additional effect
- Randomize Starting Items: begin the game with several items already in the inventory, including at least one vertical movement upgrade.
- Starting Location: you may select a starting location from the menu, or set it to be random. Some locations may be unavailable depending on your settings.
- Create spoiler log: creates a file in the save directory with all item/transition placements

## Additional features

There are three logs created in the save directory to help you with your playthrough.
- Tracker Log: this log continuously records item locations, transition connections, and hints as you discover them.
- Helper Log: this log computes which locations/transitions are accessible with your current equipment.
- Spoiler Log: this log lists the exact locations of every randomized item and/or transition.

The "cursed" option is a special hard mode for randomizer veterans. Features include:
- Masks, Vessels, Ore, Notches, Geo Chests, Eggs, and Relics are replaced by 1 geo pickups, if randomized.
- Shade Soul, Descending Dark, and Abyss Shriek are removed.
- Dreamers, Charms, Monarch Wings, and Mantis Claw are less likely to be placed as early progression items.

## Area/Room randomizer

- Area randomizer randomizes items and connections between areas, which are understood to be any region of the game with a name which appears as onscreen text, excluding dream areas, trams, and elevators.
- Room randomizer randomzies items and nearly every transition between different rooms. Not included are:
    - Warps of any kind, including those entering dream areas
	- Trams and elevators
	- Transitions within Godhome and the Shrine of Believers
	- The transitions leading to Sly's storeroom, Bretta's basement, or to any trial of the colosseum
- The Connected-Area Room randomizer works similarly to Room randomizer, with the additional constraint that it attempts to keep rooms from the same area connected, up to a certain extent, and not affecting single entrance rooms.
Also, note the following:
- Due to an imbalance in the number of left and right transitions, the Divine and Grimm tents are included in the randomization, but their vanilla entrances have been removed, and will not spawn in Dirtmouth
- The nightmare lantern must be lit to obtain Grimmchild
- Sly must be rescued to use his shop

## New game mechanics
- The lifeblood door in Abyss opens with a single lifeblood mask in item/area randomizer, or is always open in room randomizer.
- Collecting Grimmchild activates the Nightmare Lantern, and Grimmchild is given with the first 6 flames already collected.
- You can preview the items at Colosseum, Grey Mourner, and King Fragment by interaction.

## Known issues
- Using Benchwarp may cause some room changes to fail to occur. For example, occasionally Leg Eater will revert to a vanilla shop after warping. This can be fixed by exiting and reentering the room.