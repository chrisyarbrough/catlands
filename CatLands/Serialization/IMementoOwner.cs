namespace CatLands;

public interface IMementoOwner
{
	string CreateMemento();
	void RestoreState(string memento);
}