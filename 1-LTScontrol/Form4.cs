using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp.Extensions;
using OpenCvSharp;


namespace _1_LTScontrol
{
    public partial class Form4 : Form
    {
        private string selectedImagePath; // 记录选中的图片路径
        private string selectedFolderPath; // 记录图片所在文件夹路径
                                           // 获取 Form3 实例
        Form3 form3Instance = Application.OpenForms["Form3"] as Form3;
        public Form4()
        {
            InitializeComponent();
        }

        
        private void button1_Click(object sender, EventArgs e)
        {

            Form3 form3Instance = Application.OpenForms["Form3"] as Form3;
            if (form3Instance != null && form3Instance._isCameraRunning)
            {
                // 弹出提示框，询问是否停止摄像头
                DialogResult result = MessageBox.Show("摄像头正在运行，是否停止？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    // 用户点击“确定”，停止摄像头
                    form3Instance.button1.PerformClick();
                    //form3Instance.StopCamera();
                    MessageBox.Show("摄像头已停止！");
                }
                else
                {
                    // 用户点击“取消”，直接返回
                    return;
                }
            }
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择图片文件",
                Filter = "图片文件|*.jpg;*.jpeg;*.png|所有文件|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    selectedImagePath = openFileDialog.FileName; // 记录选中的图片路径
                    selectedFolderPath = System.IO.Path.GetDirectoryName(selectedImagePath); // 获取文件夹路径

                    using (Mat image = Cv2.ImRead(selectedImagePath, ImreadModes.Color))
                    {
                        if (!image.Empty())
                        {
                            Bitmap bitmap = BitmapConverter.ToBitmap(image);

                            // 假设 Form3 的实例是 form3Instance
                            if (form3Instance != null)
                            {
                                form3Instance.UpdatePictureBox(bitmap); // 调用 Form3 的方法
                            }
                            else
                            {
                                MessageBox.Show("Form3 未打开！");
                            }
                        }
                        else
                        {
                            MessageBox.Show("无法读取选中的文件");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载失败：{ex.Message}");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolderPath) || string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("请先选择一张图片！");
                return;
            }

            // 获取文件夹内的所有图片文件路径，按时间顺序排序
            string[] imagePaths = System.IO.Directory.GetFiles(selectedFolderPath, "*.*")
                .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => System.IO.File.GetCreationTime(file)) // 按文件创建时间排序
                .ToArray();

            // 找到选中图片的索引
            int startIndex = Array.IndexOf(imagePaths, selectedImagePath);
            if (startIndex == -1)
            {
                MessageBox.Show("选中的图片不在文件夹中！");
                return;
            }

            // 获取循环次数和延时
            int loopCount = (int)numericUpDown1.Value;
            int delay = (int)numericUpDown2.Value;


            if (form3Instance != null)
            {
                // 从选中图片开始播放
                string[] imagesToPlay = imagePaths.Skip(startIndex).Take(loopCount).ToArray();
                form3Instance.PlayImages(imagesToPlay, 1, delay); // 调用 Form3 的方法
                
            }
            else
            {
                MessageBox.Show("Form3 未打开！");
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }
    }
}