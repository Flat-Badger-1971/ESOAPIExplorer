using System.Threading.Tasks;
using Windows.Storage;

namespace ESOAPIExplorer.Services;

public interface IFolderPickerService
{
    public Task<StorageFolder> PickSingleFolderAsync(FolderPickerOptions options);
}
