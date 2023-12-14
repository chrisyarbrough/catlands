namespace CatLands.Editor;

public class Pref<T>
{
	private readonly string key;
	private readonly T? defaultValue;

	public T? Value
	{
		get => Prefs.Get(key, defaultValue);
		set
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			Prefs.Set(key, value);
		}
	}

	public Pref(string key, T? defaultValue = default)
	{
		this.key = key;
		this.defaultValue = defaultValue;
	}

	public static implicit operator T?(Pref<T> pref)
	{
		return pref.Value;
	}
}