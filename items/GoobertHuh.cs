using System;
using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    public class GoobertHuh : ConsumableItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        internal static Tuple<Color, string> GetGoobertRulebookDialogue()
        {
            if (StoryEventsData.EventCompleted(EventManagement.SAW_GOOBERT_AT_SHOP_NODE))
            {
                if (Part3SaveData.Data.items.Contains(ItemData.name))
                    return new (GameColors.Instance.brightLimeGreen, "Thank you! I hope he doesn't notice me here...");

                if (StoryEventsData.EventCompleted(EventManagement.FLUSHED_GOOBERT))
                    return new (GameColors.Instance.brightBlue, "You're a heartless bastard, aren't you?");

                if (StoryEventsData.EventCompleted(EventManagement.SAVED_GOOBERT))
                    return new (GameColors.Instance.brightLimeGreen, "Thank you!");

                if (StoryEventsData.EventCompleted(EventManagement.LOST_GOOBERT))
                    return new (GameColors.Instance.brightBlue, "I took care of him for you. You're welcome. I can't believe you wasted your money on that idiot.");
            }
            return new (GameColors.Instance.brightLimeGreen, "Please! You've got to help me get out of here!");
        }

        static GoobertHuh()
        {
            ItemData = ScriptableObject.CreateInstance<ConsumableItemData>();
            ItemData.name = $"{P03Plugin.CardPrefx}_GoobertHuh";
            ItemData.placedSoundId = "eyeball_drop_metal";
            ItemData.examineSoundId = "eyeball_squish";
            ItemData.pickupSoundId = "eyeball_squish";
            ItemData.rulebookCategory = AbilityMetaCategory.Part3Rulebook;
            ItemData.rulebookName = "Goobert";
            ItemData.regionSpecific = true;
            ItemData.rulebookDescription = "Please! You've got to help me get out of here!";
            ItemData.prefabId = "Prefabs/Items/GooBottleItem";
            ItemData.notRandomlyGiven = true;
            ItemSlotPatches.KNOWN_ITEMS.Add(ItemData, FixGameObject);
        }

        public static ConsumableItem FixGameObject(GameObject obj)
        {
            GameObject.Destroy(obj.GetComponentInChildren<GooBottleItem>());
            return obj.AddComponent<GoobertHuh>();
        }

        public override IEnumerator ActivateSequence()
        {
            ViewManager.Instance.SwitchToView(View.ConsumablesOnly, false, true);
            yield return new WaitForSeconds(0.2f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03GoobertAnnoyed", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            this.PlayShakeAnimation();
            yield return TextDisplayer.Instance.PlayDialogueEvent("GoobertConfused", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03GoobertShutUp", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.3f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            StoryEventsData.SetEventCompleted(EventManagement.LOST_GOOBERT);
            StoryEventsData.SetEventCompleted(EventManagement.HAS_NO_GOOBERT);
            this.PlayExitAnimation();
            yield break;
        }
    }
}