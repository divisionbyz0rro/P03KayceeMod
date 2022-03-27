using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RareDiscCardAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        private Color EmissionColor = GameColors.Instance.darkGold;

        private static readonly string[] GameObjectPaths = new string[]
        {
            "Anim/CardBase/Rails",
            "Anim/CardBase/Top",
            "Anim/CardBase/Bottom"
        };

        public override void ApplyAppearance()
        {
			foreach (string key in GameObjectPaths)
            {
                Transform tComp = this.gameObject.transform.Find(key);
                if (tComp != null && tComp.gameObject != null)
                {
                    GameObject component = tComp.gameObject;
                    MeshRenderer renderer = component.GetComponent<MeshRenderer>();
                    Material material = renderer.material;
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", EmissionColor * 0.5f);
                }
            }
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            this.ApplyAppearance();
        }

        public void ChangeColor(Color color)
        {
            this.EmissionColor = color;
            this.ApplyAppearance();
        }

        public static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "RareDiscAppearance", typeof(RareDiscCardAppearance)).Id;
        }
    }
}