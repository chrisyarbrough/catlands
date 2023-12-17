namespace CatLands;

using System.Collections;

public class Selection<T> : IEnumerable<T>
{
	protected readonly HashSet<T> Items = new();

	public bool IsEmpty => Items.Count == 0;

	public void Clear() => Items.Clear();

	public void Remove(T item) => Items.Remove(item);

	public void Add(T item) => Items.Add(item);

	/// <summary>
	/// Removed from the selection if already present, adds otherwise.
	/// Commonly used with the command/ctrl key.
	/// </summary>
	public void Toggle(T item)
	{
		if (Items.Contains(item))
			Items.Remove(item);
		else
			Items.Add(item);
	}

	/// <summary>
	/// A regular single selection.
	/// </summary>
	public virtual void Set(T item)
	{
		Items.Clear();
		Items.Add(item);
	}

	public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Selection : Selection<int>
{
	/// <summary>
	/// The item that was last selected without any modifiers.
	/// </summary>
	private int? lastSelected;

	/// <summary>
	/// Selects a continuous range from the item that was last selected without modifiers
	/// to and including the end item.
	/// </summary>
	public void SelectRange(int end)
	{
		int start = lastSelected.HasValue ? lastSelected.Value : 0;

		if (end < start)
			(start, end) = (end, start);

		Items.Clear();
		for (int i = start; i <= end; i++)
			Items.Add(i);
	}

	public override void Set(int item)
	{
		base.Set(item);
		lastSelected = item;
	}
}