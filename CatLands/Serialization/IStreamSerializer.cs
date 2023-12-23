namespace CatLands;

/// <summary>
/// Converts an object to a stream and vice versa.
/// </summary>
public interface IStreamSerializer
{
	void WriteTo(object value, Stream stream);
	T? ReadFrom<T>(Stream stream);
}