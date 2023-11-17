namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

internal static class Program
{
	private const string version = "1.0";
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void TraceLogCallback(int logType, IntPtr text, IntPtr text2);

	private static void CustomLogCallback(int logType, IntPtr textPtr, IntPtr text2Ptr)
	{
		string prefix = ((TraceLogLevel)logType).ToString()["LOG_".Length..];
		string message = Logging.GetLogMessage(textPtr, text2Ptr);

		// Process the log message and additional info as needed
		Console.WriteLine($"{prefix} {message}");
	}


	private static readonly Dictionary<string, Func<Scene, Window>> windowFactories = new ()
	{
		{ "Scene View", scene => new SceneView(scene) },
		{ "Hierarchy", scene => new HierarchyWindow(scene) },
		{ "Inspector", _ => new Inspector() },
	};

	private static List<Window> windows;
	private static Scene scene = new Scene();

	private static void Main(string[] args)
	{
		unsafe
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: <mapFilePath>");
				return;
			}
			TraceLogCallback callbackDelegate = CustomLogCallback;
			IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

			// Set the callback
			Raylib.SetTraceLogCallback((delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void>)callbackPtr);

			Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
			Raylib.InitWindow(width: 1280, height: 800, title: $"CatLands Editor {version}");
			Raylib.SetTargetFPS(120);

			rlImGui.Setup(enableDocking: true);

			Map? map = SaveFileManager.Load(filePath: args[0]);

			if (map == null)
				return;

			var mapDisplay = new MapDisplay(map);
			mapDisplay.LoadAssets();

			scene.AddChild(mapDisplay);

			windows = new List<Window>
			{
				new SceneView(scene),
				new HierarchyWindow(scene),
				new Inspector()
			};

			// TODO: Handle multiple scene views.
			var brush = new Brush(map, mapDisplay);
			scene.AddChild(brush);



			while (!Raylib.WindowShouldClose())
			{
				Raylib.BeginDrawing();
				rlImGui.Begin();
				DrawDock();

				ImGui.ShowDemoWindow();

				foreach (Window window in windows)
					window.Render();

				ImGui.End(); // End Dock
				rlImGui.End();

				Raylib.EndDrawing();
			}

			rlImGui.Shutdown();
			Raylib.CloseWindow();
		}
	}

	private static void DrawDock()
	{
		ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
		ImGui.SetNextWindowSize(new Vector2(Raylib.GetRenderWidth(), Raylib.GetRenderHeight()));
		ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
		ImGui.Begin("DockSpace Demo", ImGuiWindowFlags.MenuBar |
		                              ImGuiWindowFlags.NoDocking |
		                              ImGuiWindowFlags.NoTitleBar |
		                              ImGuiWindowFlags.NoCollapse |
		                              ImGuiWindowFlags.NoResize |
		                              ImGuiWindowFlags.NoMove |
		                              ImGuiWindowFlags.NoBringToFrontOnFocus |
		                              ImGuiWindowFlags.NoNavFocus);
		ImGui.PopStyleVar(3);
		ImGui.DockSpace(ImGui.GetID("MyDockSpace"));

		ImGui.BeginMainMenuBar();
		if (ImGui.BeginMenu("File"))
		{
			ImGui.MenuItem("New");
			ImGui.MenuItem("Open...");
			ImGui.MenuItem("Save");
		}
		if (ImGui.BeginMenu("Window"))
		{
			foreach ((string key, Func<Scene, Window> factory) in windowFactories)
			{
				if(ImGui.MenuItem(key))
				{
					windows.Add(factory.Invoke(scene));
				}
			}
		}
		ImGui.EndMenu();
		ImGui.EndMainMenuBar();
	}
}

internal class Logger
{
	public static void Initialize()
	{
		Console.SetOut(new LogBuffer());
	}

	private class LogBuffer : TextWriter
	{
		public override Encoding Encoding => Encoding.Default;
	}
}