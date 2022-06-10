using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Patchers;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
	public class TripleDiskCardAnimationController : DiskCardAnimationController
	{
        [HarmonyPatch(typeof(CardAnimationController3D), nameof(CardAnimationController3D.Awake))]
        [HarmonyPrefix]
        private static bool StopAwake(ref CardAnimationController3D __instance)
        {
            return __instance is not TripleDiskCardAnimationController;
        }

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.Expand))]
        [HarmonyPrefix]
		private static bool NewExpand(ref DiskCardAnimationController __instance, bool immediate = false)
		{
            if (__instance is TripleDiskCardAnimationController tdcac)
            {
                __instance.ShowScreenOnKeyframe();
                return false;
            }
            return true;
		}

		[HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.Contract))]
        [HarmonyPrefix]
		private static bool NewContract(ref DiskCardAnimationController __instance)
		{
            if (__instance is TripleDiskCardAnimationController)
            {
                throw new InvalidOperationException("A card with the TripleDiskCardAnimationController should never be asked to contract");
            }
            return true;
		}

        private List<TweenBase> activeTweens = new();
        private bool windupComplete = true;

        [HarmonyPatch(typeof(CardAnimationController), nameof(CardAnimationController.SetAnimationPaused))]
        [HarmonyPrefix]
        private static bool TweenAnimationPause(ref CardAnimationController __instance, bool paused)
        {
            if (__instance is TripleDiskCardAnimationController tdcac)
            {
                if (paused)
                    foreach(var item in tdcac.activeTweens)
                        item.Stop();
                if (!paused)
                    foreach (var item in tdcac.activeTweens)
                        item.Resume();
            }
            return true;
        }

        [HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.ShowWeaponAnim))]
        [HarmonyPrefix]
        private static bool DontShowWeaponAnim(ref DiskCardAnimationController __instance)
		{
			return __instance is not TripleDiskCardAnimationController;
		}

		[HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.HideWeaponAnim))]
        [HarmonyPrefix]
        private static bool DontHideWeaponAnim(ref DiskCardAnimationController __instance)
		{
			return __instance is not TripleDiskCardAnimationController;
		}

        private bool attacking = false;
        public override void PlayAttackAnimation(bool attackPlayer, CardSlot targetSlot)
        {
            this.DoingAttackAnimation = true;
            Vector3 targetSpot = targetSlot.transform.position;

            GameObject mushroomContainer = new GameObject("mushroomContainer");
            mushroomContainer.transform.SetParent(this.gameObject.transform);
            mushroomContainer.transform.localPosition = Vector3.zero;
            GameObject mushroom = GameObject.Instantiate(Resources.Load<GameObject>("prefabs/map/holomapscenery/HoloMushroom_1"), mushroomContainer.transform);
            OnboardDynamicHoloPortrait.HolofyGameObject(mushroom, GameColors.Instance.brightLimeGreen);
            mushroom.transform.localPosition = Vector3.zero;
            AudioController.Instance.PlaySound3D("mushroom_large_hit", MixerGroup.TableObjectsSFX, mushroom.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);

            Action callback = delegate() {
                this.DoingAttackAnimation = false;
                AudioController.Instance.PlaySound3D("small_mushroom_hit", MixerGroup.TableObjectsSFX, mushroom.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
                if (this.impactKeyframeCallback != null)
                    this.impactKeyframeCallback();
                GameObject.Destroy(mushroomContainer, 0.35f);
                activeTweens.Clear();
                attacking = false;
            };

            attacking = true;
            activeTweens.Add(Tween.LocalPosition(mushroom.transform, Vector3.up * 1.5f, 0.2f, 0f, Tween.EaseOut));
            activeTweens.Add(Tween.LocalPosition(mushroom.transform, Vector3.zero, 0.5f, 0.2f, Tween.EaseIn, completeCallback:callback));
            activeTweens.Add(Tween.LocalRotation(mushroom.transform, new Vector3(180f, 200f, 180f), .4f, 0f));
            activeTweens.Add(Tween.Position(mushroomContainer.transform, targetSpot, .4f, 0f));
        }
        
        private static IEnumerator UnrollWithWait(IEnumerator sequence, TripleDiskCardAnimationController controller)
        {
            while (sequence.MoveNext())
            {
                if (sequence.Current is IEnumerator ies)
                {
                    yield return UnrollWithWait(ies, controller);
                    continue;
                }

                if (sequence.Current is WaitForSeconds wfs)
                {
                    if (wfs.m_Seconds == 0.05f)
                    {
                        yield return new WaitUntil(() => !controller.attacking);
                    }
                    else
                    {
                        yield return sequence.Current;
                    }
                }
                else
                {
                    yield return sequence.Current;
                }   
            }
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static IEnumerator SmarterSlotAttackSlot(IEnumerator sequence, CombatPhaseManager __instance, CardSlot attackingSlot, CardSlot opposingSlot, float waitAfter = 0f)
        {
            // If we got this far and you're attacking the wrong part of a triple card, still replace it (happens on sniper)
            if (opposingSlot.SlotCoveredByTripleCard())
            {
                List<CardSlot> friendlySlots = BoardManager.Instance.opponentSlots;
                if (!friendlySlots.Contains(opposingSlot))
                    friendlySlots = BoardManager.Instance.playerSlots;

                CardSlot newAttackSlot = friendlySlots.FirstOrDefault(s => s.SlotHasTripleCard());
                if (newAttackSlot != null)
                {
                    yield return __instance.SlotAttackSlot(attackingSlot, newAttackSlot, waitAfter);
                    yield break;
                }
            }

            if (attackingSlot.Card == null || attackingSlot.Card.Anim is not TripleDiskCardAnimationController)
            {
                yield return sequence;
                yield break;
            }

            TripleDiskCardAnimationController controller = attackingSlot.Card.Anim as TripleDiskCardAnimationController;
            yield return UnrollWithWait(sequence, controller);
        }

		public override void PlayTransformAnimation()
		{
			AudioController.Instance.PlaySound3D("disk_card_transform", MixerGroup.CardPaperSFX, base.transform.position, 1f, 0f, null, null, null, null, false);
		}

		public override void PlayHitAnimation()
		{
			AudioController.Instance.PlaySound3D("disk_card_hit", MixerGroup.TableObjectsSFX, base.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Small), null, null, null, false);
			Singleton<TableVisualEffectsManager>.Instance.ThumpTable(0.075f);
			base.StartCoroutine(this.FlickerScreen(0.075f));
		}

		public override void PlayDeathAnimation(bool playSound = true)
		{
			if (playSound)
			{
				AudioController.Instance.PlaySound3D("disk_card_death", MixerGroup.CardPaperSFX, base.transform.position, 1f, 0f, null, null, null, null, false);
			}
			this.PlayHitAnimation();
			base.StopAllCoroutines();
            base.StartCoroutine(Die());
			this.ShowScreenOffKeyframe();
		}

        private List<Renderer> lastSetOff;
        private void SetRenderers(bool visible)
        {
            if (visible && lastSetOff != null)
            {
                foreach (Renderer r in lastSetOff)
                    r.enabled = true;
            }

            if (!visible)
            {
                lastSetOff = new();
                foreach (Renderer r in this.gameObject.GetComponentsInChildren<Renderer>())
                {
                    if (r.enabled)
                    {
                        lastSetOff.Add(r);
                        r.enabled = false;
                    }
                }
            }
        }

        private IEnumerator Die()
        {
            for (float dur = 0.3f; dur > 0f; dur -= 0.25f)
            {
                SetRenderers(false);
                yield return new WaitForSeconds(dur);
                SetRenderers(true);
                yield return new WaitForSeconds(dur);
            }
            SetRenderers(false);
            base.gameObject.transform.localPosition += Vector3.down * 3;
        }

		public override void PlayPermaDeathAnimation(bool playSound = true)
		{
			if (playSound)
			{
				AudioController.Instance.PlaySound3D("disk_card_overload", MixerGroup.CardPaperSFX, base.transform.position, 1f, 0.15f, null, null, null, null, false);
			}
			this.PlayHitAnimation();
			base.StopAllCoroutines();
            base.StartCoroutine(Die());
            base.gameObject.transform.localPosition += Vector3.down * 3;
		}

		public override void SetHovering(bool hovering)
		{
			this.transform.localPosition = hovering ? new(0f, 0f, -.1f) : new (0f, 0f, 0f);
		}

		public override void ExitBoard(float tweenLength, Vector3 destinationOffset)
		{
			base.StartCoroutine(this.ClearEffectsThenExit());
		}

		public override void SetCardRendererFlipped(bool flipped)
		{
			this.cardRenderer.transform.localRotation = Quaternion.Euler(0f, -180f, flipped ? -90f : 90f);
		}

		public override void NegationEffect(bool strong)
		{
			AudioController.Instance.PlaySound3D("disk_card_flicker", MixerGroup.CardPaperSFX, base.transform.position, 1f, 0f, null, null, null, null, false);
			base.StartCoroutine(this.FlickerScreen(0.05f));
		}

		[HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.ClearEffectsThenExit))]
        [HarmonyPostfix]
		private static IEnumerator ClearEffectsThenExit(IEnumerator sequence, DiskCardAnimationController __instance)
		{
            if (__instance is TripleDiskCardAnimationController tdcac)
            {
                yield return tdcac.ClearLatchAbility();
                tdcac.ShowScreenOffKeyframe();
                tdcac.gameObject.transform.localPosition += Vector3.down * 3;
                yield break;
            }
            yield return sequence;
		}

        public void InitializeWith(DiskCardAnimationController controller)
        {
            this.anim = controller.anim;
            this.statsLayer = controller.statsLayer;
            this.cracks = controller.cracks;
            this.weaponAnim = controller.weaponAnim;
            this.weaponRenderer = controller.weaponRenderer;
            this.weaponMeshFilter = controller.weaponMeshFilter;
            this.weaponMeshes = controller.weaponMeshes;
            this.weaponMeshOffsets = controller.weaponMeshOffsets;
            this.weaponScales = controller.weaponScales;
            this.weaponRotations = controller.weaponRotations;
            this.weaponMaterials = controller.weaponMaterials;
            this.redHologramMaterial = controller.redHologramMaterial;
            this.blueHologramMaterial = controller.blueHologramMaterial;
            this.renderersToHologram = controller.renderersToHologram;
            this.disableForHologram = controller.disableForHologram;
            this.holoPortraitParent = controller.holoPortraitParent;
            this.shieldRenderer = controller.shieldRenderer;
            this.latchModule = controller.latchModule;
            this.lightningParent = controller.lightningParent;
            this.fuseParent = controller.fuseParent;
            this.toHologramRenderersDefaultMats = controller.toHologramRenderersDefaultMats;
            this.unusedCracks = controller.unusedCracks;
            this.cardRenderer = controller.cardRenderer;
            this.impactKeyframeCallback = controller.impactKeyframeCallback;
            this.sacrificeHoveringMarker = controller.sacrificeHoveringMarker;
            this.sacrificeMarker = controller.sacrificeMarker;
            this.intendedRendererYPos = this.cardRenderer.transform.localPosition.y;

            // Lots of wonkiness happens with what I'm doing to make the triple card work
            // This solves a problem with the screen disappearing after blinking in and out
            Transform screen = this.gameObject.transform.Find("Anim/CardBase/ScreenFront");
            screen.localPosition = new Vector3(screen.localPosition.x, screen.localPosition.y, 0.01f);


            this.anim.enabled = false;
        }
	}
}
