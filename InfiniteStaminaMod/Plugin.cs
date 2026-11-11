using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
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
        private FileSystemWatcher _configWatcher;
        private DateTime _lastConfigReloadTime;
        private static readonly TimeSpan ConfigReloadDelay = TimeSpan.FromSeconds(1);

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
            SetupWatcher();

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
            if (_configWatcher != null)
            {
                _configWatcher.EnableRaisingEvents = false;
                _configWatcher.Changed -= ReadConfigValues;
                _configWatcher.Created -= ReadConfigValues;
                _configWatcher.Renamed -= ReadConfigValues;
                _configWatcher.Dispose();
                _configWatcher = null;
            }

            if (ToggleKey != null)
            {
                ToggleKey.SettingChanged -= OnToggleKeyChanged;
            }


            _harmony?.UnpatchSelf();
            _harmony = null;

            Log.LogInfo($"v{PluginVersion} unloaded.");

            Instance = null;
        }

        private void SetupWatcher()
        {
            _configWatcher = new FileSystemWatcher(
                Path.GetDirectoryName(Config.ConfigFilePath),
                Path.GetFileName(Config.ConfigFilePath));
            _configWatcher.Changed += ReadConfigValues;
            _configWatcher.Created += ReadConfigValues;
            _configWatcher.Renamed += ReadConfigValues;
            _configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size;
            _configWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            _configWatcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (_configWatcher == null || !File.Exists(Config.ConfigFilePath))
            {
                return;
            }

            DateTime now = DateTime.UtcNow;
            if (now - _lastConfigReloadTime < ConfigReloadDelay)
            {
                return;
            }

            _lastConfigReloadTime = now;

            try
            {
                Log.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch (Exception exception)
            {
                Log.LogError($"There was an issue loading {Path.GetFileName(Config.ConfigFilePath)}: {exception}");
            }
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