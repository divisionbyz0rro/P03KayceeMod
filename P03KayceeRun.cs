using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine.SceneManagement;

namespace Infiniscryption.P03KayceeRun
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class P03Plugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";
		public const string PluginName = "Infiniscryption P03 in Kaycee's Mod";
		public const string PluginVersion = "1.0";
        public const string CardPrefx = "P03KCM";

        internal static ManualLogSource Log;

        internal static bool Initialized = false;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
            
            CustomCards.RegisterCustomCards(harmony);
            StarterDecks.RegisterStarterDecks();
            AscensionChallengeManagement.UpdateP03Challenges();
            BossManagement.RegisterBosses();

            SceneManager.sceneLoaded += this.OnSceneLoaded;

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

        private void Start()
        {
            if (Chainloader.PluginInfos.ContainsKey("zorro.inscryption.infiniscryption.packmanager"))
                CustomCards.WriteP03Pack();
        }
    }
}
