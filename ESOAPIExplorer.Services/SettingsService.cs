using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private readonly Dictionary<string, object> _settings = new();
    private bool _isDirty = false;
    private JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public SettingsService()
    {
        _settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ESOAPIExplorer",
            "userSettings.json");

        Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                Dictionary<string, JsonElement> loadedSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _options);

                if (loadedSettings != null)
                {
                    foreach (KeyValuePair<string, JsonElement> kvp in loadedSettings)
                    {
                        if (kvp.Value.ValueKind == JsonValueKind.String)
                        {
                            _settings[kvp.Key] = kvp.Value.GetString();
                        }
                        else if (kvp.Value.ValueKind == JsonValueKind.Number)
                        {
                            if (kvp.Value.TryGetInt32(out int intValue))
                            {
                                _settings[kvp.Key] = intValue;
                            }
                            else if (kvp.Value.TryGetDouble(out double doubleValue))
                            {
                                _settings[kvp.Key] = doubleValue;
                            }
                        }
                        else if (kvp.Value.ValueKind == JsonValueKind.True || kvp.Value.ValueKind == JsonValueKind.False)
                        {
                            _settings[kvp.Key] = kvp.Value.GetBoolean();
                        }
                        else if (kvp.Value.ValueKind == JsonValueKind.Object)
                        {
                            _settings[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log exception or handle it appropriately
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
    }

    public T GetSetting<T>(string key, T defaultValue = default)
    {
        if (_settings.TryGetValue(key, out object value))
        {
            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            if (value is T typedValue)
            {
                return typedValue;
            }
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public void SaveSetting<T>(string key, T value)
    {
        _settings[key] = value;
        _isDirty = true;
    }

    public async Task SaveSettingsAsync()
    {
        if (!_isDirty) return;

        try
        {
            string json = JsonSerializer.Serialize(_settings,
                new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(_settingsFilePath, json);
            _isDirty = false;
        }
        catch (Exception ex)
        {
            // Log exception or handle it appropriately
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}
