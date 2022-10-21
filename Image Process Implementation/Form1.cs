using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*
 * 待改善:
 *  GetPixel SetPixel 過慢,改用Pointer會快上許多
 *  MeanFilter取用值不會變,可以改成 -left-top, +right+bottom
 *  Load新圖片,Undo還不能跳回上次load的圖片,可以修改Stack<Bitmap>成Stack<Tuple<Bitmap, Bool>>來達成
 *  Load可以先判別彩色or灰階,直方圖及許多操作都可以加速
 *  Image Registration不知道怎麼設計,目前是獨立的
 *  Image Registration目前特殊角度(垂直)還沒完善 
 *  
 * GrayScale有依據係數調整,並不是除以3 
 * Image Registration 實際上只用了對角線,因為題目沒有提到(x,y)的分別縮放
 */
namespace IP_hw1
{
    public partial class Form1 : Form
    {
        Bitmap openImg, current, readImg;
        Stack<Bitmap> hist = new Stack<Bitmap>();
        int re_c1 = 0, re_c2 = 0;
        int[] sobel_x = { -1, -2, -1, 0, 0, 0, 1, 2, 1 }, sobel_y = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
        int[,] points = new int[4, 2];
        bool regist = false;
        public Form1()
        {
            InitializeComponent();
            label2.Visible = false;
            openFileDialog1.Filter = "All Files|*.*|Bitmap Files(.bmp)|*.bmp*|Jpeg File(.jpg)|*.jpg*";
        }
        private void update()
        {
            if (current != null)
            {
                pictureBox2.Image = current;
                pictureBox2.Width = current.Width;
                pictureBox2.Height = current.Height;
            }
        }
        private void chart_ChangeColor(int color)
        {
            foreach (var obj in chart1.Series)
                obj.Enabled = false;          
            chart1.Series[color].Enabled = true;          
            chart1.ChartAreas[0].RecalculateAxesScale();

            foreach (var obj in chart2.Series)
                obj.Enabled = false;
            chart2.Series[color].Enabled = true;
            chart2.ChartAreas[0].RecalculateAxesScale();
        }
        private void button1_Click(object sender, EventArgs e) //#load image button
        {
            if (current != null)
                hist.Push(new Bitmap(current));
           
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (readImg != null)
                    readImg.Dispose();
                readImg = new Bitmap(openFileDialog1.FileName);
                openImg = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = openImg;
                pictureBox1.Width = openImg.Width;
                pictureBox1.Height = openImg.Height;
                pictureBox2.Width = openImg.Width;
                pictureBox2.Height = openImg.Height;
                current = openImg.Clone(new Rectangle(0,0,openImg.Width,openImg.Height),System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                update();
                //hist.Push(openImg);
            }
        }
        private void button2_Click(object sender, EventArgs e)  //save image button
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "All File|*.*|Bitmap Files(.bmp)|*.bmp*|Jpeg File(.jpg)|*.jpg*";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                current.Save(sfd.FileName);
        }
        private void regist_cancel()
        {
            regist = false;
            label2.Text = "Cancel";
            re_c1 = 0;
            re_c2 = 0;
        }
        private void image_registration()
        {
            hist.Push(new Bitmap(current));
            Double v1_x = points[1, 0] - points[0, 0], v1_y = points[1, 1] - points[0, 1]
                , v2_x = points[3, 0] - points[2, 0], v2_y = points[3, 1] - points[2, 1];
            Double d1 = Math.Sqrt(v1_x * v1_x + v1_y * v1_y), d2 = Math.Sqrt(v2_x * v2_x + v2_y * v2_y), m1, m2, theta_r = 0;
            Double inner = v1_x * v2_x + v1_y * v2_y, c = d1 / d2;
            Double mid_x = (points[3, 0] + points[2, 0]) / 2, mid_y = (points[3, 1] + points[2, 1])/2; 
            if (v1_x == 0) //轉到垂直
            {
                if (inner < 0) //反向
                {
                }

            }
            else if (v2_x == 0) //B向量垂直
            {
            }
            else //轉theta角
            {
                m1 = v1_y / v1_x;
                m2 = v2_y / v2_x;
                //theta_r = Math.Atan(((m1 - m2) / (1 + m1 * m2)));
                theta_r = Math.Atan2(m2 - m1, 1 + m1 * m2);
                
            }
            int new_x = Convert.ToInt32(Math.Ceiling(current.Width * c));
            int new_y = Convert.ToInt32(Math.Ceiling(current.Height * c));
            Bitmap newImg = new Bitmap(new_x, new_y);
            
            Double t_x, t_y, u, v;
            Color[] Colors = new Color[4];
            int x0, y0, R, G, B;
            for (int x = 0; x < newImg.Width; x++)
                for (int y = 0; y < newImg.Height; y++)
                {
                    t_x = x / c;
                    t_y = y / c;
                    u = t_x % 1;
                    v = t_y % 1;
                    x0 = Convert.ToInt32(Math.Floor(t_x));
                    y0 = Convert.ToInt32(Math.Floor(t_y));
                    for (int i = 0; i < 4; i++)
                    {
                        if ((x0+i/2) > (current.Width - 1) || (y0+i%2) > (current.Height - 1))
                            Colors[i] = Color.FromArgb(0, 0, 0);
                        else
                            Colors[i] = current.GetPixel(x0 + i / 2, y0 + i % 2);
                    }
                    R = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].R + (1 - u) * v * Colors[1].R
                        + u * (1 - v) * Colors[2].R + u * v * Colors[3].R);
                    G = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].G + (1 - u) * v * Colors[1].G
                        + u * (1 - v) * Colors[2].G + u * v * Colors[3].G);
                    B = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].B + (1 - u) * v * Colors[1].B
                        + u * (1 - v) * Colors[2].B + u * v * Colors[3].B);
                    newImg.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            current.Dispose();
            current = newImg;
            new_x = Convert.ToInt32(Math.Ceiling(current.Width * Math.Cos(theta_r) + current.Height * Math.Sin(theta_r)));
            new_y = Convert.ToInt32(Math.Ceiling(current.Width * Math.Sin(theta_r) + current.Height * Math.Cos(theta_r)));
            newImg = new Bitmap(new_x, new_y);
            int c_x = Convert.ToInt32(mid_x*c), c_y = Convert.ToInt32(mid_y*c);
            for (int x = 0; x < newImg.Width; x++)
                for (int y = 0; y < newImg.Height; y++)
                {
                    t_x = (x - c_x) * Math.Cos(theta_r) - (y - c_y) * Math.Sin(theta_r) + c_x;
                    t_y = (x - c_x) * Math.Sin(theta_r) + (y - c_y) * Math.Cos(theta_r) + c_y;
                    if (t_x < 0 || t_y < 0 || (t_x > (current.Width - 1)) || (t_y > (current.Height - 1)))
                        continue;
                    else
                    {
                        u = t_x % 1;
                        v = t_y % 1;
                        x0 = Convert.ToInt32(Math.Floor(t_x));
                        y0 = Convert.ToInt32(Math.Floor(t_y));
                        for (int i = 0; i < 4; i++)
                            Colors[i] = current.GetPixel(x0 + i / 2, y0 + i % 2);
                        R = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].R + (1 - u) * v * Colors[1].R
                            + u * (1 - v) * Colors[2].R + u * v * Colors[3].R);
                        G = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].G + (1 - u) * v * Colors[1].G
                            + u * (1 - v) * Colors[2].G + u * v * Colors[3].G);
                        B = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].B + (1 - u) * v * Colors[1].B
                            + u * (1 - v) * Colors[2].B + u * v * Colors[3].B);
                        newImg.SetPixel(x, y, Color.FromArgb(R, G, B));
                    }
                }
            current.Dispose();
            current = newImg;
            newImg = new Bitmap(readImg.Width, readImg.Height);
            c_x -= newImg.Width / 2;
            c_y -= newImg.Height / 2;
            Double intensity = 0;
            Color a, b;
            for (int x = 0; x < newImg.Width; x++)
                for (int y = 0; y < newImg.Height; y++)
                {
                    a = readImg.GetPixel(x, y);
                    if ((c_x + x) < 0 || (c_y + y) < 0 || (c_x + x) > (current.Width - 1) || (c_y + y) > (current.Height - 1))
                    {
                        intensity += 255;
                        continue;
                    }
                    b = current.GetPixel(c_x + x, c_y + y);
                    newImg.SetPixel(x, y, b);
                    intensity += Math.Abs((b.R - a.R) + (b.G - a.G) + (b.B - a.B) / 3);
                }
            /*
            double reverse_theta = Math.PI * 2 - theta_r;
            double TarCenter_x = (points[1, 0] - points[0, 0]) / 2, TarCenter_y = (points[1, 1] - points[0, 1]) / 2
                , OriCenter_x = (points[3, 0] - points[2, 0]) / 2, OriCenter_y = (points[3, 1] - points[2, 1]) / 2;
            double tmp_x = 0, tmp_y = 0, tar_x = 0, tar_y = 0, u = 0, v = 0;
            Bitmap newImg = new Bitmap(readImg.Width, readImg.Height);
            Color[] Colors = new Color[4]; //(x0,y0) (x0,y1) (x1,y0) (x1,y1)
            int x0, y0, R, G, B;
            for (int x = 0; x < newImg.Width; x++)
                for (int y = 0; y < newImg.Height; y++)
                {
                    //tmp_x = (x - TarCenter_x)/c;
                    //tmp_y = (y - TarCenter_y)/c;
                    tmp_x = (x - TarCenter_x);
                    tmp_y = (y - TarCenter_y);
                    //tmp_x = (x - OriCenter_x);
                    //tmp_y = (y - OriCenter_y);
                    
                    tar_x = tmp_x * Math.Cos(reverse_theta) - tmp_y * Math.Sin(reverse_theta);
                    tar_y = tmp_x * Math.Sin(reverse_theta) + tmp_y * Math.Cos(reverse_theta);


                    //tar_x = tar_x + OriCenter_x;
                    //tar_y = tar_y + OriCenter_y;

                    //tar_x += current.Width / 2;
                    //tar_y += current.Height / 2;
                    tar_x += TarCenter_x;
                    tar_y += TarCenter_y;
                    //tar_x /= c;
                    //tar_y /= c;
                    if (tar_x < 0 || tar_y < 0 || (tar_x > (current.Width - 1)) || (tar_y > (current.Height - 1)))
                        continue;
                    else
                    {
                        u = tar_x % 1;
                        v = tar_y % 1;
                        x0 = Convert.ToInt32(Math.Floor(tar_x));
                        y0 = Convert.ToInt32(Math.Floor(tar_y));
                        for (int i = 0; i < 4; i++)
                            Colors[i] = current.GetPixel(x0 + i / 2, y0 + i % 2);
                        R = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].R + (1 - u) * v * Colors[1].R
                            + u * (1 - v) * Colors[2].R + u * v * Colors[3].R);
                        G = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].G + (1 - u) * v * Colors[1].G
                            + u * (1 - v) * Colors[2].G + u * v * Colors[3].G);
                        B = Convert.ToInt16((1 - u) * (1 - v) * Colors[0].B + (1 - u) * v * Colors[1].B
                            + u * (1 - v) * Colors[2].B + u * v * Colors[3].B);
                        newImg.SetPixel(x, y, Color.FromArgb(R, G, B));
                    }
                }*/
            intensity /= (readImg.Width * readImg.Height);
            label3.Text = "Scale: " + c.ToString() + " ";
            label3.Text += "Angle: " + 180 * theta_r / Math.PI + " ";
            label3.Text += "Intensity: " + intensity.ToString();
            current.Dispose();
            current = newImg;
            update();
        }
        private void button4_Click(object sender, EventArgs e)  //convert to R-gray image button
        { 
            hist.Push(new Bitmap(current));
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int R_value = Convert.ToInt32(current.GetPixel(x, y).R * 0.299);
                    current.SetPixel(x, y, Color.FromArgb(R_value, R_value, R_value));
                }
            update();
        }
        private void button5_Click(object sender, EventArgs e)  //convert to G image button
        {
            hist.Push(new Bitmap(current));
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int G_value = Convert.ToInt32(current.GetPixel(x, y).G * 0.587);
                    current.SetPixel(x, y, Color.FromArgb(G_value, G_value, G_value));
                }
            update();
        }
        private void button6_Click(object sender, EventArgs e)  //convert to B image button
        {
            hist.Push(new Bitmap(current));
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int B_value = Convert.ToInt32(current.GetPixel(x, y).B * 0.114);
                    current.SetPixel(x, y, Color.FromArgb(B_value, B_value, B_value));
                }
            update();
        }
   
        private void button7_Click(object sender, EventArgs e)  //convert to Grayscale image button
        {
            hist.Push(new Bitmap(current));
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int R_value = Convert.ToInt32(current.GetPixel(x, y).R * 0.299);
                    int G_value = Convert.ToInt32(current.GetPixel(x, y).G * 0.587);
                    int B_value = Convert.ToInt32(current.GetPixel(x, y).B * 0.114);
                    int mean = R_value + G_value + B_value;
                    current.SetPixel(x, y, Color.FromArgb(mean, mean, mean));
                }
            update();
        }

        private void button8_Click(object sender, EventArgs e)  //mean filter button    #運算可以改成加row,col減一row,col
        {
            hist.Push(new Bitmap(current));
            Bitmap newImg = new Bitmap(current.Width, current.Height);
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int R = 0, G = 0, B = 0;
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            if (x == 0 || y == 0 || x == current.Width - 1 || y == current.Height - 1)
                                continue;
                            else
                            {
                                R += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).R);
                                G += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).G);
                                B += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).B);
                            }
                        }
                    R /= 9;
                    G /= 9;
                    B /= 9;
                    newImg.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            current.Dispose();
            current = newImg;
            update();
        }

        private void button9_Click(object sender, EventArgs e)  //median filter button
        {
            int[] median_R = new int[9], median_G = new int[9], median_B = new int[9];
            Bitmap newImg = new Bitmap(current.Width, current.Height);
            hist.Push(new Bitmap(current));
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            if (x == 0 || y == 0 || x == current.Width - 1 || y == current.Height - 1)
                                continue;
                            else
                            {
                                median_R[i * 3 + j] = Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).R);
                                median_G[i * 3 + j] = Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).G);
                                median_B[i * 3 + j] = Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).B);
                            }
                        }
                    Array.Sort(median_R);
                    Array.Sort(median_G);
                    Array.Sort(median_B);
                    newImg.SetPixel(x, y, Color.FromArgb((median_R[4] + median_R[5]) / 2, (median_G[4] + median_G[5]) / 2, (median_B[4] + median_B[5]) / 2));
                }
            current.Dispose();
            current = newImg;
            update();
        }

        private void button11_Click(object sender, EventArgs e) //thresholding button
        {
            hist.Push(new Bitmap(current));
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int R = Convert.ToInt32(current.GetPixel(x, y).R),
                    G = Convert.ToInt32(current.GetPixel(x, y).G),
                    B = Convert.ToInt32(current.GetPixel(x, y).B);
                    if ((R + G + B) / 3 >= Convert.ToInt32(textBox1.Text))
                        current.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    else
                        current.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            update();
        }

        private void button12_Click(object sender, EventArgs e) //sobel vertical
        {
            hist.Push(new Bitmap(current));
            Bitmap newImg = new Bitmap(current.Width, current.Height);
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int R = 0, G = 0, B = 0;
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            if (x == 0 || y == 0 || x == current.Width - 1 || y == current.Height - 1)
                                continue;
                            else
                            {
                                R += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).R) * sobel_x[3 * i + j];
                                G += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).G) * sobel_x[3 * i + j];
                                B += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).B) * sobel_x[3 * i + j];
                            }
                        }
                    if (R < 0)
                        R = 0;
                    if (G < 0)
                        G = 0;
                    if (B < 0)
                        B = 0;
                    R = Math.Min(R, 255);
                    G = Math.Min(G, 255);
                    B = Math.Min(B, 255);
                    newImg.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            current.Dispose();
            current = newImg;
            update();
        }

        private void button13_Click(object sender, EventArgs e) //sobel horizonal
        {
            hist.Push(new Bitmap(current));
            Bitmap newImg = new Bitmap(current.Width, current.Height);
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int R = 0, G = 0, B = 0;
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            if (x == 0 || y == 0 || x == current.Width - 1 || y == current.Height - 1)
                                continue;
                            else
                            {
                                R += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).R) * sobel_y[3 * i + j];
                                G += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).G) * sobel_y[3 * i + j];
                                B += Convert.ToInt32(current.GetPixel(x + i - 1, y + j - 1).B) * sobel_y[3 * i + j];
                            }
                        }
                    if (R < 0)
                        R = 0;
                    if (G < 0)
                        G = 0;
                    if (B < 0)
                        B = 0;
                    R = Math.Min(R, 255);
                    G = Math.Min(G, 255);
                    B = Math.Min(B, 255);
                    newImg.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            current.Dispose();
            current = newImg;
            update();
        }

        private void button10_Click(object sender, EventArgs e) //histogram button
        {
            hist.Push(new Bitmap(current));
            int[,] histogram_before = new int[3, 256], convert_table = new int[3, 256], histogram_after = new int[3, 256];
            //draw histogram 1
            foreach (var obj in chart1.Series)
                obj.Enabled = true;
            for (int i = 0; i < 3; i++)
                chart1.Series[i].Points.Clear();
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    histogram_before[0, current.GetPixel(x, y).B]++;
                    histogram_before[1, current.GetPixel(x, y).G]++;
                    histogram_before[2, current.GetPixel(x, y).R]++;
                }
            for (int i = 0; i < 256; i++)   
                for (int j = 0; j < 3; j++)
                    chart1.Series[j].Points.AddXY(i, histogram_before[j, i]);
            //calculate
            int PC = current.Width * current.Height;
            //build convert table
            for (int c = 0; c < 3; c++)
            {
                int acc = 0;
                for (int i = 0; i < 256; i++)
                {
                    acc += histogram_before[c, i];
                    convert_table[c, i] = (acc * 255) / PC;
                }
            }
            //convert
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                    current.SetPixel(x, y, Color.FromArgb(convert_table[0, current.GetPixel(x, y).R],
                        convert_table[1, current.GetPixel(x, y).G], convert_table[2, current.GetPixel(x, y).B]));
            //draw histogram 2
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    histogram_after[0, current.GetPixel(x, y).B]++;
                    histogram_after[1, current.GetPixel(x, y).G]++;
                    histogram_after[2, current.GetPixel(x, y).R]++;
                }
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 3; j++)
                    chart2.Series[j].Points.AddXY(i, histogram_after[j, i]);
            update();
        }

        private void button16_Click(object sender, EventArgs e) //hist R color
        {
            chart_ChangeColor(2);
        }

        private void button17_Click(object sender, EventArgs e) //hist G color
        {
            chart_ChangeColor(1);
        }

        private void button18_Click(object sender, EventArgs e) //hist B color
        {
            chart_ChangeColor(0);
        }

        private void button19_Click(object sender, EventArgs e) //Connected Component button
        {
            hist.Push(new Bitmap(current));
            Stack<Tuple<int, int>> connect = new Stack<Tuple<int, int>>();
            Tuple<int, int> coor;
            Color tmp;
            int sum, color_count = 0, nx, ny;
            Random r = new Random(0);
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    tmp = current.GetPixel(x, y);
                    sum = tmp.R + tmp.G + tmp.B;
                    if (sum == 0)   //new component
                        connect.Push(new Tuple<int, int>(x, y));
                    if (connect.Count != 0) 
                    {
                        color_count++;
                        Color new_c = Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
                        while (connect.Count != 0) //find whole component
                        {
                            coor = connect.Pop();
                            current.SetPixel(coor.Item1, coor.Item2, new_c);
                            for (int i = -1; i < 2; i++)
                                for (int j = -1; j < 2; j++)
                                {
                                    if (i == 0 && j == 0)
                                        continue;
                                    nx = coor.Item1 + i;
                                    ny = coor.Item2 + j;
                                    if (nx < 0 || nx >= current.Width || ny < 0 || ny >= current.Height)
                                        continue;
                                    tmp = current.GetPixel(nx, ny);
                                    sum = tmp.R + tmp.G + tmp.B;
                                    if (sum == 0)
                                        connect.Push(new Tuple<int, int>(nx, ny));
                                }
                        }
                    }
                }
            label1.Text = "message: There are " + color_count + " component.";
            update();
        }

        private void button20_Click(object sender, EventArgs e) //image registration
        {
            if (current != null)
                hist.Push(new Bitmap(current));
            regist = true;
            label2.Visible = true;
            label2.Text = "Please complete Image Registration";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (readImg != null)
                    readImg.Dispose();
                readImg = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Width = readImg.Width;
                pictureBox1.Height = readImg.Height;
                pictureBox1.Image = readImg;
            }
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (current != null)
                    current.Dispose();
                current = new Bitmap(openFileDialog1.FileName);
                pictureBox2.Width = current.Width;
                pictureBox2.Height = current.Height;
                pictureBox2.Image = current;
            }
            update();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;

            if(re_c1 == 2)
                regist_cancel();
            else if (regist)
            {
                points[re_c1, 0] = coordinates.X;
                points[re_c1, 1] = coordinates.Y;
                label3.Text = "";
                re_c1++;
                
                for (int i = 0; i < re_c1; i++)
                    label3.Text += points[i, 0].ToString() + "," + points[i, 1].ToString() + " ";
                if (re_c1 + re_c2 == 4)
                    image_registration();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            if (re_c2 == 2)
                regist_cancel();
            else if (regist)
            {
                points[re_c2 + 2, 0] = coordinates.X;
                points[re_c2 + 2, 1] = coordinates.Y;
                label3.Text = "";
                re_c2++;
                for (int i = 0; i < re_c2; i++)
                    label3.Text += points[i + 2, 0].ToString() + "," + points[i + 2, 1].ToString() + " ";
                if (re_c1 + re_c2 == 4)
                    image_registration();
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (regist)
                label1.Text = "P1:Mouse Coordination: " + e.Location.X.ToString() + " " + e.Location.Y.ToString();
            
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (regist)
                label1.Text = "P2:Mouse Coordination: " + e.Location.X.ToString() + " " + e.Location.Y.ToString();
     
        }

      

        private void button15_Click(object sender, EventArgs e) //sobel overlapping button
        {
            hist.Push(new Bitmap(current));
            Bitmap newImg = new Bitmap(readImg);
            Color tmp;
            int white;
            for (int x = 0; x < newImg.Width; x++)
                for (int y = 0; y < newImg.Height; y++)
                {
                    tmp = current.GetPixel(x, y);
                    white = tmp.G + tmp.R + tmp.B;
                    if (white == 765)
                        newImg.SetPixel(x, y, Color.FromArgb(0, 255, 0));
                }
            current.Dispose();
            current = newImg;
            update();
        }

        private void button14_Click(object sender, EventArgs e) //sobel combined button
        {
            hist.Push(new Bitmap(current));
            Bitmap newImg = new Bitmap(current.Width, current.Height);
            for (int x = 0; x < current.Width; x++)
                for (int y = 0; y < current.Height; y++)
                {
                    int R = 0, G = 0, B = 0, R_a = 0, R_b = 0, G_a = 0, G_b = 0, B_a = 0, B_b = 0;
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            if (x == 0 || y == 0 || x == current.Width - 1 || y == current.Height - 1)
                                continue;
                            else
                            {
                                R_a += current.GetPixel(x + i - 1, y + j - 1).R * sobel_x[3 * i + j];
                                G_a += current.GetPixel(x + i - 1, y + j - 1).G * sobel_x[3 * i + j];
                                B_a += current.GetPixel(x + i - 1, y + j - 1).B * sobel_x[3 * i + j];
                                R_b += current.GetPixel(x + i - 1, y + j - 1).R * sobel_y[3 * i + j];
                                G_b += current.GetPixel(x + i - 1, y + j - 1).G * sobel_y[3 * i + j];
                                B_b += current.GetPixel(x + i - 1, y + j - 1).B * sobel_y[3 * i + j];
                            }
                        }
                    R = Convert.ToInt32(Math.Sqrt(R_a * R_a + R_b * R_b));
                    G = Convert.ToInt32(Math.Sqrt(G_a * G_a + G_b * G_b));
                    B = Convert.ToInt32(Math.Sqrt(B_a * B_a + B_b * B_b));
                    if (R < 0)
                        R = 0;
                    if (G < 0)
                        G = 0;
                    if (B < 0)
                        B = 0;
                    R = Math.Min(R, 255);
                    G = Math.Min(G, 255);
                    B = Math.Min(B, 255);
                    newImg.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            current.Dispose();
            current = newImg;
            update();
        }

        private void button3_Click(object sender, EventArgs e)  //undo button
        {
            if (hist.Count != 0)
            {
                current.Dispose();
                current = hist.Pop();
            }
            update();
        }

    }
}
