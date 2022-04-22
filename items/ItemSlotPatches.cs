using HarmonyLib;
using DiskCardGame;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    internal static class ItemSlotPatches
    {
        internal static Dictionary<ConsumableItemData, Func<GameObject, ConsumableItem>> KNOWN_ITEMS = new();

        [HarmonyPatch(typeof(ItemsUtil), nameof(ItemsUtil.AllConsumables), MethodType.Getter)]
        [HarmonyPostfix]
        private static void IncludeCustomItems(ref List<ConsumableItemData> __result)
        {
            __result.AddRange(KNOWN_ITEMS.Keys);
        }

        private static Vector3 PseudoGetSize(this GameObject obj)
        {
            float minX = 0, minY = 0, minZ = 0, maxX = 0, maxY = 0, maxZ = 0;
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                minX = Mathf.Min(minX, renderer.bounds.min.x);
                minY = Mathf.Min(minY, renderer.bounds.min.y);
                minZ = Mathf.Min(minZ, renderer.bounds.min.z);
                maxX = Mathf.Max(maxX, renderer.bounds.max.x);
                maxY = Mathf.Max(maxY, renderer.bounds.max.y);
                maxZ = Mathf.Max(maxZ, renderer.bounds.max.z);
            }
            return new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
        }

        private static ConsumableItem CreateObject(ConsumableItemData data, Transform parent)
        {
            GameObject gameObject = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("prefabs/items/bombremoteitem"), parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.name = data.name;

            GameObject tempObject = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>(data.PrefabId), gameObject.transform);

            if (tempObject.GetComponent<Animator>() != null)
                GameObject.Destroy(tempObject.GetComponent<Animator>());

            GameObject.Destroy(gameObject.GetComponent<BombRemoteItem>());
            GameObject.Destroy(gameObject.gameObject.transform.Find("BombRemote").gameObject);

            return KNOWN_ITEMS[data](gameObject);
        }

        [HarmonyPatch(typeof(ItemSlot), nameof(ItemSlot.CreateItem))]
        [HarmonyPrefix]
        private static bool CreateItem(ref ItemSlot __instance, ItemData data, bool skipDropAnimation)
        {
            if (data is not ConsumableItemData)
                return true;

            if (data != null && KNOWN_ITEMS.ContainsKey(data as ConsumableItemData))
            {
                if (__instance.Item != null)
                    GameObject.Destroy(__instance.Item.gameObject);

                __instance.Item = CreateObject(data as ConsumableItemData, __instance.transform);
                __instance.Item.SetData(data);

                if (skipDropAnimation)
                    __instance.Item.PlayEnterAnimation(true);
                
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ItemPage), nameof(ItemPage.FillPage))]
        [HarmonyPrefix]
        private static bool FillPage(ref ItemPage __instance, string headerText, params object[] otherArgs)
        {			
			ConsumableItemData consumableByName = ItemsUtil.GetConsumableByName(otherArgs[0] as string);

            if (SaveManager.SaveFile.IsPart3)
                __instance.descriptionTextMesh.color = Color.white;

            if (!KNOWN_ITEMS.ContainsKey(consumableByName))
                return true;

            if (__instance.headerTextMesh != null)
				__instance.headerTextMesh.text = headerText;

			if (__instance.itemModelParent != null)
			{
				if (__instance.itemModel != null)
				{
					GameObject.Destroy(__instance.itemModel);
				}
				__instance.itemModel = CreateObject(consumableByName, __instance.itemModelParent).gameObject;

                Animator anim = __instance.itemModel.GetComponentInChildren<Animator>();
                if (anim != null)
                    anim.enabled = false;

				__instance.itemModel.transform.localPosition = Vector3.zero;
				Transform[] componentsInChildren = __instance.itemModel.GetComponentsInChildren<Transform>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].gameObject.layer = __instance.itemModelParent.gameObject.layer;
				}
			}
			else
			{
				__instance.iconRenderer.sprite = consumableByName.rulebookSprite;
			}

            __instance.nameTextMesh.text = Localization.Translate(consumableByName.rulebookName);

            string text = RuleBookPage.ParseCardDefinition(consumableByName.rulebookDescription);
            if (consumableByName.name == GoobertHuh.ItemData.name)
            {
                Tuple<Color, string> goobertDialogue = GoobertHuh.GetGoobertRulebookDialogue();
                text = goobertDialogue.Item2;
                __instance.descriptionTextMesh.color = goobertDialogue.Item1;
            }

			string englishText;

			if (__instance.itemModel != null)
				englishText = Localization.Translate(text);
			else
				englishText = string.Format(Localization.Translate("To the user: {0}"), text);
			
			__instance.descriptionTextMesh.text = Localization.Translate(englishText);
            return false;
        }
    }
}