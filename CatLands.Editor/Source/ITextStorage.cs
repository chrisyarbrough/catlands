namespace CatLands.Editor;

public interface ITextStorage
{
	void WriteAllText(string text);
	bool Exists { get; }
	string ReadAllText();
}

public class FileStorage : ITextStorage
{
	private readonly string path;

	public FileStorage(string path)
	{
		this.path = path;
	}

	public void WriteAllText(string text) => File.WriteAllText(path, text);

	public bool Exists => File.Exists(path);

	public string ReadAllText() => File.ReadAllText(path);
}