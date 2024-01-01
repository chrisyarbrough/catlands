namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

internal class AnimationSelectorWindow : Window
{
	private readonly SpriteAtlasViewModel target;

	private const ImGuiTableFlags tableFlags =
		ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

	public AnimationSelectorWindow(SpriteAtlasViewModel target) : base("Animations")
	{
		this.target = target;
	}

	protected override void DrawContent()
	{
		if (ImGui.Button("Create New (N)") || Raylib.IsKeyPressed(KeyboardKey.KEY_N))
		{
			AddNewAnimation();
		}

		ImGui.SameLine();

		ImGuiUtil.HelpMarker(
			"Select a range of tiles on the loaded sheet before creating a new animation to automatically add them as frames.");

		ImGui.BeginDisabled(target.SelectedAnimation == null);
		if (ImGui.Button("Delete"))
		{
			target.RemoveSelectedAnimation();
		}

		ImGui.EndDisabled();

		if (ImGui.BeginTable("Animations", 4, tableFlags))
		{
			ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed, 40);
			ImGui.TableSetupColumn("Name");
			ImGui.TableSetupColumn("Frame Count");
			ImGui.TableSetupColumn("Duration");
			ImGui.TableHeadersRow();

			foreach (Animation animation in target.Animations)
			{
				ImGui.TableNextRow();

				ImGui.TableSetColumnIndex(0);

				if (ImGui.Selectable(
					    animation.Name,
					    target.SelectedAnimation == animation,
					    ImGuiSelectableFlags.SpanAllColumns,
					    new Vector2(0, 17)))
				{
					target.SelectedAnimation = animation;
					TileSelection.ClearSelection();
					for (int j = 0; j < animation.FrameCount; j++)
					{
						TileSelection.AddToSelection(animation.FrameAt(j).TileId);
					}
				}

				ImGui.TableSetColumnIndex(1);
				ImGui.Text(animation.Name);

				ImGui.TableSetColumnIndex(2);
				ImGui.Text(animation.Frames.Count.ToString());

				ImGui.TableSetColumnIndex(3);
				ImGui.Text($"{animation.Duration:F2}s");
			}

			ImGui.EndTable();
		}
	}

	private void AddNewAnimation()
	{
		target.AddAnimation(new Animation()
		{
			Name = "New Anim " + target.Animations.Count(),
			Frames = TileSelection.GetSelection()
				.Select(tileId => new Animation.Frame(tileId, 0.25f)).ToList(),
		});
	}
}