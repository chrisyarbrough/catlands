using CatLands.SpriteEditor;
using ImGuiNET;

internal class AnimationEditorWindow
{
	private const ImGuiTableFlags tableFlags =
		ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

	public static void Draw(SpriteAtlas spriteAtlas, int selectedAnimationIndex)
	{
		if (ImGui.Begin("Animation Frames"))
		{
			if (spriteAtlas.Animations.Count > 0)
			{
				selectedAnimationIndex = Math.Clamp(selectedAnimationIndex, 0, spriteAtlas.Animations.Count);
				DrawAnimationControls(spriteAtlas.Animations[selectedAnimationIndex]);
			}
			else
			{
				ImGui.Text("1) Select a range of tiles on the sheet.\n" +
				           "2) Create a new animation in the Animations window.\n" +
				           "3) Or, select the desired animation.");
			}
		}

		ImGui.End();
	}

	private static void DrawAnimationControls(Animation animation)
	{
		string name = animation.Name;
		if (ImGui.InputText("Name", ref name, 64))
			animation.Name = name;

		if (ImGui.BeginTable(animation.Name, 3, tableFlags))
		{
			ImGui.TableSetupColumn("Frame", ImGuiTableColumnFlags.WidthFixed, 40);
			ImGui.TableSetupColumn("TileId", ImGuiTableColumnFlags.WidthFixed, 60);
			ImGui.TableSetupColumn("Duration");
			ImGui.TableHeadersRow();

			for (int i = 0; i < animation.Frames.Count; i++)
			{
				ImGui.TableNextRow();
				ImGui.PushID(i);
				Animation.Frame frame = animation.Frames[i];
				DrawFrameControls(i, frame);
				ImGui.PopID();
			}

			ImGui.EndTable();
		}
	}

	private static void DrawFrameControls(int i, Animation.Frame frame)
	{
		ImGui.TableSetColumnIndex(0);
		ImGui.Text(i.ToString());

		ImGui.TableSetColumnIndex(1);
		int tileId = frame.TileId;

		ImGui.PushItemWidth(-1);
		if (ImGui.InputInt("##TileId", ref tileId, step: 0, step_fast: 0))
			frame.TileId = tileId;
		ImGui.PopItemWidth();

		ImGui.TableSetColumnIndex(2);
		ImGui.PushItemWidth(-1);
		float duration = frame.Duration;
		if (ImGui.DragFloat("##Duration", ref duration,
			    v_speed: 0.05f, v_min: 0f, v_max: float.MaxValue, format: "%.2fs"))
		{
			frame.Duration = duration;
		}

		ImGui.PopItemWidth();
	}
}