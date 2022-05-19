using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
	public class ConduitSpawnCrypto : ConduitSpawn
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        static ConduitSpawnCrypto()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Mine Cryptocurrency";
            info.rulebookDescription = "When part of a conduit, [creature] will generate cryptocurrency.";
            info.canStack = true;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            ConduitSpawnCrypto.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(ConduitSpawnCrypto),
                TextureHelper.GetImageAsTexture("ability_minecrypto.png", typeof(ConduitSpawnCrypto).Assembly)
            ).Id;
        }

		public override string GetSpawnCardId()
		{
			return CustomCards.GOLLYCOIN;
		}
	}
}
