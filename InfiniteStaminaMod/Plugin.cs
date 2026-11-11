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
        private const string PluginVersion = "1.1.3";

        private float _lastMessageTime;
        private static readonly KeyboardShortcut DefaultHotKey = new KeyboardShortcut(KeyCode.F6);
        private const float MessageCooldown = 0.2f;
        private Harmony _harmony;
        private FileSystemWatcher _configWatcher;
        private DateTime _configReloadTime;
        private bool _configReloadPending;
        private DateTime _nextConfigPollTime;
        private string _configFileContents;
        private ConfigEntry<bool>[] _staminaSettings;
        private static readonly TimeSpan ConfigReloadDelay = TimeSpan.FromSeconds(1);

        internal static Plugin Instance { get; private set; }
        internal ConfigEntry<KeyboardShortcut> ToggleKey;
        internal ConfigEntry<bool> ModEnabled;
        internal ConfigEntry<bool> OnlySelectedActivities;
        internal ConfigEntry<bool> NoRunningStaminaDrain;
        internal ConfigEntry<bool> NoSwimmingStaminaDrain;
        internal ConfigEntry<bool> NoEncumberedStaminaDrain;
        internal ConfigEntry<bool> DebugLogging;
        internal ConfigEntry<bool> NoCultivatorStaminaDrain;
        internal ConfigEntry<bool> NoBuildingStaminaDrain;
        internal ConfigEntry<bool> NoHoeStaminaDrain;
        internal ConfigEntry<bool> NoPickaxeStaminaDrain;

        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            bool saveOnConfigSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet = false;

            try
            {
                DebugLogging = Config.Bind("General", "DebugLogging", false,
                    new ConfigDescription("Enable all mod log messages, including debug, info, and errors.", null,
                        new ConfigurationManagerAttributes { Order = int.MaxValue }));

                string farming = ConfigMigration.ReadAndRemove(Config, "NoFarmingStaminaDrain");
                string terraforming = ConfigMigration.ReadAndRemove(Config, "NoTerraformingStaminaDrain");

                string enabled = ConfigMigration.ReadAndRemove(Config, "Enabled");

                ModEnabled = ConfigMigration.BindBoolean(Config, "ModEnabled", enabled,
                    new ConfigDescription("Master switch. True: infinite stamina for every action unless OnlySelectedActivities is on. False: vanilla stamina for everything.", null,
                        new ConfigurationManagerAttributes { Order = 100 }));

                ToggleKey = Config.Bind("General", "ToggleKey", DefaultHotKey,
                    new ConfigDescription("Hotkey to toggle the master switch. Individual activity settings are preserved.", null,
                        new ConfigurationManagerAttributes { Order = 90 }));

                string activityFilters = ConfigMigration.ReadAndRemove(Config, "UseActivityFilters");
                string movedActivityFilters = ConfigMigration.ReadAndRemove(Config, "UseActivityFilters", "Stamina Filters");

                OnlySelectedActivities = ConfigMigration.BindBoolean(Config, "OnlySelectedActivities",
                    string.IsNullOrEmpty(movedActivityFilters) ? activityFilters : movedActivityFilters,
                    new ConfigDescription("True: infinite stamina only for the activities enabled below. False: every action is free. Requires ModEnabled.", null,
                        new ConfigurationManagerAttributes { Order = 100 }), "Stamina Filters", false);

                string running = ConfigMigration.ReadAndRemove(Config, "EnableOnlyWhileRunning");

                NoRunningStaminaDrain = ConfigMigration.BindActivityBoolean(Config, "NoRunningStaminaDrain", running,
                    new ConfigDescription("Prevent running stamina drain. Only applies when ModEnabled and OnlySelectedActivities are true.", null,
                        new ConfigurationManagerAttributes { Order = 80 }));

                NoSwimmingStaminaDrain = ConfigMigration.BindActivityBoolean(Config, "NoSwimmingStaminaDrain", string.Empty,
                    new ConfigDescription("Prevent swimming stamina drain only when ModEnabled and OnlySelectedActivities are true.", null,
                        new ConfigurationManagerAttributes { Order = 75 }));

                NoEncumberedStaminaDrain = ConfigMigration.BindActivityBoolean(Config, "NoEncumberedStaminaDrain", string.Empty,
                    new ConfigDescription("Prevent encumbrance stamina drain only when ModEnabled and OnlySelectedActivities are true.", null,
                        new ConfigurationManagerAttributes { Order = 70 }));

                NoBuildingStaminaDrain = ConfigMigration.BindActivityBoolean(Config, "NoBuildingStaminaDrain", string.Empty,
                    new ConfigDescription("Prevent stamina consumption from building, removing pieces, and repairs, only when ModEnabled and OnlySelectedActivities are true.", null,
                        new ConfigurationManagerAttributes { Order = 60 }));

                NoCultivatorStaminaDrain = ConfigMigration.BindActivityBoolean(Config, "NoCultivatorStaminaDrain", farming,
                    new ConfigDescription("Prevent stamina consumption from cultivator use and Farming skill attacks, only when ModEnabled and OnlySelectedActivities are true.", null,
                        new ConfigurationManagerAttributes { Order = 50 }));

                NoHoeStaminaDrain = ConfigMigration.BindActivityBoolean(Config, "NoHoeStaminaDrain", terraforming,
                    new ConfigDescription("Prevent stamina consumption from hoe use, only when ModEnabled and OnlySelectedActivities are true.", null,
                        new ConfigurationManagerAttributes { Order = 40 }));

                NoPickaxeStaminaDrain = ConfigMigration.BindActivityBoolean(Config, "NoPickaxeStaminaDrain", terraforming,
                    new ConfigDescription("Prevent stamina consumption from Pickaxes skill attacks, including digging and mining, only when ModEnabled and OnlySelectedActivities are true.", null,
                        new ConfigurationManagerAttributes { Order = 30 }));

                Config.Save();
            }
            finally
            {
                Config.SaveOnConfigSet = saveOnConfigSet;
            }

            _staminaSettings = new[]
            {
                ModEnabled, OnlySelectedActivities, NoRunningStaminaDrain, NoSwimmingStaminaDrain, NoEncumberedStaminaDrain, NoBuildingStaminaDrain,
                NoCultivatorStaminaDrain, NoHoeStaminaDrain, NoPickaxeStaminaDrain
            };

            foreach (ConfigEntry<bool> setting in _staminaSettings)
            {
                setting.SettingChanged += OnStaminaSettingChanged;
            }

            ToggleKey.SettingChanged += OnToggleKeyChanged;
            SetupWatcher();

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (DebugLogging.Value)
            {
                Log.LogInfo($"v{PluginVersion} loaded.");
            }
        }

        private void Update()
        {
            PollConfigChanges();

            if (_configReloadPending && DateTime.UtcNow >= _configReloadTime)
            {
                _configReloadPending = false;
                ReloadConfig();
            }

            if (!IsGameActive())
            {
                return;
            }

            if (!IsShortcutPressedAllowingExtraKeys(ToggleKey.Value))
            {
                return;
            }

            ModEnabled.Value = !ModEnabled.Value;

            bool isEnabled = ModEnabled.Value;

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

            if (_staminaSettings != null)
            {
                foreach (ConfigEntry<bool> setting in _staminaSettings)
                {
                    setting.SettingChanged -= OnStaminaSettingChanged;
                }
            }


            if (ToggleKey != null)
            {
                ToggleKey.SettingChanged -= OnToggleKeyChanged;
            }


            _harmony?.UnpatchSelf();
            _harmony = null;

            if (DebugLogging.Value)
            {
                Log.LogInfo($"v{PluginVersion} unloaded.");
            }

            Instance = null;
        }

        internal DateTime ShowStaminaUntil { get; private set; }

        private void OnStaminaSettingChanged(object sender, EventArgs e)
        {
            Player player = Player.m_localPlayer;

            if (player == null)
            {
                return;
            }

            SettingChangedEventArgs change = e as SettingChangedEventArgs;

            if (!ModEnabled.Value && change?.ChangedSetting != ModEnabled)
            {
                return;
            }

            player.AddStamina(player.GetMaxStamina() - player.GetStamina());
            ShowStaminaUntil = DateTime.UtcNow.AddSeconds(1);
        }

        private void SetupWatcher()
        {
            _configFileContents = File.ReadAllText(Config.ConfigFilePath);

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

        private void PollConfigChanges()
        {
            DateTime now = DateTime.UtcNow;

            if (now < _nextConfigPollTime)
            {
                return;
            }

            _nextConfigPollTime = now + ConfigReloadDelay;

            try
            {
                if (!File.Exists(Config.ConfigFilePath))
                {
                    return;
                }

                string contents = File.ReadAllText(Config.ConfigFilePath);

                if (contents != _configFileContents)
                {
                    _configFileContents = contents;
                    _configReloadTime = now + ConfigReloadDelay;
                    _configReloadPending = true;
                }
            }
            catch (IOException)
            {
            }
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (_configWatcher == null || !File.Exists(Config.ConfigFilePath))
            {
                return;
            }

            _configReloadTime = DateTime.UtcNow + ConfigReloadDelay;
            _configReloadPending = true;
        }

        private void ReloadConfig()
        {
            if (!File.Exists(Config.ConfigFilePath))
            {
                return;
            }

            bool saveOnConfigSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet = false;

            try
            {
                Config.Reload();

                if (DebugLogging.Value)
                {
                    Log.LogInfo("Configuration reloaded.");
                }
            }
            catch (Exception exception)
            {
                if (DebugLogging.Value)
                {
                    Log.LogError($"There was an issue loading {Path.GetFileName(Config.ConfigFilePath)}: {exception}");
                }
            }
            finally
            {
                Config.SaveOnConfigSet = saveOnConfigSet;
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

            if (DebugLogging.Value)
            {
                Log.LogInfo(message);
            }

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
            if (DebugLogging.Value)
            {
                Log.LogInfo(message);
            }

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