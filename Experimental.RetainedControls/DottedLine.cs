namespace Experimental.RetainedControls;

using System.Numerics;
using Raylib_cs;

public readonly struct DottedLine
{
	private static ShaderAsset shader;
	private static int resolutionId;
	private static readonly float[] vector2Buffer = new float[2];

	private readonly Line line;

	public DottedLine(Vector2 a, Vector2 b)
	{
		line = new Line(a, b);
	}

	public void Draw(Vector2 a, Vector2 b)
	{
		if (shader == null)
		{
			const string path = "/Users/Chris/Projects/CatLands/Experimental.RetainedControls/DottedLine.glsl";
			shader = new ShaderAsset(null, path, shader =>
			{
				resolutionId = Raylib.GetShaderLocation(shader, "resolution");
			});
		}

		Vector2 center = (a + b) / 2f;
		const int length = 150;
		Vector2 fadeStart = line.ClosestPointTo(a + Vector2.Normalize(a - center) * length);
		Vector2 fadeEnd = line.ClosestPointTo(b + Vector2.Normalize(b - center) * length);

		shader.Update();
		Raylib.BeginShaderMode(shader);

		SetShaderVector2(resolutionId, Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
		SetShaderVector2(Raylib.GetShaderLocation(shader, "fadeStart"), fadeStart.X, fadeStart.Y);
		SetShaderVector2(Raylib.GetShaderLocation(shader, "fadeEnd"), fadeEnd.X, fadeEnd.Y);

		Raylib.DrawLineEx(fadeStart, fadeEnd, 2, Color.YELLOW);

		Raylib.EndShaderMode();
	}

	private static void SetShaderVector2(int id, float x, float y)
	{
		vector2Buffer[0] = x;
		vector2Buffer[1] = y;
		Raylib.SetShaderValue(shader, id, vector2Buffer, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
	}
}