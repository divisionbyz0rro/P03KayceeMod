using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class ActiveIfComboStoryFlag : ActiveIfCondition
    {
        public override bool ConditionIsMet()
        {
            P03Plugin.Log.LogDebug($"Story event {StoryFlag} complete? {StoryEventsData.EventCompleted(StoryFlag)}.");
            P03Plugin.Log.LogDebug($"AntiStory event {AntiStoryFlag} complete? {StoryEventsData.EventCompleted(AntiStoryFlag)}.");
            bool response = ((int)StoryFlag <= (int)StoryEvent.NUM_EVENTS || StoryEventsData.EventCompleted(StoryFlag)) &&
                   ((int)AntiStoryFlag <= (int)StoryEvent.NUM_EVENTS || !StoryEventsData.EventCompleted(AntiStoryFlag));
            P03Plugin.Log.LogDebug($"Should be active? {response}");
            return response;
        }

        [SerializeField]
        public StoryEvent StoryFlag;

        [SerializeField]
        public StoryEvent AntiStoryFlag;
    }
}