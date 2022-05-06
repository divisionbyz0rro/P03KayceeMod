using HarmonyLib;
using InscryptionAPI.Card;
using DiskCardGame;
using InscryptionAPI.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class ExpansionPackCards_1
    {
        internal const string EXP_1_PREFIX = "P03KCMXP1";

        static ExpansionPackCards_1()
        {
            // Wolfbeast
            CardInfo wolfBeast = CardManager.New(EXP_1_PREFIX, "WolfBeast", "B30WULF", 2, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_wolfbeast.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:6)
                .AddAbilities(Ability.Transformer, Ability.DoubleStrike);

            // Wolfbot
            CardInfo wolfBot = CardManager.New(EXP_1_PREFIX, "WolfBot", "B30WULF", 1, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_wolfbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:6)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Ability.Transformer);

            wolfBeast.SetEvolve(wolfBot, 1);
            wolfBot.SetEvolve(wolfBeast, 1);

            // Viperbeast
            CardInfo viperBeast = CardManager.New(EXP_1_PREFIX, "ViperBeast", "V1P3R", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_viperbeast.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .AddAbilities(Ability.Transformer, Ability.Deathtouch);

            // Viperbot
            CardInfo viperBot = CardManager.New(EXP_1_PREFIX, "ViperBot", "V1P3R", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_viperbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(Ability.Transformer);

            viperBeast.SetEvolve(viperBot, 1);
            viperBot.SetEvolve(viperBeast, 1);

            // Mantisbeast
            CardInfo mantisBeast = CardManager.New(EXP_1_PREFIX, "MantisBeast", "4S-M4NT-D3US", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mantisbeast.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .AddAbilities(Ability.Transformer, Ability.TriStrike);

            // Mantisbot
            CardInfo mantisBot = CardManager.New(EXP_1_PREFIX, "MantisBot", "4S-M4NT-D3US", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mantisbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .SetRare()
                .AddAbilities(Ability.Transformer);

            mantisBeast.SetEvolve(mantisBot, 1);
            mantisBot.SetEvolve(mantisBeast, 1);

            mantisBeast.temple = CardTemple.Tech;
            mantisBot.temple = CardTemple.Tech;

            // Seedbot
            CardManager.New(EXP_1_PREFIX, "SeedBot", "SeedBot", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_seedbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:1)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddAbilities(TreeStrafe.AbilityID);


            // Rampager Latcher
            CardManager.New(EXP_1_PREFIX, "ConveyorLatcher", "Conveyor Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conveyorlatcher.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.StrafeSwap, LatchRampage.AbilityID);

            // flying Latcher
            CardManager.New(EXP_1_PREFIX, "FlyingLatcher", "Sky Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skylatcher.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.Flying, LatchFlying.AbilityID);

            // W0om
            CardManager.New(EXP_1_PREFIX, "Worm", "W0rm", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_worm.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(LatchDeathLatch.AbilityID);

            // Mirror Tentacle
            CardManager.New(EXP_1_PREFIX, "MirrorTentacle", "4D4952524F52", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_mirrorsquid.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetNeutralP03Card()
                .SetStatIcon(SpecialStatIcon.Mirror);

            // Battery Tentacle
            CardManager.New(EXP_1_PREFIX, "BatteryTentacle", "42415454455259", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_batterysquid.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:2)
                .SetNeutralP03Card()
                .SetStatIcon(BatteryPower.AbilityID);

            // Salmon and beastmaster
            CardManager.New(EXP_1_PREFIX, "Salmon", "S4LM0N", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_salmon.png", typeof(ExpansionPackCards_1).Assembly));

            CardManager.New(EXP_1_PREFIX, "BeastMaster", "B3A5T M4ST3R", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_beastmaster.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .SetRegionalP03Card(CardTemple.Nature)
                .SetRare()
                .AddAbilities(SummonFamiliar.AbilityID);

            // Bull
            CardManager.New(EXP_1_PREFIX, "BuckingBull", "T4URU5", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_bull.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .SetRegionalP03Card(CardTemple.Nature)
                .SetRare()
                .AddAbilities(BuckWild.AbilityID);

            // Googlebot
            CardManager.New(EXP_1_PREFIX, "GoogleBot", "SearchBot", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_googlebot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:2)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(Ability.Tutor);

            // Ammo Bot
            CardManager.New(EXP_1_PREFIX, "AmmoBot", "AmmoBot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ammobot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetNeutralP03Card()
                .AddAbilities(FullyLoaded.AbilityID);

            // oil Bot
            CardManager.New(EXP_1_PREFIX, "OilJerry", "Oil Jerry", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_oil_jerry.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:2)
                .SetNeutralP03Card()
                .AddAbilities(FullOfOil.AbilityID);

            // Necrobot
            CardManager.New(EXP_1_PREFIX, "Necrobot", "Necronomaton", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_necrobot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(Necromancer.AbilityID);

            // Zombie Process
            CardInfo zombie = CardManager.New(EXP_1_PREFIX, "ZombieProcess", "Zombie Process", 2, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_zombieprocess.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(Ability.Brittle, Ability.IceCube);

            CardInfo gravestone = CardManager.New(EXP_1_PREFIX, "ZombieGravestone", "Zombie Process", 0, 2)
                .AddAppearances(CardAppearanceBehaviour.Appearance.HologramPortrait)
                .AddAbilities(Ability.PreventAttack, Ability.Evolve);

            zombie.SetIceCube(gravestone);
            gravestone.SetEvolve(zombie, 3);
            gravestone.holoPortraitPrefab = CardManager.BaseGameCards.CardByName("TombStone").holoPortraitPrefab;

            // Recycle Angel
            CardManager.New(EXP_1_PREFIX, "RoboAngel", "AngelBot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_recyclenangel.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .SetRegionalP03Card(CardTemple.Undead)
                .SetRare()
                .AddAbilities(AcceleratedLifecycle.AbilityID);

            // Conduit protector
            CardManager.New(EXP_1_PREFIX, "ConduitProtector", "Conduit Protector", 0, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conduitprotector.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitProtector.AbilityID);

            // Skelecell
            CardManager.New(EXP_1_PREFIX, "Skelecell", "Skel-E-Cell", 3, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skelecell.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(Ability.Brittle, CellUndying.AbilityID);

            // Flying Conduit
            CardManager.New(EXP_1_PREFIX, "ConduitFlying", "Airspace Conduit", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conduitflying.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitGainFlying.AbilityID);

            // Flying Conduit
            CardManager.New(EXP_1_PREFIX, "ConduitDebuffEnemy", "Foul Conduit", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conduitdebuffenemy.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:2)
                .SetRegionalP03Card(CardTemple.Tech)
                .AddAbilities(ConduitGainDebuffEnemy.AbilityID);

            // Skyplane
            CardManager.New(EXP_1_PREFIX, "Spyplane", "Spyplane", 3, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skyplane.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:6)
                .SetNeutralP03Card()
                .AddAbilities(Ability.Flying);

            // Executor
            CardManager.New(EXP_1_PREFIX, "Executor", "Executor", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_executor.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:6)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.Deathtouch);

            // Copy Pasta
            CardManager.New(EXP_1_PREFIX, "CopyPasta", "Copypasta", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_copypasta.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetNeutralP03Card()
                .SetRare()
                .AddAbilities(MirrorImage.AbilityID);

            // Frankenbot
            CardInfo frankenBot = CardManager.New(EXP_1_PREFIX, "FrankenBot", "FrankenCell", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_frankenbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetRegionalP03Card(CardTemple.Nature)
                .AddMetaCategories(CustomCards.TechRegion)
                .AddAbilities(CellEvolve.AbilityID);

            CardInfo frankenBeast = CardManager.New(EXP_1_PREFIX, "FrankenBeast", "FrankenCell", 3, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_frankenbeast.png", typeof(ExpansionPackCards_1).Assembly))
                .AddAbilities(CellDeEvolve.AbilityID, GuidManager.GetEnumValue<Ability>("extraVoid.inscryption.voidSigils", "Electric"));

            frankenBot.SetEvolve(frankenBeast, 1);
            frankenBeast.SetEvolve(frankenBot, 1);

            // Clock man
            CardManager.New(EXP_1_PREFIX, "Clockbot", "Mr:Clock", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_clockbot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetNeutralP03Card()
                .AddAbilities(RotatingAlarm.AbilityID);
            
            // Titans
            CardManager.New(EXP_1_PREFIX, "RubyTitan", "Ruby Titan", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ruby_titan.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:6)
                .AddDecal(
                    TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly),
                    TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly),
                    TextureHelper.GetImageAsTexture("decal_ruby_titan.png", typeof(CustomCards).Assembly)
                )
                .SetRegionalP03Card(CardTemple.Wizard)
                .SetRare()
                .AddAbilities(RubyPower.AbilityID);

            CardManager.New(EXP_1_PREFIX, "SapphireTitan", "Sapphire Titan", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_sapphire_titan.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:4)
                .AddDecal(
                    TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly),
                    TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly),
                    TextureHelper.GetImageAsTexture("decal_sapphire_titan.png", typeof(CustomCards).Assembly)
                )
                .SetRegionalP03Card(CardTemple.Wizard)
                .SetRare() 
                .AddAbilities(SapphirePower.AbilityID);

            CardManager.New(EXP_1_PREFIX, "EmeraldTitan", "Emerald Titan", 1, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_emerald_titan.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:5)
                .AddDecal(
                    TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly),
                    TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly),
                    TextureHelper.GetImageAsTexture("decal_emerald_titan.png", typeof(CustomCards).Assembly)
                )
                .SetRegionalP03Card(CardTemple.Wizard)
                .SetRare()
                .AddAbilities(EmeraldPower.AbilityID);
        }
    }
}