using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class MirrorImage : AbilityBehaviour
	{
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        static MirrorImage()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Rubber Stamp";
            info.rulebookDescription = "Whenever you play [creature], it becomes a copy of another creature of your choosing. If this creature has other abilities, those abilities will be transferred (up to the maximum of 4).";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            MirrorImage.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(MirrorImage),
                TextureHelper.GetImageAsTexture("ability_copy.png", typeof(MirrorImage).Assembly)
            ).Id;
        }

        private static List<CardSlot> GetCopyableSlots()
        {
            List<CardSlot> possibles = new();
            foreach (var slot in BoardManager.Instance.playerSlots.Concat(BoardManager.Instance.opponentSlots))
                if (slot.Card != null)
                    possibles.Add(slot);
            
            return possibles;
        }

        public override bool RespondsToPlayFromHand() => GetCopyableSlots().Count > 0;

        public override IEnumerator OnPlayFromHand()
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.2f);

            

            CardSlot target = null;
            List<CardSlot> copyableSlots = GetCopyableSlots();

            if (copyableSlots.Count == 0)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("MirrorImageFail", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield break;
            }                

            yield return TextDisplayer.Instance.PlayDialogueEvent("MirrorImage", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            yield return BoardManager.Instance.ChooseTarget(
                copyableSlots,
                copyableSlots,
                slot => target = slot,
                null,
                null,
                null,
                CursorType.Target
            );

            CardInfo clone = CardLoader.Clone(target.Card.Info);
            foreach (CardModificationInfo mod in target.Card.Info.mods)
                clone.mods.Add(mod.Clone() as CardModificationInfo);

            foreach (CardModificationInfo mod in this.Card.Info.mods)
                if (clone.Abilities.Count < 4)
                    clone.mods.Add(mod.Clone() as CardModificationInfo);

            this.Card.SetInfo(clone);
            yield return new WaitForSeconds(0.2f);
        }
	}
}
