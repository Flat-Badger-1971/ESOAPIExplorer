using System.Threading.Tasks;
using Windows.Storage;

namespace ESOAPIExplorer.Services
{
    public interface ILuaLanguageServerDefinitionsGeneratorService
    {
        public Task Generate(StorageFolder folder);
    }
}