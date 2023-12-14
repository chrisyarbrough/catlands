namespace CatLands.SpriteEditor;

internal class AnimationEditorData
{
	private SpriteAtlas spriteAtlas;
	public int selectedAnimationIndex;

	public void SetSpriteAtlas(SpriteAtlas spriteAtlas)
	{
		this.spriteAtlas = spriteAtlas;
		this.selectedAnimationIndex = spriteAtlas.Animations.Count > 0 ? 0 : -1;
	}
}