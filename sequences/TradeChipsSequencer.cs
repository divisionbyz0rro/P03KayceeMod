using System;
using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using DiskCardGame;
using System.Linq;
using Infiniscryption.P03KayceeRun.Patchers;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
	public class TradeChipsSequencer : CardChoicesSequencer
	{
        // This is pretty much just a copy-paste of TradePeltsSequencer.

        public static TradeChipsSequencer Instance { get; private set; }

		private static bool activating = false;

        public TradeChipsSequencer()
        {
            Traverse handlerTraverse = Traverse.Create(SpecialNodeHandler.Instance);
            Traverse cardChoiceTraverse = Traverse.Create(handlerTraverse.Field("cardChoiceSequencer").GetValue<CardSingleChoicesSequencer>());

            this.selectableCardPrefab = cardChoiceTraverse.Field("selectableCardPrefab").GetValue<GameObject>();
            this.deckPile = cardChoiceTraverse.Field("deckPile").GetValue<CardPile>();
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool HandleTradeTokens(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            if (nodeData is TradeChipsNodeData)
            {
                if (TradeChipsSequencer.Instance == null)
                    TradeChipsSequencer.Instance = __instance.gameObject.AddComponent<TradeChipsSequencer>();
                
                SpecialNodeHandler.Instance.StartCoroutine(TradeChipsSequencer.Instance.TradeTokens(nodeData as TradeChipsNodeData));
                return false;
            }
            return true;
        }

		public IEnumerator TradeTokens(NodeData nodeData)
		{
			if (activating) // hacky workaround for the duplicate trade defect
				yield break;

			activating = true;

            P03Plugin.Log.LogInfo("Starting trade sequencer");
			ViewManager.Instance.SwitchToView(View.Default, false, true);
			yield return new WaitForSeconds(0.5f); // TODO: Write some snarky P03 dialogue about trading here

            P03Plugin.Log.LogInfo("Getting trade tiers");
			List<List<CardInfo>> tradingTiers = this.GetTradingTiers();
			bool hasPelts = tradingTiers.Count > 0;
			if (hasPelts)
			{
                P03Plugin.Log.LogInfo("Going topdown");
				ViewManager.Instance.SwitchToView(View.TradingTopDown, false, true);
				yield return new WaitForSeconds(0.15f);

                P03Plugin.Log.LogInfo("Spawning deck");
                Vector3 originalDeckLocation = this.deckPile.transform.localPosition;
                this.deckPile.transform.localPosition = DECK_LOCATION;
				yield return this.deckPile.SpawnCards(Part3SaveData.Data.deck.Cards.Count, 0.75f);

				foreach (List<CardInfo> tier in tradingTiers)
				{
                    P03Plugin.Log.LogInfo("Clearing");
					this.tradeCards.Clear();
					this.tokenCards.Clear();

                    P03Plugin.Log.LogInfo("Creating token cards");
					yield return new WaitForSeconds(0.15f);
					yield return this.CreateTokenCards(tier);

                    P03Plugin.Log.LogInfo("Creating trade cards");
					yield return new WaitForSeconds(0.15f);
					yield return this.CreateTradeCards(this.GetTradeCardInfos(tier[0]), CARDS_PER_ROW, tier[0].name.Equals(CustomCards.RARE_DRAFT_TOKEN));

                    P03Plugin.Log.LogInfo("Adding rulebook");
					TableRuleBook.Instance.SetOnBoard(true);

                    P03Plugin.Log.LogInfo("Setting selectable card propeties");
					foreach (SelectableCard card in this.tradeCards)
					{
						card.SetEnabled(true);
						card.SetInteractionEnabled(true);
						SelectableCard selectableCard = card;
						selectableCard.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(selectableCard.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
						{
							this.OnCardSelected(c as SelectableCard);
						}));
					}
					
					yield return new WaitForSeconds(0.25f);

                    P03Plugin.Log.LogInfo("Waiting for trading to be done");
					ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
					base.EnableViewDeck(ViewController.ControlMode.TradePelts, base.transform.position);

					yield return new WaitUntil(() => this.tokenCards.Count == 0);

					ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
					base.DisableViewDeck();
					yield return new WaitForSeconds(0.15f);

                    P03Plugin.Log.LogInfo("Disabling cards");
					foreach (SelectableCard card in this.tradeCards)
						card.SetEnabled(false);
					
                    P03Plugin.Log.LogInfo("Cleaning up");
					yield return this.CleanupTradeCards(this.tradeCards);
					yield return new WaitForSeconds(0.1f);
					yield return this.deckPile.DestroyCards(0.5f);
                    this.deckPile.transform.localPosition = originalDeckLocation;
				}
			}
			else
			{
				yield return this.NoPeltsSequence();
			}

			TableRuleBook.Instance.SetOnBoard(false);
			ViewManager.Instance.SwitchToView(View.Default, false, false);

			activating = false;

			if (GameFlowManager.Instance != null)
			{
				GameFlowManager.Instance.TransitionToGameState(GameState.Map, null);
			}
			yield break;
		}

		private void OnCardSelected(SelectableCard card)
		{
			if (this.tokenCards.Count > 0)
			{
				this.RemoveLastToken();
				card.SetEnabled(false);
				this.tradeCards.Remove(card);
				this.deckPile.MoveCardToPile(card, true, 0f, 0.7f);
				this.deckPile.AddToPile(card.transform);
				Part3SaveData.Data.deck.AddCard(card.Info);
				AscensionStatsData.TryIncrementStat(AscensionStat.Type.PeltsTraded);
			}
		}

		private void RemoveLastToken()
		{
			SelectableCard selectableCard = this.tokenCards[this.tokenCards.Count - 1];
			Tween.Position(selectableCard.transform, selectableCard.transform.position + Vector3.forward * 5.5f, 0.15f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
			GameObject.Destroy(selectableCard.gameObject, 0.3f);
			this.tokenCards.Remove(selectableCard);
			Part3SaveData.Data.deck.RemoveCard(selectableCard.Info);
		}

		private IEnumerator NoPeltsSequence()
		{
			// Right nowe we do nothing here. Maybe later.
			yield break;
		}

		private IEnumerator CreateTokenCards(List<CardInfo> cardInfos)
		{
			for (int i = 0; i < cardInfos.Count; i++)
			{
				this.deckPile.Draw();
				yield return new WaitForSeconds(0.15f);
			}
			for (int j = 0; j < cardInfos.Count; j++)
			{
                P03Plugin.Log.LogInfo("Instantiating prefab");
				GameObject cardObj = GameObject.Instantiate<GameObject>(this.selectableCardPrefab, base.transform);
				cardObj.SetActive(true);

                P03Plugin.Log.LogInfo("Setting data");
				SelectableCard card = cardObj.GetComponent<SelectableCard>();
				card.SetInfo(cardInfos[j]);

				Vector3 destinationPos = this.TOKEN_CARDS_ANCHOR + this.TOKEN_CARD_SPACING * (float)j;
				destinationPos.x = this.TOKEN_CARDS_ANCHOR.x + this.TOKEN_CARD_SPACING.x * (float)(j % 4);
				card.transform.position = destinationPos + Vector3.back * 4f;
				Tween.Position(card.transform, destinationPos, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);

				card.Anim.PlayRiffleSound();
				this.tokenCards.Add(card);
				card.SetEnabled(false);
				card.SetInteractionEnabled(false);
				yield return new WaitForSeconds(0.15f);
				cardObj = null;
				card = null;
				destinationPos = default(Vector3);
			}
            yield break;
		}

		private IEnumerator CreateTradeCards(List<CardInfo> cards, int cardsPerRow, bool rareCards)
		{
			for (int i = 0; i < cards.Count; i++)
			{
				int x = i % cardsPerRow;
				int y = (i >= cardsPerRow) ? 0 : 1;

				GameObject cardObj = GameObject.Instantiate<GameObject>(this.selectableCardPrefab, base.transform);
				cardObj.gameObject.SetActive(true);

				SelectableCard card = cardObj.GetComponent<SelectableCard>();
				card.SetInfo(cards[i]);

				foreach (SpecialCardBehaviour specialBehaviour in cardObj.GetComponents<SpecialCardBehaviour>())
					specialBehaviour.OnShownForCardChoiceNode();

				Vector3 destinationPos = this.CARDS_ANCHOR + new Vector3(this.CARD_SPACING.x * (float)x, 0f, this.CARD_SPACING.y * (float)y);
				if (rareCards)
					destinationPos.z = -2f;

				Vector3 destinationRot = new Vector3(90f, 90f, 90f);
				card.transform.position = destinationPos + new Vector3(0f, 0.25f, 3f);
				card.transform.eulerAngles = destinationRot + new Vector3(0f, 0f, -7.5f + UnityEngine.Random.value * 7.5f);
				Tween.Position(card.transform, destinationPos, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
				Tween.Rotation(card.transform, destinationRot, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);

				this.tradeCards.Add(card);
				card.SetEnabled(false);
				card.Anim.PlayQuickRiffleSound();

				yield return new WaitForSeconds(0.05f);
			}
            yield break;
		}

		private IEnumerator CleanupTradeCards(List<SelectableCard> selectableCards)
		{
			for (int i = selectableCards.Count - 1; i >= 0; i--)
			{
				SelectableCard card = selectableCards[i];
				Vector3 destinationPos = card.transform.position + new Vector3(0f, 0.25f, 5f);
				Vector3 destinationRot = card.transform.eulerAngles + new Vector3(0f, 0f, -7.5f + UnityEngine.Random.value * 7.5f);
				Tween.Position(card.transform, destinationPos, 0.15f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
				Tween.Rotation(card.transform, destinationRot, 0.15f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
				GameObject.Destroy(card.gameObject, 0.3f);
				yield return new WaitForSeconds(0.05f);
				card = null;
				destinationPos = default(Vector3);
				destinationRot = default(Vector3);
			}
            yield break;
		}

		private static string AbilitiesKey(List<Ability> abilities)
		{
			List<string> strings = abilities.Select(a => a.ToString()).ToList();
			strings.Sort();
			return string.Join(",", strings);
		}

		private List<List<CardInfo>> GroupTokens(List<CardInfo> tokens)
		{
			List<List<CardInfo>> retval = new();

			List<CardInfo> emptyTokens = tokens.Where(t => t.Abilities.Count == 0).ToList();
			retval.Add(emptyTokens);

			foreach (var grp in tokens.Where(t => !emptyTokens.Contains(t)).GroupBy(ci => AbilitiesKey(ci.Abilities)))
				retval.Add(grp.ToList());
			
			return retval;
		}

		private List<List<CardInfo>> GetTradingTiers()
		{
            // When copied from the pelt trader, there was a notion of multiple trading tiers
            // I'm going to leave that in, in case I decide to try to make the whole 'rare' chip thing work
			List<List<CardInfo>> list = new List<List<CardInfo>>();

			// Find all of the regular draft tokens
			foreach (string name in new string[] { CustomCards.DRAFT_TOKEN, CustomCards.UNC_TOKEN, CustomCards.RARE_DRAFT_TOKEN })
			{
				List<List<CardInfo>> group = GroupTokens(Part3SaveData.Data.deck.Cards.Where(c => c.name.Equals(name)).ToList());
				if (group.Count > 0)
					list.AddRange(group);
			}

			return list.Where(c => c.Count > 0).ToList();
		}

		public static bool IsValidDraftCard(CardInfo card)
		{
			foreach (CardMetaCategory cat in card.metaCategories)
			{
				if (cat == CustomCards.NeutralRegion) return true;
				if (cat == CustomCards.WizardRegion) return true;
				if (cat == CustomCards.TechRegion) return true;
				if (cat == CustomCards.UndeadRegion) return true;
				if (cat == CustomCards.NatureRegion) return true;
				if (cat == CardMetaCategory.ChoiceNode && card.temple == CardTemple.Tech) return true;
			}

			return false;
		}

		private int randomSeedSegment = 1;

		private List<CardInfo> GetTradeCardInfos(CardInfo token)
		{
            // Any part 3 card is fair game in this case
            // I don't care about unlocks or temples right now
            List<CardInfo> cards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(IsValidDraftCard);

			if (token.name != CustomCards.RARE_DRAFT_TOKEN)
				cards.RemoveAll(c => c.metaCategories.Contains(CardMetaCategory.Rare));
			if (token.name == CustomCards.RARE_DRAFT_TOKEN)
				cards.RemoveAll(c => !c.metaCategories.Contains(CardMetaCategory.Rare));

			cards.RemoveAll(x => x.name == "EmptyVessel");
            cards.RemoveAll(x => x.onePerDeck && Part3SaveData.Data.deck.Cards.Exists((CardInfo y) => y.name == x.name));

			int numberOfCards = token.name == CustomCards.RARE_DRAFT_TOKEN ? NUM_RARE_CARDS : NUM_CARDS;
			int randomSeed = P03AscensionSaveData.RandomSeed + 100 * randomSeedSegment++;
            //List<CardInfo> result = CardLoader.GetDistinctCardsFromPool(randomSeed, numberOfCards, cards).Select(CustomCards.ModifyCardForAscension).ToList();
			List<CardInfo> result = CardLoader.GetDistinctCardsFromPool(randomSeed, numberOfCards, cards).ToList();

			if (token.name == CustomCards.UNC_TOKEN) // The uncommon token adds a random ability to the card
			{
				foreach (CardInfo info in result)
				{
					CardModificationInfo mod = new CardModificationInfo();
					Ability ability = AbilitiesUtil.GetRandomLearnedAbility(randomSeed++, false, 0, 5, AbilityMetaCategory.Part3Modular);
					while (info.HasAbility(ability))
						ability = AbilitiesUtil.GetRandomLearnedAbility(randomSeed++, false, 0, 5, AbilityMetaCategory.Part3Modular);

					mod.abilities.Add(ability);
					info.mods.Add(mod);
				}
			}

			// If the token has abilities, transfer those to the cards
			if (token.Abilities.Count > 0)
			{
				foreach (CardInfo info in result)
				{
					foreach (Ability ab in token.Abilities.Where(a => !info.HasAbility(a)))
					{
						if (info.Abilities.Count < 4)
							info.mods.Add(new (ab));
					}	
				}
			}

			return result;
		}

		private List<SelectableCard> tradeCards = new List<SelectableCard>();

		private List<SelectableCard> tokenCards = new List<SelectableCard>();

		private readonly Vector3 CARDS_ANCHOR = new Vector3(-0.7f, 5.01f, -0.4f);

		private const float RARE_CARDS_ANCHOR_Z = -2f;

		private readonly Vector2 CARD_SPACING = new Vector3(1.4f, -2.07f);

		private readonly Vector3 TOKEN_CARDS_ANCHOR = new Vector3(-2.95f, 5.01f, -1.15f);

		private readonly Vector3 TOKEN_CARD_SPACING = new Vector3(0.3f, 0.04f, -0.3f);

        private readonly Vector3 DECK_LOCATION = new Vector3(3.7f, 5.01f, -2.2f);

		private const int RARE_CARD_TIER = 2;

		private const int NUM_CARDS = 8;

		private const int NUM_RARE_CARDS = 3;

		private const int CARDS_PER_ROW = 4;
	}
}
