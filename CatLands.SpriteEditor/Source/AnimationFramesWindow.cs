namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;

internal class AnimationFramesWindow : Window
{
	private readonly AnimationEditor data;

	private static CatLands.Selection selection = new();

	private const ImGuiTableFlags tableFlags =
		ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

	public AnimationFramesWindow(AnimationEditor data) : base("Frames")
	{
		this.data = data;
	}

	protected override void DrawContent()
	{
		if (data.TryGetSelectedAnimation(out Animation animation))
		{
			DrawAnimationControls(animation);
		}
		else
		{
			ImGui.Text("1) Select a range of tiles on the sheet.\n" +
			           "2) Create a new animation in the Animations window.\n" +
			           "3) Or, select the desired animation.");
		}
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

		if (ImGui.Button("Add New"))
		{
			Animation.Frame frame = new(0, 0.25f);
			if (selection.IsEmpty)
				animation.Frames.Add(frame);
			else
				animation.Frames.Insert(selection.Max() + 1, frame);
		}

		ImGui.SameLine();

		ImGui.BeginDisabled(selection.IsEmpty);
		if (ImGui.Button("Remove"))
		{
			foreach (int i in selection.OrderByDescending(x => x))
				animation.Frames.RemoveAt(i);

			selection.Clear();
		}

		ImGui.EndDisabled();
		ImGui.SameLine();
	}

	private static void DrawFrameControls(int i, Animation.Frame frame)
	{
		ImGui.TableSetColumnIndex(0);
		if (ImGui.Selectable(
			    i.ToString(),
			    selection.Contains(i),
			    ImGuiSelectableFlags.None,
			    new Vector2(0, 17)))
		{
			if (ImGui.GetIO().KeyShift)
			{
				selection.SelectRange(i);
			}
			else if (ImGui.GetIO().KeySuper || ImGui.GetIO().KeyCtrl)
			{
				selection.Toggle(i);
			}
			else
			{
				selection.Set(i);
			}
		}

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