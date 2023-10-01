using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paint_2._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Rectangle rectangle = Screen.PrimaryScreen.Bounds;
            map = new Bitmap(rectangle.Width, rectangle.Height);
            graphics = Graphics.FromImage(map);
            string directory = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            directory = Directory.GetParent(directory).ToString();
            directory = Directory.GetParent(directory).ToString();
            pictureMap = new Bitmap(directory + @"\Images\cs.jpg");
        }

        Bitmap pictureMap;
        byte state = 0;

        int oldX;
        int oldY;

        Bitmap map = new Bitmap(256, 256);
        List<Point> pointsPrimitive;
        Bitmap mapPrimitive = new Bitmap(256, 256);
        Graphics graphics;

        Color color = Color.Black;

        bool isMouseDown = false;

        private void DrawLine(int x1, int y1, int x2, int y2)
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

        private void FillPicture(int x, int y, Color oldColor)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return;
            if (map.GetPixel(x, y) != oldColor)
                return;
            int startX = x;
            while (map.GetPixel(x, y) == oldColor)
            {
                map.SetPixel(x, y, pictureMap.GetPixel
                (x % pictureMap.Width, y % pictureMap.Height));
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
                map.SetPixel(x, y, pictureMap.GetPixel
                (x % pictureMap.Width, y % pictureMap.Height));
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

        private void FillBorder(int x, int y, Color oldColor)
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

        private void Fill(int x, int y, Color oldColor)
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

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (state == 0 || state == 1)
            {
                isMouseDown = true;
                oldX = e.X;
                oldY = e.Y;
            }
            else if (state == 2)
            {
                Fill(e.X, e.Y, map.GetPixel(e.X, e.Y));
                pictureBox1.Image = map;
            }
            else if (state == 3)
            {
                FillPicture(e.X, e.Y, map.GetPixel(e.X, e.Y));
                pictureBox1.Image = map;
            }
            else if (state == 4)
            {
                FillBorder(e.X, e.Y, map.GetPixel(e.X, e.Y));
                pictureBox1.Image = map;
            }
            else if (state == 5)
            {
                pointsPrimitive.Add(new Point(e.X, e.Y));
                map.SetPixel(e.X, e.Y, color);
                pictureBox1.Image = map;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (state != 0 && state != 1)
                return;
            isMouseDown = false;
            if (state == 1)
            {
                DrawLine(oldX, oldY, e.X, e.Y);
                pictureBox1.Image = map;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (state != 0 || !isMouseDown)
                return;
            DrawLine(oldX, oldY, e.X, e.Y);
            pictureBox1.Image = map;
            oldX = e.X;
            oldY = e.Y;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            state = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            state = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            state = 2;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            state = 3;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            state = 4;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < map.Width; i++)
                for (int j = 0; j < map.Height; j++)
                    map.SetPixel(i, j, Color.White);
            pictureBox1.Image = map;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            color = (sender as Button).BackColor;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            state = 5;
            pointsPrimitive = new List<Point>();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            int count = pointsPrimitive.Count;
            if (count > 1)
            {
                for (int i = 1; i < count; i++)
                    DrawLine(pointsPrimitive[i - 1].X, pointsPrimitive[i - 1].Y,
                        pointsPrimitive[i].X, pointsPrimitive[i].Y);
                DrawLine(pointsPrimitive[0].X, pointsPrimitive[0].Y,
                        pointsPrimitive[count - 1].X, pointsPrimitive[count - 1].Y);
            }
            pictureBox1.Image = map;
        }

        private void button24_Click(object sender, EventArgs e)
        {

        }
    }
}
