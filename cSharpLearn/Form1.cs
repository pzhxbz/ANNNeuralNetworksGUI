using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeuralNetworks;

namespace cSharpLearn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e)
        {

        }
        private NeuralNetworks.Ann ann;
        private bool is_drawing = false;
        private System.Drawing.Point start_point, end_point;
        private Bitmap draw_map;
        private int ann_output_num;
        private double[] input_train_data;
        private void Form1_Load(object sender, EventArgs e)
        {
            ann = new Ann(64, 20, 10, 0.3);
            ann.load();
            draw_map = new Bitmap(pictureBox1.Height, pictureBox1.Width);
            pictureBox1.DrawToBitmap(draw_map, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));

        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            start_point = new Point(e.X, e.Y);
            end_point = new Point(e.X, e.Y);
            is_drawing = true;

        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            is_drawing = false;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            Graphics g = Graphics.FromImage(draw_map);
            if (e.Button == MouseButtons.Left)
            {
                if (is_drawing)
                {
                    Point currentPoint = new Point(e.X, e.Y);


                    Rectangle rect = new Rectangle(currentPoint, new Size(25, 25));
                    g.DrawEllipse(new Pen(Color.White, 25), rect);
                    end_point.X = currentPoint.X;
                    end_point.Y = currentPoint.Y;
                    pictureBox1.Image = draw_map;
                }

            }
        }
        public bool ThumbnailCallback()
        {
            return false;
        }
        private void Start_Recognise_Click(object sender, EventArgs e)
        {

            Bitmap map = (Bitmap)pictureBox1.Image;
            Bitmap thumbnail_map = (Bitmap)map.GetThumbnailImage(8, 8, ThumbnailCallback, IntPtr.Zero);
            /*{
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "BMP Files(*.bmp)|*.bmp|JPG Files(*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files(*.*)|*.*";
                sfd.FileName = "123.bmp";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    thumbnail_map.Save(sfd.FileName);
                    sfd.Dispose();
                }
            }*/

            double[] input_data = new double[64];
            int conter = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Color get_color = thumbnail_map.GetPixel(i, j);
                    double input_color = (1.0 * get_color.R + 1.0 * get_color.G + 1.0 * get_color.B) / 3;
                    input_data[conter++] = input_color / 255;
                }
            }
            ann.set_input(input_data);
            input_train_data = input_data;
            double[] output = ann.get_output();
            ann_output_num = 0;
            for (int i = 0; i < 10; i++)
            {
                double temp1 = output[ann_output_num];
                double temp2 = output[i];
                if (output[ann_output_num] < output[i])
                {
                    ann_output_num = i;
                }
            }
            this.result.Text = "结果：" + Convert.ToString(ann_output_num);
            this.button1.Enabled = true;
            this.button2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("没有选择任何一项", "你在逗我?????");
                return;
            }
            train_ann(listBox1.SelectedIndex);
            listBox1.ClearSelected();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            train_ann(ann_output_num);
        }
        private void train_ann(int answer_num)
        {
            double[] answer = new double[10];
            for (int i = 0; i < 10; i++)
            {
                answer[i] = (i == answer_num ? 1 : 0);
            }
            ann.train(input_train_data, answer);
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            Graphics g = Graphics.FromImage(draw_map);
            g.Clear(Color.Black);
            pictureBox1.Image = draw_map;
            this.result.Text = "结果:";
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("是否保存本次训练数据？", "？？？？？", MessageBoxButtons.YesNoCancel) == System.Windows.Forms.DialogResult.Yes)
            {
                ann.save();
                Dispose();
                Application.Exit();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }

}
