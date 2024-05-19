using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace lab_05
{
    public partial class MainWindow : Window
    {
        private double gridSize = 30;
        private double speed;
        private int fps;

        private TriangleDrawer? triangle;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DealWithGrid();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DealWithGrid();
        }

        #region BasicGridSettings
        private void DealWithGrid()
        {
            DrawAxes();
            TransormCoordinateSystem();
        }

        private void DrawAxes()
        {
            axisCanvas.Children.Clear();

            double width = axisCanvas.ActualWidth;
            double height = axisCanvas.ActualHeight;

            Line xAxis = new();
            Line yAxis = new();


            xAxis.Stroke = Brushes.Black;
            yAxis.Stroke = Brushes.Black;

            xAxis.StrokeThickness = 2;
            yAxis.StrokeThickness = 2;

            xAxis.X1 = 0;
            xAxis.Y1 = height / 2;
            xAxis.X2 = width;
            xAxis.Y2 = height / 2;

            yAxis.X1 = width / 2;
            yAxis.Y1 = 0;
            yAxis.X2 = width / 2;
            yAxis.Y2 = height;

            axisCanvas.Children.Add(xAxis);
            axisCanvas.Children.Add(yAxis);

            GenerateAxisNumbers();

            DrawGrid();
        }

        private void DrawGrid()
        {
            double width = axisCanvas.ActualWidth;
            double height = axisCanvas.ActualHeight;

            for (double i = -height / 2; i <= 0; i += gridSize)
            {
                Line line = new Line()
                {
                    Opacity = 0.3,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    X1 = 0,
                    X2 = width,
                    Y1 = i + height,
                    Y2 = i + height
                };

                axisCanvas.Children.Add(line);
            }

            for (double i = height / 2; i >= 0; i -= gridSize)
            {
                Line line = new Line()
                {
                    Opacity = 0.3,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    X1 = 0,
                    X2 = width,
                    Y1 = i,
                    Y2 = i
                };

                axisCanvas.Children.Add(line);
            }

            for (double i = -width / 2; i <= 0; i += gridSize)
            {
                Line line = new Line()
                {
                    Opacity = 0.3,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    X1 = i + width,
                    X2 = i + width,
                    Y1 = 0,
                    Y2 = height
                };

                axisCanvas.Children.Add(line);
            }

            for (double i = width / 2; i >= 0; i -= gridSize)
            {
                Line line = new Line()
                {
                    Opacity = 0.3,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    X1 = i,
                    X2 = i,
                    Y1 = 0,
                    Y2 = height
                };

                axisCanvas.Children.Add(line);
            }
        }

        private void GenerateAxisNumbers()
        {
            double width = axisCanvas.ActualWidth;
            double height = axisCanvas.ActualHeight;

            double quantOfX = width / gridSize;
            double quantOfY = height / gridSize;

            for (int i = (int)(-(quantOfX / 2)); i <= (quantOfX / 2); ++i)
            {
                TextBlock xText = new TextBlock()
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness((i + (quantOfX / 2)) * gridSize, height / 2, 0, 0),
                    FontSize = gridSize / 2.5
                };
                axisCanvas.Children.Add(xText);
            }

            for (int i = (int)((quantOfY / 2)); i >= (-quantOfY / 2); --i)
            {
                TextBlock yText = new TextBlock()
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(width / 2, (-i + (quantOfY / 2)) * gridSize, 0, 0),
                    FontSize = gridSize / 2.5
                };
                axisCanvas.Children.Add(yText);
            }
        }

        private void TransormCoordinateSystem()
        {
            double centerX = axisCanvas.ActualWidth / 2;
            double centerY = axisCanvas.ActualHeight / 2;

            TranslateTransform transform = new TranslateTransform(centerX, centerY);
            mainCanvas.RenderTransform = transform;
        }
        #endregion

        private void GridCell_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            gridSize = e.NewValue;
            DealWithGrid();
            triangle?.HandleGridSizeChange(e.OldValue, e.NewValue);

        }

        private void MotionSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speed = e.NewValue;
        }

        private async void StartMotion_Click(object sender, RoutedEventArgs e)
        {
            if(!double.TryParse(StepsTextBox.Text, out double steps) || !double.TryParse(ScaleTextBox.Text, out double scale)){
                MessageBox.Show("Please, check typed parameters!");
                return;
            }
            if(triangle is null)
            {
                return;
            }

            //1 - speed, 2 - cells to move, 3 - scaleFactor, 4 - fps
            await triangle.StartAnimation(1000/speed, steps*gridSize, scale, (1000/fps));
        }

        private void StopMotion_Click(object sender, RoutedEventArgs e)
        {
            if (triangle is null)
            {
                return;
            }
            triangle.StopAnimation();
        }

        private void ResetMotion_Click(object sender, RoutedEventArgs e)
        {
            if (triangle is null)
            {
                return;
            }
            triangle.ResetAnimation();
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            double maxWidth = Math.Max(mainCanvas.ActualWidth, axisCanvas.ActualWidth);
            double maxHeight = Math.Max(mainCanvas.ActualHeight, axisCanvas.ActualHeight);

            // Create a RenderTargetBitmap to capture the combined content
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)maxWidth, (int)maxHeight, 96, 96, PixelFormats.Pbgra32);

            // Render both canvases onto the bitmap (order doesn't matter here)
            renderTargetBitmap.Render(axisCanvas);
            renderTargetBitmap.Render(mainCanvas);
            string fileName = "canvas_image.png"; // Replace with desired filename and extension (png, jpg, etc.)
            using (FileStream outputStream = File.Create(fileName))
            {
                BitmapEncoder encoder = new PngBitmapEncoder(); // Choose encoder based on desired format
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(outputStream);
            }

        }

        private void SaveMatrix_Click(object sender, RoutedEventArgs e)
        {
            if (triangle is null)
            {
                return;
            }
            triangle.SendResultTransformMatrixToFile();
        }

        private void DrawTriangle_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.Children.Clear();

            string[] vertexes =
            {
                Vertex1_TextBox.Text,
                Vertex2_TextBox.Text,
                Vertex3_TextBox.Text,
            };

            List<Point> points = [];
            foreach(var vertex in vertexes)
            {
                string[] aloneCoords = vertex.Split(' ');
                List<double> coords = new();
                foreach(var coord in aloneCoords)
                {
                    if(!double.TryParse(coord, out var c))
                    {
                        MessageBox.Show("Please, check typed coordinates!");
                        return;
                    }
                    coords.Add(c);
                    
                }
                if (coords.Count != 2) return;

                points.Add(new Point(coords[0] * gridSize, coords[1] * (-gridSize)));
            }

            triangle = new(points, speed, mainCanvas);

            triangle.Draw();
        }

        public static Window GetMainWindowController()
        {
            var mn = Application.Current.MainWindow as MainWindow ?? throw new Exception("Can't get MainWindow");
            return mn;
        }

        private void FPS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            fps = (int)e.NewValue;
        }
    }
}