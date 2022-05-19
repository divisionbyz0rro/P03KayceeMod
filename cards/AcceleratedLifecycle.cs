using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class AcceleratedLifecycle : ActivatedAbilityBehaviour
	{
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        internal static List<CardSlot> BuffedSlots = new();

        private bool ActivatedThisTurn = false;

        public override int EnergyCost => 2;

        static AcceleratedLifecycle()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Rapid Recycle";
            info.rulebookDescription = "Pay 2 Energy to choose another card you control to die and immediately respawn. You may only activate this ability once per turn.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.activated = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            AcceleratedLifecycle.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(AcceleratedLifecycle),
                TextureHelper.GetImageAsTexture("ability_lifecycle.png", typeof(AcceleratedLifecycle).Assembly)
            ).Id;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            ActivatedThisTurn = false;
            yield break;
        }

        public override bool CanActivate()
        {
            return GetValidTargets().Count > 0 && !ActivatedThisTurn;
        }

        private List<CardSlot> GetValidTargets()
        {
            List<CardSlot> retval = new();
            foreach (CardSlot slot in BoardManager.Instance.GetSlots(!this.Card.OpponentCard))
                if (slot.Card != null && slot.Card != this.Card)
                    retval.Add(slot);
            return retval;
        }

        public override IEnumerator Activate()
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.2f);
            yield return BoardManager.Instance.ChooseSlot(GetValidTargets(), true);
            if (BoardManager.Instance.LastSelectedSlot == null)
            {
                ViewManager.Instance.SwitchToView(View.Default, false, false);
                yield break;
            }
            CardSlot target = BoardManager.Instance.LastSelectedSlot;
            CardInfo targetInfo = target.Card.Info;
            yield return target.Card.Die(false, null, true);
            yield return BoardManager.Instance.CreateCardInSlot(targetInfo, target);
            ActivatedThisTurn = true;
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }
    }
}
