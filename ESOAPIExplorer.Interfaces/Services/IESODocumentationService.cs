using ESOAPIExplorer.Models;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public interface IESODocumentationService
{
    public Task InitialiseAsync();
    public EsoUIDocumentation Documentation { get; set; }
    public EsoUIDocumentation Data { get; set; }
}