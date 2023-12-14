namespace CatLands;

using Raylib_cs;

public static class MapTextures
{
	public static int TextureCount => atlases.Count;

	private static readonly Dictionary<string, SpriteAtlas> atlases = new();

	public static void UpdateLoadState()
	{
		if (Map.Current == null)
			return;

		for (int i = 0; i < Map.Current.LayerCount; i++)
		{
			Layer layer = Map.Current.GetLayer(i);
			if (!atlases.ContainsKey(layer.TexturePath))
			{
				atlases[layer.TexturePath] = SpriteAtlas.Load(layer.TexturePath);
			}
		}
	}

	public static Texture2D GetTexture(string id) => atlases[id].Texture;

	public static SpriteAtlas GetAtlas(string textureId) => atlases[textureId];

	public static Rectangle GetSpriteRect(string textureId, int spriteId)
	{
		return atlases[textureId].SpriteRects[spriteId];
	}
}