using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace WpfApp1
{
	public class Settings
	{
		private static string _settingsFilePath = "settings.config";
		private static MediaPlayer _mediaPlayer = new MediaPlayer();

		public string AudioPath { get; set; }
		public double Volume { get; set; }

		public static Settings LoadSettings()
		{
			if (File.Exists(_settingsFilePath))
			{
				var settingsJson = File.ReadAllText(_settingsFilePath);
				var settings = JsonSerializer.Deserialize<Settings>(settingsJson);

				if (!string.IsNullOrEmpty(settings?.AudioPath))
				{
					//PlayAudio(settings.AudioPath, settings.Volume);
				}
				else
				{
					var defaultAudioPath = GetDefaultAudioPath();
					if (!string.IsNullOrEmpty(defaultAudioPath))
					{
						settings.AudioPath = defaultAudioPath;
						//PlayAudio(defaultAudioPath, settings.Volume);
						settings.SaveSettings();
					}
				}

				return settings;
			}
			else
			{
				var defaultAudioPath = GetDefaultAudioPath();
				var settings = new Settings
				{
					AudioPath = defaultAudioPath,
					Volume = 50
				};

				if (!string.IsNullOrEmpty(defaultAudioPath))
				{
					//PlayAudio(defaultAudioPath, settings.Volume);
					settings.SaveSettings();
				}

				return settings;
			}
		}

		public void SaveSettings()
		{
			var settingsJson = JsonSerializer.Serialize(this);
			File.WriteAllText(_settingsFilePath, settingsJson);
		}

		private static string GetDefaultAudioPath()
		{
			var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
			if (Directory.Exists(assetsPath))
			{
				var audioFiles = Directory.GetFiles(assetsPath, "*.mp3")
					.Concat(Directory.GetFiles(assetsPath, "*.wav")).ToArray();
				if (audioFiles.Length > 0)
				{
					return audioFiles[0];
				}
			}
			return string.Empty;
		}

		public static void PlayAudio(string audioPath, double volume)
		{
			_mediaPlayer.Open(new Uri(audioPath));
			_mediaPlayer.Volume = volume / 100.0;
			_mediaPlayer.Play();
		}

		public void UpdateVolume(double volume)
		{
			Volume = volume;
			_mediaPlayer.Volume = volume / 100.0;
			SaveSettings();
		}
	}
}

