var app = new TestApp();
app.Run();

public class TestApp : RaylibApp<EditModel>
{
	protected override void Update()
	{
		editModel.Update();
	}
}