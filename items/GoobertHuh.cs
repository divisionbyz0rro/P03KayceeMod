using System.Collections;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Items
{
    public class GoobertHuh : ConsumableItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        static GoobertHuh()
        {
            ItemData = ScriptableObject.CreateInstance<ConsumableItemData>();
            ItemData.name = $"{P03Plugin.CardPrefx}_GoobertHuh";
            ItemData.placedSoundId = "eyeball_drop_metal";
            ItemData.examineSoundId = "eyeball_squish";
            ItemData.pickupSoundId = "eyeball_squish";
            ItemData.rulebookCategory = AbilityMetaCategory.Part3Rulebook;
            ItemData.rulebookName = "Goobert";
            ItemData.rulebookDescription = "Please! You've got to help me get out of here!";
            ItemData.prefabId = "Prefabs/Items/GooBottleItem";
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
            this.PlayExitAnimation();
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield break;
        }
    }
}