namespace CatLands;

public class ChangeTracker
{
	private int version;

	public void NotifyChange()
	{
		version++;
	}

	public bool HasChanged(ref int? knownVersion)
	{
		bool hasChanged = knownVersion != this.version;
		knownVersion = this.version;
		return hasChanged;
	}
}