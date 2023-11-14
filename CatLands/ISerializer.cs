namespace CatLands;

public interface ISerializer
{
	void Serialize(Map map, Stream stream);
	Map? Deserialize(Stream stream);
}