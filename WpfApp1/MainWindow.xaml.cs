using System.Windows;
using WpfApp1.Pages;

namespace WpfApp1;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private Settings _settings;
	public MainWindow()
    {
        InitializeComponent();
		_settings= Settings.LoadSettings();
		Loaded += (_, _) => NavView.Navigate(typeof(DefaultPage));
		Settings.PlayAudio(_settings.AudioPath, _settings.Volume);
	}
}