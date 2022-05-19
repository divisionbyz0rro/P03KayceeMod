using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class GeneratorOverloadNode : HoloMapNode
    {
        private void SetThingsActive()
        {
            bool generatorAlive = !StoryEventsData.EventCompleted(EventManagement.GENERATOR_FAILURE);
            
            foreach (GameObject obj in LivingGeneratorPieces)
                obj.SetActive(generatorAlive);

            foreach (GameObject obj in DeadGeneratorPieces)
                obj.SetActive(!generatorAlive);
        }

        public override void OnReturnToMap()
        {
            SetThingsActive();
            base.OnReturnToMap();
        }

        public override void OnSetActive(bool active)
        {
            SetThingsActive();
            base.OnSetActive(active);
        }

        public List<GameObject> LivingGeneratorPieces = new();
        public List<GameObject> DeadGeneratorPieces = new();
    }
}