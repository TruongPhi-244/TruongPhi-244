using System;
using SharpGL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace lab2
{

    public partial class Form1 : Form
    {
        Color UserColor;
        Color userFillColor;
        Point pStart, pEnd, index; //toa do bat dau, ket thuc, toa do nhan chuot
        Line userLine;
        bool isDraw = false; // Bien cho he thong nhan biet tin hieu ve
        bool drawPolygon = false; // Bien cho phep nguoi dung ve polygon
        Stopwatch stopwatch;
        bool canFillColor; // Bien cho phep to mau
        List<Point> Polygon = null; //polygon la tap hop cac duong thang
        CheckPolygonConvex polygonCheck = new CheckPolygonConvex();
        FillCollor colorFloodFill = new FillCollor();
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openGLControl_Load(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();
            UserColor = Color.Black;
            userLine = new Line();
        }
        private void openGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;
            gl.ClearColor(1, 1, 1, 0);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Viewport(0, 0, openGLControl.Width, openGLControl.Height);
            gl.Ortho2D(0, openGLControl.Width, 0, openGLControl.Height);

        }

        private void openGLControl_Resized(object sender, EventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Viewport(0, 0, openGLControl.Width, openGLControl.Height);
            gl.Ortho2D(0, openGLControl.Width, 0, openGLControl.Height);

        }
        private void openGLControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && drawPolygon == true) // nhan chuot trai de ve hinh
            {
                if (isDraw == false)
                {
                    Polygon = new List<Point>();
                    isDraw = true;
                }
                if (isDraw == true)
                {
                    OpenGL gl = openGLControl.OpenGL;

                    stopwatch = Stopwatch.StartNew();
                    //time_exc.Text = " ";
                    pStart.X = e.Location.X;
                    pStart.Y = openGLControl.Height - e.Location.Y;
                    Polygon.Add(new Point(pStart.X, pStart.Y)); // dua diem bat dau vao tap diem cua polygon
                    pEnd = pStart;
                }
            }
            else if (e.Button == MouseButtons.Left && canFillColor == true)
            {
                index.X = e.Location.X;
                index.Y = openGLControl.Height - e.Location.Y;
                if (polygonCheck.PolygonIsConvex(Polygon))
                {
                    OpenGL gl = openGLControl.OpenGL;
                    MessageBox.Show("This is convex polygon");
                    byte[] pixel = new byte[4];
                    gl.ReadPixels(index.X, index.Y, 1, 1, OpenGL.GL_RGB, OpenGL.GL_FLOAT, pixel);
                    Bitmap bm = new Bitmap(openGLControl.Width, openGLControl.Height);
                    //MessageBox.Show(userFillColor.R.ToString());
                    colorFloodFill.FloodFill(bm, index, userFillColor, UserColor);
                }
                else
                    MessageBox.Show("This is unconvex polygon");
            }
            else if (e.Button == MouseButtons.Left && drawPolygon == false && canFillColor == false) // set truong hop cho viec chon doi tuong
            {
                OpenGL gl = openGLControl.OpenGL;
                index.X = e.Location.X;
                index.Y = openGLControl.Height - e.Location.Y;
                for (int i = 0; i < Polygon.Count - 1; i++)
                {
                    double distance = Math.Sqrt(Math.Pow(index.X - Polygon[i].X, 2) + Math.Pow(index.Y - Polygon[i].Y, 2)); // neu khoang cach con tro chuot
                    if (distance >= 0 && distance <= 100) // <= 100 va >= 0 voi bat ki diem nao cua polygon thi thuc hien highlight
                    {

                        gl.PointSize(8);
                        gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                        gl.Begin(OpenGL.GL_POINTS);
                        gl.Color(1.0, 0, 0);
                        for (int j = 0; j < Polygon.Count(); j++) // To cac dinh cua polygon
                        {

                            gl.Vertex(Polygon[j].X, Polygon[j].Y);
                        }
                        gl.End();
                        gl.Flush();
                        gl.PointSize(1);
                        gl.Begin(OpenGL.GL_POINTS);
                        for (int j = 0; j < Polygon.Count() - 1; j++) // Noi cac dinh cua polygon
                        {
                            userLine.DrawLine(gl, Polygon[j], Polygon[j + 1], UserColor);
                        }
                        gl.End();
                        gl.Flush();
                        canFillColor = true;
                        return;
                    }
                }

                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT); // Neu nguoi dung nhan xa hon tat ca diem cua polygon
                gl.Begin(OpenGL.GL_POINTS); // thi cho phep xoa highlight cac toa do dinh
                for (int j = 0; j < Polygon.Count() - 1; j++) // Noi cac dinh cua polygon
                {
                    userLine.DrawLine(gl, Polygon[j], Polygon[j + 1], UserColor);
                }
                gl.End();
                gl.Flush();
                canFillColor = false;

            }
            else if (e.Button == MouseButtons.Right) // nhan chuot phai de hoan tat ve hinh
            {
                pEnd.X = e.Location.X;
                pEnd.Y = openGLControl.Height - e.Location.Y;
                OpenGL gl = openGLControl.OpenGL;
                isDraw = false;
                drawPolygon = false;
                bool flag = false; //bien de ve lai hinh
                double distance = Math.Sqrt(Math.Pow(pEnd.X - Polygon[0].X, 2) + Math.Pow(pEnd.Y - Polygon[0].Y, 2)); // kiem tra khoang cach cua diem cuoi voi diem dau cua polygon
                if (distance <= 20 && distance > 0)//neu khoang cach < 20 va > 0 thi noi diem dau voi diem cuoi
                {
                    pEnd = Polygon[0]; // gan diem cuoi cung thanh diem dau tien cua polygon
                    flag = true; // neu diem dau va diem cuoi gan nhau thi ta cho phep noi 2 diem do 
                }
                Polygon.Add(new Point(pEnd.X, pEnd.Y));
                // dua diem cuoi cung vao danh sach diem cua polygon
                if (flag)
                {

                    gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                    gl.Begin(OpenGL.GL_POINTS);
                    for (int i = 0; i < Polygon.Count() - 1; i++) // tien hanh ve lai polygon
                    {
                        userLine.DrawLine(gl, Polygon[i], Polygon[i + 1], UserColor);
                    }
                    gl.End();
                    gl.Flush();

                }


            }
        }
        private void openGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;

            if (isDraw) //khi nhan chuot trai thi ta ve hinh
            {
                pEnd.X = e.Location.X;
                pEnd.Y = openGLControl.Height - e.Location.Y;
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                gl.Begin(OpenGL.GL_POINTS);
                for (int i = 0; i < Polygon.Count() - 1; i++) // ve lai cac duong thang truoc khi ve duong tiep theo
                {
                    userLine.DrawLine(gl, Polygon[i], Polygon[i + 1], UserColor);
                }
                userLine.DrawLine(gl, pStart, pEnd, UserColor); // ve duong thang theo con tro chuot cua nguoi dung
                gl.End();
                gl.Flush();
            }
        }

        private void Draw_Click(object sender, EventArgs e)
        {
            drawPolygon = true;
        }
        private void bt_usercoller_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                UserColor = colorDialog1.Color;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog2.ShowDialog() == DialogResult.OK)
            {
                userFillColor = colorDialog2.Color;
                if (canFillColor)
                {

                }
            }

        }

        public class Line
        {
            public Line()
            {
            }

            public void DrawLine(OpenGL Gl, Point start, Point end, Color userColor)
            {
                int p, stepx, stepy;
                int Dx = end.X - start.X;
                int Dy = end.Y - start.Y;
                int _2_Dx = 2 * (end.X - start.X);
                int _2_Dy = 2 * (Dy = end.Y - start.Y);
                int x = start.X;
                int y = start.Y;
                if (_2_Dx < 0) { _2_Dx = -_2_Dx; stepx = -1; } else if (_2_Dx > 0) { stepx = 1; } else { stepx = 0; }
                if (_2_Dy < 0) { _2_Dy = -_2_Dy; stepy = -1; } else if (_2_Dy > 0) { stepy = 1; } else { stepy = 0; }
                Gl.Begin(OpenGL.GL_POINTS);
                Gl.Color(userColor.R / 255.0, userColor.G / 255.0, userColor.B / 255.0, 0);
                Gl.Vertex(x, y);
                if (_2_Dx >= _2_Dy)
                {
                    p = _2_Dy - Dx;
                    while (x != end.X)
                    {
                        if (p < 0)
                            p += _2_Dy;
                        else
                        {
                            p += _2_Dy - _2_Dx;
                            y += stepy;
                        }
                        x += stepx;
                        Gl.Vertex(x, y);
                    }
                }
                else
                {
                    p = _2_Dx - Dy;
                    while (y != end.Y)
                    {
                        if (p < 0)
                            p += _2_Dx;
                        else
                        {
                            p += _2_Dx - _2_Dy;
                            x += stepx;
                        }
                        y += stepy;
                        Gl.Vertex(x, y);
                    }
                }
                Gl.End();
            }
        }



        public class CheckPolygonConvex
        {
            public static float CrossProductLength(float Ax, float Ay, float Bx, float By, float Cx, float Cy)
            {
                // Lay toa do vector giua 2 dien ke nhau
                float BAx = Ax - Bx;
                float BAy = Ay - By;
                float BCx = Cx - Bx;
                float BCy = Cy - By;

                // tinh tich vo huong giua 2 vector
                return (BAx * BCy - BAy * BCx);
            }
            public bool PolygonIsConvex(List<Point> polygon)
            {
                //De xac dinh la da giac loi thi tich vo huong 
                // giua 2 duong thang bat ki luon duong, neu am 
                // thi da giac khong phai da giac loi
                bool got_negative = false;
                bool got_positive = false;
                int num_points = polygon.Count();
                int B, C;
                for (int A = 0; A < num_points; A++)
                {
                    B = (A + 1) % num_points;
                    C = (B + 1) % num_points;

                    float cross_product =
                        CrossProductLength(
                            polygon[A].X, polygon[A].Y,
                            polygon[B].X, polygon[B].Y,
                            polygon[C].X, polygon[C].Y);
                    if (cross_product < 0)
                    {
                        got_negative = true;
                    }
                    else if (cross_product > 0)
                    {
                        got_positive = true;
                    }
                    if (got_negative && got_positive) return false;
                }
                return true;
            }
        }

        public class FillCollor
        {
            public bool SameColor(Color c1, Color c2)
            {
                return ((c1.A == c2.A) && (c1.B == c2.B) && (c1.G == c2.G) && (c1.R == c2.R));
            }


            public void FloodFill(Bitmap bm, Point p, Color Color, Color LineColor)
            {
                /* byte[] pixel = new byte[4];
                  gl.ReadPixels(x, y, 1, 1, OpenGL.GL_RGB, OpenGL.GL_FLOAT, pixel);
                  if (pixel[0] == 0 && pixel[1] == 0 && pixel[2] == 128)
                  {
                      gl.PointSize(1);
                      gl.Begin(OpenGL.GL_POINTS);
                      gl.Color(replacementColor.R / 255.0, replacementColor.G / 255.0, replacementColor.B / 255.0, 0);
                      gl.Vertex(x, y);
                      gl.End();
                      gl.Flush();
                      FloodFill(gl, x + 1, y, replacementColor, oldColor);
                      FloodFill(gl, x - 1, y, replacementColor, oldColor);
                      FloodFill(gl, x, y + 1, replacementColor, oldColor);
                      FloodFill(gl, x, y - 1, replacementColor, oldColor);
                  }*/
                Stack S = new Stack();
                S.Push(p);
                while (S.Count != 0)
                {
                    p = (Point)S.Pop();
                    Color CurrentColor = bm.GetPixel(p.X, p.Y);
                    //MessageBox.Show(LineColor.R.ToString());
                    if (CurrentColor.A != Color.A || CurrentColor.B != Color.B || CurrentColor.G != Color.G || CurrentColor.R != Color.R
                        && CurrentColor.A != LineColor.A || CurrentColor.B != LineColor.B || CurrentColor.G != LineColor.G || CurrentColor.R != LineColor.R)
                    {
                        bm.SetPixel(p.X, p.Y, Color);
                        S.Push(new Point(p.X - 1, p.Y));
                        S.Push(new Point(p.X + 1, p.Y));
                        S.Push(new Point(p.X, p.Y - 1));
                        S.Push(new Point(p.X, p.Y + 1));

                    }


                }
            }
        }
    }
}

