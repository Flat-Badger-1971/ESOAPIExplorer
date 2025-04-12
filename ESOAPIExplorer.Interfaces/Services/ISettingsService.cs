using System.Threading.Tasks;

namespace ESOAPIExplorer.Services
{
    public interface ISettingsService
    {
        T GetSetting<T>(string key, T defaultValue = default);
        void SaveSetting<T>(string key, T value);
        Task SaveSettingsAsync();
    }
}
