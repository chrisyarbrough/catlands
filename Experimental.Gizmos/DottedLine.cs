namespace Experimental.Gizmos;

using System.Numerics;
using Raylib_cs;

public readonly struct DottedLine
{
	// TODO: Find solution for file paths in the published app, but also keep hot reloading feature for development.
	public static ShaderAsset Shader => shader ??= new ShaderAsset(
		vsFileName: null,
		fsFileName: "/Users/Chris/Projects/CatLands/Experimental.Gizmos/DottedLine.glsl",
		shader => resolutionId = Raylib.GetShaderLocation(shader, "resolution"));

	private static ShaderAsset shader;
	private static int resolutionId;
	private static readonly float[] vector2Buffer = new float[2];

	public Vector2 Center => line.Center;
	public Vector2 ClosestPointTo(Vector2 point) => line.ClosestPointTo(point);

	private readonly Line line;

	public DottedLine(Vector2 a, Vector2 b)
	{
		line = new Line(a, b);
	}

	public void Draw(Vector2 a, Vector2 b)
	{
		Vector2 center = (a + b) / 2f;
		const int length = 150;
		Vector2 fadeStart = line.ClosestPointTo(a + Vector2.Normalize(a - center) * length);
		Vector2 fadeEnd = line.ClosestPointTo(b + Vector2.Normalize(b - center) * length);

		using (Shader.DrawingBlock())
		{
			SetShaderVector2(resolutionId, Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
			SetShaderVector2(Raylib.GetShaderLocation(Shader, "fadeStart"), fadeStart.X, fadeStart.Y);
			SetShaderVector2(Raylib.GetShaderLocation(Shader, "fadeEnd"), fadeEnd.X, fadeEnd.Y);

			Raylib.DrawLineEx(fadeStart, fadeEnd, 2, Color.YELLOW);
		}
	}

	private static void SetShaderVector2(int id, float x, float y)
	{
		vector2Buffer[0] = x;
		vector2Buffer[1] = y;
		Raylib.SetShaderValue(Shader, id, vector2Buffer, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
	}
}