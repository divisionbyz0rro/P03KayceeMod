using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class InvisibleCard : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        public override void ApplyAppearance()
        {
            if (this.Card.Anim is DiskCardAnimationController)
            {
                foreach (Renderer renderer in this.gameObject.transform.Find("Anim").gameObject.GetComponentsInChildren<Renderer>())
                    renderer.enabled = false;
            }
        }

        public override void OnPreRenderCard()
        {
            base.OnPreRenderCard();
            ApplyAppearance();
        }

        static InvisibleCard()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "InvisibleCard", typeof(InvisibleCard)).Id;
        }
    }
}