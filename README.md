# P03 Kaycee's Mod
## Version 2.0: The NPC Update

**Check out this super spicy 720p trailer made in a free video editor**

[![SUPER SPICY 720p TRAILER](https://img.youtube.com/vi/f68rs2rJ_jM/0.jpg)](https://www.youtube.com/watch?v=f68rs2rJ_jM)

If you enjoyed the energy-based robotic gameplay of Inscryption Act 3 and wished it was available as part of Kaycee's Mod, then this is the mod for you.

Installing this mod will give you the option to play against either Leshy or P03 when you start a new run. Selecting Leshy will give you the standard game you're used to, but selecting P03 will send you to the land of Botopia, where you will draft robotic cards (including all new cards created just for this mod), explore procedurally generated maps, fight off bounty hunters, and purchase upgrades with your hard-earned robobucks. And at the end of it all, P03 is waiting for you in an all-new boss fight.


## Feedback

A handful of people have played this mod, but I expect the number of bugs discovered to grow pretty quickly now that it has been officially released!

Please feel free to give me feedback on the Inscryption modding discord - @divisionbyzorro

Or submit issues on GitHub: [P03 Issues List](https://github.com/divisionbyz0rro/P03KayceeMod/issues)

## How does this mod work?

When you install this mod, you will see two 'new run' options on the opening screen for Kaycee's Mod. You can either start a New Leshy Run (the default KCM experience) or a new P03 run (this mod's experience).

P03 runs differ from Leshy runs in a few significant ways:

1) You have more choice over how the maps play out:
    - You can fight through the four zones in any order you choose. 
    - You are not forced to use any map nodes that you don't want to. (Pro tip: you still need to add cards to your deck - matches last longer than you might be used to).
2) Most of your choices are based on how you manage your currency. You will find currency on the map, and you earn it by 'overkilling' P03 during battles. (Pro tip: you should deliberately overkill as much as possible if you want to build a strong of a deck as possible). Every upgrade you select will cost you robobucks. The only thing that's free is adding new cards to your deck.

There are also some changes from the way P03's gameplay worked the first time through:

1) Bosses have been updated. G0lly and the Archivist are the most significantly different; both of them have completely reworked second phases. 
2) There is a final boss fight against P03. He has...some thoughts about what you're doing.
3) Some events work differently (see below).
4) There are now NPCs that will give you quests with rewards for your run.
5) And there...might be some other hidden secrets as well.

The runs are still similar to Leshy in significant ways:

1) You will still start with a draft.
2) There are a number of starter decks that you can pick from to help guide your selections through the rest of the game.
3) You must complete four battles against enemies before facing the boss. Once you beat the boss, you're done with that zone and can't go back.
4) There are now rare cards available to you. You will be given a Rare Token for completing a boss, which you can spend at the draft node to get a rare for your deck.

### Events

Some events play the same in this mod as they did the first time you played through Botopia. However, some are different. Here's what you need to know before spending your robobucks on an event:

- **Build-a-Card**: This is mostly the same as before, but the ability pool has been modified. The Unkillable sigil is no longer able to be added to a build-a-card, and a few new abilities (such as conduits and cells) are added.
- **Gemify Cards**: This behaves the same as before.
- **Items**: You can buy an item like you would in Act 1. However, you can only get one item from each shop.
- **Overclock**: This is significantly different. As before, the overclocked card gets +1 attack. However, when an overclocked card dies, it is not just removed from your deck. It is replaced with an Exeskeleton with the same set of abilities as the original card. So if you overclock a Sniper and it dies, you will get an Exeskeleton with the Sniper sigil.
- **Recycle**: Instead of getting robobucks back for your recycled card, you get a draft token. Normal cards get you a standard Token, with all of the card's abilities imprinted on the token. Those abilities will transfer to the card you draft with it. Rare cards get you a Rare token, which can be exchanged for another rare card.
- **Transformer**: You now select two cards instead of one, and the transformation causes one card to transform into the other.

### NPCs

New in Version 2.0 - you will now encounter NPcs scattered throughout the map who will give you optional side quests that you can complete for some additional rewards!

### Challenges

Some challenges simply don't work in this context. Any challenge that doesn't work will be 'locked' and you won't be able to select it.

If you've created a new custom challenge and you want it to be compatible with this mod, I have come up with a way to do it - and apologies for this being a bit of a kludge. I want to make sure that you can make your challenge compatible without having to make this mod a dependency, so here's how you're going to do this.

Step one: Put this code in your plugin.cs file:

```c#
internal static string P03CompatibleChallengeList
{
    get { return ModdedSaveManager.SaveData.GetValue("zorro.inscryption.infiniscryption.p03kayceerun", "P03CompatibleChallenges"); }
    set { ModdedSaveManager.SaveData.SetValue("zorro.inscryption.infiniscryption.p03kayceerun", "P03CompatibleChallenges", value); }
}
```

This creates a static reference to a common variable which holds a list of all compatible P03 custom challenges.

Step two: add the following patch to your plugin somewhere:

```c#
[HarmonyPrefix, HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
private static void RegisterChallenges()
{
    P03CompatibleChallengeList += "," + MyFirstCustomChallengeID.ToString();
    P03CompatibleChallengeList += "," + MySecondCustomChallengeID.ToString();
}
```

This will update the value of that common variable to add each challenge ID to the list.

Once you've done this, your challenges will be unlocked when the player enters the P03 mod.

## Adding more cards to the pool

If you want to add more cards to the pool for use in this mod, you need to add one of five new unique metacategories to your cards. These metacategories control which regions the cards can appear in choice nodes. For example, a card that has the 'TechRegionCards' can only show up in the draft node at the hub of the map, or in choice nodes in the Tech region ("Resplendent Bastion").

All of these metacategories have this plugin's GUID: 'zorro.inscryption.infiniscryption.p03kayceerun'

The five metacategories are:

- **NeutralRegionCards**: For cards that can appear in any region.
- **WizardRegionCards**: For cards that should appear in the Wizard region (these cards should be gem-related)
- **TechRegionCards**: For cards that should appear in the Tech region (these cards should be conduit-related)
- **NatureRegionCards**: For cards that should appear in the Nature region (these cards should be beast/animal related)
- **UndeadRegionCards**: For cards that should appear in the Undead region (these cards should be death-related)

Note that this does not control which battles/encounters the cards appear in.

Here is an example of how to do this in code.

```c#
public static readonly CardMetaCategory WizardRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>("zorro.inscryption.infiniscryption.p03kayceerun", "WizardRegionCards");

CardInfo myCard = (...);
myCard.AddMetaCategories(WizardRegion);
```

## Modding encounters

Unfortunately, custom encounters and regions are not currently supported by this mod. Perhaps sometime in the future.

## Credits

While the full credits will play in-game when you win for the first time, I have to thank everyone that made this possible here as well:

**Principal Artist**: Makako

**Other Contributing Artists**
- Froenzi
- Answearing Machine
- Nevernamed

**Card Design, Balance, and Playtesting**
- TheGreenDigi
- Bitty45
- Jury
- Eye Fly
- Froenzi
- Atrum (Lin)
- Sire
- Sylvie
- Tresh

## Requirements

- [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/)
- [API](https://inscryption.thunderstore.io/package/API_dev/API/)
- [All The Sigils](https://inscryption.thunderstore.io/package/AllTheSigils/All_The_Sigils/)

## Changelog 

<details>
<summary>Changelog</summary>

2.0
- The NPC Update! Quests! New cards! A new boss maybe?!

1.1.3
- Okay - no more trying to be clever. The interoperability between this mod and the pack manager mod is now contained in a separate BepInEx plugin, so if it fails (because you don't have Pack Manager installed), nothing in the P03 plugin is affected.
- Hopefully. Finally. Fixed the starter deck bug.

1.1.2
- Some additional convenience code that I wrote apparently re-introduced an old bug where this mod chokes if the Pack Management mod is not also installed. I think I have that fixed now.

1.1.1
- Fixed defect where the new P03 items would show up in Leshy's runs

1.1.0
- Previous versions of this mod would leave your save file in a state where unloading the mod would permanently corrupt your save file. This has now been fixed. If you load up this version of the mod and take any action that would cause the game to save your file to the disk, your save file should now be able to handle this mod being uninstalled.
- Fixed an issue where sometimes the trading sequence would activate multiple times at once
- Replaced the Transformer event
- Added some new items and replaced some of the items that you start with.
- Replaced some of the temporary card portraits with kickass new art by Makako and Nevernamed
- Used some serious hacks to make the Deck Editor mod compatible with this mod
- Moved project to its own repo to make collaboration with other modders easier
- Fixed an issue where tranferring Transformer via the shredder made the recipient turn into an Add3r permanently.
- Filled the main challenge screen with a bunch of new challenges
- Random balance tweaks to some cards
- Rebalanced a lot of the encounters to be harder at higher difficulties, and hopefully fixed some issues with a couple of encounters being too difficult at low difficulties.
- Registered all of the custom metacategories created by this mod with the Pack Management mod to improve compatibility with cards that are missing 
- Preparing myself for the inevitable amount of bugs that will appear once I release this.


1.0.7
- Patch 1.0.5 broke Build-A-Card. This should fix it.

1.0.6
- Fixed defect where activating chapter select would corrupt your entire save file unrecoverably. Let's take a moment of silence for all of the save files that were lost over the past few days...

1.0.5
- Okay, *really* fixed the P03 starter deck/Leshy starter deck issue...I hope. I really hope.
- Prevent cards from being Skeleclocked more than once.
- Removed GainBattery from Build-A-Card
- Rebalanced the Energy Conduit, Thick Droid, and Automaton
- Fixed a defect with the rare card appearance and Leshy runs
- Build-A-Card now randomly selects a set of abilities for you to choose from
- Updated dependency to API 2.02

1.0.4
- Fixed it so that P03 no longer puts his starter decks over Leshy's when you have no starter decks unlocked and play a Leshy run.
- Fixed an incompatibility with the Pack Manager mod that caused the 'A Random Card Is Played' option in the Canvas boss fight to softlock the game. 

1.0.3
- Fixed defect where sometimes the RNG would generate a map that didn't have enough room to hold all possible nodes.

1.0.2
- Fixed defect where the game's internal data files were not loading correctly after being checked into and out of GIT.

1.0.1
- Properly created soft dependency on Pack Manager mod
- Properly handle what happens when you have no starter decks unlocked.

1.0
- Initial version.
</details>