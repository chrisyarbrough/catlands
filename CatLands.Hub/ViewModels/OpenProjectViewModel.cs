namespace CatLands.Hub.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

public class OpenProjectViewModel : ViewModelBase
{
	private Project? selectedProjectPreview;

	public Project? SelectedProjectPreview
	{
		get => selectedProjectPreview;
		set => this.RaiseAndSetIfChanged(ref selectedProjectPreview, value);
	}

	public void OpenProject()
	{
		Console.WriteLine("Opened!");
	}

	public ReactiveCommand<Unit, Unit> OpenProjectCommand { get; }

	public ReactiveCommand<Unit, Unit> AddFromDiskCommand { get; }
	public ObservableCollection<Project> ProjectPreviews { get; protected set; }

	public OpenProjectViewModel()
	{
		ProjectPreviews = new ObservableCollection<Project>
		{
			new()
			{
				Name = "My Game",
				Path = "/Users/Chris/Downloads/project-genesis-preview-flying",
				PreviewImagePath = "/Users/Chris/Downloads/project-genesis-preview-flying.jpg",
				IsFavorite = true,
				LastOpened = DateTime.Parse("2023-12-25 18:39")
			},
			new()
			{
				Name = "Another Game",
				Path = "/Users/Chris/Downloads/project-genesis-preview-flying",
				PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg",
				IsFavorite = false,
				LastOpened = DateTime.Parse("2023-12-24 1:39")
			},
			new()
			{
				Name = "My Project Number Two",
				Path = "/Users/Chris/SomeCoolPath/AndAnotherOne/Not Even So Nice/",
				PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg",
				IsFavorite = false,
				LastOpened = DateTime.Parse("2023-12-20 12:07")
			},
		};
		foreach (var p in ProjectPreviews)
			p.PreviewImage = LoadImage(p.PreviewImagePath);

		OpenProjectCommand = ReactiveCommand.Create(OpenProject,
			this.WhenAnyValue(x => x.SelectedProjectPreview, (Project? x) => x != null));

		AddFromDiskCommand = ReactiveCommand.Create(AddFromDisk);
	}

	private async void AddFromDisk()
	{
		var filesService = App.Current?.Services?.GetService<IFilesService>();
		if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

		var file = await filesService.OpenFileAsync();
		if (file is null) return;

		// Limit the text file to 1MB so that the demo wont lag.
		if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1)
		{
			await using var readStream = await file.OpenReadAsync();
			using var reader = new StreamReader(readStream);
			string s = await reader.ReadToEndAsync();
		}
		else
		{
			throw new Exception("File exceeded 1MB limit.");
		}
	}

	private Bitmap LoadImage(string imagePath)
	{
		using var stream = File.OpenRead(imagePath);
		return new Bitmap(stream);
	}
}