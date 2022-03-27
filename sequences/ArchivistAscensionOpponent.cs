using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class ArchivistAscensionOpponent : ArchivistBossOpponent
    {
        public static readonly string[] filetypes = new string[] { "GB", "MB", "KB", "B" };
        public static readonly int[] damages = new int[] { 4,3,2,1 };

        [HarmonyPatch(typeof(ArchivistBossOpponent), nameof(ArchivistBossOpponent.DamageFileSequence))]
        [HarmonyPostfix]
        public static IEnumerator ReplaceFindFileSequence(IEnumerator sequence)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break; 
            }

            ViewManager.Instance.SwitchToView(View.Default, false, false);
			yield return new WaitForSeconds(0.1f);
            int index = EventManagement.CompletedZones.Count;
			int damage = damages[index];
            string prefabSuffix = filetypes[index];
			CustomCoroutine.WaitThenExecute(0.15f, delegate
			{
				AudioController.Instance.PlaySound3D("archivist_spawn_filecube", MixerGroup.TableObjectsSFX, LifeManager.Instance.Scales.transform.position, 1f, 0f, null, null, null, null, false);
			}, false);
			yield return LifeManager.Instance.ShowDamageSequence(damage, 1, false, 0.25f, ResourceBank.Get<GameObject>("Prefabs/Environment/ScaleWeights/Weight_DataFile_" + prefabSuffix), 0f, true);
			yield break;
        }

        public override IEnumerator IntroSequence(EncounterData encounter)
        {
            yield return base.IntroSequence(encounter);

            if (SaveFile.IsAscension)
            {

                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                int index = EventManagement.CompletedZones.Count;
                int damage = damages[index];

                yield return TextDisplayer.Instance.PlayDialogueEvent($"ArchivistLibrarianDamage{damage}", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(0.25f);
                ViewManager.Instance.SwitchToView(View.Default, false, false);
            }
        }

        private CardInfo GetPhaseTwoBlocker()
        {
            CardInfo info = CardLoader.GetCardByName("DeadTree");

            if (EventManagement.CompletedZones.Count >= 1)
                info.mods.Add(new CardModificationInfo(0,2));
            if (EventManagement.CompletedZones.Count >= 2)
                info.mods.Add(new CardModificationInfo(0,2));
            if (EventManagement.CompletedZones.Count == 3)
                info.mods.Add(new (Ability.Reach));
            
            return info;
        } 

        public override IEnumerator StartNewPhaseSequence()
        {
            if (!SaveFile.IsAscension)
            {
                yield return base.StartNewPhaseSequence();
                yield break;
            }

            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("ArchivistAscensionPhaseTwo", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Board);

            // Clear out the queue and the board
            yield return this.ClearQueue();
            yield return this.ClearBoard();

            yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName(CustomCards.VIRUS_SCANNER), BoardManager.Instance.OpponentSlotsCopy[0]);
            yield return new WaitForSeconds(0.25f);

            for (int i = 1; i < 5; i++)
            {
                yield return BoardManager.Instance.CreateCardInSlot(GetPhaseTwoBlocker(), BoardManager.Instance.OpponentSlotsCopy[i]);
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(0.75f);

            // Make room for cards if necessary
            if (BoardManager.Instance.PlayerSlotsCopy[4].Card != null)
            {
                // Find the last empty slot
                int slot = -1;
                for (int i = 0; i < 4; i++)
                    if (BoardManager.Instance.PlayerSlotsCopy[i].Card == null)
                        slot = i;

                if (slot >= 0)
                {
                    for (int i = slot + 1; i < 5; i++)
                    {
                        CardSlot targetSlot = BoardManager.Instance.PlayerSlotsCopy[i - 1];
                        PlayableCard card = BoardManager.Instance.PlayerSlotsCopy[i].Card;
                        yield return BoardManager.Instance.AssignCardToSlot(card, targetSlot);
                        yield return new WaitForSeconds(0.25f);
                    }
                }
                else
                {
                    yield return BoardManager.Instance.PlayerSlotsCopy[4].Card.Die(true, null, true); // sorry
                    yield return new WaitForSeconds(0.25f); 
                }
            }

            yield return new WaitForSeconds(0.75f);

            yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName(CustomCards.OLD_DATA), BoardManager.Instance.PlayerSlotsCopy[4]);
            yield return new WaitForSeconds(0.75f);

            yield return base.ReplaceBlueprint("ArchivistBossP2", false);
	        yield return new WaitForSeconds(0.25f);
        }
    }
}