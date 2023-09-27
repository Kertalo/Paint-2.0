using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
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
            Console.WriteLine(map.Width);
        }

        byte state = 0;

        int oldX;
        int oldY;

        Bitmap map = new Bitmap(256, 256);
        Graphics graphics;
        Pen pen = new Pen(Color.Black, 3);

        bool isMouseDown = false;

        private void DrawLineBR(int x1, int y1, int x2, int y2)
        {
            int deltaX = Math.Abs(x2 - x1);
            int deltaY = Math.Abs(y2 - y1);
            int signX = x1 < x2 ? 1 : -1;
            int signY = y1 < y2 ? 1 : -1;
            int error = deltaX - deltaY;
            map.SetPixel(x2, y2, Color.Black);
            while (x1 != x2 || y1 != y2)
            {
                map.SetPixel(x1, y1, Color.Black);
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

        private void Fill(int x, int y, Color fillColor)
        {
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                return;
            if (map.GetPixel(x, y) != fillColor)
                return;
            map.SetPixel(x, y, Color.Black);
            Fill(x + 1, y, fillColor);
            Fill(x - 1, y, fillColor);
            Fill(x, y + 1, fillColor);
            Fill(x, y - 1, fillColor);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (state == 2)
            {
                Fill(e.X, e.Y, map.GetPixel(e.X, e.Y));
                pictureBox1.Image = map;
                return;
            }
            isMouseDown = true;
            oldX = e.X;
            oldY = e.Y;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            if (state != 1)
                return;
            DrawLineBR(oldX, oldY, e.X, e.Y);
            pictureBox1.Image = map;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (state != 0 || !isMouseDown)
                return;
            DrawLineBR(oldX, oldY, e.X, e.Y);
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
    }
}
