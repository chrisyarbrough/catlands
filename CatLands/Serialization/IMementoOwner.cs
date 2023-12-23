namespace CatLands.SpriteEditor;

public interface IMementoOwner
{
	string CreateMemento();
	void RestoreState(string memento);
}