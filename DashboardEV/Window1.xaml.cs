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
using System.Windows.Shapes;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Subscribing;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LiveCharts.Wpf;
using System.Timers;
using System.IO;
using System.Net;
using System.IO.Ports;
using System.Windows.Media.Animation;
using System.Globalization;
using System.IO;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Configurations;
using System.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Events;



namespace DashboardEV
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public IMqttClient Client { get; private set; }

        public AxesCollection AxisYCollection { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public SeriesCollection SeriesCollection2 { get; set; }
        public List<string> Labels { get; set; }
        private double[] temp = { 10, 20, 32, 40, 50, 65, 70, 80, 90, 100};
        LineSeries mylineseries;
        LineSeries mylineseries2;
        LineSeries mylineseries3;
        LineSeries mylineseries4;
        //public DateTime(int minute, int second);

        public Window1(IMqttClient client)
        {
            Client = client;
            InitializeComponent();

      

            btnSubscribe.Click += async (o, e)
                => await BtnSubscribe_Click(o, e);


            //Instantiate a line chart
            mylineseries = new LineSeries();
            mylineseries2 = new LineSeries();
            mylineseries3 = new LineSeries();
            mylineseries4 = new LineSeries();

            //Set the title of the polyline
            mylineseries.Title = "Throttle";
            mylineseries2.Title = "Voltage";
            mylineseries3.Title = "Current";
            mylineseries4.Title = "Watt";


            //line chart line form
            mylineseries.LineSmoothness = 2;
            mylineseries2.LineSmoothness = 0;
            mylineseries3.LineSmoothness = 0;
            mylineseries4.LineSmoothness = 0;

            //Distance style of line chart
            mylineseries.PointGeometry = DefaultGeometries.Triangle;
            mylineseries2.PointGeometry = DefaultGeometries.Square;
            mylineseries3.PointGeometry = DefaultGeometries.Circle;
            mylineseries4.PointGeometry = DefaultGeometries.Diamond;

            mylineseries.Values = new ChartValues<double>(temp);
            mylineseries2.Values = new ChartValues<double>(temp);
            mylineseries3.Values = new ChartValues<double>(temp);
            mylineseries4.Values = new ChartValues<double>(temp);

            SeriesCollection = new SeriesCollection { };
            SeriesCollection.Add(mylineseries);
            SeriesCollection.Add(mylineseries2);
            SeriesCollection.Add(mylineseries3);
            SeriesCollection.Add(mylineseries4);

            AxisYCollection = new AxesCollection { };
            new Axis { Foreground = Brushes.Red };
            new Axis { Foreground = Brushes.Red };



            //Add the abscissa
            Labels = new List<string> { " "};

            DataContext = this;

        }


        


        private async Task BtnSubscribe_Click(object sender, EventArgs e)
        {
            try
            {
                btnSubscribe.IsEnabled = false;
                txtTopic.IsEnabled = false;

                var result = (await Client.SubscribeAsync(
                        new TopicFilterBuilder()
                        .WithTopic(txtTopic.Text).Build()
                    )).Items[0];

                switch (result.ResultCode)
                {
                    case MqttClientSubscribeResultCode.GrantedQoS0:
                    case MqttClientSubscribeResultCode.GrantedQoS1:
                    case MqttClientSubscribeResultCode.GrantedQoS2:
                        //panelFeed.Visibility = Visibility.Visible;
                        //Height += panelFeed.Height;31

                        Client.UseApplicationMessageReceivedHandler(me =>
                        {


                            var msg = me.ApplicationMessage;
                            var data = Encoding.UTF8.GetString(msg.Payload);

                            
                            
                            //Add the data of the line chart
                            
                            //_trend = 8;
                            //linestart();
                            //DataContext = this;

                            //txtStream.Dispatcher.Invoke(new Action(() => { txtStream.AppendText(DataParser(data)); }), DispatcherPriority.Normal);
                            this.Dispatcher.Invoke(() =>
                            {
                                ampGauge.Value = DataCurrent(data);
                                voltageGauge.Value = DataVoltage(data);
                                throttleBar.Value = DataThrottle(data);
                                throttleLabel.Content = DataThrottle(data) + "%";
                                wattHours.Content = DataWH(data);
                                ampereHours.Content = DataAH(data);
                                amperePeak.Content = DataAP(data);
                                wattPeak.Content = DataWP(data);
                                speedGauge.Value = DataSpeed(data);
                                rpmGauge.Value = DataRPM(data);
                                wattGauge.Value = DataWatt(data);
                                Labels.Add(DateTime.Now.ToString());
                                Labels.RemoveAt(0);
                                SeriesCollection[0].Values.Add(DataThrottle(data));
                                SeriesCollection[0].Values.RemoveAt(0);
                                SeriesCollection[1].Values.Add(DataVoltage(data));
                                SeriesCollection[1].Values.RemoveAt(0);
                                SeriesCollection[2].Values.Add(DataCurrent(data));
                                SeriesCollection[2].Values.RemoveAt(0);
                                SeriesCollection[3].Values.Add(DataWatt(data)/10);
                                SeriesCollection[3].Values.RemoveAt(0);

                            });

                        });

                        break;
                    default:
                        throw new Exception(result.ResultCode.ToString());
                }

            }
            catch (Exception ex)
            {
                txtTopic.IsEnabled = true;
                btnSubscribe.IsEnabled = true;
                this.Error(ex);
            }
        }


        public string DataParser(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.rpm.ToString() + "      " + mobil.kecepatan.ToString() + "\n";


        }

        public double DataCurrent(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.a;


        }

        public double DataVoltage(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.v;

        }


        public double DataThrottle(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);

            return mobil.throttle;
        }


        public double DataWH(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.wh;
        }

        public double DataAH(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.ah;
        }

        public double DataAP(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.ap;
        }

        public double DataWP(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.wp;
        }

        public double DataSpeed(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.kecepatan;
        }

        public double DataRPM(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);


            return mobil.rpm;
        }

        public double DataWatt(string data)
        {
            JObject json = JObject.Parse(data);
            var results = json["m2m:rsp"]["pc"]["m2m:cin"]["con"].ToString();

            MobilModel mobil = JsonConvert.DeserializeObject<MobilModel>(results);
             

            return mobil.w;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
           
                Close();

        }

        private void Axis_OnRangeChanged(RangeChangedEventArgs eventargs)
        {
            var vm = (ScrollableViewModel)DataContext;

            var currentRange = eventargs.Range;

            if (currentRange < TimeSpan.TicksPerDay * 2)
            {
                vm.Formatter = x => new DateTime((long)x).ToString("t");
                return;
            }

            if (currentRange < TimeSpan.TicksPerDay * 60)
            {
                vm.Formatter = x => new DateTime((long)x).ToString("dd MMM yy");
                return;
            }

            if (currentRange < TimeSpan.TicksPerDay * 540)
            {
                vm.Formatter = x => new DateTime((long)x).ToString("MMM yy");
                return;
            }

            vm.Formatter = x => new DateTime((long)x).ToString("yyyy");
        }

        public void Dispose()
        {
            var vm = (ScrollableViewModel)DataContext;
            vm.Values.Dispose();
        }


    }
    
}
