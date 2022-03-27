using System.Collections;
using TMPro;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03KayceeRun.Faces
{
    [HarmonyPatch]
	public class P03LivesFace : ManagedBehaviour
	{
        public static GameObject P03LivesFaceObject { get; private set; }

        public static P03LivesFace Instance { get; private set; }

        public static readonly P03AnimationController.Face LivesFace = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "P03LivesFace");

		public IEnumerator ShowLives(int currentLives, int newLives)
		{
			this.UpdateText(currentLives);
			yield return new WaitForSeconds(0.4f);
			P03AnimationController.Instance.SetHeadBool("shuddering", true);
			float sign = Mathf.Sign((float)(newLives - currentLives));
			while (currentLives != newLives)
			{
				yield return new WaitForSeconds(0.6f);
				this.UpdateText(currentLives);
				currentLives += (int)(1f * sign);
				AudioController.Instance.PlaySound3D("robo_scale_tick", MixerGroup.TableObjectsSFX, P03AnimationController.Instance.HeadParent.position, 1f, 0f, new AudioParams.Pitch(1f + (float)(newLives - currentLives) * -0.01f), null, null, null, false);
			}
			P03AnimationController.Instance.SetHeadBool("shuddering", false);
			yield return new WaitForSeconds(0.1f);
			this.UpdateText(newLives);
			yield return new WaitForSeconds(0.6f);
			yield break;
		}

		private void UpdateText(int amount)
		{
			this.text.text = amount.ToString();
		}

		[SerializeField]
		private TextMeshPro text = null;

        private static List<GameObject> _faces;

        [HarmonyPatch(typeof(P03AnimationController), "Start")]
        [HarmonyPostfix]
        public static void CreateLivesFace(ref P03AnimationController __instance)
        {
            // Find all the faces
            P03FaceRenderer renderer = __instance.gameObject.GetComponentInChildren<P03FaceRenderer>();
            Traverse rendererTraverse = Traverse.Create(renderer);
            _faces = rendererTraverse.Field("faceObjects").GetValue<List<GameObject>>();

            // Clone the currency face
            GameObject currencyFace = _faces[(int)P03AnimationController.Face.Currency];
            P03LivesFaceObject = GameObject.Instantiate(currencyFace, currencyFace.transform.parent);

            // Remove the side icons
            foreach (Transform t in P03LivesFaceObject.transform)
                if (t.gameObject.name.StartsWith("scrolling"))
                    t.gameObject.SetActive(false);

            // Remove the currency controller
            P03CurrencyFace currencyController = P03LivesFaceObject.GetComponent<P03CurrencyFace>();
            Component.DestroyImmediate(currencyController);

            // Add the lives controller
            Instance = P03LivesFaceObject.AddComponent<P03LivesFace>();
            Instance.text = P03LivesFaceObject.transform.Find("CurrencyText").gameObject.GetComponent<TextMeshPro>();
            Instance.text.color = GameColors.Instance.red;

            // Replace the sprites
            foreach (SpriteRenderer sp in P03LivesFaceObject.GetComponentsInChildren<SpriteRenderer>())
            {
                sp.sprite = Sprite.Create(TextureHelper.GetImageAsTexture("p03_face_lives_coin.png", typeof(P03LivesFace).Assembly), new Rect(0f, 0f, 256f, 256f), new Vector2(0.5f, 0.5f));
                sp.color = GameColors.Instance.glowRed;
            }

            P03LivesFaceObject.SetActive(false);
        }

        [HarmonyPatch(typeof(P03FaceRenderer), "DisplayFace")]
        [HarmonyPrefix]
        public static bool DisplayLivesFace(ref GameObject __result, P03AnimationController.Face face)
        {
            P03LivesFaceObject.SetActive(false);
            if ((int)face == (int)LivesFace)
            {
                foreach (GameObject f in _faces)
                    f.SetActive(false);

                P03LivesFaceObject.SetActive(true);
                __result = P03LivesFaceObject;
                return false;
            }
            return true;
        }

        public static IEnumerator ShowChangeLives(int change, bool changeView = true)
		{
            P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
			if (changeView)
				ViewManager.Instance.SwitchToView(View.P03Face, false, true);
			
			yield return new WaitForSeconds(0.1f);
			P03AnimationController.Instance.SwitchToFace(LivesFace, true, true);
            int livesRemaining = EventManagement.NumberOfLivesRemaining;
			yield return Instance.ShowLives(livesRemaining, livesRemaining + change);

			if (changeView)
				Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
            
            P03AnimationController.Instance.SwitchToFace(currentFace, true, true);
			
			yield break;
		}
	}
}
