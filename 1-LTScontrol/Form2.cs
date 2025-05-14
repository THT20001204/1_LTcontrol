using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leadshine;
using System.IO; // 添加此命名空间以解决“File”未定义的问题

namespace _1_LTScontrol
{
    public partial class Form2 : Form
    {
        private Timer positionUpdateTimer; // 定义一个定时器
        private ushort axis = 0; // 轴号
        private ushort ConnectNo = 2;//连接号
        bool[] checkBoxStates = new bool[16];


        public Form2()
        {
            InitializeComponent();
            positionUpdateTimer = new Timer();
            positionUpdateTimer.Interval = 100; // 每100毫秒更新一次
            positionUpdateTimer.Tick += PositionUpdateTimer_Tick;

            // 绑定 Load 和 FormClosing 事件
            this.Load += Form2_Load;
            this.FormClosing += Form2_FormClosing;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
            // 加载上次保存的值
            numericUpDown2.Value = Properties.Settings.Default.NumericUpDown2Value;
            numericUpDown3.Value = Properties.Settings.Default.NumericUpDown3Value;
            numericUpDown4.Value = Properties.Settings.Default.NumericUpDown4Value;
            LoadTextBoxValues();
            // 页面加载时启动定时器
            positionUpdateTimer.Start();
        }
        private void LoadTextBoxValues()
        {
            for (int i = 1; i <= 48; i++)
            {
                // 动态查找 TextBox 控件
                TextBox textBox = Controls.Find($"textBox{i}", true).FirstOrDefault() as TextBox;

                // 动态获取对应的 Settings 属性值
                var property = typeof(Properties.Settings).GetProperty($"TextBox{i}Value");
                if (textBox != null && property != null)
                {
                    textBox.Text = property.GetValue(Properties.Settings.Default)?.ToString();
                }
            }
        }


        private void PositionUpdateTimer_Tick(object sender, EventArgs e)
        {
            //double position = 0; // 用于存储位置
            //double position1 = 0;
            //double position2 = 0;
            //int result = LTSMC.smc_get_position_unit(ConnectNo, 0, ref position);
            //int result1 = LTSMC.smc_get_position_unit(ConnectNo, 1, ref position1);
            //int result2 = LTSMC.smc_get_position_unit(ConnectNo, 2, ref position2);

            //if (result == 0)
            //{
                
            //    label7.Text = $"{position:F2}";
            //}
            //else if (result != 0)
            //{
            //    label7.Text = "none";
            //}
            //if (result1 == 0)
            //{
            //    label8.Text = $"{position1:F2}";
            //}
            //else if (result1 != 0)
            //{
            //    label8.Text = "none";
            //}
            //if (result2 == 0)
            //{
            //    label9.Text = $"{position2:F2}";
            //}
            //else if (result2 != 0)
            //{
            //    label9.Text = "none";
            //}

            Condition_monitoring(GlobalData.MessageToReceive);

            GlobalData.DefaultMessage = "Upgrade;" + numericUpDown5.Value + ";"+"0;0;0;0;0;0;";
            label33.Text = GlobalData.DefaultMessage;
            label34.Text = GlobalData.MessageToReceive;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked || checkBox3.Checked)
            {
                checkBox2.Checked = false;
            }
            else
            {
                if (checkBox2.Checked)
                {
                    numericUpDown1.Value = 5; // 设置值为 10
                }
                else
                {
                    numericUpDown1.Value = 0; // 设置值为 0
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked || checkBox3.Checked)
            {
                checkBox1.Checked = false;
            }
            else
            {
                if (checkBox1.Checked)
                {
                    numericUpDown1.Value = 10; // 设置值为 10
                }
                else
                {
                    numericUpDown1.Value = 0; // 设置值为 0
                }
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked || checkBox1.Checked)
            {
                checkBox3.Checked = false;
            }
            else
            {
                if (checkBox3.Checked)
                {
                    numericUpDown1.Value = 1; // 设置值为 10
                }
                else
                {
                    numericUpDown1.Value = 0; // 设置值为 0
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }


        private void button1_Click(object sender, EventArgs e)
        {
            double s = (double)numericUpDown1.Value;
            numericUpDown5.Value = 0;
            // 初始化连接
            //short str;
            //str = LTSMC.smc_board_init(ConnectNo, 2, "192.168.5.11", 3000);
            ////设置脉冲输出模式
            //LTSMC.smc_set_pulse_outmode (ConnectNo, 0, 0); // 设置脉冲输出模式
            // // 设置脉冲当量
            //LTSMC.smc_set_equiv (ConnectNo, 0, 6400);
            //// 设置加速度
            //LTSMC.smc_set_profile_unit(ConnectNo, 0, 0 ,5.0, 5.0, 5.0, 5.0 ); // 设置加速度
            ////执行运动
            //short ret = LTSMC.smc_pmove_unit(ConnectNo, 0, s, 0);
            GlobalData.MessageToSend = "Upgrade;XTranslate;"+ s+";";
            GlobalData.IsCustomMessage = true;
            //label10.Text =;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double s = (double)numericUpDown1.Value;
            numericUpDown5.Value = 0;
            // 初始化连接
            GlobalData.MessageToSend = "Upgrade;XTranslate;" + -s + ";";
            GlobalData.IsCustomMessage = true;
            //label10.Text = ret.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double s = (double)numericUpDown1.Value;
            numericUpDown5.Value = 1;
            GlobalData.MessageToSend = "Upgrade;YTranslate;" + -s + ";";
            GlobalData.IsCustomMessage = true;
            //label10.Text = ret.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double s = (double)numericUpDown1.Value;
            numericUpDown5.Value = 1;
            GlobalData.MessageToSend = "Upgrade;YTranslate;" + s + ";";
            GlobalData.IsCustomMessage = true;
            //label10.Text = ret.ToString();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            double s = (double)numericUpDown1.Value;
            numericUpDown5.Value = 2;
            // 初始化连接
            GlobalData.MessageToSend = "Upgrade;ZTranslate;" + s + ";";
            GlobalData.IsCustomMessage = true;
            //label10.Text = ret.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            double s = (double)numericUpDown1.Value;
            numericUpDown5.Value = 2;
            GlobalData.MessageToSend = "Upgrade;ZTranslate;" + -s + ";";
            GlobalData.IsCustomMessage = true;
            //label10.Text = ret.ToString();

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            double s1 = (double)numericUpDown2.Value;
            double s2 = (double)numericUpDown3.Value;
            double s3 = (double)numericUpDown4.Value;
            // 初始化连接
            if (!checkBoxALL.Checked) 
            { 
                SaveCheckBoxStates(); // 先保存 CheckBox 状态
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (checkBoxStates[i]) // 如果当前 CheckBox 被选中
                    {
                        // 计算当前 i 对应的 textBox 编号
                        int textBoxNumA = 3 * (i + 1);     // 3, 6, 9, ...
                        int textBoxNumB = 3 * (i + 1) - 1; // 2, 5, 8, ...
                        int textBoxNumC = 3 * (i + 1) - 2; // 1, 4, 7, ...

                        // 获取对应的 TextBox 控件
                        TextBox textBoxA = Controls.Find($"textBox{textBoxNumA}", true).FirstOrDefault() as TextBox;
                        TextBox textBoxB = Controls.Find($"textBox{textBoxNumB}", true).FirstOrDefault() as TextBox;
                        TextBox textBoxC = Controls.Find($"textBox{textBoxNumC}", true).FirstOrDefault() as TextBox;

                        // 获取对应的 Label 控件
                        Label label7 = Controls.Find("label7", true).FirstOrDefault() as Label;
                        Label label8 = Controls.Find("label8", true).FirstOrDefault() as Label;
                        Label label9 = Controls.Find("label9", true).FirstOrDefault() as Label;

                        // 赋值
                        if (textBoxA != null && label7 != null) double.TryParse(textBoxA.Text, out s1);
                        if (textBoxB != null && label8 != null) double.TryParse(textBoxB.Text, out s2);
                        if (textBoxC != null && label9 != null) double.TryParse(textBoxC.Text, out s3);
                        break; // 如果只需要处理第一个选中的 CheckBox，就保留 break
                    }
                }
            }
            numericUpDown2.Value = (decimal)s1;
            numericUpDown3.Value = (decimal)s2;
            numericUpDown4.Value = (decimal)s3;
            s1 = (double)numericUpDown2.Value;
            s2 = (double)numericUpDown3.Value;
            s3 = (double)numericUpDown4.Value;
            GlobalData.MessageToSend = "Upgrade;Coordinate;" + s1 + ";" +s2+ ";" + s3 + ";";
            GlobalData.IsCustomMessage = true;
        }
        private void Condition_monitoring(string cond)
        {
            //======状态监控======//
            string[] conds = cond.Split(';');

            double[] current_speed = new double[1];
            axis = (ushort)numericUpDown5.Value;
            label26.Text = conds[2];
            label11.Text = conds[3];
            label12.Text = conds[4];
            label13.Text = conds[5];
            label14.Text = conds[6];
            label15.Text = conds[7];
            label23.Text = conds[8];
            label32.Text = conds[10];
            label31.Text = conds[11];
            label7.Text = conds[12];
            label8.Text = conds[13];
            label9.Text = conds[14];



        }
        private void button8_Click(object sender, EventArgs e)
        {
            Condition_monitoring(GlobalData.MessageToReceive);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //short ret= LTSMC.nmcs_set_axis_enable(2,2);
            //label10.Text = ret.ToString();
            double pos = 0;
            LTSMC.smc_get_target_position_unit(ConnectNo, 2, ref pos);

            short ret = LTSMC.smc_set_softlimit_unit(ConnectNo, 2, 1, 1, 1, -10, 1000);
            label10.Text = ret.ToString();
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
        }

        private void label26_Click(object sender, EventArgs e)
        {

        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存当前值  
            Properties.Settings.Default.NumericUpDown2Value = numericUpDown2.Value;
            Properties.Settings.Default.NumericUpDown3Value = numericUpDown3.Value;
            Properties.Settings.Default.NumericUpDown4Value = numericUpDown4.Value;
            SaveTextBoxValues();
            Properties.Settings.Default.Save();


            Properties.Settings.Default.Save();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            short ret = 0;  
            LTSMC.smc_set_home_profile_unit(ConnectNo, axis, 0, 5, 0.1, 0.1);
            //设置回零方式
            ret = LTSMC.smc_set_homemode(ConnectNo, axis, 0, 1, 2, 1);
            //启动回零运动
            ret = LTSMC.smc_home_move(ConnectNo, axis);
            //判断当前轴状态
        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();

            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 0 && checkBoxStates[i])
                    {
                        checkBox4.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 1 && checkBoxStates[i])
                    {
                        checkBox5.Checked = false;
                        break;
                    }
                }
            }
        }
        private void SaveCheckBoxStates()
        {
            // 创建一个 bool 数组来存储 checkBox4 到 checkBox19 的 Checked 状态
           
            // 将每个 CheckBox 的 Checked 状态存入数组
            checkBoxStates[0] = checkBox4.Checked;
            checkBoxStates[1] = checkBox5.Checked;
            checkBoxStates[2] = checkBox6.Checked;
            checkBoxStates[3] = checkBox7.Checked;
            checkBoxStates[4] = checkBox8.Checked;
            checkBoxStates[5] = checkBox9.Checked;
            checkBoxStates[6] = checkBox10.Checked;
            checkBoxStates[7] = checkBox11.Checked;
            checkBoxStates[8] = checkBox12.Checked;
            checkBoxStates[9] = checkBox13.Checked;
            checkBoxStates[10] = checkBox14.Checked;
            checkBoxStates[11] = checkBox15.Checked;
            checkBoxStates[12] = checkBox16.Checked;
            checkBoxStates[13] = checkBox17.Checked;
            checkBoxStates[14] = checkBox18.Checked;
            checkBoxStates[15] = checkBox19.Checked;

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 2 && checkBoxStates[i])
                    {
                        checkBox6.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 3 && checkBoxStates[i])
                    {
                        checkBox7.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 4 && checkBoxStates[i])
                    {
                        checkBox8.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 5 && checkBoxStates[i])
                    {
                        checkBox9.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 6 && checkBoxStates[i])
                    {
                        checkBox10.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 7 && checkBoxStates[i])
                    {
                        checkBox11.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 8 && checkBoxStates[i])
                    {
                        checkBox12.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 9 && checkBoxStates[i])
                    {
                        checkBox13.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 10 && checkBoxStates[i])
                    {
                        checkBox14.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 11 && checkBoxStates[i])
                    {
                        checkBox15.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 12 && checkBoxStates[i])
                    {
                        checkBox16.Checked = false;
                        break;
                    }
                }
            }

        }

        private void checkBox17_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 13 && checkBoxStates[i])
                    {
                        checkBox17.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 14 && checkBoxStates[i])
                    {
                        checkBox18.Checked = false;
                        break;
                    }
                }
            }
        }

        private void checkBox19_CheckedChanged(object sender, EventArgs e)
        {
            SaveCheckBoxStates();
            if (!checkBoxALL.Checked)
            {
                for (int i = 0; i < checkBoxStates.Length; i++)
                {
                    if (i != 15 && checkBoxStates[i])
                    {
                        checkBox19.Checked = false;
                        break;
                    }
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            SaveCheckBoxStates(); // 先保存 CheckBox 状态
            for (int i = 0; i < checkBoxStates.Length; i++)
            {
                bool stt ;
                if (checkBoxALL.Checked) { stt = true; }else { stt = checkBoxStates[i]; }
                if (stt) // 如果当前 CheckBox 被选中
                {
                    // 计算当前 i 对应的 textBox 编号
                    int textBoxNumA = 3 * (i + 1);     // 3, 6, 9, ...
                    int textBoxNumB = 3 * (i + 1) - 1; // 2, 5, 8, ...
                    int textBoxNumC = 3 * (i + 1) - 2; // 1, 4, 7, ...

                    // 获取对应的 TextBox 控件
                    TextBox textBoxA = Controls.Find($"textBox{textBoxNumA}", true).FirstOrDefault() as TextBox;
                    TextBox textBoxB = Controls.Find($"textBox{textBoxNumB}", true).FirstOrDefault() as TextBox;
                    TextBox textBoxC = Controls.Find($"textBox{textBoxNumC}", true).FirstOrDefault() as TextBox;

                    // 获取对应的 Label 控件
                    Label label7 = Controls.Find("label7", true).FirstOrDefault() as Label;
                    Label label8 = Controls.Find("label8", true).FirstOrDefault() as Label;
                    Label label9 = Controls.Find("label9", true).FirstOrDefault() as Label;

                    // 赋值
                    if (textBoxA != null && label7 != null) textBoxA.Text = label7.Text;
                    if (textBoxB != null && label8 != null) textBoxB.Text = label8.Text;
                    if (textBoxC != null && label9 != null) textBoxC.Text = label9.Text;
                    if (!checkBoxALL.Checked) { break; }
                    // 如果只需要处理第一个选中的 CheckBox，就保留 break
                }
            }
        }
        private void SaveTextBoxValues()
        {
            for (int i = 1; i <= 48; i++)
            {
                // 动态查找 TextBox 控件
                TextBox textBox = Controls.Find($"textBox{i}", true).FirstOrDefault() as TextBox;

                // 动态获取对应的 Settings 属性
                var property = typeof(Properties.Settings).GetProperty($"TextBox{i}Value");
                if (textBox != null && property != null)
                {
                    property.SetValue(Properties.Settings.Default, textBox.Text);
                }
            }
        }

        private void checkBoxALL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxALL.Checked)
            {
                // 遍历从 checkBox4 到 checkBox19 的控件
                for (int i = 4; i <= 19; i++)
                {
                    // 获取当前复选框的名称
                    string checkBoxName = $"checkBox{i}";

                    // 根据名称获取对应的复选框控件
                    CheckBox currentCheckBox = this.Controls.Find(checkBoxName, true).FirstOrDefault() as CheckBox;

                    if (currentCheckBox != null)
                    {
                        currentCheckBox.Checked = true;
                    }
                }
            }
            else if (!checkBoxALL.Checked) 
            {
                // 遍历从 checkBox4 到 checkBox19 的控件
                for (int i = 4; i <= 19; i++)
                {
                    // 获取当前复选框的名称
                    string checkBoxName = $"checkBox{i}";

                    // 根据名称获取对应的复选框控件
                    CheckBox currentCheckBox = this.Controls.Find(checkBoxName, true).FirstOrDefault() as CheckBox;

                    if (currentCheckBox != null)
                    {
                        currentCheckBox.Checked = false;
                    }
                }
            }
        }

        private void label32_Click(object sender, EventArgs e)
        {

        }
    }
}
