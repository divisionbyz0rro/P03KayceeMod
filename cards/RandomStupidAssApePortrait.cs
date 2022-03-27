using System.Collections.Generic;
using DiskCardGame;
using System.Linq;
using UnityEngine;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RandomStupidAssApePortrait : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        public class ApeAppearanceModification : CardModificationInfo
        {
            [SerializeField]
            public int[] spriteIndices;
        }

        public class RandomApePortrait : DynamicCardPortrait
        {
            public const int NUMBER_OF_CHOICES = 4;
            public const int NUMBER_OF_LAYERS = 7;

            private static Sprite[,] APE_SPRITES;

            private static string[] LAYER_NAMES = new string[] { "bg", "fur", "mouth", "eyes", "hats", "glasses", "shirts"};

            internal static void GenerateApeSprites()
            {
                APE_SPRITES = new Sprite[NUMBER_OF_LAYERS, NUMBER_OF_CHOICES];
                for (int i = 0; i < NUMBER_OF_LAYERS; i++)
                    for (int j = 0; j < NUMBER_OF_CHOICES; j++)
                        APE_SPRITES[i, j] = TextureHelper.GetImageAsTexture($"{LAYER_NAMES[i]} ({j+1}).png", typeof(RandomStupidAssApePortrait).Assembly).ConvertTexture(TextureHelper.SpriteType.CardPortrait, FilterMode.Trilinear);
            }

            public override void ApplyCardInfo(CardInfo card)
            {
                ApeAppearanceModification mod = card.Mods.Find(mod => mod is ApeAppearanceModification) as ApeAppearanceModification;
                if (mod == null)
                {
                    mod = new();
                    mod.spriteIndices = new int[NUMBER_OF_LAYERS];
                    UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
                    for (int i = 0; i < NUMBER_OF_LAYERS; i++)
                        mod.spriteIndices[i] = UnityEngine.Random.Range(0, NUMBER_OF_CHOICES);
                    card.Mods.Add(mod);
                }

                List<SpriteRenderer> renderers = this.gameObject.GetComponentsInChildren<SpriteRenderer>().ToList();
                SpriteRenderer myRenderer = this.gameObject.GetComponent<SpriteRenderer>();
                if (myRenderer != null && !renderers.Contains(myRenderer))
                    renderers.Add(myRenderer);
                P03Plugin.Log.LogInfo($"I found {renderers.Count} sprite renderers for apes");
                renderers.Sort((a, b) => a.sortingOrder - b.sortingOrder);
                
                for (int i = 0; i < renderers.Count; i++)
                    renderers[i].sprite = APE_SPRITES[i, mod.spriteIndices[i]];
            }
        }

        private static GameObject prefabPortrait = null;

        private static GameObject CloneSpecialPortrait()
        {
            CardInfo mole = CardLoader.GetCardByName("Mole_Telegrapher");
            GameObject myObj = GameObject.Instantiate(mole.AnimatedPortrait);
            myObj.AddComponent<RandomApePortrait>();

            // We need to make six more game objects with sprite renderers and increase the layer with each one
            SpriteRenderer spriteRenderer = myObj.GetComponentInChildren<SpriteRenderer>();
            GameObject cloneObj = null;
            for (int i = 0; i < RandomApePortrait.NUMBER_OF_LAYERS - 1; i++)
            {
                GameObject newLayer = GameObject.Instantiate(cloneObj ?? spriteRenderer.gameObject, spriteRenderer.gameObject.transform);
                newLayer.transform.localPosition = Vector3.zero;
                SpriteRenderer renderer = newLayer.GetComponent<SpriteRenderer>();
                renderer.sortingOrder = spriteRenderer.sortingOrder + 10 * (i + 1);

                cloneObj = cloneObj ?? newLayer;
            }

            return myObj;
        }

        public override void ApplyAppearance()
        {
            if (prefabPortrait == null)
                prefabPortrait = CloneSpecialPortrait();
               
            base.Card.RenderInfo.prefabPortrait = prefabPortrait;
            base.Card.RenderInfo.hidePortrait = true;
            base.Card.renderInfo.hiddenCost = true;
        }

        public static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "RandomStupidAssApePortrait", typeof(RandomStupidAssApePortrait)).Id;
        }
    }
}