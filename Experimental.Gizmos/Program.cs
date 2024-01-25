using Experimental.Gizmos;

var app = new TestApp();
app.Run();

namespace Experimental.Gizmos
{
	public class TestApp : RaylibApp<EditModel>
	{
		protected override void Update()
		{
			EditModel.Update();
		}
	}
}