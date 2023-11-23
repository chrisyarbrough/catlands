namespace CatLands;

using Raylib_cs;

public class MapTextures
{
	public int TextureCount => textures.Count;

	private readonly Dictionary<string, Texture2D> textures = new();

	public void UpdateLoadState()
	{
		if (Map.Current == null)
			return;

		for (int i = 0; i < Map.Current.LayerCount; i++)
		{
			Layer layer = Map.Current.GetLayer(i);
			if (!textures.ContainsKey(layer.TexturePath))
				textures[layer.TexturePath] = Raylib.LoadTexture(layer.TexturePath);
		}
	}

	public Texture2D Get(string id) => textures[id];
}