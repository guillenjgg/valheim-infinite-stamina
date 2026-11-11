using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace InfiniteStaminaMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.infinitestamina";
        private const string PluginName = "HexInfiniteStamina";
        private const string PluginVersion = "1.1.2";

        private float _lastMessageTime;
        private static readonly KeyboardShortcut DefaultHotKey = new KeyboardShortcut(KeyCode.F6);
        private const float MessageCooldown = 0.2f;
        private Harmony _harmony;

        internal static Plugin Instance { get; private set; }
        internal ConfigEntry<KeyboardShortcut> ToggleKey;
        internal ConfigEntry<bool> ModEnabled;
        internal ConfigEntry<bool> EnableOnlyWhileRunning;
        

        internal static ManualLogSource Log { get; private set; }


        private void Awake()
        {
            Instance = this;
            Log = Logger;

            ToggleKey = Config.Bind(
                "General",
                "ToggleKey",
                DefaultHotKey,
                "Hotkey to toggle infinite stamina.");

            ModEnabled = Config.Bind(
                "General",
                "Enabled",
                true,
                "Whether infinite stamina is enabled.");

            EnableOnlyWhileRunning = Config.Bind(
                "General",
                "EnableOnlyWhileRunning",
                false,
                "If true, stamina is only infinite while the player is running.");

            ToggleKey.SettingChanged += OnToggleKeyChanged;

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(assembly);

            Log.LogInfo($"v{PluginVersion} loaded.");
        }

        private void Update()
        {
            if (!IsGameActive())
            {
                return;
            }

            if (!IsShortcutPressedAllowingExtraKeys(ToggleKey.Value))
            {
                return;
            }

            Player player = Player.m_localPlayer;
            ModEnabled.Value = !ModEnabled.Value;
            bool isEnabled = ModEnabled.Value;

            if (isEnabled)
            {
                float missingStamina = player.GetMaxStamina() - player.GetStamina();
                if (missingStamina > 0f)
                {
                    player.AddStamina(missingStamina);
                }
            }

            ShowStatus(isEnabled);
        }

        private void OnDestroy()
        {
            if (ToggleKey != null)
            {
                ToggleKey.SettingChanged -= OnToggleKeyChanged;
            }


            _harmony?.UnpatchSelf();
            _harmony = null;

            Log.LogInfo($"v{PluginVersion} unloaded.");

            Instance = null;
        }

        private bool IsGameActive()
        {
            return Time.timeScale != 0f && Player.m_localPlayer != null;
        }

        private void ShowStatus(bool isEnabled)
        {
            if (Time.time - _lastMessageTime < MessageCooldown)
            {
                return;
            }

            _lastMessageTime = Time.time;

            string message = isEnabled
                ? "Infinite Stamina: ENABLED"
                : "Infinite Stamina: DISABLED";

            Log.LogInfo(message);

            if (MessageHud.instance != null)
            {
                MessageHud.instance.ShowMessage(
                    MessageHud.MessageType.Center,
                    message);
            }
        }

        private void OnToggleKeyChanged(object sender, EventArgs e)
        {
            string message = $"Hotkey updated -> {ToggleKey.Value}";
            Log.LogInfo(message);

            if (MessageHud.instance != null)
            {
                MessageHud.instance.ShowMessage(
                    MessageHud.MessageType.Center,
                    message);
            }
        }


        private static bool IsShortcutPressedAllowingExtraKeys(KeyboardShortcut shortcut)
        {
            if (shortcut.MainKey == KeyCode.None)
            {
                return false;
            }

            if (!Input.GetKeyDown(shortcut.MainKey))
            {
                return false;
            }

            foreach (var modifier in shortcut.Modifiers)
            {
                if (!Input.GetKey(modifier))
                {
                    return false;
                }
            }

            return true;
        }
    }
}