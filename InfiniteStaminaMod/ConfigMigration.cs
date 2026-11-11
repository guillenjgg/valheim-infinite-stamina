using BepInEx.Configuration;

namespace InfiniteStaminaMod
{
    internal static class ConfigMigration
    {
        internal static string ReadAndRemove(ConfigFile config, string key)
        {
            ConfigEntry<string> entry = config.Bind("General", key, string.Empty);
            string value = entry.Value;

            config.Remove(entry.Definition);

            return value;
        }

        internal static ConfigEntry<bool> BindBoolean(ConfigFile config, string key, string legacyValue, ConfigDescription description)
        {
            string currentValue = ReadAndRemove(config, key);
            ConfigEntry<bool> entry = config.Bind("General", key, true, description);

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
