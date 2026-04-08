using System.Threading.Tasks;
using Windows.Storage;

namespace ESOAPIExplorer.Services;

public interface IFilePickerService
{
    public Task<StorageFile> PickSingleFileAsync(FileOpenPickerOptions options);
    public Task<StorageFile> PickSaveFileAsync(FileSavePickerOptions options);
}
