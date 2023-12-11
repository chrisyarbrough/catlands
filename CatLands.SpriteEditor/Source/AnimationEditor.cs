using System.Numerics;
using CatLands.SpriteEditor;
using ImGuiNET;

internal class AnimationEditor
{
	private static int selectedAnimationIndex;

	private const ImGuiTableFlags tableFlags =
		ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

	public static void Draw(SpriteAtlas spriteAtlas)
	{
		ImGui.ShowDemoWindow();
		DrawAnimationsSelectionWindow(spriteAtlas);
		DrawAnimationFramesWindow(spriteAtlas);
	}

	private static void DrawAnimationsSelectionWindow(SpriteAtlas spriteAtlas)
	{
		if (ImGui.Begin("Animations"))
		{
			if (ImGui.Button("Create New"))
			{
				spriteAtlas.Add(new Animation()
				{
					Name = "New Animation",
					Frames = Selection.GetSelection()
						.Select(tileId => new Animation.Frame(tileId, 0.25f)).ToList(),
				});
				SaveDirtyTracker.MarkDirty();
			}

			ImGui.SameLine();

			HelpMarker(
				"Select a range of tiles on the loaded sheet before creating a new animation to automatically add them as frames.");

			ImGui.BeginTable("Animations", 4, tableFlags);
			ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed, 40);
			ImGui.TableSetupColumn("Name");
			ImGui.TableSetupColumn("Frame Count");
			ImGui.TableSetupColumn("Duration");
			ImGui.TableHeadersRow();
			
			for (int i = 0; i < spriteAtlas.Animations.Count; i++)
			{
				Animation animation = spriteAtlas.Animations[i];
				ImGui.TableNextRow();

				ImGui.TableSetColumnIndex(0);
				if (ImGui.Selectable(
					    i.ToString(),
					    selectedAnimationIndex == i,
					    ImGuiSelectableFlags.SpanAllColumns,
					    new Vector2(0, 17)))
				{
					selectedAnimationIndex = i;
				}

				ImGui.TableSetColumnIndex(1);
				ImGui.Text(animation.Name);

				ImGui.TableSetColumnIndex(2);
				ImGui.Text(animation.Frames.Count.ToString());
				
				ImGui.TableSetColumnIndex(3);
				ImGui.Text($"{animation.Duration.TotalSeconds:F2}s");
			}

			ImGui.EndTable();
		}

		ImGui.End();
	}

	private static void DrawAnimationFramesWindow(SpriteAtlas spriteAtlas)
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

	private static void HelpMarker(string text)
	{
		ImGui.TextDisabled("(?)");
		if (ImGui.BeginItemTooltip())
		{
			ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
			ImGui.TextUnformatted(text);
			ImGui.PopTextWrapPos();
			ImGui.EndTooltip();
		}
	}
}