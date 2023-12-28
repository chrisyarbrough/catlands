namespace CatLands.Hub.ViewModels;

using System;
using System.Collections.ObjectModel;

public class DesignerOpenProjectViewModel : OpenProjectViewModel
{
	public DesignerOpenProjectViewModel()
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
	}
}