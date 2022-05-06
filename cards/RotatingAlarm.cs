using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class RotatingAlarm : AbilityBehaviour, IPassiveAttackBuff
    {
        public enum AlarmState : int
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }

        private AlarmState CurrentState = AlarmState.Up;

        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private static Dictionary<AlarmState, Texture> Textures = new();

        static RotatingAlarm()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Alarm Clock";
            info.rulebookDescription = "The creature that this sigil is pointing to will gain +1 attack. The clock will turn at the beginning of the player's turn.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = true;
            info.flipYIfOpponent = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook };

            RotatingAlarm.AbilityID = AbilityManager.Add(
                P03Plugin.PluginGuid,
                info,
                typeof(RotatingAlarm),
                TextureHelper.GetImageAsTexture("ability_alarmup.png", typeof(RotatingAlarm).Assembly)
            ).Id;

            Textures.Add(AlarmState.Up, TextureHelper.GetImageAsTexture("ability_alarmup.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Left, TextureHelper.GetImageAsTexture("ability_alarmleft.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Right, TextureHelper.GetImageAsTexture("ability_alarmright.png", typeof(RotatingAlarm).Assembly));
            Textures.Add(AlarmState.Down, TextureHelper.GetImageAsTexture("ability_alarmdown.png", typeof(RotatingAlarm).Assembly));
        }

        public static AlarmState GetNextAbility(AlarmState current)
        {
            if (current == AlarmState.Up)
                return AlarmState.Right;
            if (current == AlarmState.Right)
                return AlarmState.Down;
            if (current == AlarmState.Down)
                return AlarmState.Left;
            return AlarmState.Up;
        }

        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return playerUpkeep != this.Card.OpponentCard;
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            ViewManager.Instance.SwitchToView(View.Board, false, false);
            yield return new WaitForSeconds(0.25f);
            AudioController.Instance.PlaySound3D("cuckoo_clock_open", MixerGroup.TableObjectsSFX, this.gameObject.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), null, null, null, false);
            yield return new WaitForSeconds(0.1f);
            this.CurrentState = GetNextAbility(this.CurrentState);
            this.Card.RenderInfo.OverrideAbilityIcon(AbilityID, Textures[this.CurrentState]);
	        this.Card.RenderCard();
            yield return new WaitForSeconds(0.3f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            yield break;
        }

        public int GetPassiveAttackBuff(PlayableCard target)
        {
            if (this.CurrentState == AlarmState.Up && target.Slot == this.Card.Slot.opposingSlot)
                return 1;
            if (this.CurrentState == AlarmState.Down && target == this.Card)
                return 1;
            if (this.CurrentState == AlarmState.Left && target.Slot == BoardManager.Instance.GetAdjacent(this.Card.Slot, true))
                return 1;
            if (this.CurrentState == AlarmState.Right && target.Slot == BoardManager.Instance.GetAdjacent(this.Card.Slot, false))
                return 1;
            return 0;
        }
    }
}