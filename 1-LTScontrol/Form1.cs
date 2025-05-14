using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leadshine;

namespace _1_LTScontrol
{
    public partial class Form1 : Form
    {
        // 定义一个变量
        private int myVariable = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            short str;
            str = LTSMC.smc_board_init(2, 2, "192.168.5.11", 3000);
            if (str != 0)
            {
                MessageBox.Show("初始化失败: " + str.ToString());
            }
            else
            {
                // 显示带有按钮的提示框
                DialogResult result = MessageBox.Show("初始化成功，是否下一步？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                // 如果用户点击“是”，则显示Form2
                if (result == DialogResult.Yes)
                {
                    Form3 newForm2 = new Form3();
                    newForm2.Show();
                    this.Hide();
                }
                else if (result == DialogResult.No) {
                    this.Close();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
  
            Process.Start(@"D:\LTcontrol\LTScontrol\1-LTScontrol\APP\main.exe");
        }

        private void fontDialog1_Apply(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            short str, ret, res;
            str = LTSMC.smc_board_close(2);
            if (str!=0)
            {
                MessageBox.Show("关闭失败: " + str.ToString());
            }
            else
            {
                MessageBox.Show("关闭成功");
                this.Close();
            }
        }  

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
