using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace InfiniteStaminaMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class InfiniteStaminaPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.hex.infinitestamina";
        public const string PluginName = "Infinite Stamina";
        public const string PluginVersion = "1.0.0";

        public static InfiniteStaminaPlugin Instance { get; private set; }

        private Harmony _harmony;
        private ConfigEntry<KeyboardShortcut> _toggleKey;
        private ConfigEntry<bool> _modEnabled;
        private float _lastMessageTime;
        private static readonly KeyboardShortcut DefaultHotKey = new KeyboardShortcut(KeyCode.F7);
        private const float MessageCooldown = 0.2f;

        internal static ManualLogSource Log { get; private set; }

        public bool IsInfiniteStaminaEnabled => _modEnabled != null && _modEnabled.Value;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _toggleKey = Config.Bind(
                "General",
                "ToggleKey",
                DefaultHotKey,
                "Hotkey to toggle infinite stamina.");

            _modEnabled = Config.Bind(
                "General",
                "Enabled",
                false,
                "Whether infinite stamina is enabled.");

            _toggleKey.SettingChanged += OnToggleKeyChanged;

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();

            Log.LogInfo($"v{PluginVersion} loaded.");
        }

        private void Update()
        {
            if (!IsGameActive())
            {
                return;
            }

            if (!IsShortcutPressedAllowingExtraKeys(_toggleKey.Value))
            {
                return;
            }

            Player player = Player.m_localPlayer;
            _modEnabled.Value = !_modEnabled.Value;
            bool isEnabled = _modEnabled.Value;

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
            if (_toggleKey != null)
            {
                _toggleKey.SettingChanged -= OnToggleKeyChanged;
            }

            _harmony?.UnpatchSelf();
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
            string message = $"Hotkey updated -> {_toggleKey.Value}";
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