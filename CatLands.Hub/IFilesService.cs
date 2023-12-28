namespace CatLands.Hub;

using System.Threading.Tasks;
using Avalonia.Platform.Storage;

public interface IFilesService
{
	public Task<IStorageFile?> OpenFileAsync();
	public Task<IStorageFile?> SaveFileAsync();
}