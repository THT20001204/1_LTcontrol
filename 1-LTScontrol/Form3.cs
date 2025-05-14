using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace _1_LTScontrol
{
    public partial class Form3 : Form
    {
        // 摄像头相关变量
        private VideoCapture _capture;
        private Thread _cameraThread;
        public bool _isCameraRunning = false;


        // ROI 绘制相关变量
        private bool _roiDrawing = false;
        // Add explicit namespace to resolve ambiguity for Point
        private System.Drawing.Point _roiStart;
        private Rectangle _roiRect = Rectangle.Empty;

        // The issue is caused by the following line in the constructor:
        // _roiRect = new Rectangle(e.Location, new System.Drawing.Size(0, 0));

        // The variable "e" is not defined in this context. It seems like this line was mistakenly added here.
        // To fix the issue, simply remove or replace this line with a valid initialization for _roiRect.

        public Form3()
        {
            InitializeComponent();

            // Correct initialization of _roiRect
            _roiRect = Rectangle.Empty;

            this.FormClosing += Form3_FormClosing;

            // 工具栏按钮事件
            btnStartRoi.Click += BtnStartRoi_Click;
            btnClearRoi.Click += BtnClearRoi_Click;

            // PictureBox 鼠标事件
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.Paint += PictureBox1_Paint;
            this.Load += new System.EventHandler(this.Form3_Load);

        }
        private void EmbedForm2InTabPage2()
        {
            // 创建 Form2 的实例
            Form2 form2 = new Form2();

            // 设置 Form2 为非顶级控件
            form2.TopLevel = false;

            // 移除 Form2 的边框样式
            form2.FormBorderStyle = FormBorderStyle.None;

            // 设置 Form2 的 Dock 属性为 Fill，使其填充整个 TabPage
            form2.Dock = DockStyle.Fill;

            // 将 Form2 添加到 tabPage2 的控件集合中
            tabPage2.Controls.Add(form2);

            // 显示 Form2
            form2.Show();
        }


        private void BtnStartRoi_Click(object sender, EventArgs e)
        {
            _roiDrawing = true;
            pictureBox1.Cursor = Cursors.Cross;
        }

        private void BtnClearRoi_Click(object sender, EventArgs e)
        {
            _roiRect = Rectangle.Empty;
            pictureBox1.Invalidate();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {


            if (_roiDrawing && e.Button == MouseButtons.Left)
            {
                _roiStart = e.Location;
                _roiRect = new Rectangle(e.Location, new System.Drawing.Size(0, 0)); // 显式指定 System.Drawing.Size
            }
        }
        // 判断当前图像是否为背景图像的方法
        private bool IsBackgroundImage()
        {
            // 假设背景图像是通过某种方式加载的，例如文件名为 "background.jpg"
            // 你可以根据实际逻辑修改此方法
            
            if (pictureBox1.Image == null )
            {
                return true;
            }
            return false;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)//
        {
            if (_roiDrawing && e.Button == MouseButtons.Left)
            {
                int x = Math.Min(_roiStart.X, e.X);
                int y = Math.Min(_roiStart.Y, e.Y);
                int w = Math.Abs(_roiStart.X - e.X);
                int h = Math.Abs(_roiStart.Y - e.Y);
                _roiRect = new Rectangle(x, y, w, h);
                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_roiDrawing && e.Button == MouseButtons.Left)
            {
                _roiDrawing = false;
                pictureBox1.Cursor = Cursors.Default;
            }
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (_roiRect != Rectangle.Empty)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, _roiRect);
                }
            }
        }
        // 修改后的摄像头开启方法
        internal void OpenCamera()
        {
            _capture = new VideoCapture(0); // 0 表示默认摄像头
            if (!_capture.IsOpened())
            {
                MessageBox.Show("无法打开摄像头");
                return;
            }

            using (Mat frame = new Mat())
            {
                while (_isCameraRunning)
                {
                    _capture.Read(frame); // 读取一帧
                    if (frame.Empty()) break;

                    // 将 OpenCV Mat 转换为 Bitmap
                    using (Bitmap bitmap = BitmapConverter.ToBitmap(frame))
                    {
                        // 跨线程安全更新 PictureBox
                        pictureBox1.Invoke((MethodInvoker)delegate
                        {
                            if (pictureBox1.Image != null)
                            {
                                pictureBox1.Image.Dispose(); // 释放旧图像
                            }
                            pictureBox1.Image = (Bitmap)bitmap.Clone(); // 显示新帧
                        });
                    }

                    Thread.Sleep(30); // 控制帧率（约33FPS）
                }
            }

            StopCamera();
        }

        // 停止摄像头
        internal void StopCamera()
        {
            _isCameraRunning = false;// 停止摄像头线程
            _capture?.Release();//
            _capture?.Dispose();
            _capture = null;

            // 清理 PictureBox 图像
            pictureBox1.Invoke((MethodInvoker)delegate
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
            });
        }
        // 在 Form3 类中添加以下方法
        private void label1_Click(object sender, EventArgs e)
        {
            // 在这里添加您希望的逻辑，或者如果不需要任何操作，可以留空
        }

        // 按钮点击事件
        public void button1_Click(object sender, EventArgs e)
        {
            if (!_isCameraRunning)
            {
                _isCameraRunning = true;
                _cameraThread = new Thread(OpenCamera);
                _cameraThread.Start();
                button1.Text = "停止摄像头";
            }
            else
            {
                StopCamera();
                button1.Text = "开启摄像头";
            }
        }
        public void UpdatePictureBox(Bitmap bitmap)
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke((MethodInvoker)(() => UpdatePictureBox(bitmap)));
            }
            else
            {
                pictureBox1.Image?.Dispose(); // 释放旧图像
                pictureBox1.Image = bitmap;   // 显示新图像
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Form4 newForm = new Form4();
            newForm.Show();
        }
        public void PlayImages(string[] imagePaths, int loopCount, int delay)
        {
            Thread imageThread = new Thread(() =>
            {
                for (int i = 0; i < loopCount; i++) // 循环次数
                {
                    foreach (string imagePath in imagePaths)
                    {
                        if (!System.IO.File.Exists(imagePath)) continue;

                        // 加载图片并更新 PictureBox
                        using (Bitmap bitmap = new Bitmap(imagePath))
                        {
                            UpdatePictureBox((Bitmap)bitmap.Clone());
                        }

                        Thread.Sleep(delay); // 延时
                    }
                }
            });

            imageThread.IsBackground = true;
            imageThread.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2();
            newForm.Show();
        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCamera(); // 停止摄像头并释放资源
            Thread.Sleep(500); // 等待 0.5秒
            Application.Exit();
        }

        private void btnStartRoi_Click_1(object sender, EventArgs e)
        {
            if (IsBackgroundImage())
            {
                MessageBox.Show("当前图像为背景图像，无法绘制 ROI。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void Form3_Load(object sender, EventArgs e)//Form3显示时加载
        {
            EmbedForm2InTabPage2();
        }


        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            TCP TCPForm = new TCP();
            TCPForm.Show();
        }
    }
}