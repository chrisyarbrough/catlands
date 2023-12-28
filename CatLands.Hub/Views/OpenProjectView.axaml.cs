namespace CatLands.Hub.Views;

using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using ViewModels;

public partial class OpenProjectView : UserControl
{
	public OpenProjectView()
	{
		InitializeComponent();
	}

	private void OnDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (DataContext is OpenProjectViewModel viewModel)
		{
			Observable.Start(() => {}).InvokeCommand(viewModel, vm => vm.OpenProjectCommand);
		}
	}
}