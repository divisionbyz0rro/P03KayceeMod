using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class UnlockAscensionItemSequencer : SelectItemsSequencer
	{
        public static UnlockAscensionItemSequencer Instance {get; private set; }

        public override void Start()
        {
            if (this.slots == null)
            {
                GameObject slots = GameObject.Instantiate(SpecialNodeHandler.Instance.unlockItemSequencer.gameObject.transform.Find("ItemSlots").gameObject, this.gameObject.transform);
                GameObject centerSlot = slots.transform.Find("ItemSlot_Center").gameObject;
                GameObject leftSlot = GameObject.Instantiate(centerSlot, centerSlot.transform.parent);
                leftSlot.name = "ItemSlot_Left";
                leftSlot.transform.localPosition = new(-2.5f, 5f, -2.28f);
                GameObject rightSlot = GameObject.Instantiate(centerSlot, centerSlot.transform.parent);
                rightSlot.name = "ItemSlot_Right";
                rightSlot.transform.localPosition = new(2.5f, 5f, -2.28f);

                this.slots = new List<SelectableItemSlot>() {
                    leftSlot.GetComponent<SelectableItemSlot>(),
                    centerSlot.GetComponent<SelectableItemSlot>(),
                    rightSlot.GetComponent<SelectableItemSlot>()
                };

                foreach (SelectableItemSlot slot in this.slots)
                    if (slot.gameObject.GetComponent<AlternateInputInteractable>() == null)
                        slot.gameObject.AddComponent<AlternateInputInteractable>();

                this.slotsGamepadControl = slots.GetComponentInChildren<GamepadGridControl>();
            }
            base.Start();
        }

        [HarmonyPatch(typeof(HoloMapShopNode), nameof(HoloMapShopNode.CanAfford))]
        [HarmonyPrefix]
        public static bool CanotAffordItemsIfFull(ref bool __result, ref HoloMapShopNode __instance)
        {
            if (SaveFile.IsAscension && __instance.nodeToBuy.nodeType == UnlockAscensionItemNodeData.UnlockItemsAscension)
            {
                if (Part3SaveData.Data.items.Count >= P03AscensionSaveData.NumberOfItems)
                {
                    ItemsManager.Instance.ShakeConsumableSlots(0.1f);
                    __result = false;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool HandleAscensionItems(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            if (nodeData is UnlockAscensionItemNodeData)
            {
                if (UnlockAscensionItemSequencer.Instance == null)
                    UnlockAscensionItemSequencer.Instance = __instance.gameObject.AddComponent<UnlockAscensionItemSequencer>();
                
                SpecialNodeHandler.Instance.StartCoroutine(UnlockAscensionItemSequencer.Instance.SelectItem(nodeData as UnlockAscensionItemNodeData));
                return false;
            }
            return true;
        }

        private List<ConsumableItemData> GetItems()
        {
            int randomSeed = P03AscensionSaveData.RandomSeed;
            List<string> items = new() { "Battery", "ShieldGenerator", "BombRemote", "PocketWatch" };
            while (items.Count > 3)
                items.RemoveAt(SeededRandom.Range(0, items.Count, randomSeed++));

            return items.Select(ItemsUtil.GetConsumableByName).ToList();
        }

		public IEnumerator SelectItem(UnlockAscensionItemNodeData nodeData)
		{
			ViewManager.Instance.SwitchToView(View.Default, false, true);

			yield return new WaitForSeconds(0.1f);
            SelectableItemSlot selectedSlot = (SelectableItemSlot) null;
            List<ConsumableItemData> data = GetItems();
            
            foreach (SelectableItemSlot slot in this.slots)
            {
                ConsumableItemData item = data[this.slots.IndexOf(slot)];
                slot.gameObject.SetActive(true);
                slot.CreateItem(item, false);
                slot.CursorSelectStarted += i => selectedSlot = i as SelectableItemSlot;
                slot.CursorEntered += i => Singleton<OpponentAnimationController>.Instance.SetLookTarget(i.transform, Vector3.up * 2f);
                slot.GetComponent<AlternateInputInteractable>().AlternateSelectStarted = i => RuleBookController.Instance.OpenToItemPage(slot.Item.Data.name, true);
            
                yield return new WaitForSeconds(0.1f);
            }

            this.SetSlotCollidersEnabled(true);

            yield return new WaitUntil(() => selectedSlot != null);

            RuleBookController.Instance.SetShown(false);
            Part3SaveData.Data.items.Add(selectedSlot.Item.Data.name);

            this.DisableSlotsAndExitItems(selectedSlot);
            yield return new WaitForSeconds(0.2f);
            selectedSlot.Item.PlayExitAnimation();
            yield return new WaitForSeconds(0.1f);
            ItemsManager.Instance.UpdateItems();

            foreach (SelectableItemSlot slot in this.slots)
            {
                slot.ClearDelegates();
                slot.GetComponent<AlternateInputInteractable>().ClearDelegates();
            }

            this.SetSlotsActive(false);

            OpponentAnimationController.Instance.ClearLookTarget();

            foreach (SelectableItemSlot slot in this.slots)
                UnityEngine.Object.Destroy(slot.Item.gameObject);

            SaveManager.SaveToFile(false);

            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.TransitionToGameState(GameState.Map, null);
		}
	}
}