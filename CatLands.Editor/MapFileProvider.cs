namespace CatLands.Editor;

using NativeFileDialogSharp;

// TODO: This should probably be a SceneManager instead.
public static class MapFileProvider
{
	private static string mapFilePath = string.Empty;

	public static void OpenOrGetFromCommandLine(string[] args)
	{
		if (args.Length == 1)
		{
			Console.WriteLine("Map file path passed via commandline: " + args[0]);
			LoadMap(args[0]);
		}
		else
		{
			if (args.Length > 1)
			{
				Console.WriteLine("More than one argument passed to commandline. Usage: <mapFilePath>");
				Environment.Exit(1);
			}

			if (args.Length == 0)
			{
				DialogResult result = Open();

				if (result.IsCancelled)
					Environment.Exit(2);

				if (result.IsError)
					Environment.Exit(3);
			}
		}
	}

	public static DialogResult Open()
	{
		DialogResult result = Dialog.FileOpen(
			filterList: "json;bin",
			defaultPath: Directory.GetCurrentDirectory());

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

	public static void Save()
	{
		if (mapFilePath.Length == 0)
		{
			DialogResult result = Dialog.FileSave(
				filterList: "json;bin",
				defaultPath: Directory.GetCurrentDirectory());

			if (result.IsOk)
			{
				mapFilePath = result.Path;
				SaveFileManager.Save(mapFilePath, Map.Current);
			}
			else if (result.IsError)
				Console.WriteLine(result.ErrorMessage);
		}
	}

	private static void LoadMap(string value)
	{
		mapFilePath = value;
		Map.Current = SaveFileManager.Load(value)!;
		CommandManager.Clear();
	}

	public static void New()
	{
		mapFilePath = string.Empty;
		Map.Current = new Map();
		CommandManager.Clear();
	}
}