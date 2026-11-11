using BepInEx.Configuration;

namespace InfiniteStaminaMod
{
    internal static class ConfigMigration
    {
        internal static string ReadAndRemove(ConfigFile config, string key, string section = "General")
        {
            ConfigEntry<string> entry = config.Bind(section, key, string.Empty);
            string value = entry.Value;

            config.Remove(entry.Definition);

            return value;
        }

        internal static ConfigEntry<bool> BindActivityBoolean(ConfigFile config, string key, string legacyValue, ConfigDescription description)
        {
            string previousValue = ReadAndRemove(config, key);

            return BindBoolean(config, key, string.IsNullOrEmpty(previousValue) ? legacyValue : previousValue,
                description, "Stamina Filters", false);
        }

        internal static ConfigEntry<bool> BindBoolean(ConfigFile config, string key, string legacyValue, ConfigDescription description, string section = "General", bool defaultValue = true)
        {
            string currentValue = ReadAndRemove(config, key, section);
            ConfigEntry<bool> entry = config.Bind(section, key, defaultValue, description);

            if (!string.IsNullOrEmpty(currentValue))
            {
                entry.SetSerializedValue(currentValue);
            }
            else if (bool.TryParse(legacyValue, out bool migratedValue))
            {
                entry.Value = migratedValue;
            }

            return entry;
        }
    }
}
