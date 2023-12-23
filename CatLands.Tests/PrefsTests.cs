namespace CatLands.Tests;

using Editor;
using NSubstitute;

public class PrefsTests
{
	[Fact]
	public void SerializesSimpleTypes()
	{
		Prefs.Storage = Substitute.For<ITextStorage>();
		string storage = "";
		Prefs.Storage.Exists.Returns(true);
		Prefs.Storage.WriteAllText(Arg.Do<string>(x => storage = x));
		Prefs.Storage.ReadAllText().Returns(_ => storage);

		Prefs.Set("string", "value");
		Prefs.Set("int", 1);
		Prefs.Set("bool", true);
		Prefs.Set("float", 1.2f);
		
		Prefs.Load();

		Prefs.Get<string>("string").Should().Be("value");
		Prefs.Get<int>("int").Should().Be(1);
		Prefs.Get<bool>("bool").Should().Be(true);
		Prefs.Get<float>("float").Should().Be(1.2f);
	}
}