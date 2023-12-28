namespace CatLands.Hub.ViewModels;

using System;
using Avalonia.Media.Imaging;
using ReactiveUI;

public class Project : ReactiveObject
{
	private string name;
	
	public string Name { 
		get => name; 
		set => this.RaiseAndSetIfChanged(ref name, value); }
	private string path;
	public string Path { get => path; set => this.RaiseAndSetIfChanged(ref path, value); }
	public DateTime LastOpened { get; set; } = DateTime.Parse("2023-12-25 18:39");
	public TimeSpan TimeSinceLastOpened => DateTime.Now - LastOpened;

	private bool b;

	public bool IsFavorite
	{
		get => b;
		set => this.RaiseAndSetIfChanged(ref b, value);
	}
	public string PreviewImagePath { get; set; }
	
	private Bitmap? previewImage;
	
	public Bitmap? PreviewImage { get => previewImage;
		set => this.RaiseAndSetIfChanged(ref previewImage, value);
	}
}