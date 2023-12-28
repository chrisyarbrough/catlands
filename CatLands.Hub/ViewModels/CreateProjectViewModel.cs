namespace CatLands.Hub.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using Avalonia.Media.Imaging;
using ReactiveUI;

public class CreateProjectViewModel : ViewModelBase
{
	public ObservableCollection<Project> Templates { get; init; }

	private Project? selectedTemplate;

	public Project? SelectedTemplate
	{
		get => selectedTemplate;
		set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
	}

	public ReactiveCommand<Unit, Unit> CreateProjectCommand { get; init; }
	public Project NewProject { get; init; } = new();

	public CreateProjectViewModel()
	{
		Templates = new ObservableCollection<Project>()
		{
			new() { Name = "Template 1x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
		};
		foreach (var p in Templates)
		{
			p.PreviewImage = LoadImage(p.PreviewImagePath);
		}

		NewProject = new Project();
		CreateProjectCommand = ReactiveCommand.Create(() => { Console.WriteLine("Created!"); },
			this.WhenAnyValue(x => x.NewProject.Name, x => x.NewProject.Path,
				(name, path) => !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(path)));
	}

	protected Bitmap LoadImage(string imagePath)
	{
		using var stream = File.OpenRead(imagePath);
		return new Bitmap(stream);
	}
}

public class DesignerCreateProjectViewModel : CreateProjectViewModel
{
	public DesignerCreateProjectViewModel()
	{
		Templates = new ObservableCollection<Project>()
		{
			new() { Name = "Template 1x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
			new() { Name = "Template 2x", PreviewImagePath = "/Users/Chris/Downloads/maxresdefault.jpg", },
		};
		foreach (var p in Templates)
			p.PreviewImage = LoadImage(p.PreviewImagePath);
		this.RaisePropertyChanged(nameof(SelectedTemplate));
	}
}