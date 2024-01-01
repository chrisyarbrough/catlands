namespace CatLands.SpriteEditor;

using System.Collections;
using System.Numerics;
using Raylib_cs;

internal class SpriteAtlasViewModel
{
	public event Action<SpriteAtlas>? TargetChanged;

	public IntPtr TexturePointer => new(spriteAtlas.Texture.Id);


	public Animation? SelectedAnimation { get; set; }

	public bool TryGetSelectedAnimation(out Animation animation)
	{
		animation = SelectedAnimation!;
		return animation != null;
	}

	public Rect this[int id]
	{
		get => spriteAtlas[id];
		set
		{
			if (spriteAtlas[id] != value)
			{
				//undoManager.RecordSnapshot();
				spriteAtlas[id] = value;
				//appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
			}
		}
	}
	
	public void RecordSnapshot() => undoManager.RecordSnapshot();
	public void EvaluateDirty() => appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
	
public bool HasSprite(int id) => spriteAtlas.HasSprite(id);
	public IEnumerable<Animation> Animations => spriteAtlas.Animations;
	public IEnumerable<(int, Rect)> Sprites => spriteAtlas.Sprites;

	private SpriteAtlas spriteAtlas;
	private UndoManager undoManager;
	private readonly AppWindow appWindow;

	public SpriteAtlasViewModel(AppWindow appWindow)
	{
		this.appWindow = appWindow;
	}

	public void SetTarget(SpriteAtlas target)
	{
		if (target != spriteAtlas)
		{
			spriteAtlas = target;
			undoManager = new UndoManager(spriteAtlas);
			undoManager.UndoRedoPerformed += () => appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
			SelectedAnimation = target.Animations.Count > 0 ? target.Animations[0] : default;
			TargetChanged?.Invoke(spriteAtlas);
		}
	}

	public void RemoveSelectedAnimation()
	{
		Execute(() =>
		{
			spriteAtlas.Animations.Remove(SelectedAnimation);
			SelectedAnimation = null;
		});
	}

	public void AddAnimation(Animation animation)
	{
		Execute(() =>
		{
			spriteAtlas.Add(animation);
			SelectedAnimation = animation;
		});
	}

	public void SetRect(int index, Rect rect)
	{
		Execute(() => { spriteAtlas.SetSprite(index, rect); });
	}

	public void DeleteTiles(IEnumerable<int> tileIds)
	{
		Execute(() =>
		{
			foreach (int tileId in tileIds)
			{
				spriteAtlas.RemoveTile(tileId);
			}
		});
	}

	public void Undo() => undoManager.Undo();
	public void Redo() => undoManager.Redo();

	public void Save()
	{
		spriteAtlas.Save();
		undoManager.MarkClean();
		appWindow.SetUnsavedChangesIndicator(hasUnsavedChanges: false);
	}

	private void Execute(Action action)
	{
		undoManager.RecordSnapshot();
		action.Invoke();
		appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
	}

	public bool GetRenderInfo(int tileId, out Vector2 vector2, out Vector2 vector3, out Vector2 vector4)
	{
		return spriteAtlas.GetRenderInfo(tileId, out vector2, out vector3, out vector4);
	}

	public void GenerateSpriteRects(Func<Texture2D, IEnumerable<Rect>> slice)
	{
		Execute(() => { spriteAtlas.GenerateSpriteRects(slice); });
	}

	public int MergeTiles(IEnumerable<int> tileIds)
	{
		undoManager.RecordSnapshot();
		Rectangle mergedRect = this[tileIds.First()];

		foreach (int i in tileIds)
		{
			Rectangle spriteRect = this[i];

			// Expand the mergedRect to include the current spriteRect
			float minX = Math.Min(mergedRect.X, spriteRect.X);
			float minY = Math.Min(mergedRect.Y, spriteRect.Y);
			float maxX = Math.Max(mergedRect.X + mergedRect.Width, spriteRect.X + spriteRect.Width);
			float maxY = Math.Max(mergedRect.Y + mergedRect.Height, spriteRect.Y + spriteRect.Height);

			mergedRect.X = minX;
			mergedRect.Y = minY;
			mergedRect.Width = maxX - minX;
			mergedRect.Height = maxY - minY;
		}

		foreach (int i in tileIds.OrderByDescending(x => x))
		{
			spriteAtlas.RemoveTile(i);
		}

		int id = spriteAtlas.Add(mergedRect);
		appWindow.SetUnsavedChangesIndicator(undoManager.IsDirty());
		return id;
	}

	public bool TryGetRect(int index, out Rect rect)
	{
		if (spriteAtlas.HasSprite(index))
		{
			rect = spriteAtlas[index];
			return true;
		}

		rect = default;
		return false;
	}
}