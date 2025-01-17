namespace CatLands.Editor;

using NativeFileDialogSharp;

public static class SceneManager
{
	private static string mapsDirectory => Path.Combine(Program.AssetsDirectory, "Maps");

	public static void TryLoadInitialScene(string[] args)
	{
		if (args.Length == 2)
		{
			string mapFilePath = args[1];
			Console.WriteLine("Map file path passed via commandline: " + mapFilePath);
			LoadMap(mapFilePath);
		}
		else
		{
			if (Prefs.TryGet("LastMapFilePath", out string? loadedMapFilePath))
			{
				Console.WriteLine("Found map file path in prefs: " + loadedMapFilePath);
				if (File.Exists(loadedMapFilePath))
					LoadMap(loadedMapFilePath);
				else
				{
					Prefs.Remove("LastMapFilePath");
					CreateMap();
				}
			}
		}
	}

	public static DialogResult Open()
	{
		DialogResult result = Dialog.FileOpen(
			filterList: "json;bin",
			defaultPath: mapsDirectory);

		if (result.IsOk)
		{
			Console.WriteLine("Map file path selected via native file dialog: " + result.Path);
			LoadMap(result.Path);
		}
		else
		{
			if (result.IsError)
				Console.WriteLine(result.ErrorMessage);
		}

		return result;
	}

	public static void Save(Map map)
	{
		if (!Prefs.TryGet("LastMapFilePath", out string? loadedMapFilePath))
		{
			DialogResult result = Dialog.FileSave(
				filterList: "json;bin",
				defaultPath: mapsDirectory);

			if (result.IsOk)
			{
				loadedMapFilePath = result.Path;
			}
			else if (result.IsError)
			{
				Console.WriteLine(result.ErrorMessage);
			}
		}

		map.FilePath = loadedMapFilePath;
		SerializedAsset.Save(loadedMapFilePath, map);
		Prefs.Set("LastMapFilePath", loadedMapFilePath);
	}

	private static void LoadMap(string filePath)
	{
		Map.Current = SerializedAsset.Load<Map>(filePath)!;
		Map.Current.FilePath = filePath;
		Prefs.Set("LastMapFilePath", filePath);
		CommandManager.Clear();
	}

	public static void CreateMap()
	{
		Prefs.Remove("LastMapFilePath");
		Map.Current = new Map();
		CommandManager.Clear();
	}
}