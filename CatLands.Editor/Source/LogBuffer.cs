namespace CatLands.Editor;

using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;

public class LogWindow : Window
{
	public LogWindow() : base("Console")
	{
	}

	protected override void DrawContent()
	{
		if (ImGui.Button("Clear"))
			LogBuffer.Clear();

		ImGui.SameLine();
		bool copy = ImGui.Button("Copy");
		ImGui.SameLine();
		ImGui.Separator();
		ImGui.BeginChild("scrolling");
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0, 1));
		if (copy) ImGui.LogToClipboard();

		foreach (string message in LogBuffer.messages)
			ImGui.TextUnformatted(message);

		// Keep scrolling to the bottom if already there and new messages arrive.
		if (ImGui.GetScrollY() / ImGui.GetScrollMaxY() > 0.99f)
			ImGui.SetScrollHereY(1.0f);

		ImGui.PopStyleVar();
		ImGui.EndChild();
	}
}

public class LogBuffer
{
	private static TraceLogCallback callbackDelegate = CustomLogCallback;

	public static unsafe void Initialize()
	{
		IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

		// Set the callback
		Raylib.SetTraceLogCallback((delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void>)callbackPtr);
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void TraceLogCallback(int logType, IntPtr text, IntPtr text2);

	public static List<string> messages = new List<string>();

	public static void Clear()
	{
		messages.Clear();
	}

	private static void CustomLogCallback(int logType, IntPtr textPtr, IntPtr text2Ptr)
	{
		string prefix = ((TraceLogLevel)logType).ToString()["LOG_".Length..];
		string message = Logging.GetLogMessage(textPtr, text2Ptr);

		messages.Add(message);

		// Process the log message and additional info as needed
		Console.WriteLine($"{prefix} {message}");
	}
}