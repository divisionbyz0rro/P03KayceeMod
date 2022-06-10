# P03 Kaycee's Mod
## Version 2.0: The NPC Update

**Check out this super spicy 720p trailer made in a free video editor**

[![SUPER SPICY 720p TRAILER](https://img.youtube.com/vi/f68rs2rJ_jM/0.jpg)](https://www.youtube.com/watch?v=f68rs2rJ_jM)

If you enjoyed the energy-based robotic gameplay of Inscryption Act 3 and wished it was available as part of Kaycee's Mod, then this is the mod for you.

Installing this mod will give you the option to play against either Leshy or P03 when you start a new run. Selecting Leshy will give you the standard game you're used to, but selecting P03 will send you to the land of Botopia, where you will draft robotic cards (including all new cards created just for this mod), explore procedurally generated maps, fight off bounty hunters, and purchase upgrades with your hard-earned robobucks. And at the end of it all, P03 is waiting for you in an all-new boss fight.


## Feedback

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

If you've created a new custom challenge and you want it to be compatible with this mod, you need to set a flag named "P03" on the challenge using the Challenge Manager in API 2.4+


```c#
ChallengeManager.FullChallenge fch = ChallengeManager.AddSpecific(...)
fch.SetFlags("P03");
```

If the challenge does not have this flag set, it will always be locked when the player is setting up a P03 run.

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

**Kickass Final Boss Music**
- Purist, the Specter

## Requirements

- [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/)
- [API](https://inscryption.thunderstore.io/package/API_dev/API/)
- [All The Sigils](https://inscryption.thunderstore.io/package/AllTheSigils/All_The_Sigils/)

## Changelog 

<details>
<summary>Changelog</summary>

2.3.3
- There was a really silly bug in the audio import code - this should hopefully solve the modman compatibility issue.
- You'll know if the audio bug is fixed because this will be the last time you see me talk about it.
- If there's another mention of it, it means I didn't fix it this time either.

2.3.2
- Repackaged to hopefully fix modman compatibility issues

2.3.1
- Rewrote the audio importing code to not assume the location of the audio files.

2.3
- Added some kick-ass new music from Purist to the final boss fight
- Fixed an issue with holographic cards breaking due to floating point parsing in certain locales
- Rewrote the Gem Cycler ability to fix breaking portraits
- Fixed a couple of miscellaneous visual defects
- Fixed defect in transformer cards where cards eventually transformed permanently into Add3rs

2.2.8
- Prevent upgrades from being assigned to the lower level of the tower in Gaudy Gem Land
- Updated dialogue for some quests

2.2.7
- I goofed and screwed up the packaging for version 2.2.6. This will fix that.

2.2.6
- Changed how challenge compatibility works

2.2.4
- Prevent Too Easy and Donation quests from appearing in the final zone.
- Fixed the dialogue for the Lost Friend quest and buffed the reward for that quest.
- Added a small additional reward for the broken generator quest

2.2.3
- Balance tweaks to Recycle Angel
- Fixed visual issue where card slots would not properly reset their colors in certain situations.
- Tweaked the Gembound Ripper encounter
- Compatibility with API 2.4+

2.2.2
- Fixed the orange and green blessings to not work when the cards are in your hand.

2.2.1
- Fixed the interaction between Transform and Permadeath - cards should permadie even if they are on their opposite side when they die now
- Fixed the interaction between Transform and Build-A-Card - custom cards should no longer lose their attack/health when merged in a transform node
- Fixed the attack animation of REDACTED
- Fixed the REDACTED ability of REDCATED to actually do what it says
- Fixed the interaction of Guard Dog and REDACTED

2.2.0
- Fast travel between zones is less restricted - you can now continue exploring a region after you beat the boss. You still cannot travel back to a region you have cleared and left, however.
- Fixed a visual defect with Zombie Process
- Fixed a visual defect with ability icons on REDACTED
- Fixed a defect with board slots not properly resetting after REDACTED
- Updated artwork for Skeleton Master

2.1.4
- Fixed defect with Skeleton Master
- Fixed defect with CopyPasta (opposing slots are now properly selectable)
- Fixed defect with ability conduits erroneously duplicating existing abilities on cards
- Tweaked the Spyplane encounter

2.1.3
- Fixed Mr:Clock to show the correct state of the rotation when it enters the battlefield.
- Fixed a defect with trading cards with Transformer to REDACTED
- Made some more of the new abilities able to be acquired in Add Ability nodes

2.1.2
- Set the 'not randomly selectable' flag for the new custom items so that they won't be picked by the Pack Rat in Act 1
- Hopefully fixed issues with the 'ConduitGainAbility' manager
- Fixed the Radio and Power Tower quests to not accidentally give you duplicate copies of the quest cards.
- Replaced the art for Executor
- Tweaked the Wing Latcher encounter, the Mr:Clock encounter, and the Bombs and Shields encounter.

2.1.1
- Fixed defect with the GOLD!! quest

2.1
- Fixed defects with Mr:Clock, Oroboros, and Gem Cycler

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