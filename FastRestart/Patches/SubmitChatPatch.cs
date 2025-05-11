using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace FastRestart.Patches
{
    [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
    internal class SubmitChatPatch
    {
        private static bool Prefix(ref InputAction.CallbackContext context)
        {
            if (!context.performed) return true;
            HUDManager _instance = HUDManager.Instance;

            if (string.IsNullOrEmpty(_instance.chatTextField.text)) return true;

            PlayerControllerB local = GameNetworkManager.Instance.localPlayerController;
            if (local == null) return true;

            StartOfRound manager = local.playersManager;
            if(manager == null) return true;

            string text = _instance.chatTextField.text;

            if (FastRestartBase.verifying)
            {
                if (text.ToLower() == "y" || text.ToLower() == "yes")
                {
                    ResetTextbox(_instance, local);
                    if (!local.isInHangarShipRoom || !manager.inShipPhase || manager.travellingToNewLevel)
                    {
                        FastRestartBase.SendChatMessage("Cannot restart, ship must be in orbit.");
                        return false;
                    }
                    FastRestartBase.AcceptRestart(manager);
                    return false;
                }
                if (text.ToLower() == "n" || text.ToLower() == "no")
                {
                    ResetTextbox(_instance, local);
                    FastRestartBase.DeclineRestart();
                    return false;
                }
                return true;
            }
            if(text.ToLower() == "/restart" || text.ToLower() == "/reset")
            {
                ResetTextbox(_instance, local);
                if(!GameNetworkManager.Instance.isHostingGame)
                {
                    FastRestartBase.SendChatMessage("Must be lobby host to reset game.");
                    return false;
                }

                if (FastRestartBase.bypassConfirm) FastRestartBase.AcceptRestart(manager);
                else FastRestartBase.ConfirmRestart();

                return false;
            }
            return true;
        }

        private static void ResetTextbox(HUDManager manager, PlayerControllerB local)
        {
            local.isTypingChat = false;
            manager.chatTextField.text = "";
            EventSystem.current.SetSelectedGameObject(null);
            manager.PingHUDElement(manager.Chat, 2f, 1f, 0.2f);
            manager.typingIndicator.enabled = false;
        }
    }
}
