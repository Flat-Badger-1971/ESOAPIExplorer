using System.Text;

namespace ESOAPIExplorer.Services
{
    public interface ILuaLanguageServerDefinitionsGeneratorService
    {
        StringBuilder Generate();
    }
}