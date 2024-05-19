using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Timers;
using System.Windows.Threading;
using System.IO;

namespace lab_05
{
    internal class TriangleDrawer
    {
        private List<Point> points;
        private Polygon? triangle;
        public double Speed { get; set; }
        private Canvas canvas;

        private bool isStopped = false;
        public int LastStep { get; private set; }
        private List<Point> lastPoints;

        string resultTransformMatrix;

        private double stepsAlongLine;
        private double scaleFactor;

        public TriangleDrawer(List<Point> points, double speed, Canvas canvas)
        {
            this.points = points;
            Speed = speed;
            this.canvas = canvas;
        }

        public void HandleGridSizeChange(double prevVal, double newVal)
        {
            for(int i = 0; i < points.Count; i++)
            {
                Point p = new()
                {
                    X = points[i].X * (newVal / prevVal),
                    Y = points[i].Y * (newVal / prevVal)
                };
                points[i] = p;
            }

            canvas.Children.Clear();
            Draw();
        }

        public void Draw()
        {
            triangle = new()
            {
                Points = new PointCollection(points),
                Fill = Brushes.Blue,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };

            canvas.Children.Clear();
            canvas.Children.Add(triangle);
        }


        public async Task StartAnimation(double steps, double units, double scale, int fps)
        {
            var mw = MainWindow.GetMainWindowController();
            List<Point> newPoints = points.Select(p => p).ToList();
            lastPoints = newPoints.Select(p => p).ToList();
            GenerateResultTransformMatrix(units, scale);
            for (int k = LastStep; k < steps; ++k)
            {
                isStopped = false;
                for (int i = 0; i < newPoints.Count; i++)
                {
                    newPoints[i] = Translate(newPoints[i], units / steps, -units / steps);
                    newPoints[i] = Scale(newPoints[i], Math.Pow(scale, 1 / steps), Math.Pow(scale, 1 / steps));
                }
                points = newPoints;
                mw.Dispatcher.Invoke(() =>
                {
                    Draw();

                });
                await Task.Delay(fps);

                if (isStopped)
                {
                    LastStep = k;
                    return;
                }
            }
            
        }

        public void StopAnimation()
        {
            isStopped = true;
        }

        public void ResetAnimation()
        {
            points = lastPoints;
            Draw();
        }

        private static double[,] MatrixMultiply(double[,] matrix, double[,] col)
        {
            if (matrix.GetLength(1) != col.GetLength(0))
            {
                throw new ArgumentException("Incompatible matrix dimensions for multiplication.");
            }

            int rowsA = matrix.GetLength(0);
            int colsB = col.GetLength(1);
            double[,] result = new double[rowsA, colsB];

            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < matrix.GetLength(1); k++)
                    {
                        result[i, j] += matrix[i, k] * col[k, j];
                    }
                }
            }

            return result;
        }

        private Point Translate(Point p, double dx, double dy)
        {
            double[,] translateMatrix =
            {
                {1, 0, dx },
                {0, 1, dy },
                {0, 0, 1 }
            };

            double[,] pointRaw =
            {
                { p.X },
                { p.Y },
                { 1 }
            };

            double[,] pointTransformed = MatrixMultiply(translateMatrix, pointRaw);

            return new Point(pointTransformed[0,0], pointTransformed[1,0]);

        }

        private Point Scale(Point p, double scaleX, double scaleY)
        {
            double[,] scaleMatrix =
            {
                {scaleX, 0, 0 },
                {0, scaleY, 0 },
                {0, 0, 1 }
            };

            double[,] pointRaw =
            {
                { p.X },
                { p.Y },
                { 1 }
            };

            double[,] pointTransformed = MatrixMultiply(scaleMatrix, pointRaw);

            return new Point(pointTransformed[0, 0], pointTransformed[1, 0]);
        }

        private void GenerateResultTransformMatrix(double units, double scale)
        {
            string output = "|X_New|";
            output += $"|1, 0, {units} |";
            output += $"|{scale}, {0}, {0} |    | x |\n";

            output += "|Y_New| = | ";
            output += $"0, 1, {units} | x | ";
            output += $"0, {scale}, 0  | x | y |\n";

            output += "|1|    | ";
            output += $"{0},  {0},  {1}    |    | ";
            output += $"{0},  {0},  {1}            |    | 1 |\n";

            output += "\n\n";

            double[,] translateMatrix =
            {
                {1, 0, units },
                {0, 1, units },
                {0, 0, 1 }
            };

            double[,] scaleMatrix =
            {
                {scale, 0, 0 },
                {0, scale, 0 },
                {0, 0, 1 }
            };

            double[,] res = MatrixMultiply(translateMatrix, scaleMatrix);

            for(int i = 0; i < res.GetLength(0); ++i)
            {
                for(int k = 0; k < res.GetLength(1); ++k)
                {
                    output += $"{res[i, k]} ";
                }
                output += "\n";
            }

            resultTransformMatrix = output;
        }

        public void SendResultTransformMatrixToFile()
        {
            File.WriteAllText("result.txt", resultTransformMatrix);
        }
    }
}
