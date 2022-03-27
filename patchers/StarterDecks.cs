using HarmonyLib;
using DiskCardGame;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using InscryptionAPI.Ascension;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class StarterDecks
    {
        public static string DEFAULT_STARTER_DECK { get; private set; }

        private static StarterDeckInfo CreateStarterDeckInfo(string title, string iconKey, string[] cards)
        {
            Texture2D icon = TextureHelper.GetImageAsTexture($"{iconKey}.png", typeof(StarterDecks).Assembly);
            return new() {
                name=$"P03_{title}",
                title=title,
                iconSprite = Sprite.Create(icon, new Rect(0f, 0f, 35f, 44f), new Vector2(0.5f, 0.5f)),
                cards=cards.Select(CardLoader.GetCardByName).ToList()
            };

            
        }

        public static void RegisterStarterDecks()
        {
            DEFAULT_STARTER_DECK = StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Snipers", "starterdeck_icon_snipers", new string[] {"Sniper", "BustedPrinter", "SentryBot" })).Info.name;
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Random", "starterdeck_icon_random", new string[] {"Amoebot", "GiftBot", "GiftBot" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Shield", "starterdeck_icon_shield", new string[] {"GemShielder", "Shieldbot", "LatcherShield" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Energy", "starterdeck_icon_energy", new string[] {"CloserBot", "BatteryBot", "BatteryBot" }));
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Conduit", "starterdeck_icon_conduit", new string[] {"CellTri", "CellBuff", "HealerConduit" }), unlockLevel: 4);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Nature", "starterdeck_icon_evolve", new string[] {"XformerGrizzlyBot", "XformerBatBot", "XformerPorcupineBot" }), unlockLevel: 4);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("Gems", "starterdeck_icon_gems", new string[] {"SentinelBlue", "SentinelGreen", "SentinelOrange"}), unlockLevel: 8);
            StarterDeckManager.Add(P03Plugin.PluginGuid, CreateStarterDeckInfo("FullDraft", "starterdeck_icon_token", new string[] {CustomCards.UNC_TOKEN, CustomCards.DRAFT_TOKEN, CustomCards.DRAFT_TOKEN }), unlockLevel: 8);

            StarterDeckManager.ModifyDeckList += delegate(List<StarterDeckManager.FullStarterDeck> decks)
            {
                CardTemple acceptableTemple = ScreenManagement.ScreenState;

                // Only keep decks where at least one card belongs to this temple
                decks.RemoveAll(info => info.Info.cards.FirstOrDefault(ci => ci.temple == acceptableTemple) == null);

                return decks;
            };
        }     
    }
}