namespace Experimental.Gizmos;

public interface IModel
{
	event DirtyHandler Changed;

	void Save();
}

public delegate void DirtyHandler(bool isDirty);