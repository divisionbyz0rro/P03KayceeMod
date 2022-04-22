using System.Collections;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using Pixelplacement;
using System;
using DigitalRuby.LightningBolt;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Patchers;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    public class LifeItem : ConsumableItem
    {
        public class LifeItemUglyHack : ManagedBehaviour
        {
            public override void ManagedUpdate()
            {
                base.ManagedUpdate();
                this.transform.localEulerAngles = Vector3.zero;
                this.transform.localPosition = new Vector3(0f, 0.322f, 0f);
            }
        }

        public static ConsumableItemData ItemData { get; private set; }
        private const string PREFAB = "Weight_DataFile_GB";

        static LifeItem()
        {
            ItemData = ScriptableObject.CreateInstance<ConsumableItemData>();
            ItemData.name = $"{P03Plugin.CardPrefx}_LifeCube";
            ItemData.placedSoundId = "metal_object_short";
            ItemData.examineSoundId = "metal_object_short";
            ItemData.pickupSoundId = "archivist_spawn_filecube";
            ItemData.regionSpecific = true;
            ItemData.rulebookCategory = AbilityMetaCategory.Part3Rulebook;
            ItemData.rulebookName = "Data Cube";
            ItemData.rulebookDescription = "Can be placed on the scales for some damage, if you're into that sort of thing.";
            ItemData.prefabId = $"Prefabs/Environment/ScaleWeights/{PREFAB}";
            ItemSlotPatches.KNOWN_ITEMS.Add(ItemData, FixGameObject);
        }

        public static ConsumableItem FixGameObject(GameObject obj)
        {
            GameObject.Destroy(obj.GetComponentInChildren<Rigidbody>());
            GameObject.Destroy(obj.GetComponentInChildren<Part3Weight>());
            
            Transform weight = obj.transform.Find($"{PREFAB}(Clone)");
            // weight.transform.localEulerAngles = Vector3.zero;
            // weight.transform.localPosition = new Vector3(0f, 0.322f + 0.322f + .1636f, 0f);
            weight.gameObject.AddComponent<LifeItemUglyHack>();

            weight.Find("Cube").gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            return obj.AddComponent<LifeItem>();
        }

        public override IEnumerator ActivateSequence()
        {
            base.PlayExitAnimation();
            yield return new WaitForSeconds(0.5f);
            yield return LifeManager.Instance.ShowDamageSequence(2, 1, false, 0f, ResourceBank.Get<GameObject>("Prefabs/Environment/ScaleWeights/Weight_DataFile_KB"), 0.1f);
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default);
            yield return EventManagement.SayDialogueOnce("P03AscensionLifeItem", EventManagement.USED_LIFE_ITEM);
            yield break;
        }
    }
}