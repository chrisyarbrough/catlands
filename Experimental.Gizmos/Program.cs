using Experimental.Gizmos;

var app = new TestApp();
app.Run();

namespace Experimental.Gizmos
{
	using Raylib_cs;

	public class TestApp : RaylibApp<EditModel>
	{
		protected override void Update(bool captureInput)
		{
			if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
			{
				Rect rect = Rect.Handle(Raylib.GetMousePosition(), size: 50);
				EditModel.AddRect(rect);
			}

			if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) && Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
			{
				EditModel.Undo();
			}

			if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
			{
				EditModel.DeleteSelected();
			}

			EditModel.Update(captureInput);
		}
	}
}