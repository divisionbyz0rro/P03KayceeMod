using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infiniscryption.P03KayceeRun
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class P03Plugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";
		public const string PluginName = "Infiniscryption P03 in Kaycee's Mod";
		public const string PluginVersion = "2.3";   
        public const string CardPrefx = "P03KCM"; 

        internal static P03Plugin Instance;  

        internal static ManualLogSource Log; 

        internal static bool Initialized = false;
        
        internal string DebugCode
        {
            get
            {
                return Config.Bind("P03KayceeMod", "DebugCode", "nothing", new BepInEx.Configuration.ConfigDescription("A special code to use for debugging purposes only. Don't change this unless your name is DivisionByZorro or he told you how it works.")).Value;
            }
        }

        internal string SecretCardComponents
        {
            get
            {
                return Config.Bind("P03KayceeMod", "SecretCardComponents", "nothing", new BepInEx.Configuration.ConfigDescription("The secret code for the secret card")).Value;
            }
        }

        private void Awake()
        {
            Instance = this;

            Log = base.Logger;
            Log.LogInfo($"Debug code = {DebugCode}");

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(); 

            foreach (Type t in typeof(P03Plugin).Assembly.GetTypes())
            {
                try
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
                } catch (TypeLoadException ex)
                {
                    Log.LogWarning("Failed to force load static constructor!");
                    Log.LogWarning(ex);
                }
            }
            
            CustomCards.RegisterCustomCards(harmony);
            StarterDecks.RegisterStarterDecks();
            AscensionChallengeManagement.UpdateP03Challenges();
            BossManagement.RegisterBosses();

            SceneManager.sceneLoaded += this.OnSceneLoaded;

            EncounterBlueprintHelper.TestAllKnownEncounterData();

            Initialized = true;

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FixDeckEditor()
        {
            Traverse.Create((Chainloader.PluginInfos["inscryption_deckeditor"].Instance as DeckEditor)).Field("save").SetValue(SaveManager.SaveFile);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Chainloader.PluginInfos.ContainsKey("inscryption_deckeditor"))
                FixDeckEditor();
        }
    }
}
