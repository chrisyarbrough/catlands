namespace CatLands;

public interface ISerializer
{
	string FileExtension { get; }

	void Serialize(object value, Stream stream);
	T? Deserialize<T>(Stream stream);

	protected static void Write(string text, Stream stream)
	{
		using var writer = new StreamWriter(stream, leaveOpen: true);
		writer.Write(text);
	}
}