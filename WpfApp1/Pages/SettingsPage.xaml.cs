using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace WpfApp1.Pages
{
	public partial class SettingsPage : Page
	{
		private Settings _settings;

		public SettingsPage()
		{
			InitializeComponent();
			_settings = Settings.LoadSettings();
			AudioPathTextBox.Text = _settings.AudioPath;
			VolumeSlider.Value = _settings.Volume;
		}

		private void BrowseAudioPath_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*",
				Title = "选择音频文件位置"
			};

			if (dialog.ShowDialog() == true)
			{
				_settings.AudioPath = dialog.FileName;
				AudioPathTextBox.Text = dialog.FileName;
				_settings.SaveSettings();
				Settings.PlayAudio(dialog.FileName, _settings.Volume);
			}
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_settings != null)
			{
				_settings.UpdateVolume(e.NewValue);
				_settings.SaveSettings();
			}
		}
	}
}


