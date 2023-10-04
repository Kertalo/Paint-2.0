using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace Paint_2._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            map = new Bitmap(pictureBox.Width, pictureBox.Height);
            mapPrim = new Bitmap(map.Width, map.Height);
            ClearClick(button1, EventArgs.Empty);
            string directory = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            directory = Directory.GetParent(directory).ToString();
            directory = Directory.GetParent(directory).ToString();
            mapPict = new Bitmap(directory + @"\Images\night.png");
        }

        byte state;
        bool isMouseDown = false;
        int oldX;
        int oldY;

        Bitmap map;
        Bitmap mapPict;
        Bitmap mapPrim;
        List<Point> pointsPrim;

        Color color = Color.Black;

        void DrawLine(int x1, int y1, int x2, int y2, Bitmap map)
        {
            if (x1 < 0 || y1 < 0 || x2 < 0 || y2 < 0 ||
                x1 >= map.Width || y1 >= map.Height ||
                x2 >= map.Width || y2 >= map.Height)
                return;
            int deltaX = Math.Abs(x2 - x1);
            int deltaY = Math.Abs(y2 - y1);
            int signX = x1 < x2 ? 1 : -1;
            int signY = y1 < y2 ? 1 : -1;
            int error = deltaX - deltaY;
            map.SetPixel(x2, y2, color);
            while (x1 != x2 || y1 != y2)
            {
                map.SetPixel(x1, y1, color);
                int error2 = error * 2;
                if (error2 > -deltaY)
                {
                    error -= deltaY;
                    x1 += signX;
                }
                if (error2 < deltaX)
                {
                    error += deltaX;
                    y1 += signY;
                }
            }
        }

        void Fill(int x, int y, Color oldColor)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return;
            if (map.GetPixel(x, y) != oldColor)
                return;
            int startX = x;
            while (map.GetPixel(x, y) == oldColor)
            {
                map.SetPixel(x, y, color);
                x++;
                if (x >= map.Width)
                {
                    x--;
                    break;
                }
            }
            int rightX = x;
            x = startX - 1;
            if (x < 0)
                x++;
            while (map.GetPixel(x, y) == oldColor)
            {
                map.SetPixel(x, y, color);
                x--;
                if (x < 0)
                {
                    x++;
                    break;
                }
            }
            for (int i = x + 1; i < rightX; i++)
            {
                Fill(i, y - 1, oldColor);
                Fill(i, y + 1, oldColor);
            }
        }

        void FillPicture(int x, int y, Color oldColor)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return;
            if (map.GetPixel(x, y) != oldColor)
                return;
            int startX = x;
            while (map.GetPixel(x, y) == oldColor)
            {
                map.SetPixel(x, y, mapPict.GetPixel
                    (x % mapPict.Width, y % mapPict.Height));
                x++;
                if (x >= map.Width)
                {
                    x--;
                    break;
                }
            }
            int rightX = x;
            x = startX - 1;
            if (x < 0)
                x++;
            while (map.GetPixel(x, y) == oldColor)
            {
                map.SetPixel(x, y, mapPict.GetPixel
                    (x % mapPict.Width, y % mapPict.Height));
                x--;
                if (x < 0)
                {
                    x++;
                    break;
                }
            }
            for (int i = x + 1; i < rightX; i++)
            {
                FillPicture(i, y - 1, oldColor);
                FillPicture(i, y + 1, oldColor);
            }
        }

        void FillBorder(int x, int y, Color oldColor)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return;
            while (map.GetPixel(x, y) == oldColor)
            {
                x++;
                if (x >= map.Width)
                    return;
            }
            byte rotation = 2;
            List<Point> points = new List<Point>();
            while (true)
            {
                if (points.Contains(new Point(x, y)))
                    break;
                points.Add(new Point(x, y));
                map.SetPixel(x, y, color);
                if (rotation == 2)
                {
                    if (map.GetPixel(x - 1, y) != oldColor)
                    {
                        x -= 1;
                        rotation = 3;
                    }
                    else if (map.GetPixel(x - 1, y + 1) != oldColor)
                    {
                        x -= 1;
                        y += 1;
                    }
                    else if (map.GetPixel(x, y + 1) != oldColor)
                    {
                        y += 1;
                    }
                    else if (map.GetPixel(x + 1, y + 1) != oldColor)
                    {
                        x += 1;
                        y += 1;
                    }
                    else if (map.GetPixel(x + 1, y) != oldColor)
                    {
                        x += 1;
                        rotation = 1;
                    }
                    else if (map.GetPixel(x - 1, y - 1) != oldColor)
                    {
                        x -= 1;
                        y -= 1;
                        rotation = 3;
                    }
                    else if (map.GetPixel(x + 1, y - 1) != oldColor)
                    {
                        x += 1;
                        y -= 1;
                        rotation = 1;
                    }
                    else
                        rotation = 3;
                }
                else if (rotation == 3)
                {
                    if (map.GetPixel(x, y - 1) != oldColor)
                    {
                        y -= 1;
                        rotation = 0;
                    }
                    else if (map.GetPixel(x - 1, y - 1) != oldColor)
                    {
                        x -= 1;
                        y -= 1;
                    }
                    else if (map.GetPixel(x - 1, y) != oldColor)
                    {
                        x -= 1;
                    }
                    else if (map.GetPixel(x - 1, y + 1) != oldColor)
                    {
                        x -= 1;
                        y += 1;
                    }
                    else if (map.GetPixel(x, y + 1) != oldColor)
                    {
                        y += 1;
                        rotation = 2;
                    }
                    else if (map.GetPixel(x + 1, y - 1) != oldColor)
                    {
                        x += 1;
                        y -= 1;
                        rotation = 0;
                    }
                    else if (map.GetPixel(x + 1, y + 1) != oldColor)
                    {
                        x += 1;
                        y += 1;
                        rotation = 2;
                    }
                    else
                        rotation = 2;
                }
                else if (rotation == 0)
                {
                    if (map.GetPixel(x + 1, y) != oldColor)
                    {
                        x += 1;
                        rotation = 1;
                    }
                    else if (map.GetPixel(x + 1, y - 1) != oldColor)
                    {
                        x += 1;
                        y -= 1;
                    }
                    else if (map.GetPixel(x, y - 1) != oldColor)
                    {
                        y -= 1;
                    }
                    else if (map.GetPixel(x - 1, y - 1) != oldColor)
                    {
                        x -= 1;
                        y -= 1;
                    }
                    else if (map.GetPixel(x - 1, y) != oldColor)
                    {
                        x -= 1;
                        rotation = 3;
                    }
                    else if (map.GetPixel(x + 1, y + 1) != oldColor)
                    {
                        x += 1;
                        y += 1;
                        rotation = 1;
                    }
                    else if (map.GetPixel(x - 1, y + 1) != oldColor)
                    {
                        x -= 1;
                        y += 1;
                        rotation = 3;
                    }
                    else
                        rotation = 1;
                }
                else if (rotation == 1)
                {
                    if (map.GetPixel(x, y + 1) != oldColor)
                    {
                        y += 1;
                        rotation = 2;
                    }
                    else if (map.GetPixel(x + 1, y + 1) != oldColor)
                    {
                        x += 1;
                        y += 1;
                    }
                    else if (map.GetPixel(x + 1, y) != oldColor)
                    {
                        x += 1;
                    }
                    else if (map.GetPixel(x + 1, y - 1) != oldColor)
                    {
                        x += 1;
                        y -= 1;
                    }
                    else if (map.GetPixel(x, y - 1) != oldColor)
                    {
                        y -= 1;
                        rotation = 0;
                    }
                    else if (map.GetPixel(x - 1, y + 1) != oldColor)
                    {
                        x -= 1;
                        y += 1;
                        rotation = 2;
                    }
                    else if (map.GetPixel(x - 1, y - 1) != oldColor)
                    {
                        x -= 1;
                        y -= 1;
                        rotation = 0;
                    }
                    else
                        rotation = 2;
                }
            }
        }

        Bitmap MapAndPrimitive()
        {
            Bitmap newMap = new Bitmap(map.Width, map.Height);
            for (int i = 0; i < map.Width; i++)
                for (int j = 0; j < map.Height; j++)
                    if (mapPrim.GetPixel(i, j).ToArgb() != Color.White.ToArgb())
                        newMap.SetPixel(i, j, mapPrim.GetPixel(i, j));
                    else
                        newMap.SetPixel(i, j, map.GetPixel(i, j));
            return newMap;
        }

        int[] Multiplication(int[] coordinates, int[,] matrix)
        {
            int[] res = new int[coordinates.Length];
            for (int i = 0; i < matrix.GetLength(1); i++)
                for (int j = 0; j < coordinates.Length; j++)
                    res[i] += coordinates[j] * matrix[j, i];
            return res;
        }

        int[] MultiplicationD(int[] coordinates, double[,] matrix, Point center)
        {
            coordinates[0] -= center.X;
            coordinates[1] -= center.Y;
            int[] res = new int[coordinates.Length];
            for (int i = 0; i < matrix.GetLength(1); i++)
                for (int j = 0; j < coordinates.Length; j++)
                    res[i] += (int)(coordinates[j] * matrix[j, i]);
            res[0] += center.X;
            res[1] += center.Y;
            return res;
        }

        void PencilClick(object sender, EventArgs e)
        {
            state = 0;
        }

        void PrimitiveClick(object sender, EventArgs e)
        {
            state = 1;
            pointsPrim = new List<Point>();
        }

        void FillClick(object sender, EventArgs e)
        {
            state = 2;
        }

        void FillPictureClick(object sender, EventArgs e)
        {
            state = 3;
        }

        void BorderClick(object sender, EventArgs e)
        {
            state = 4;
        }

        void ClearClick(object sender, EventArgs e)
        {
            Graphics graphics = Graphics.FromImage(map);
            graphics.Clear(Color.White);
            graphics = Graphics.FromImage(mapPrim);
            graphics.Clear(Color.White);
            CancelClick(sender, e);
            pictureBox.Image = map;
        }

        void CancelClick(object sender, EventArgs e)
        {
            pointsPrim = new List<Point>();
            Graphics graphics = Graphics.FromImage(mapPrim);
            graphics.Clear(Color.White);
            pictureBox.Image = map;
            panel3.Visible = false;
            panel4.Visible = false;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = false;
            if (state == 5 || state == 6)
                state = 1;
        }

        void AcceptClick(object sender, EventArgs e)
        {
            if (state == 5)
            {
                map = MapAndPrimitive();
                CancelClick(sender, e);
                return;
            }
            state = 5;
            int count = pointsPrim.Count;
            if (count > 2)
            {
                panel7.Visible = true;
                DrawLine(pointsPrim[count - 1].X, pointsPrim[count - 1].Y,
                    pointsPrim[0].X, pointsPrim[0].Y, mapPrim);
            }
            pictureBox.Image = MapAndPrimitive();
            if (count == 2)
            {
                panel5.Visible = true;
                panel6.Visible = true;
            }
            panel4.Visible = true;
        }

        void ColorClick(object sender, EventArgs e)
        {
            color = (sender as Button).BackColor;
        }

        Point FindCenter(List<Point> points)
        {
            float sum_x = 0;
            float sum_y = 0;
            int count = points.Count;
            foreach (var p in points)
            {
                sum_x += p.X;
                sum_y += p.Y;
            }
            return new Point((int)sum_x / count, (int)sum_y / count);
        }

        void ShiftClick(object sender, EventArgs e)
        {
            int[,] matrix = new int[3, 3];
            matrix[0, 0] = 1;
            matrix[1, 1] = 1;
            matrix[2, 2] = 1;
            matrix[2, 1] = 0;
            matrix[2, 0] = 0;
            if (Int32.TryParse(textBox1.Text, out int dx))
                matrix[2, 0] = dx;
                
            if (Int32.TryParse(textBox2.Text, out int dy))
                matrix[2, 1] = -dy;
                
            Bitmap newMapPrim = new Bitmap(map.Width, map.Height);
            Graphics graphics = Graphics.FromImage(newMapPrim);
            graphics.Clear(Color.White);
            for (int i = 0; i < map.Width; i++)
                for (int j = 0; j < map.Height; j++)
                {
                    Color c = mapPrim.GetPixel(i, j);
                    if (c.ToArgb() != Color.White.ToArgb())
                    {
                        int[] newXY = Multiplication(new int[] { i, j, 1 }, matrix);
                        if (newXY[0] >= 0 && newXY[1] >= 0 && newXY[0] < map.Width && newXY[1] < map.Height)
                            newMapPrim.SetPixel(newXY[0], newXY[1], mapPrim.GetPixel(i, j));
                    }
                }
            mapPrim = newMapPrim;
            pictureBox.Image = MapAndPrimitive();
        }

        void RotateClick(object sender, EventArgs e)
        {
            Point center = FindCenter(pointsPrim);
            double[,] matrix = new double[3, 3];
            matrix[2, 2] = 1;
            if (!double.TryParse(textBox3.Text, out double angle))
                return;
            matrix[0, 0] = Math.Cos(angle * Math.PI / 180);
            matrix[0, 1] = Math.Sin(angle * Math.PI / 180);
            matrix[1, 0] = -Math.Sin(angle * Math.PI / 180);
            matrix[1, 1] = Math.Cos(angle * Math.PI / 180);
            Bitmap newMapPrim = new Bitmap(map.Width, map.Height);
            Graphics graphics = Graphics.FromImage(newMapPrim);
            graphics.Clear(Color.White);
            for (int i = 0; i < pointsPrim.Count; i++)
            {
                int[] newXY = MultiplicationD(new int[] 
                { pointsPrim[i].X, pointsPrim[i].Y, 1 }, matrix, center);
                if (newXY[0] >= 0 && newXY[1] >= 0 && newXY[0] < map.Width && newXY[1] < map.Height)
                    pointsPrim[i] = new Point(newXY[0], newXY[1]);
            }
            for (int i = 0; i < pointsPrim.Count - 1; i++)
                DrawLine(pointsPrim[i].X, pointsPrim[i].Y,
                    pointsPrim[i + 1].X, pointsPrim[i + 1].Y, newMapPrim);
            DrawLine(pointsPrim[pointsPrim.Count - 1].X, pointsPrim[pointsPrim.Count - 1].Y,
                    pointsPrim[0].X, pointsPrim[0].Y, newMapPrim);
            mapPrim = newMapPrim;
            pictureBox.Image = MapAndPrimitive();
        }

        void ScaleShift(object sender, EventArgs e)
        {
            Point center = FindCenter(pointsPrim);
            double[,] matrix = new double[3, 3];
            matrix[2, 2] = 1;
            if (!double.TryParse(textBox4.Text, out double scale))
                return;
            matrix[0, 0] = scale;
            matrix[1, 1] = scale;
            Bitmap newMapPrim = new Bitmap(map.Width, map.Height);
            Graphics graphics = Graphics.FromImage(newMapPrim);
            graphics.Clear(Color.White);
            for (int i = 0; i < pointsPrim.Count; i++)
            {
                int[] newXY = MultiplicationD(new int[]
                { pointsPrim[i].X, pointsPrim[i].Y, 1 }, matrix, center);
                if (newXY[0] >= 0 && newXY[1] >= 0 && newXY[0] < map.Width && newXY[1] < map.Height)
                    pointsPrim[i] = new Point(newXY[0], newXY[1]);
            }
            for (int i = 0; i < pointsPrim.Count - 1; i++)
                DrawLine(pointsPrim[i].X, pointsPrim[i].Y,
                    pointsPrim[i + 1].X, pointsPrim[i + 1].Y, newMapPrim);
            DrawLine(pointsPrim[pointsPrim.Count - 1].X, pointsPrim[pointsPrim.Count - 1].Y,
                    pointsPrim[0].X, pointsPrim[0].Y, newMapPrim);
            mapPrim = newMapPrim;
            pictureBox.Image = MapAndPrimitive();
        }

        void PictureBoxMouseDown(object sender, MouseEventArgs e)
        {
            if (state == 5)
                return;

            if (state == 0)
            {
                isMouseDown = true;
                oldX = e.X;
                oldY = e.Y;
            }
            else if (state == 1)
            {
                pointsPrim.Add(new Point(e.X, e.Y));
                int count = pointsPrim.Count;
                if (count > 1)
                    DrawLine(pointsPrim[count - 2].X, pointsPrim[count - 2].Y,
                        pointsPrim[count - 1].X, pointsPrim[count - 1].Y, mapPrim);
                else
                {
                    mapPrim.SetPixel(e.X, e.Y, color);
                    panel3.Visible = true;
                }
                pictureBox.Image = MapAndPrimitive();
            }
            else if (state == 2)
            {
                Fill(e.X, e.Y, map.GetPixel(e.X, e.Y));
                pictureBox.Image = map;
            }
            else if (state == 3)
            {
                FillPicture(e.X, e.Y, map.GetPixel(e.X, e.Y));
                pictureBox.Image = map;
            }
            else if (state == 4)
            {
                FillBorder(e.X, e.Y, map.GetPixel(e.X, e.Y));
                pictureBox.Image = map;
            }
            else if (state == 6)
            {
                pointsPrim.Add(new Point(e.X, e.Y));
                mapPrim.SetPixel(e.X, e.Y, color);
                if (pointsPrim.Count >= 4)
                {
                    state = 5;
                    DrawLine(pointsPrim[2].X, pointsPrim[2].Y,
                        pointsPrim[3].X, pointsPrim[3].Y, mapPrim);
                    var point = CrossLines(new Tuple<PointF, PointF>(pointsPrim[0], pointsPrim[1]),
                        new Tuple<PointF, PointF>(pointsPrim[2], pointsPrim[3]));
                    if (point.Item1)
                        label1.Text = "(" + (int)point.Item2.X 
                            + ", " + (int)point.Item2.Y + ")";
                    else
                        label1.Text = "Fail";
                }
                pictureBox.Image = MapAndPrimitive();
            }
            else if (state == 7)
            {
                pointsPrim.Add(new Point(e.X, e.Y));
                mapPrim.SetPixel(e.X, e.Y, color);
                state = 5;
                PointLinePosition();
                pictureBox.Image = MapAndPrimitive();
            }
            else if (state == 8)
            {
                mapPrim.SetPixel(e.X, e.Y, color);
                state = 5;
                if (IsPointInside(new Point(e.X, e.Y)))
                    label3.Text = "Inside";
                else
                    label3.Text = "Outside";
                pictureBox.Image = MapAndPrimitive();
            }
        }

        void PictureBoxMouseUp(object sender, MouseEventArgs e)
        {
            if (state == 0)
                isMouseDown = false;
        }

        void PictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (state != 0 || !isMouseDown)
                return;
            DrawLine(oldX, oldY, e.X, e.Y, map);
            pictureBox.Image = map;
            oldX = e.X;
            oldY = e.Y;
        }

        Tuple<bool, Point> CrossLines(Tuple<PointF, PointF> line1,
            Tuple<PointF, PointF> line2)
        {
            PointF s1 = new PointF();
            PointF s2 = new PointF();

            s1.X = line1.Item2.X - line1.Item1.X;
            s1.Y = line1.Item2.Y - line1.Item1.Y;
            s2.X = line2.Item2.X - line2.Item1.X;
            s2.Y = line2.Item2.Y - line2.Item1.Y;

            float s = (-s1.Y * (line1.Item1.X - line2.Item1.X)
                + s1.X * (line1.Item1.Y - line2.Item1.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
            float t = (s2.X * (line1.Item1.Y - line2.Item1.Y)
                - s2.Y * (line1.Item1.X - line2.Item1.X)) / (-s2.X * s1.Y + s1.X * s2.Y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                double x = line1.Item1.X + (t * s1.X);
                double y = line1.Item1.Y + (t * s1.Y);
                return new Tuple<bool, Point>
                    (true, new Point((int)x, (int)y));
            }
            else
                return new Tuple<bool, Point>
                    (false, new Point(-1, -1));
        }

        void CrossClick(object sender, EventArgs e)
        {
            state = 6;
        }

        void PointLinePosition()
        {
            var line = new Tuple<PointF, PointF>(pointsPrim[0], pointsPrim[1]);
            PointF point = pointsPrim[pointsPrim.Count() - 1];
            var p = (line.Item2.X - line.Item1.X) * (point.Y - line.Item1.Y)
                - (line.Item2.Y - line.Item1.Y) * (point.X - line.Item1.X);
            if (p > 0)
                label2.Text = "Right";
            else
                label2.Text = "Left";
        }

        void LinePositionLineClick(object sender, EventArgs e)
        {
            state = 7;
        }

        bool IsPointInside(Point point)
        {
            int count = 0;
            Point point2 = new Point(0, point.Y);
            //DrawLine(point.X, point.Y, point2.X, point2.Y, map);
            pictureBox.Image = map;
            Tuple<bool, Point> isCross;
            for (int j = 0; j < pointsPrim.Count() - 1; j++)
            {
                isCross = CrossLines(
                    new Tuple<PointF, PointF>(point, point2),
                    new Tuple<PointF, PointF>(pointsPrim[j], pointsPrim[j + 1]));
                if (isCross.Item1)
                    count++;
            }
            isCross = CrossLines(
                    new Tuple<PointF, PointF>(point, point2),
                    new Tuple<PointF, PointF>(pointsPrim[0], pointsPrim[pointsPrim.Count - 1]));
            if (isCross.Item1)
                count++;
            
            //for (int i = 0; i < pointsPrim.Count - 1; i++)
            //{
            //    Point currentVertex = pointsPrim[i];
            //    Point nextVertex = pointsPrim[i + 1];
            //if (IntersectsRay(point, currentVertex, nextVertex))
            //}
            return count % 2 == 1;
        }

        private void PointInsideClick(object sender, EventArgs e)
        {
            state = 8;
        }
    }
}
