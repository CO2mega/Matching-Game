using System;
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
			ControlButtons.Visibility = Visibility.Collapsed;

			// 订阅页面的 SizeChanged 事件
			this.SizeChanged += Page_SizeChanged;
		}

		private void StartGameButton_Click(object sender, RoutedEventArgs e)
		{
			GameModeComboBox.Visibility = Visibility.Collapsed;
			StartGameButton.Visibility = Visibility.Collapsed;

			SetGameDifficulty();
			InitializeGameMap();

			TimeProgressBar.Maximum = 300; 
			TimeProgressBar.Value = 300;

			GameMapGrid.Visibility = Visibility.Visible;
			ControlButtons.Visibility = Visibility.Visible;
			StartTimer();
		}

		private void SetGameDifficulty()
		{
			switch (GameModeComboBox.SelectedIndex)
			{
				case 0:
					rowCount = 8;
					columnCount = 8;
					break;
				case 1:
					rowCount = 12;
					columnCount = 12;
					break;
				case 2: 
					rowCount = 16;
					columnCount = 16;
					break;
				default:
					rowCount = 10;
					columnCount = 10;
					break;
			}

			// 初始化 GameMapGrid 的行和列
			GameMapGrid.RowDefinitions.Clear();
			GameMapGrid.ColumnDefinitions.Clear();
			for (int i = 0; i < rowCount; i++)
			{
				GameMapGrid.RowDefinitions.Add(new RowDefinition());
			}
			for (int j = 0; j < columnCount; j++)
			{
				GameMapGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}

		private void InitializeGameMap()
		{
			GameMapGrid.Children.Clear();
			GameMapGrid.RowDefinitions.Clear();
			GameMapGrid.ColumnDefinitions.Clear();
			firstSelected = null;
			secondSelected = null;
			lastClickedPosition = new Point(-1, -1);


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

			// 确保图片成对分配
			var imageIndices = new List<int>();
			int pairsPerImage = totalCells / (imageFiles.Length * 2);
			int remainingPairs = (totalCells / 2) % imageFiles.Length;

			for (int i = 0; i < imageFiles.Length; i++)
			{

				for (int j = 0; j < pairsPerImage * 2; j++)

					imageIndices.Add(i);



				if (remainingPairs > 0)
				{
					imageIndices.Add(i);
					imageIndices.Add(i);
					remainingPairs--;
				}
			}

			// 随机打乱图片索引
			var random = new Random();
			imageIndices = imageIndices.OrderBy(_ => random.Next()).ToList();

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

			
			if (lastClickedPosition.X == clickedRow && lastClickedPosition.Y == clickedColumn)
			{
				
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
						// 显示确认对话框
						if (MessageBox.Show($"你胜利了！用时: {300 - timeLeft} 秒\n是否重新开始？", "游戏胜利", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							ResetGame(); // 用户确认后重置游戏
						}
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
			// 获取 GameMapGrid 的单元格宽度和高度
			double cellWidth = GameMapGrid.ActualWidth / columnCount;
			double cellHeight = GameMapGrid.ActualHeight / rowCount;

			// 获取图像的行列位置
			int row1 = Grid.GetRow(img1);
			int col1 = Grid.GetColumn(img1);
			int row2 = Grid.GetRow(img2);
			int col2 = Grid.GetColumn(img2);

			// 获取消除路径的点列表
			List<Point> pathPoints = GetEliminationPath(row1, col1, row2, col2);

			// 如果路径点少于 2 个，直接返回
			if (pathPoints.Count < 2)
			{
				return;
			}

			// 遍历路径点，逐段绘制线条
			for (int i = 0; i < pathPoints.Count - 1; i++)
			{
				// 计算每个点的实际位置
				double x1 = pathPoints[i].X * cellWidth + cellWidth / 2;
				double y1 = pathPoints[i].Y * cellHeight + cellHeight / 2;
				double x2 = pathPoints[i + 1].X * cellWidth + cellWidth / 2;
				double y2 = pathPoints[i + 1].Y * cellHeight + cellHeight / 2;

				// 创建线条
				Line line = new Line
				{
					X1 = x1,
					Y1 = y1,
					X2 = x2,
					Y2 = y2,
					Stroke = Brushes.Red,
					StrokeThickness = 2
				};

				// 将线条添加到 LineCanvas
				LineCanvas.Children.Add(line);

				// 设置定时器移除线条
				DispatcherTimer lineTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
				lineTimer.Tick += (s, e) =>
				{
					LineCanvas.Children.Remove(line);
					lineTimer.Stop();
				};
				lineTimer.Start();
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
			
			if (row1 != row2 && col1 != col2)
			{
				return (IsEmpty(row1, col1, row1, col2) && IsEmpty(row1, col2, row2, col2)) ||
					   (IsEmpty(row1, col1, row2, col1) && IsEmpty(row2, col1, row2, col2));
			}
			return false;
		}

		private bool CanConnectWithThreeLines(int row1, int col1, int row2, int col2)
		{
			
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

				// 更新计时器文本
				TimerTextBlock.Text = $"剩余时间: {timeLeft / 60:D2}:{timeLeft % 60:D2}";

				// 更新进度条
				TimeProgressBar.Value = timeLeft;
			}
			else
			{
				timer.Stop();
				// 显示确认对话框
				if (MessageBox.Show("时间到！是否重新开始？", "游戏结束", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					ResetGame(); // 用户确认后重置游戏
				}
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
		private void GameMapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
			LineCanvas.Width = GameMapGrid.ActualWidth;
			LineCanvas.Height = GameMapGrid.ActualHeight;

			
			if (rowCount > 0 && columnCount > 0)
			{
				ImageSize = (int)Math.Min((GameMapGrid.ActualWidth / columnCount), (GameMapGrid.ActualHeight / rowCount));
				if (ImageSize <= 0)
				{
					ImageSize = 40; 
				}
			}
		}
		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
			GameMapGrid.Width = e.NewSize.Width * 0.8;
			GameMapGrid.Height = e.NewSize.Height * 0.8; 

			
			LineCanvas.Width = GameMapGrid.Width;
			LineCanvas.Height = GameMapGrid.Height;

		
			if (rowCount > 0 && columnCount > 0)
			{
				ImageSize = (int)Math.Min((GameMapGrid.Width / columnCount), (GameMapGrid.Height / rowCount));
				if (ImageSize <= 0)
				{
					ImageSize = 40; 
				}
			}
		}
		private void ResetGame()
		{
			// 停止计时器
			timer.Stop();

			// 重置计时器和进度条
			timeLeft = 300; // 示例：5 分钟
			TimeProgressBar.Value = TimeProgressBar.Maximum;

			// 清空选择状态
			firstSelected = null;
			secondSelected = null;
			lastClickedPosition = new Point(-1, -1);

			// 清空游戏网格
			GameMapGrid.Children.Clear();
			GameMapGrid.RowDefinitions.Clear();
			GameMapGrid.ColumnDefinitions.Clear();

			// 重置 UI 状态
			GameModeComboBox.Visibility = Visibility.Visible;
			StartGameButton.Visibility = Visibility.Visible;
			GameMapGrid.Visibility = Visibility.Collapsed;
			ControlButtons.Visibility = Visibility.Collapsed;

			// 清空绘制的线条
			LineCanvas.Children.Clear();
		}
	}
}