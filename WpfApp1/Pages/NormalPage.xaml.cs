﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace WpfApp1.Pages
{
	/// <summary>
	///     NormalPage.xaml 的交互逻辑
	/// </summary>
	public partial class NormalPage : Page
	{
		private int rowCount;
		private int columnCount;
		private int ImageSize = 40;
		private DispatcherTimer timer;
		private int timeLeft;
		private Image firstSelected;
		private Image secondSelected;
		private Point lastClickedPosition;

		public NormalPage()
		{
			InitializeComponent();
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += Timer_Tick;
			GameMapGrid.Visibility = Visibility.Collapsed;
			ControlButtons.Visibility = Visibility.Collapsed; // Initially hide the control buttons
		}

		private void StartGameButton_Click(object sender, RoutedEventArgs e)
		{
			GameModeComboBox.Visibility = Visibility.Collapsed;
			StartGameButton.Visibility = Visibility.Collapsed;

			SetGameDifficulty();
			InitializeGameMap();
			ImageSize = (int)Math.Min((GameMapGrid.Width / columnCount), (GameMapGrid.Height / rowCount));
			if (ImageSize <= 0)
			{
				ImageSize = 40; // 设置一个默认值
			}
			GameMapGrid.Visibility = Visibility.Visible;
			ControlButtons.Visibility = Visibility.Visible; // Show control buttons when game starts
			StartTimer();
		}

		private void SetGameDifficulty()
		{
			switch (GameModeComboBox.SelectedIndex)
			{
				case 0: // 简单
					rowCount = 8;
					columnCount = 8;
					break;
				case 1: // 普通
					rowCount = 12;
					columnCount = 12;
					break;
				case 2: // 困难
					rowCount = 16;
					columnCount = 16;
					break;
				default:
					rowCount = 10;
					columnCount = 10;
					break;
			}
			GameMapGrid.Width = columnCount * ImageSize;
			GameMapGrid.Height = rowCount * ImageSize;
			ImageSize = (int)Math.Min((GameMapGrid.Width / columnCount), (GameMapGrid.Height / rowCount));
			if (ImageSize <= 0)
			{
				ImageSize = 40; // 设置一个默认值
			}
			LineCanvas.Width = GameMapGrid.Width;
			LineCanvas.Height = GameMapGrid.Height;
		}

		private void InitializeGameMap()
		{
			GameMapGrid.Children.Clear();
			GameMapGrid.RowDefinitions.Clear();
			GameMapGrid.ColumnDefinitions.Clear();
			firstSelected = null;
			secondSelected = null;
			lastClickedPosition = new Point(-1, -1); // Initialize to an invalid position

			// 定义行和列
			for (int i = 0; i < rowCount; i++)
			{
				GameMapGrid.RowDefinitions.Add(new RowDefinition());
			}
			for (int j = 0; j < columnCount; j++)
			{
				GameMapGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			int totalCells = rowCount * columnCount;
			

			// 获取图像文件夹中的所有图像
			var imageFiles = Directory.GetFiles("Assets/Icons", "*.png");
			if (imageFiles.Length == 0)
			{
				throw new InvalidOperationException("No images found in the Assets/Icons folder.");
			}

			// 分块分配每个图片的数量
			var imageDistribution = new int[imageFiles.Length];
			int chunkSize = totalCells / imageFiles.Length;
			for (int i = 0; i < imageFiles.Length; i++)
			{
				imageDistribution[i] = chunkSize;
			}

			// 如果总格子数不能被图像数整除，分配剩余的格子
			int remainingCells = totalCells % imageFiles.Length;
			var random = new Random();
			while (remainingCells > 0)
			{
				int index = random.Next(imageFiles.Length);
				imageDistribution[index] += 2; // 确保每个图片的数量为偶数
				remainingCells -= 2;
			}

			// 创建图像列表
			var imageIndices = imageFiles.SelectMany((file, index) => Enumerable.Repeat(index, imageDistribution[index]))
										 .OrderBy(_ => Guid.NewGuid())
										 .ToList();

			// 向网格添加图像
			for (int i = 0; i < totalCells; i++)
			{
				int row = i / columnCount;
				int col = i % columnCount;

				Image img = new Image
				{
					Width = ImageSize,
					Height = ImageSize,
					Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/Icons/{System.IO.Path.GetFileName(imageFiles[imageIndices[i]])}")),
					Stretch = Stretch.Uniform
				};
				img.MouseLeftButtonDown += Image_MouseLeftButtonDown;
				Grid.SetRow(img, row);
				Grid.SetColumn(img, col);
				GameMapGrid.Children.Add(img);
			}
		}

		private void ShuffleButton_Click(object sender, RoutedEventArgs e)
		{
			// 获取当前所有图像和空格
			var elements = GameMapGrid.Children.Cast<UIElement>().ToList();

			// 打乱顺序
			var random = new Random();
			var shuffledElements = elements.OrderBy(_ => random.Next()).ToList();

			// 清空网格并重新添加打乱顺序后的图像和空格
			GameMapGrid.Children.Clear();
			for (int i = 0; i < shuffledElements.Count; i++)
			{
				int row = i / columnCount;
				int col = i % columnCount;

				var element = shuffledElements[i];
				Grid.SetRow(element, row);
				Grid.SetColumn(element, col);
				GameMapGrid.Children.Add(element);
			}
		}

		private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Image clickedImage = sender as Image;
			int clickedRow = Grid.GetRow(clickedImage);
			int clickedColumn = Grid.GetColumn(clickedImage);

			// 检查是否点击了相同的位置
			if (lastClickedPosition.X == clickedRow && lastClickedPosition.Y == clickedColumn)
			{
				// 如果点击了相同的位置，则重置选择
				if (firstSelected != null)
				{
					RemoveBorderEffect(firstSelected);
					firstSelected = null;
				}
				if (secondSelected != null)
				{
					RemoveBorderEffect(secondSelected);
					secondSelected = null;
				}
				return;
			}

			lastClickedPosition = new Point(clickedRow, clickedColumn);

			if (firstSelected == null)
			{
				firstSelected = clickedImage;
				AddBorderEffect(firstSelected);
			}
			else if (secondSelected == null)
			{
				secondSelected = clickedImage;
				AddBorderEffect(secondSelected);
				CheckAndEliminateImages();
			}
		}

		private void AddBorderEffect(Image image)
		{
			RemoveBorderEffect(image); // 移除任何现有的边框效果

			Border border = new Border
			{
				BorderBrush = Brushes.Red,
				BorderThickness = new Thickness(2),
				Width = ImageSize,
				Height = ImageSize
			};

			int row = Grid.GetRow(image);
			int column = Grid.GetColumn(image);

			GameMapGrid.Children.Remove(image); // 从当前父级中移除图像
			border.Child = image; // 将图像添加到边框中

			Grid.SetRow(border, row);
			Grid.SetColumn(border, column);
			GameMapGrid.Children.Add(border); // 将边框（包含图像）添加到网格中
		}

		private void RemoveBorderEffect(Image image)
		{
			Border parentBorder = null;

			foreach (UIElement element in GameMapGrid.Children)
			{
				if (element is Border border && border.Child == image)
				{
					parentBorder = border;
					break;
				}
			}

			if (parentBorder != null)
			{
				int row = Grid.GetRow(parentBorder);
				int column = Grid.GetColumn(parentBorder);

				parentBorder.Child = null; // 断开图像与边框的连接
				GameMapGrid.Children.Remove(parentBorder);
				GameMapGrid.Children.Add(image);

				Grid.SetRow(image, row);
				Grid.SetColumn(image, column);
			}
		}

		private void CheckAndEliminateImages()
		{
			if (firstSelected != null && secondSelected != null)
			{
				// 检查图像是否相同
				if (!AreImagesSame(firstSelected, secondSelected))
				{
					// 如果图像不相同，则重置选择
					RemoveBorderEffect(firstSelected);
					RemoveBorderEffect(secondSelected);
					firstSelected = null;
					secondSelected = null;
					return;
				}

				// 获取所选图像的位置
				int row1 = Grid.GetRow(firstSelected);
				int col1 = Grid.GetColumn(firstSelected);
				int row2 = Grid.GetRow(secondSelected);
				int col2 = Grid.GetColumn(secondSelected);

				// 检查是否可以消除图像
				if (CanEliminate(row1, col1, row2, col2))
				{
					DrawLineBetweenImages(firstSelected, secondSelected);

					// 从父级中移除图像
					RemoveImageFromParent(firstSelected);
					RemoveImageFromParent(secondSelected);

					// 将网格标记为空
					AddEmptyGridCell(row1, col1);
					AddEmptyGridCell(row2, col2);

					// 检查是否所有图像都已消除
					if (GameMapGrid.Children.OfType<Image>().Count() == 0)
					{
						timer.Stop();
						MessageBox.Show($"你胜利了！用时: {300 - timeLeft} 秒");
					}
				}

				// 重置选择
				RemoveBorderEffect(firstSelected);
				RemoveBorderEffect(secondSelected);
				firstSelected = null;
				secondSelected = null;
				lastClickedPosition = new Point(-1, -1); // 重置最后点击的位置
			}
		}

		private bool AreImagesSame(Image img1, Image img2)
		{
			return img1.Source.ToString() == img2.Source.ToString();
		}

		private void RemoveImageFromParent(Image image)
		{
			Border parentBorder = null;
			foreach (UIElement element in GameMapGrid.Children)
			{
				if (element is Border border && border.Child == image)
				{
					parentBorder = border;
					break;
				}
			}

			if (parentBorder != null)
			{
				parentBorder.Child = null; // 断开图像与边框的连接
				GameMapGrid.Children.Remove(parentBorder); // 从网格中移除边框
			}
			else
			{
				GameMapGrid.Children.Remove(image); // 如果图像不在边框内，则直接从网格中移除图像
			}
		}

		private void DrawLineBetweenImages(Image img1, Image img2)
		{
			int row1 = Grid.GetRow(img1);
			int col1 = Grid.GetColumn(img1);
			int row2 = Grid.GetRow(img2);
			int col2 = Grid.GetColumn(img2);

			List<Point> pathPoints = GetEliminationPath(row1, col1, row2, col2);

			if (pathPoints.Count > 1)
			{
				for (int i = 0; i < pathPoints.Count - 1; i++)
				{
					Line line = new Line
					{
						X1 = pathPoints[i].X * ImageSize + ImageSize / 2,
						Y1 = pathPoints[i].Y * ImageSize + ImageSize / 2,
						X2 = pathPoints[i + 1].X * ImageSize + ImageSize / 2,
						Y2 = pathPoints[i + 1].Y * ImageSize + ImageSize / 2,
						Stroke = Brushes.Red,
						StrokeThickness = 2
					};

					LineCanvas.Children.Add(line);
					DispatcherTimer lineTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
					lineTimer.Tick += (s, e) =>
					{
						LineCanvas.Children.Remove(line);
						lineTimer.Stop();
					};
					lineTimer.Start();
				}
			}
		}

		private List<Point> GetEliminationPath(int row1, int col1, int row2, int col2)
		{
			List<Point> pathPoints = new List<Point>();

			if (CanConnectWithOneLine(row1, col1, row2, col2))
			{
				pathPoints.Add(new Point(col1, row1));
				pathPoints.Add(new Point(col2, row2));
			}
			else if (CanConnectWithTwoLines(row1, col1, row2, col2))
			{
				if (row1 != row2 && col1 != col2)
				{
					if (IsEmpty(row1, col1, row1, col2) && IsEmpty(row1, col2, row2, col2))
					{
						pathPoints.Add(new Point(col1, row1));
						pathPoints.Add(new Point(col2, row1));
						pathPoints.Add(new Point(col2, row2));
					}
					else if (IsEmpty(row1, col1, row2, col1) && IsEmpty(row2, col1, row2, col2))
					{
						pathPoints.Add(new Point(col1, row1));
						pathPoints.Add(new Point(col1, row2));
						pathPoints.Add(new Point(col2, row2));
					}
				}
			}
			else if (CanConnectWithThreeLines(row1, col1, row2, col2))
			{
				for (int row = 0; row < rowCount; row++)
				{
					if (row != row1 && row != row2 && IsEmpty(row1, col1, row, col1) && IsEmpty(row, col1, row, col2) && IsEmpty(row, col2, row2, col2))
					{
						pathPoints.Add(new Point(col1, row1));
						pathPoints.Add(new Point(col1, row));
						pathPoints.Add(new Point(col2, row));
						pathPoints.Add(new Point(col2, row2));
						break;
					}
				}
				for (int col = 0; col < columnCount; col++)
				{
					if (col != col1 && col != col2 && IsEmpty(row1, col1, row1, col) && IsEmpty(row1, col, row2, col) && IsEmpty(row2, col, row2, col2))
					{
						pathPoints.Add(new Point(col1, row1));
						pathPoints.Add(new Point(col, row1));
						pathPoints.Add(new Point(col, row2));
						pathPoints.Add(new Point(col2, row2));
						break;
					}
				}
			}

			return pathPoints;
		}

		private bool CanEliminate(int row1, int col1, int row2, int col2)
		{
			return CanConnectWithOneLine(row1, col1, row2, col2) ||
				   CanConnectWithTwoLines(row1, col1, row2, col2) ||
				   CanConnectWithThreeLines(row1, col1, row2, col2);
		}

		private bool CanConnectWithOneLine(int row1, int col1, int row2, int col2)
		{
			// 检查两个图像是否可以通过一条线连接
			if (row1 == row2)
			{
				for (int col = Math.Min(col1, col2) + 1; col < Math.Max(col1, col2); col++)
				{
					if (GameMapGrid.Children.Cast<UIElement>().Any(e => Grid.GetRow(e) == row1 && Grid.GetColumn(e) == col && !(e is Border)))
					{
						return false;
					}
				}
				return true;
			}
			if (col1 == col2)
			{
				for (int row = Math.Min(row1, row2) + 1; row < Math.Max(row1, row2); row++)
				{
					if (GameMapGrid.Children.Cast<UIElement>().Any(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col1 && !(e is Border)))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private bool CanConnectWithTwoLines(int row1, int col1, int row2, int col2)
		{
			// 检查两个图像是否可以通过两条线连接
			if (row1 != row2 && col1 != col2)
			{
				return (IsEmpty(row1, col1, row1, col2) && IsEmpty(row1, col2, row2, col2)) ||
					   (IsEmpty(row1, col1, row2, col1) && IsEmpty(row2, col1, row2, col2));
			}
			return false;
		}

		private bool CanConnectWithThreeLines(int row1, int col1, int row2, int col2)
		{
			// 检查两个图像是否可以通过三条线连接
			for (int row = 0; row < rowCount; row++)
			{
				if (row != row1 && row != row2 && IsEmpty(row1, col1, row, col1) && IsEmpty(row, col1, row, col2) && IsEmpty(row, col2, row2, col2))
				{
					return true;
				}
			}
			for (int col = 0; col < columnCount; col++)
			{
				if (col != col1 && col != col2 && IsEmpty(row1, col1, row1, col) && IsEmpty(row1, col, row2, col) && IsEmpty(row2, col, row2, col2))
				{
					return true;
				}
			}
			return false;
		}

		private bool IsEmpty(int row1, int col1, int row2, int col2)
		{
			if (row1 == row2)
			{
				for (int col = Math.Min(col1, col2); col <= Math.Max(col1, col2); col++)
				{
					if (GameMapGrid.Children.Cast<UIElement>().Any(e => Grid.GetRow(e) == row1 && Grid.GetColumn(e) == col && !(e is Border)))
					{
						return false;
					}
				}
				return true;
			}
			if (col1 == col2)
			{
				for (int row = Math.Min(row1, row2); row <= Math.Max(row1, row2); row++)
				{
					if (GameMapGrid.Children.Cast<UIElement>().Any(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col1 && !(e is Border)))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private void AddEmptyGridCell(int row, int column)
		{
			Border emptyCell = new Border
			{
				BorderBrush = Brushes.Transparent,
				BorderThickness = new Thickness(1),
				Background = Brushes.Transparent,
				Width = ImageSize,
				Height = ImageSize
			};

			Grid.SetRow(emptyCell, row);
			Grid.SetColumn(emptyCell, column);
			GameMapGrid.Children.Add(emptyCell);
		}

		private void StartTimer()
		{
			timeLeft = 300; // 示例：5 分钟
			timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (timeLeft > 0)
			{
				timeLeft--;
				TimerTextBlock.Text = $"剩余时间: {timeLeft / 60:D2}:{timeLeft % 60:D2}";
			}
			else
			{
				timer.Stop();
				MessageBox.Show("时间到！");
				// 处理游戏结束逻辑
			}
		}

		private void PauseButton_Click(object sender, RoutedEventArgs e)
		{
			if (timer.IsEnabled)
			{
				timer.Stop();
				PauseButton.Visibility = Visibility.Collapsed;
				ResumeButton.Visibility = Visibility.Visible;
				GameMapGrid.Visibility = Visibility.Collapsed;
			}
		}

		private void ResumeButton_Click(object sender, RoutedEventArgs e)
		{
			if (!timer.IsEnabled)
			{
				timer.Start();
				PauseButton.Visibility = Visibility.Visible;
				ResumeButton.Visibility = Visibility.Collapsed;
				GameMapGrid.Visibility = Visibility.Visible;
			}
		}
	}
}