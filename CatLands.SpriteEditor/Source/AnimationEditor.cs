namespace CatLands.SpriteEditor;

internal class AnimationEditor
{
	private readonly UndoManager undoManager;
	private readonly AppWindow appWindow;

	public AnimationEditor(UndoManager undoManager, AppWindow appWindow)
	{
		this.undoManager = undoManager;
		this.appWindow = appWindow;
	}

	public SpriteAtlas SpriteAtlas { get; private set; }

	public int SelectedAnimationIndex;

	public IntPtr TexturePointer => new(SpriteAtlas!.Texture.Id);

	public bool TryGetSelectedAnimation(out Animation animation)
	{
		if (SelectedAnimationIndex == -1)
		{
			animation = default!;
			return false;
		}

		animation = SpriteAtlas.Animations[SelectedAnimationIndex];
		return true;
	}

	public void SetSpriteAtlas(SpriteAtlas spriteAtlas)
	{
		this.SpriteAtlas = spriteAtlas;
		this.SelectedAnimationIndex = spriteAtlas.Animations.Count > 0 ? 0 : -1;
	}

	public void AddAnimation(Animation animation)
	{
		undoManager.RecordSnapshot();
		SelectedAnimationIndex = SpriteAtlas.Add(animation);
		appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
	}

	public void RemoveSelectedAnimation()
	{
		SpriteAtlas.Animations.RemoveAt(SelectedAnimationIndex);
		SelectedAnimationIndex = SpriteAtlas.Animations.ClampedIndex(SelectedAnimationIndex);
		appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
	}

	public void SetRect(int index, Rect rect)
	{
		if (rect != SpriteAtlas.SpriteRects[index])
		{
			undoManager.RecordSnapshot();
			SpriteAtlas.SpriteRects[index] = rect;
			appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
		}
	}

	public IList<Animation> Animations => SpriteAtlas.Animations;

	public T CreateWindow<T>() where T : Window
	{
		var constructor = typeof(T).GetConstructor(new[] { GetType() });

		if (constructor == null)
		{
			throw new InvalidOperationException(
				$"No constructor found in {typeof(T)} that takes {GetType()} as a parameter.");
		}

		return (T)constructor.Invoke(new object[] { this });
	}
}