using System;
using System.Collections;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Helpers;
using InscryptionAPI.Resource;

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

        public static GameObject GetGameObject()
        {
            GameObject gameObject = ShockerItem.GetBaseGameObject("Prefabs/Items/GooBottleItem", "GoobertBottle");
            GameObject.Destroy(gameObject.GetComponentInChildren<GooBottleItem>());
            gameObject.AddComponent<GoobertHuh>();
            return gameObject;
        }

        static GoobertHuh()
        {
            string prefabPathKey = "p03kayceemodgoobert";
            ResourceBankManager.Add(P03Plugin.PluginGuid, $"Prefabs/Items/{prefabPathKey}", GetGameObject());

            ItemData = ConsumableItemManager.New(
                P03Plugin.PluginGuid,
                "Goobert",
                "Please! You've got to help me get out of here!",
                TextureHelper.GetImageAsTexture("ability_full_of_oil.png", typeof(ShockerItem).Assembly), // TODO: get a proper texture so this can be used in Part 1 maybe?
                typeof(LifeItem),
                GetGameObject() // Make another copy for the manager
            ).SetAct3()
            .SetExamineSoundId("eyeball_squish")
            .SetPickupSoundId("eyeball_squish")
            .SetPlacedSoundId("eyeball_drop_metal")
            .SetRegionSpecific(true)
            .SetPrefabID(prefabPathKey)
            .SetNotRandomlyGiven(true);
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