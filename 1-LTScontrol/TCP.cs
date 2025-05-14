using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.NetworkInformation;
using Leadshine;
using OpenCvSharp;


namespace _1_LTScontrol
{


    public partial class TCP : Form
    {
        //private System.Windows.Forms.ListView listView1; // 明确指定使用 System.Windows.Forms.ListView  
        private System.Windows.Forms.Timer timer; // 明确指定使用 System.Windows.Forms.Timer  
        private TcpClient tcpClient; // 添加字段声明    
        private NetworkStream stream; // 添加字段声明    
        public string messageToSend = "Hello, Server!"; // 发送的消息
        private CancellationTokenSource sendCts;


        private List<TcpConnectionInformation> lastConnections = new List<TcpConnectionInformation>();

        public TCP()
        {
            InitializeComponent();

            // 初始化 ListView 控件
            //listView1 = new System.Windows.Forms.ListView();
            //listView2.Dock = DockStyle.Fill;
            listView2.View = View.Details;//
            listView2.Columns.AddRange(new[] {
        new ColumnHeader { Text = "Local Endpoint", Width = 150 },
        new ColumnHeader { Text = "Remote Endpoint", Width = 150 },
        new ColumnHeader { Text = "State", Width = 100 }
    });
            this.Controls.Add(listView2);

            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 只采集数据，不直接操作UI
            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var connections = properties.GetActiveTcpConnections();
                // 只在UI线程刷新
                if (listView2.InvokeRequired)
                {
                    listView2.Invoke(new Action(() => UpdateConnectionsUI(connections)));
                }
                else
                {
                    UpdateConnectionsUI(connections);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void UpdateConnectionsUI(TcpConnectionInformation[] connections)
        {
            listView2.BeginUpdate();
            listView2.Items.Clear();

            foreach (var conn in connections)
            {
                var item = new ListViewItem(new[] {
            $"{conn.LocalEndPoint.Address}:{conn.LocalEndPoint.Port}",
            $"{conn.RemoteEndPoint.Address}:{conn.RemoteEndPoint.Port}",
            conn.State.ToString()
        });
                listView2.Items.Add(item);
            }
            listView2.EndUpdate();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string ip = textBox1.Text;
            int port = int.Parse(textBox2.Text);


            tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(ip, port);
                stream = tcpClient.GetStream();
                MessageBox.Show("连接成功！");

                // 启动发送循环
                _ = SendLoopAsync();
                // 启动接收循环
                _ = ReceiveLoopAsync();

                //// 发送消息
                //byte[] sendData = Encoding.UTF8.GetBytes(messageToSend);
                //await stream.WriteAsync(sendData, 0, sendData.Length);
                //MessageBox.Show($"Sent: {messageToSend}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败: " + ex.Message);
            }
        }

        private CancellationTokenSource receiveCts;
        // Add the missing field declaration for `lastSentMessage` at the class level.
        private string lastSentMessage;

        private async Task ReceiveLoopAsync()
        {
            byte[] buffer = new byte[1024];
            receiveCts = new CancellationTokenSource();
            try
            {
                while (tcpClient != null && tcpClient.Connected && !receiveCts.Token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, receiveCts.Token);
                    if (bytesRead > 0)
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        // 这里可以将消息显示到界面上，比如追加到TextBox或ListBox
                        GlobalData.MessageToReceive = msg;
                        string[] parts = msg.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] tosend = GlobalData.MessageToSend.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        // 判断是否为自定义消息的回应

                        // Fix for CS0428: Call the ToString() method instead of referencing it as a method group.
                        label3.Text = GlobalData.IsCustomMessage.ToString();
                        label4.Text = parts[0].ToString();
                        label5.Text = tosend[0].ToString();
                        label6.Text = parts[1].ToString();
                        label7.Text = msg;
                        if (GlobalData.IsCustomMessage && parts[0] == tosend[0] && parts[1] != "0")
                        {
                            // 收到自定义消息回应，恢复默认消息
                            GlobalData.IsCustomMessage = false;
                            GlobalData.DefaultMessage = "Upgrade;0;0;0;0;0;0;0;";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // 连接断开或异常
                Invoke(new Action(() =>
                {
                    MessageBox.Show("接收异常: " + ex.Message);
                }));
            }
        }
        private async Task SendLoopAsync()
        {
            sendCts = new CancellationTokenSource();
            try
            {
                while (tcpClient != null && tcpClient.Connected && !sendCts.Token.IsCancellationRequested)
                {
                    // 判断是否有自定义消息需要发送
                    if (GlobalData.IsCustomMessage)
                    {
                        lastSentMessage = GlobalData.MessageToSend;
                    }
                    else
                    {
                        lastSentMessage = GlobalData.DefaultMessage;
                    }
                    byte[] sendData = Encoding.UTF8.GetBytes(lastSentMessage); // 修复：将 MessageToSend 替换为正确的字段名 messageToSend
                    await stream.WriteAsync(sendData, 0, sendData.Length, sendCts.Token);
                    // 可选：在界面显示已发送
                    // Invoke(new Action(() => { textBoxReceive.AppendText($"已发送: {messageToSend}\r\n"); }));
                    await Task.Delay(1000, sendCts.Token); // 每300ms发送一次
                }
            }
            catch (TaskCanceledException)
            {
                // 正常中断
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("发送异常: " + ex.Message);
                }));
            }
        }



        public void StopReceive()
        {
            receiveCts?.Cancel();
        }
        public void StopSend()
        {
            sendCts?.Cancel();
        }

        private void TCP_Load(object sender, EventArgs e)
        {
        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. 停止发送和接收循环
                StopSend();
                StopReceive();

                // 2. 关闭网络流和 TCP 客户端
                if (stream != null)
                {
                    stream.Close(); // 或 stream.Dispose()
                    stream = null;
                }

                if (tcpClient != null)
                {
                    if (tcpClient.Connected)
                    {
                        tcpClient.Close(); // 关闭连接
                    }
                    tcpClient.Dispose(); // 释放资源
                    tcpClient = null;
                }

                MessageBox.Show("连接已断开");
            }
            catch (ObjectDisposedException)
            {
                // 忽略对象已释放的异常
            }
            catch (Exception ex)
            {
                MessageBox.Show($"断开连接失败: {ex.Message}");
            }
        }
    }
}
