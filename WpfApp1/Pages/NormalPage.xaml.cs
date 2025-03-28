using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Pages;

/// <summary>
///     NormalPage.xaml 的交互逻辑
/// </summary>
public partial class NormalPage : Page
{
	private const int RowCount = 16;
	private const int ColumnCount = 10;
	private const int ImageSize = 40;

	public NormalPage()
	{
		InitializeComponent();
	}

	private void StartGameButton_Click(object sender, RoutedEventArgs e)
	{
		InitializeGameMap();
	}

	private void InitializeGameMap()
	{
		GameMapGrid.Children.Clear();
		GameMapGrid.RowDefinitions.Clear();
		GameMapGrid.ColumnDefinitions.Clear();

		// Define rows and columns
		for (int i = 0; i < RowCount; i++)
		{
			GameMapGrid.RowDefinitions.Add(new RowDefinition());
		}
		for (int j = 0; j < ColumnCount; j++)
		{
			GameMapGrid.ColumnDefinitions.Add(new ColumnDefinition());
		}

		// Add images to the grid
		for (int i = 0; i < RowCount; i++)
		{
			for (int j = 0; j < ColumnCount; j++)
			{
				Image img = new Image
				{
					Width = ImageSize,
					Height = ImageSize,
					// Set the image source here (e.g., from resources or file)
				};
				img.MouseLeftButtonDown += Image_MouseLeftButtonDown;
				Grid.SetRow(img, i);
				Grid.SetColumn(img, j);
				GameMapGrid.Children.Add(img);
			}
		}
	}

	private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		// Handle image click event and implement the game rules for elimination
	}
}