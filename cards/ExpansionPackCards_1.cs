using HarmonyLib;
using InscryptionAPI.Card;
using DiskCardGame;
using InscryptionAPI.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    internal static class ExpansionPackCards_1
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


            // Rampager Latcher
            CardManager.New(EXP_1_PREFIX, "ConveyorLatcher", "Conveyor Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_conveyorlatcher.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:2)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.StrafeSwap, LatchRampage.AbilityID);

            // Rampager Latcher
            CardManager.New(EXP_1_PREFIX, "FlyingLatcher", "Sky Latcher", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_skylatcher.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:1)
                .SetRegionalP03Card(CardTemple.Undead)
                .AddAbilities(Ability.Flying, LatchFlying.AbilityID);


            // Ammo Bot
            CardManager.New(EXP_1_PREFIX, "AmmoBot", "AmmoBot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ammobot.png", typeof(ExpansionPackCards_1).Assembly))
                .SetCost(energyCost:3)
                .SetNeutralP03Card()
                .AddAbilities(FullyLoaded.AbilityID);
        }
    }
}