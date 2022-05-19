using System.Collections;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using Pixelplacement;
using System;
using DigitalRuby.LightningBolt;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Items
{
    [HarmonyPatch]
    public class ShockerItem : ConsumableItem
    {
        public static ConsumableItemData ItemData { get; private set; }

        private static readonly Vector3 BASE_POSITION = new(0f, 0.2f, 0f);

        static ShockerItem()
        {
            ItemData = ScriptableObject.CreateInstance<ConsumableItemData>();
            ItemData.name = $"{P03Plugin.CardPrefx}_Shocker";
            ItemData.placedSoundId = "metal_object_short";
            ItemData.examineSoundId = "metal_object_short";
            ItemData.pickupSoundId = "teslacoil_spark";
            ItemData.rulebookCategory = AbilityMetaCategory.Part3Rulebook;
            ItemData.rulebookName = "Amplification Coil";
            ItemData.regionSpecific = true;
            ItemData.rulebookDescription = "Increases your max energy. I suppose you can find some use for this.";
            ItemData.prefabId = "prefabs/specialnodesequences/teslacoil";

            ItemSlotPatches.KNOWN_ITEMS.Add(ItemData, FixGameObject);
        }

        public static ConsumableItem FixGameObject(GameObject obj)
        {
            Transform coil = obj.transform.Find("TeslaCoil(Clone)");
            coil.localPosition = BASE_POSITION;
            Renderer renderer = obj.transform.Find("TeslaCoil(Clone)/Base/Rod/rings_low").gameObject.GetComponent<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", GameColors.Instance.blue);

            GameObject.Destroy(obj.GetComponentInChildren<AutoRotate>());
            return obj.AddComponent<ShockerItem>();
        }

        public override bool ExtraActivationPrerequisitesMet()
        {
            return (ResourcesManager.Instance.PlayerMaxEnergy < 6 
                 || ResourcesManager.Instance.PlayerEnergy < ResourcesManager.Instance.PlayerMaxEnergy);
        }

        public override void OnExtraActivationPrerequisitesNotMet()
        {
            base.OnExtraActivationPrerequisitesNotMet();
            this.PlayShakeAnimation();
        }

        private Transform _coilTransform;
        private Transform CoilTransform => (_coilTransform ??= this.gameObject.transform.Find("TeslaCoil(Clone)"));

        public override IEnumerator ActivateSequence()
        {
            Vector3 target = this.CoilTransform.position + (Vector3.up * 11);
            Tween.Position(this.CoilTransform, target, 0.5f, 0f);
            this.PlayPickUpSound();
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default, false, true);
            yield return new WaitForSeconds(0.1f);
            this.CoilTransform.position = new Vector3(0f, 11f, 0f);
            yield return new WaitForEndOfFrame();
            Tween.Position(this.CoilTransform, new Vector3(0f, 5.3f, 0f), 0.3f, 0f, completeCallback:() => this.PlayPlacedSound());
            yield return new WaitForSeconds(0.3f);

            Renderer renderer = this.gameObject.transform.Find("TeslaCoil(Clone)/Base/Rod/ball_low").gameObject.GetComponent<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            Tween.ShaderColor(renderer.material, "_EmissionColor", GameColors.Instance.blue, 0.1f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);

            yield return new WaitForSeconds(0.5f);

            GameObject selfLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            selfLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            selfLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            selfLightning.GetComponent<LightningBoltScript>().EndObject = Camera.main.gameObject;

            GameObject resourceLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            resourceLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            resourceLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            resourceLightning.GetComponent<LightningBoltScript>().EndObject = ResourceDrone.Instance.gameObject;

            GameObject lifeLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            lifeLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            lifeLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            lifeLightning.GetComponent<LightningBoltScript>().EndObject = LifeManager.Instance.scales.gameObject;
            lifeLightning.GetComponent<LightningBoltScript>().EndPosition = Vector3.up * 2f;

            GameObject upLightning = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"), this.gameObject.transform.parent);
            upLightning.GetComponent<LightningBoltScript>().StartObject = this.CoilTransform.gameObject;
            upLightning.GetComponent<LightningBoltScript>().StartPosition = Vector3.up * 2f;
            upLightning.GetComponent<LightningBoltScript>().EndObject = this.CoilTransform.gameObject;
            upLightning.GetComponent<LightningBoltScript>().EndPosition = Vector3.up * 11f;

            selfLightning.SetActive(false);
            resourceLightning.SetActive(false);
            lifeLightning.SetActive(false);

            List<GameObject> lightnings = new List<GameObject>() { selfLightning, resourceLightning, lifeLightning };

            for (int i = 0; i < 30; i++)
            {
                lightnings[UnityEngine.Random.Range(0, 3)].SetActive(true);
                AudioController.Instance.PlaySound3D("teslacoil_spark", MixerGroup.TableObjectsSFX, selfLightning.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
                yield return new WaitForSeconds(0.1f);
                selfLightning.SetActive(false);
                resourceLightning.SetActive(false);
                lifeLightning.SetActive(false);
            }
            
            foreach (GameObject obj in lightnings)
                obj.SetActive(true);

            AudioController.Instance.PlaySound3D("teslacoil_spark", MixerGroup.TableObjectsSFX, selfLightning.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);

            yield return ResourcesManager.Instance.AddMaxEnergy(1);
            yield return ResourcesManager.Instance.AddEnergy(1);

            yield return new WaitForSeconds(0.5f);

            foreach (GameObject obj in lightnings)
                GameObject.Destroy(obj);

            GameObject.Destroy(upLightning);

            yield return new WaitForSeconds(0.15f);

            target = this.CoilTransform.position + (Vector3.up * 11);
            Tween.Position(this.CoilTransform, target, 1f, 0f);
            yield return new WaitForSeconds(1.1f);
        }
    }
}