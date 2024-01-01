var app = new TestApp();
app.Run();

internal class TestApp : RaylibApp<EditModel>
{
	protected override void Update()
	{
		Event updateEvent = new Event(EventPhase.Update);
		Handles.BeginLayoutPhase(updateEvent);
		editModel.OnGUI(updateEvent);

		Event drawEvent = new Event(EventPhase.Draw);
		Handles.BeginDrawPhase(drawEvent);
		editModel.OnGUI(drawEvent);
	}
}