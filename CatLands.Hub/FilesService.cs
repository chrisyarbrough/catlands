namespace CatLands.Hub;

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

public class FilesService : IFilesService
{
	private readonly Window target;

	public FilesService(Window target)
	{
		this.target = target;
	}

	public async Task<IStorageFile?> OpenFileAsync()
	{
		var files = await target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
		{
			Title = "Open Text File",
			AllowMultiple = false
		});

		return files.Count >= 1 ? files[0] : null;
	}

	public async Task<IStorageFile?> SaveFileAsync()
	{
		return await target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
		{
			Title = "Save Text File"
		});
	}
}