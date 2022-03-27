using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using System.Linq;
using Pixelplacement;
using HarmonyLib;
using DigitalRuby.LightningBolt;
using Infiniscryption.P03KayceeRun.Helpers;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class P03AscensionOpponent : Part3BossOpponent
    {
        public override string PreIntroDialogueId => "";

        public override string PostDefeatedDialogueId => "P03AscensionDefeated";

        private List<string> PhaseTwoWeirdCards = new () { "MantisGod", "Coyote", "Moose", "Grizzly", "FrankNStein", "Amalgam", "Adder" };

        private CardInfo PhaseTwoBlocker;

        private static readonly CardSlot CardSlotPrefab = ResourceBank.Get<CardSlot>("Prefabs/Cards/CardSlot_Part3");

		private static readonly HighlightedInteractable OpponentQueueSlotPrefab = ResourceBank.Get<HighlightedInteractable>("Prefabs/Cards/QueueSlot");

        private bool FasterEvents = false;

        private List<Color> slotColors;
        private List<Color> queueSlotColors;

        private void InitializeCards()
        {
            FasterEvents = StoryEventsData.EventCompleted(EventManagement.HAS_DEFEATED_P03);

            PhaseTwoBlocker = CardLoader.GetCardByName("MoleMan");

            int difficulty = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty);

            if (difficulty >= 1)
            {
                PhaseTwoWeirdCards.Remove("Coyote");
                PhaseTwoWeirdCards.Remove("Grizzly");
                PhaseTwoWeirdCards.Add("Shark");
                PhaseTwoWeirdCards.Add("Moose");
                PhaseTwoBlocker.mods.Add(new (Ability.DeathShield));
            }
            if (difficulty == 2)
            {
                PhaseTwoWeirdCards.Remove("FrankNStein");
                PhaseTwoWeirdCards.Remove("Adder");
                PhaseTwoWeirdCards.Add("Urayuli");
                PhaseTwoBlocker.mods.Add(new (Ability.Sharp));
            }
        }

        public override IEnumerator PreDefeatedSequence()
        {
            return base.PreDefeatedSequence();
        }

        [HarmonyPatch(typeof(BountyHunter), nameof(BountyHunter.OnDie))]
        [HarmonyPostfix]
        public static IEnumerator NoOuttroDuringBoss(IEnumerator sequence)
        {
            if (TurnManager.Instance.opponent is P03AscensionOpponent)
                yield break;

            yield return sequence;
        }

        private CardInfo GenerateCard(int turn)
        {
            if (this.NumLives == 3)
                return BountyHunterGenerator.GenerateCardInfo(BountyHunterGenerator.GenerateMod(turn, 5 * turn + 6));

            if (this.NumLives == 2)
            {
                int randomSeed = P03AscensionSaveData.RandomSeed + 100 * TurnManager.Instance.TurnNumber + BoardManager.Instance.opponentSlots.Where(s => BoardManager.Instance.GetCardQueuedForSlot(s) != null).Count();
                string cardName = PhaseTwoWeirdCards[SeededRandom.Range(0, PhaseTwoWeirdCards.Count, randomSeed)];
                return CardLoader.GetCardByName(cardName);
            }

            return null;
        }

        public override IEnumerator StartBattleSequence()
        {
            this.NumLives = 3;

            this.InitializeCards();

           	yield return new WaitForSeconds(1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03IntroductionToModding", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.P03FaceClose, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03IntroductionClose", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);

            yield return base.QueueCard(GenerateCard(0), BoardManager.Instance.OpponentSlotsCopy[2], true, true, true);
            yield return new WaitForSeconds(0.15f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseOne", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            
            yield return new WaitForSeconds(0.45f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        public override IEnumerator StartNewPhaseSequence()
        {
            if (this.NumLives == 2)
            {
                yield return new WaitForSeconds(1f);
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return this.ClearBoard();
                yield return this.ClearQueue();

                ViewManager.Instance.SwitchToView(View.Default, false, false);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Angry, true, true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseTwo", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseTwoInControl", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(1f);

                ViewManager.Instance.SwitchToView(View.Default, false, false);
                yield return new WaitForSeconds(0.15f);
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return BoardManager.Instance.CreateCardInSlot(PhaseTwoBlocker, BoardManager.Instance.OpponentSlotsCopy[2]);
                yield return new WaitForSeconds(0.75f);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseTwoWeirdCards", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(1f);

                if (!FasterEvents)
                {
                    ViewManager.Instance.SwitchToView(View.BoneTokens, false, false);
                    GameObject prefab = Resources.Load<GameObject>("prefabs/cardbattle/CardBattle").GetComponentInChildren<Part1ResourcesManager>().gameObject;
                    GameObject part1ResourceManager = GameObject.Instantiate(prefab, Part3ResourcesManager.Instance.gameObject.transform.parent);
                    WeirdManager = part1ResourceManager.GetComponent<Part1ResourcesManager>();
                    
                    yield return WeirdManager.AddBones(50);
                    yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseTwoBones", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                    
                    yield return new WaitForSeconds(1f);
                }

                ViewManager.Instance.SwitchToView(View.Board, false, false);

                foreach(CardSlot slot in BoardManager.Instance.OpponentSlotsCopy)
                {
                    if (slot.Card == null && slot.opposingSlot.Card != null)
                    {
                        yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("DeadTree"), slot);
                        yield return new WaitForSeconds(0.45f);
                    }
                }
                
                adjustedPlanP2 = new int[TurnManager.Instance.TurnNumber + MODDERS_PART_2.Length];
                for (int i = 0; i < MODDERS_PART_2.Length; i++)
                    adjustedPlanP2[TurnManager.Instance.TurnNumber + i] = MODDERS_PART_2[i];

                yield return new WaitForSeconds(0.35f);
                yield return QueueNewCards(true, true);
                yield return new WaitForSeconds(0.5f);

                ViewManager.Instance.SwitchToView(View.Default, false, false);       
                yield break;         
            }

            yield return PhaseThreeSequence();

        }

        [HarmonyPatch(typeof(ViewManager), nameof(ViewManager.GetViewInfo))]
        [HarmonyPostfix]
        public static void ChangeFOVForBoss(ref ViewInfo __result, View view)
        {
            if (view == View.Board || view == View.BoardCentered)
            {
                if (BoardManager.Instance != null && BoardManager.Instance.playerSlots != null && BoardManager.Instance.playerSlots.Count == 7)
                {
                    __result.fov = 63f;
                }
            }
        }

        private void FixOpposingSlots()
        {
            for (int i = 0; i < BoardManager.Instance.playerSlots.Count; i++)
            {
                if (BoardManager.Instance.playerSlots[i].opposingSlot == null)
                    BoardManager.Instance.playerSlots[i].opposingSlot = BoardManager.Instance.opponentSlots[i];

                if (BoardManager.Instance.opponentSlots[i].opposingSlot == null)
                    BoardManager.Instance.opponentSlots[i].opposingSlot = BoardManager.Instance.playerSlots[i];
            }
        }

        private float GetXPos(bool beginning, bool isOpponent, bool isQueue)
        {
            if (!isOpponent)
                return beginning ? BoardManager.Instance.playerSlots.First().transform.localPosition.x : BoardManager.Instance.playerSlots.Last().transform.localPosition.x;
            else if (isQueue)
                return beginning ? BoardManager.Instance.opponentQueueSlots.First().transform.localPosition.x : BoardManager.Instance.opponentQueueSlots.Last().transform.localPosition.x;
            else
                return beginning ? BoardManager.Instance.opponentSlots.First().transform.localPosition.x : BoardManager.Instance.opponentSlots.Last().transform.localPosition.x;
        }

        private IEnumerator CreateSlot(HighlightedInteractable prefab, bool beginning, Transform parent, bool isOpponent, bool isQueue)
        {
            HighlightedInteractable slot = (HighlightedInteractable)UnityEngine.Object.Instantiate(prefab, parent);
			string nameBase = isOpponent ? "OpponentSlot" : "Playerslot";
            nameBase += beginning ? "-1" : "5";
            slot.name = nameBase;

            float deltaX = BoardManager.Instance.playerSlots[1].transform.localPosition.x - BoardManager.Instance.playerSlots[0].transform.localPosition.x;

            float xPos = beginning ? GetXPos(beginning, isOpponent, isQueue) - deltaX : GetXPos(beginning, isOpponent, isQueue) + deltaX;

            Vector3 refVec = !isOpponent ? BoardManager.Instance.playerSlots[0].transform.localPosition : isQueue ? BoardManager.Instance.opponentQueueSlots[0].transform.localPosition : BoardManager.Instance.opponentSlots[0].transform.localPosition;

			slot.transform.localPosition = new Vector3(xPos, refVec.y, refVec.z);

            if (isQueue)
            {
                if (beginning) BoardManager.Instance.opponentQueueSlots.Insert(0, slot);
                else BoardManager.Instance.opponentQueueSlots.Add(slot);
            }
            else 
            {
                if (isOpponent)
                {
                    Transform quad = slot.transform.Find("Quad");
				    quad.rotation = UnityEngine.Quaternion.Euler(90f, 180f, 0f);
                }

                List<CardSlot> slots = isOpponent ? BoardManager.Instance.opponentSlots : BoardManager.Instance.playerSlots;
                if (beginning) slots.Insert(0, slot as CardSlot);
                else slots.Add(slot as CardSlot);
            }

            BoardManager.Instance.allSlots = null;
            List<CardSlot> dummy = BoardManager.Instance.AllSlots; // Force the boardmanager to reset its list of slots

            if (isQueue)
                slot.SetColors(queueSlotColors[0], queueSlotColors[1], queueSlotColors[2]);
            else
                slot.SetColors(slotColors[0], slotColors[1], slotColors[2]);

            if (FasterEvents)
            {
                slot.OnCursorEnter();
                yield return new WaitForSeconds(0.05f);    
                slot.OnCursorExit();
                yield break;
            }

            GameObject lightning = Object.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/LightningBolt"));
            lightning.GetComponent<LightningBoltScript>().EndObject = slot.gameObject;
            Object.Destroy(lightning, 0.65f);
            slot.OnCursorEnter();
            yield return new WaitForSeconds(0.95f);
            slot.OnCursorExit();
            yield break;
        }

        private IEnumerator PhaseThreeSequence()
        {
            // Phase three
            yield return this.ClearQueue();
            yield return this.ClearBoard();

            OpponentAnimationController.Instance.ClearLookTarget();

            yield return new WaitForSeconds(1f);

            if (WeirdManager != null)
            {
                yield return WeirdManager.SpendBones(WeirdManager.PlayerBones);
                yield return new WaitForSeconds(0.5f);
                GameObject.Destroy(WeirdManager.gameObject, 0.25f);
                WeirdManager = null;
            }
            ViewManager.Instance.SwitchToView(View.Default, false, false);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Angry, true, true);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseThree", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            List<AudioHelper.AudioState> audioState = AudioHelper.PauseAllLoops();

            yield return new WaitForSeconds(FasterEvents ? 0.6f : 1.5f);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Happy, true, true);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseThreeStartShowingOff", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking, true, true);
            yield return new WaitForSeconds(FasterEvents ? 1f : 2f);
            PhaseTwoEffects();
            yield return new WaitForSeconds(FasterEvents ? 1f : 2f);

            float durationOfEffect = FasterEvents ? 3f: 6.5f;

            CameraEffects.Instance.Shake(0.05f, 100f); // Essentially just shake forever; I'll manually stop the shake later
            AudioSource source = AudioController.Instance.PlaySound2D("glitch_escalation", MixerGroup.TableObjectsSFX, volume: 0.4f);

            // Tween each of the four things that need to move
            Transform itemTrans = ItemsManager.Instance.gameObject.transform;
            Vector3 newItemPos = new Vector3(6.75f, itemTrans.localPosition.y, itemTrans.localPosition.z);
            Tween.LocalPosition(itemTrans, newItemPos, durationOfEffect, 0f);

            Transform hammerTrans = ItemsManager.Instance.Slots.FirstOrDefault(s => s.name.ToLowerInvariant().StartsWith("hammer")).gameObject.transform;
            Vector3 newHammerPos = new Vector3(-9.5f, hammerTrans.localPosition.y, hammerTrans.localPosition.z);
            Tween.LocalPosition(hammerTrans, newHammerPos, durationOfEffect, 0f);

            Transform bellTrans = (BoardManager.Instance as BoardManager3D).bell.gameObject.transform;
            Vector3 newBellPos = new Vector3(-5f, bellTrans.localPosition.y, bellTrans.localPosition.z);
            Tween.LocalPosition(bellTrans, newBellPos, durationOfEffect, 0f);

            Transform scaleTrans = LifeManager.Instance.Scales3D.gameObject.transform;
            Vector3 newScalePos = new Vector3(-6, scaleTrans.localPosition.y, scaleTrans.localPosition.z);
            Tween.LocalPosition(scaleTrans, newScalePos, durationOfEffect, 0f);
            yield return new WaitForSeconds(durationOfEffect);

            // Create two new slots
            Transform playerSlots = BoardManager3D.Instance.gameObject.transform.Find("PlayerSlots");
            Transform opponentSlots = BoardManager3D.Instance.gameObject.transform.Find("OpponentSlots");
            
            yield return CreateSlot(CardSlotPrefab, true, playerSlots, false, false);
            yield return CreateSlot(CardSlotPrefab, true, opponentSlots, true, false);
            yield return CreateSlot(OpponentQueueSlotPrefab, true, opponentSlots, true, true);
            yield return CreateSlot(CardSlotPrefab, false, playerSlots, false, false);
            yield return CreateSlot(CardSlotPrefab, false, opponentSlots, true, false);
            yield return CreateSlot(OpponentQueueSlotPrefab, false, opponentSlots, true, true);

            FixOpposingSlots();

            CameraEffects.Instance.StopShake();
            AudioController.Instance.FadeSourceVolume(source, 0f, 1f);
            yield return new WaitForSeconds(1f);
            source.Stop();
            yield return new WaitForSeconds(1f);

            AudioHelper.ResumeAllLoops(audioState);
            yield return new WaitForSeconds(1f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseThreeBehold", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            ViewManager.Instance.SwitchToView(View.Board, false, false);

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseThreeSevenSlots1", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            // We're guaranteed that two lanes will be empty so this is going to work for sure
            foreach (CardSlot slot in BoardManager.Instance.playerSlots.Where(s => s.Card != null))
            {
                yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("BrokenBot"), slot.opposingSlot);
                yield return new WaitForSeconds(0.66f);
            }
            yield return new WaitForSeconds(0.33f);

            CardInfo firewallA = CardLoader.GetCardByName(CustomCards.FIREWALL);
            firewallA.mods.Add(new(Ability.GuardDog));
            firewallA.mods.Add(new(Ability.DeathShield));
            yield return BoardManager.Instance.CreateCardInSlot(firewallA, BoardManager.Instance.opponentSlots[0]);
            yield return new WaitForSeconds(0.66f);

            CardInfo firewallB = CardLoader.GetCardByName(CustomCards.FIREWALL);
            firewallB.mods.Add(new(Ability.StrafeSwap));
            firewallB.mods.Add(new(Ability.DeathShield));
            yield return BoardManager.Instance.CreateCardInSlot(firewallB, BoardManager.Instance.opponentSlots[6]);
            yield return new WaitForSeconds(1.5f);

            yield return this.ReplaceBlueprint("P03FinalBoss");
            yield return new WaitForSeconds(1f);
            ViewManager.Instance.SwitchToView(View.Default);
        }

        private Part1ResourcesManager WeirdManager = null;

        private void PhaseTwoEffects(bool showEffects = true)
        {
            TableVisualEffectsManager.Instance.SetDustParticlesActive(!showEffects);
            if (showEffects)
            {
                UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetColor(GameColors.Instance.nearWhite);
                UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetAlpha(1f);
                UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(0f, 1f);
                base.SpawnScenery("LightQuadTableEffect");

                Color angryColor = GameColors.Instance.red;
                Color partiallyTransparentRed = new Color(angryColor.r, angryColor.g, angryColor.b, 0.5f);

                TableVisualEffectsManager.Instance.ChangeTableColors(angryColor, Color.black, GameColors.Instance.nearWhite, partiallyTransparentRed, angryColor, Color.white, GameColors.Instance.gray, GameColors.Instance.gray, GameColors.Instance.lightGray);

                slotColors = new () { partiallyTransparentRed, angryColor, Color.white };
                queueSlotColors = new () { GameColors.Instance.gray, GameColors.Instance.gray, GameColors.Instance.lightGray };

                FactoryManager.Instance.HandLight.color = GameColors.Instance.orange;
            }
            else
            {
                TableVisualEffectsManager.Instance.ResetTableColors();
                FactoryManager.Instance.HandLight.color = GameColors.Instance.blue;
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default, true, true);

                if (WeirdManager != null)
                {
                    GameObject.Destroy(WeirdManager, 0.25f);
                    WeirdManager = null;    
                }
            }
        }

        private static readonly int[] MODDERS_PART_1 = new int[] { 0, 1, 1, 1, 1, 1, 0, 2, 1, 0, 2};

        private static readonly int[] MODDERS_PART_2 = new int[] { 2, 1, 2, 0, 1, 2, 0, 1, 2, 1};

        private int[] adjustedPlanP2;

        public override IEnumerator QueueNewCards(bool doTween = true, bool changeView = true)
        {
            if (base.NumLives == 1)
            {
                yield return base.QueueNewCards(doTween, changeView);
                yield break;
            }

            List<CardSlot> slotsToQueue = BoardManager.Instance.OpponentSlotsCopy.FindAll((CardSlot x) => x.Card == null || (x.Card != null && !x.Card.Info.HasTrait(Trait.Terrain)));
            slotsToQueue.RemoveAll((CardSlot x) => base.Queue.Exists((PlayableCard y) => y.QueuedSlot == x));
            int numCardsToQueue = 0;
            int[] plan = (base.NumLives == 3) ? MODDERS_PART_1 : this.adjustedPlanP2;
            if (TurnManager.Instance.TurnNumber < plan.Length)
                numCardsToQueue = plan[TurnManager.Instance.TurnNumber];
            
            for (int i = 0; i < numCardsToQueue; i++)
            {
                if (slotsToQueue.Count > 0)
                {
                    //int statPoints = Mathf.RoundToInt((float)Mathf.Min(6, TurnManager.Instance.TurnNumber + 1) * 2.5f);
                    CardSlot slot = slotsToQueue[Random.Range(0, slotsToQueue.Count)];
                    CardInfo card = GenerateCard(TurnManager.Instance.TurnNumber);
                    if (card != null)
                    {
                        yield return base.QueueCard(card, slot, doTween, changeView, true);
                        slotsToQueue.Remove(slot);
                    }
                }
            }
            yield return base.QueueNewCards();
        }

        public IEnumerator ShopForModSequence(string modName, bool shopping = true, bool firstPlay = false)
        {
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            
            if (shopping)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03ShoppingForMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(0.3f);
            }
            if (!firstPlay)
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03ReplayMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] { modName }, null);
            else
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03SelectedMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] { modName }, null);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking);
            yield return new WaitForSeconds(0.3f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        private bool scalesHidden = false;

        [HarmonyPatch(typeof(Scales3D), nameof(Scales3D.AddDamage))]
        [HarmonyPostfix]
        public static IEnumerator DontShowDamageWhenScalesHidden(IEnumerator sequence)
        {
            if (TurnManager.Instance != null &&
                TurnManager.Instance.Opponent is P03AscensionOpponent &&
                (TurnManager.Instance.Opponent as P03AscensionOpponent).scalesHidden)
                yield break;

            yield return sequence;
        }

        public IEnumerator UnityEngineSequence()
        {
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03UnityMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Consumables, false, false);
            yield return new WaitForSeconds(0.5f);
            ResourceDrone.Instance.gameObject.transform.localPosition = ResourceDrone.Instance.gameObject.transform.localPosition + Vector3.up * 6f;
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Scales, false, false);
            yield return new WaitForSeconds(0.5f);
            foreach(Renderer rend in LifeManager.Instance.Scales3D.gameObject.GetComponentsInChildren<Renderer>())
                rend.enabled = false;

            scalesHidden = true;
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03UnityModDone", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Default);
        }

        public IEnumerator APISequence()
        {
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03ApiInstalled", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);            
            ViewManager.Instance.SwitchToView(View.Consumables, false, false);
            InteractionCursor.Instance.InteractionDisabled = true;
            yield return new WaitForSeconds(0.2f);
            yield return ResourcesManager.Instance.RefreshEnergy();
            yield return new WaitForSeconds(0.6f);
            int maxEnergy = ResourcesManager.Instance.PlayerMaxEnergy;
            Traverse resourceTrav = Traverse.Create(ResourcesManager.Instance).Property("PlayerMaxEnergy");

            while (maxEnergy > 3)
            {
                yield return ResourcesManager.Instance.SpendEnergy(ResourcesManager.Instance.PlayerEnergy);
                maxEnergy -= 1;
                resourceTrav.SetValue(maxEnergy);
                yield return ResourcesManager.Instance.RefreshEnergy();
                yield return new WaitForSeconds(0.6f);
            }

            InteractionCursor.Instance.InteractionDisabled = false;
            ViewManager.Instance.SwitchToView(View.Default);
        }

        public IEnumerator DraftSequence()
        {
            ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);
            for (int i = 0; i < 2; i++)
            {
                IEnumerable<CardSlot> slots = BoardManager.Instance.OpponentSlotsCopy.Where(c => c != null && c.Card == null);
                CardSlot slot = i == 0 ? slots.FirstOrDefault() : slots.LastOrDefault();
                if (slot == null)
                    continue;

                CardInfo draftToken = CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN);
                yield return BoardManager.Instance.CreateCardInSlot(draftToken, slot);
                yield return new WaitForSeconds(0.55f);
            }
            yield return new WaitForSeconds(1f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        public IEnumerator ExchangeTokensSequence()
        {

            ViewManager.Instance.SwitchToView(View.P03Face, false, false);

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03Drafting", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            if (PlayerHand.Instance.cardsInHand.Count == 0)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03NoCardsInHand", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield break;
            }

            InteractionCursor.Instance.InteractionDisabled = true;

            int seed = P03AscensionSaveData.RandomSeed + 10 * TurnManager.Instance.TurnNumber;
            
            List<CardSlot> possibleSlots = BoardManager.Instance.OpponentSlotsCopy.Where(s => s.Card == null).ToList();
            CardSlot slot = possibleSlots[SeededRandom.Range(0, possibleSlots.Count, seed++)];

            ViewManager.Instance.SwitchToView(View.Hand, false, false);
            foreach (PlayableCard card in PlayerHand.Instance.CardsInHand)
            {
                PlayerHand.Instance.OnCardInspected(card);
                yield return new WaitForSeconds(0.33f);
            }
            List<PlayableCard> possibles = PlayerHand.Instance.CardsInHand.Where(c => c.Info.name != CustomCards.DRAFT_TOKEN).ToList();
            PlayableCard cardToSteal = possibles[SeededRandom.Range(0, possibles.Count, seed++)];
            PlayerHand.Instance.OnCardInspected(cardToSteal);
            yield return new WaitForSeconds(0.75f);

            PlayerHand.Instance.RemoveCardFromHand(cardToSteal);
            cardToSteal.SetEnabled(false);
            cardToSteal.Anim.SetTrigger("fly_off");
            Tween.Position(cardToSteal.transform, cardToSteal.transform.position + new Vector3(0f, 3f, 5f), 0.4f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate()
            {
                Object.Destroy(cardToSteal.gameObject);
            }, true);
            yield return new WaitForSeconds(0.75f);

            CardInfo draftToken = CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN);
            draftToken.mods.Add(new (Ability.DrawRandomCardOnDeath));
            PlayableCard tokenCard = CardSpawner.SpawnPlayableCard(draftToken);
            yield return PlayerHand.Instance.AddCardToHand(tokenCard, Vector3.zero, 0f);
            yield return new WaitForSeconds(0.6f);

            ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);
            yield return BoardManager.Instance.CreateCardInSlot(cardToSteal.Info, slot);
            yield return new WaitForSeconds(0.65f);

            ViewManager.Instance.SwitchToView(View.Default);
            InteractionCursor.Instance.InteractionDisabled = false;
            yield break;
        }

        public IEnumerator HammerSequence()
        {
            List<CardSlot> slots = BoardManager.Instance.PlayerSlotsCopy.Where(s => s != null && s.Card != null).ToList();

            if (slots.Count == 0)
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03AngryNoHammer", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03HammerModHappy", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            int seed = P03AscensionSaveData.RandomSeed + 10 * TurnManager.Instance.TurnNumber + 234;
            CardSlot target = slots[SeededRandom.Range(0, slots.Count, seed)];

            // Find the hammer item
            ItemSlot hammerSlot = ItemsManager.Instance.Slots.First(s => s.Item is HammerItem);
            HammerItem hammer = hammerSlot.Item as HammerItem;

            hammer.PlayExitAnimation();
            InteractionCursor.Instance.InteractionDisabled = true;
            yield return new WaitForSeconds(0.1f);
            //UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0.6f, 0.2f);
            ViewManager.Instance.SwitchToView(hammer.SelectionView, false, false);
            InteractionCursor.Instance.InteractionDisabled = false;

            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            
            foreach (CardSlot slot in BoardManager.Instance.PlayerSlotsCopy.Where(s => s != null && s.Card != null))
            {
                Transform firstPersonItem = FirstPersonController.Instance.AnimController.SpawnFirstPersonAnimation(hammer.FirstPersonPrefabId, null).transform;
                firstPersonItem.localPosition = hammer.FirstPersonItemPos + Vector3.right * 3f + Vector3.forward * 1f;
                firstPersonItem.localEulerAngles = hammer.FirstPersonItemEulers;
                yield return new WaitForSeconds(0.3f);
                hammer.MoveItemToPosition(firstPersonItem, slot.transform.position);
                yield return new WaitForSeconds(0.5f);
                yield return hammer.OnValidTargetSelected(slot, firstPersonItem.gameObject);
                yield return new WaitForSeconds(1f);
                GameObject.Destroy(firstPersonItem.gameObject);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }

            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            
            //UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0f, 0.2f);
            InteractionCursor.Instance.InteractionDisabled = false;

            hammerSlot.OnCursorEnter();
            hammerSlot.OnCursorExit();

            yield break;
        }

        [HarmonyPatch(typeof(Opponent), nameof(Opponent.ReplaceBlueprint))]
        [HarmonyPostfix]
        public static IEnumerator Postfix(IEnumerator sequence, string blueprintId, bool removeLockedCards = false)
        {
            if (!SaveFile.IsAscension || !(TurnManager.Instance.opponent is P03AscensionOpponent) || !blueprintId.Equals("P03FinalBoss"))
            {
                yield return sequence;
                yield break;
            }

            TurnManager.Instance.Opponent.Blueprint = (new EncounterBlueprintHelper(DataHelper.GetResourceString(blueprintId, "dat"))).AsBlueprint();
            
            List<List<CardInfo>> plan = EncounterBuilder.BuildOpponentTurnPlan(TurnManager.Instance.Opponent.Blueprint, EventManagement.EncounterDifficulty, removeLockedCards);
            TurnManager.Instance.Opponent.ReplaceAndAppendTurnPlan(plan);
            yield return TurnManager.Instance.Opponent.QueueNewCards(true, true);
            yield break;
        }
    }
}