using ESOAPIExplorer.Models;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public interface IESODocumentationService
{
    public Task InitialiseAsync();
    public Task ReloadAsync(bool clearCache = false);
    public EsoUIDocumentation Documentation { get; set; }
    // public EsoUIDocumentation Data { get; set; }
}