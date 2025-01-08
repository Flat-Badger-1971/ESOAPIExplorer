using ESOAPIExplorer.Services;
using System.Threading.Tasks;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class ExportViewModel(ILuaCheckRcGeneratorService luaCheckRcGeneratorService) : ViewModelBase
{
    public override async Task InitializeAsync(object data)
    {
        await base.InitializeAsync(data);

        luaCheckRcGeneratorService.Generate();
    }
}
