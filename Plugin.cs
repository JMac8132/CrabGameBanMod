using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.IO;
using System.Text.RegularExpressions;
using SteamManager = MonoBehaviourPublicObInUIgaStCSBoStcuCSUnique;
using GameManager = MonoBehaviourPublicDi2UIObacspDi2UIObUnique;
using LobbyManager = MonoBehaviourPublicCSDi2UIInstObUIloDiUnique;

namespace BanMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.0")]
    public class Plugin : BasePlugin
    {
        public static string path = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "plugins", "BannedPlayers.txt");
        
        public override void Load()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Log.LogInfo("Mod created by JMac");
        }

        public static bool IsHost()
        {
            return SteamManager.Instance.IsLobbyOwner() && !LobbyManager.Instance.Method_Public_Boolean_0();
        }

        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Start))]
        [HarmonyPostfix]
        public static void SteamManagerStart()
        {
            if (!File.Exists(path)) File.Create(path).Dispose();
        }

        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.StartLobby))]
        [HarmonyPostfix]
        public static void LobbyManagerStartLobby()
        {
            string pattern = @".* - (\d{17})";
            using StreamReader reader = new StreamReader(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    string matchedNumber = match.Groups[1].Value;
                    if (ulong.TryParse(matchedNumber, out ulong playerId))
                    {
                        LobbyManager.bannedPlayers.Add(playerId);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.BanPlayer))]
        [HarmonyPrefix]
        public static void LobbyManagerBanPlayer(ulong param_1)
        {
            if (!IsHost()) return;

            using (StreamWriter writer = new(path, true))
            {
                if (GameManager.Instance.activePlayers.ContainsKey(param_1)) writer.WriteLine($"{GameManager.Instance.activePlayers[param_1].username} - {param_1}");
                else if (GameManager.Instance.spectators.ContainsKey(param_1)) writer.WriteLine($"Spectator - {param_1}");

                writer.WriteLine();
            }
        }

        [HarmonyPatch(typeof(MonoBehaviourPublicGataInefObInUnique), "Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicCSDi2UIInstObUIloDiUnique), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicVesnUnique), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicObjomaOblogaTMObseprUnique), "Method_Public_Void_PDM_2")]
        [HarmonyPatch(typeof(MonoBehaviourPublicTeplUnique), "Method_Private_Void_PDM_32")]
        [HarmonyPrefix]
        public static bool Prefix(System.Reflection.MethodBase __originalMethod)
        {
            return false;
        }
    }
}
