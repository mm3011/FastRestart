using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace FastRestart
{   
    [BepInPlugin(modGUID,modName,modVersion)]
    public class FastRestartBase : BaseUnityPlugin
    {
        //Mod Info
        private const string modGUID = "kos.fastRestart";
        private const string modName = "Fast Restart";
        private const string modVersion = "1.0.0";

        public static bool verifying = false;
        private ConfigEntry<bool> overrideConfirmation;
        public static bool bypassConfirm = false;

        private Harmony harmony;
        private static MethodInfo chat;

        private void Awake()
        {
            overrideConfirmation = Config.Bind("Config", "Override Confirmation", false, "Ignore the confirmation step of restarting.");
            bypassConfirm = overrideConfirmation.Value;

            harmony = new Harmony(modGUID);
            harmony.PatchAll();

            chat = AccessTools.Method(typeof(HUDManager), "AddChatMessage");

            Logger.LogInfo($"{modGUID} loaded!");
        }

        public static void SendChatMessage(string message)
        {
            chat?.Invoke(HUDManager.Instance, new object[] { message, "" });
            HUDManager.Instance.lastChatMessage = "";
        }

        public static void ConfirmRestart()
        {
            verifying = true;
            SendChatMessage("Are you sure? Y/N.");
        }

        public static void AcceptRestart(StartOfRound manager)
        {
            SendChatMessage("Restart confirmed.");
            verifying = false;

            int[] stats = new int[]
            {
                manager.gameStats.daysSpent,
                manager.gameStats.scrapValueCollected,
                manager.gameStats.deaths,
                manager.gameStats.allStepsTaken
            };
            manager.FirePlayersAfterDeadlineClientRpc(stats);
        }

        public static void DeclineRestart()
        {
            SendChatMessage("Restart aborted.");
            verifying = false;
        }
    }
}
