using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Formatter;
using LiveCharts;
using LiveCharts.Wpf;
using System.IO;
using System.Windows.Data;
using System.Windows.Controls;
using System.Timers;
using System.Globalization;
using System.Windows.Media.Animation;

namespace DashboardEV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            btnConnect.Click += async (o, e) => {
                await BtnConnect_ClickAsync(o, e);
            };
        }

        private async Task BtnConnect_ClickAsync(object sender, System.EventArgs e)
        {
            try
            {
                btnConnect.IsEnabled = false;

                var client = new MqttFactory().CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(txtHost.Text, int.Parse(txtPort.Text))
                    .WithCredentials(txtUsername.Text, txtPassword.Text)
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                    .Build();

                var auth = await client.ConnectAsync(options);

                if (auth.ResultCode != MqttClientConnectResultCode.Success)
                {
                    throw new Exception(auth.ResultCode.ToString());
                }
                else
                {
                    var feedFrm = new Window1(client);

                    try
                    {
                        Hide();
                        feedFrm.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        this.Error(ex);
                    }

                    Close();

                }
            }
            catch (Exception ex)
            {
                this.Error(ex);
                btnConnect.IsEnabled = true;
            }
        }
    }
}

