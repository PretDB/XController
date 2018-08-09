using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using Android.OS;

namespace XController
{
    public enum enum_Command
    {
        None = 100,
        Stop =0,
        Forward,
        Backward,
        LeftShift,
        RightShift,
        LeftRotate,
        RightRotate,
        IR = 10,
        Sonic,
        Track,
        Light
    };

    public enum enum_Device
    {
        None,
        Car0,
        Car1,
        Marker
    }

	public partial class MainPage : TabbedPage
	{
        public readonly string string_VideoUri = "/?action=stream";
        public readonly string string_NoDevice = @"
            <html>
                <title>Device Not Found</title>
                <style>
                    h1 { text-align: center }
                    table { height: 100%; width: 100% }
                </style>
                <body>
                    <h1>Device Not Found</h1>
                </body>
            <html>";
        public Handler handler_Toast = new Handler((Message msg) =>
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, msg.Obj.ToString(), Android.Widget.ToastLength.Short).Show();
        });



        public int Int_TCPPort
        {
            private set;
            get;
        } = 6688;
        public int Int_UDPPort
        {
            private set;
            get;
        } = 6868;

        private UdpClient udpClient;

        private IPAddress IPAddress_Car0 = IPAddress.Any;
        private IPAddress IPAddress_Car1 = IPAddress.Any;
        private IPAddress IPAddress_Marker = IPAddress.Any;
        private IPAddress IPAddress_Local = IPAddress.Any;

        private TcpClient tcpClient_Car0 = new TcpClient();
        private TcpClient tcpClient_Car1 = new TcpClient();
        private TcpClient tcpClient_Marker = new TcpClient();

        private Thread thread_UDPListener;

        private ObservableCollection<Target> targets;
        private enum_Device Device_CurrentTarget
        {
            get
            {
                return this._currentTarget;
            }
            set
            {
                this._currentTarget = value;
                switch(value)
                {
                    case enum_Device.Car0:
                        this.ConfigureWebVideo(this.IPAddress_Car0);
                        break;

                    case enum_Device.Car1:
                        this.ConfigureWebVideo(this.IPAddress_Car1);
                        break;
                }
            }
        }
        private enum_Device _currentTarget;


		public MainPage()
		{
			InitializeComponent();
            InitializeNetwork();
            InitializeTarget();
		}

        private void button_Stop_Pressed(object sender, EventArgs e)
        {
            this.button_Stop_Clicked(sender, e);
        }

        private void button_Forward_Pressed(object sender, EventArgs e)
        {
            this.MessageEmitter(this.MessageAssembler(enum_Command.Forward));
        }

        private void button_Back_Pressed(object sender, EventArgs e)
        {
            this.MessageEmitter(this.MessageAssembler(enum_Command.Backward));
        }

        private void button_LShift_Pressed(object sender, EventArgs e)
        {
            this.MessageEmitter(this.MessageAssembler(enum_Command.LeftShift));
        }

        private void button_LRotate_Pressed(object sender, EventArgs e)
        {
            this.MessageEmitter(this.MessageAssembler(enum_Command.LeftRotate));
        }

        private void button_RShift_Pressed(object sender, EventArgs e)
        {
            this.MessageEmitter(this.MessageAssembler(enum_Command.RightShift));
        }

        private void button_RRotate_Pressed(object sender, EventArgs e)
        {
            this.MessageEmitter(this.MessageAssembler(enum_Command.RightRotate));
        }

        private void button_Stop_Clicked(object sender, EventArgs e)
        {
            this.Toast("急停", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.Stop));
        }

        private void button_IRAvoidance_Clicked(object sender, EventArgs e)
        {
            this.Toast("红外避障模式", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.IR));
        }

        private void button_SonarAvoidance_Clicked(object sender, EventArgs e)
        {
            this.Toast("超声避障模式", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.Sonic));
        }

        private void button_Track_Clicked(object sender, EventArgs e)
        {
            this.Toast("黑线寻迹模式", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.Track));
        }

        private void button_LightTrack_Clicked(object sender, EventArgs e)
        {
            this.Toast("光跟踪模式", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.Light));
        }

        private void picker_Target_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(picker_Target.SelectedIndex != 0)
            {
                Target result = targets[picker_Target.SelectedIndex - 1];
                this.Device_CurrentTarget = result.device;
                this.Toast("已选择控制" + result.ToString(), false);
            }
        }

        private void button_SetttingsConfirm_Clicked(object sender, EventArgs e)
        {

        }

        private void Toast(string msg, bool isLong, bool isThread = false)
        {
            if (isThread == false)
            {
                var inf = DependencyService.Get<IInfo>();
                if (inf != null)
                {
                    inf.Toast(msg, isLong);
                }
            }
            else
            {
                Message m = this.handler_Toast.ObtainMessage();
                m.Obj = msg;
                this.handler_Toast.SendMessage(m);
            }
        }

        private void InitializeNetwork()
        {
            IPAddress localIP = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            this.IPAddress_Local = localIP;
            this.Toast("本机IP地址：" + localIP.ToString(), false);

            this.udpClient = new UdpClient(this.Int_UDPPort);

            // Using a thread.
            ThreadStart threadStart = new ThreadStart(this.UDPListener);
            this.thread_UDPListener = new Thread(threadStart);
            this.thread_UDPListener.Start();
        }

        /// <summary>
        /// Listen UDP message, which contains IP address of whose devices on
        /// local network including car0, car1 and the marker.
        /// </summary>
        private void UDPListener()
        {
            byte[] rawRecv = new byte[1024];
            string encRecv;
            string tmp;

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                rawRecv = this.udpClient.Receive(ref sender);
                encRecv = Encoding.UTF8.GetString(rawRecv);

                JObject jObject = JObject.Parse(encRecv);
                if (jObject["Type"].ToString() != "heartbeat")
                {
                    Thread.Sleep(300);
                    continue;
                }
                // Get sender's IP Address
                tmp = jObject["FromIP"].ToString();
                IPAddress tmpIPAddress = IPAddress.Parse(tmp);

                enum_Device device = enum_Device.None;

                if (jObject["FromRole"].ToString() == "car")
                {
                    switch ((int)jObject["FromID"])
                    {
                        case 0:
                            if (tmpIPAddress.ToString() != this.IPAddress_Car0.ToString())
                            {
                                this.Toast("小车0已发现，IP：" + tmp, false, true);
                                this.IPAddress_Car0 = tmpIPAddress;
                                device = enum_Device.Car0;
                            }
                            break;
                        case 1:
                            if (tmpIPAddress != this.IPAddress_Car1)
                            {
                                this.Toast("小车1已发现，IP：" + tmp, false, true);
                                this.IPAddress_Car1 = tmpIPAddress;
                                device = enum_Device.Car1;
                            }
                            break;
                    }
                }
                else if (jObject["FromRole"].ToString() == "marker" && tmpIPAddress != this.IPAddress_Marker)
                {
                    this.Toast("定位装置已发现，IP：" + tmp, false, true);
                    this.IPAddress_Marker = tmpIPAddress;
                    device = enum_Device.Marker;
                }
                else
                {
                    device = enum_Device.None;
                }
                this.TCPConnectionManager(device, new IPEndPoint(tmpIPAddress, this.Int_TCPPort));
            }
        }

        /// <summary>
        /// Managing the TCP Connection
        /// </summary>
        /// <param name="device"></param>
        /// <param name="iPEndPoint"></param>
        private void TCPConnectionManager(enum_Device device, IPEndPoint iPEndPoint)
        {
            try
            {
                switch (device)
                {
                    case enum_Device.Car0:
                        if (iPEndPoint != this.tcpClient_Car0.Client.RemoteEndPoint)
                        {
                            try
                            {
                                this.tcpClient_Car0 = new TcpClient();
                                this.tcpClient_Car0.Connect(iPEndPoint);
                            }
                            catch (System.Reflection.TargetInvocationException e)
                            {
                                this.Toast(e.Source.ToString(), true, true);
                            }

                            if (this.tcpClient_Car0.Connected)
                            {
                                this.Toast("小车0已连接", false, true);
                            }
                            else
                            {
                                this.Toast("小车0连接失败", false, true);
                            }
                        }
                        break;

                    case enum_Device.Car1:
                        if (iPEndPoint != this.tcpClient_Car1.Client.RemoteEndPoint)
                        {
                            this.tcpClient_Car1 = new TcpClient();
                            this.tcpClient_Car1.Connect(iPEndPoint);

                            this.tcpClient_Car1 = new TcpClient();
                            this.tcpClient_Car1.Connect(iPEndPoint);
                            if (this.tcpClient_Car1.Connected)
                            {
                                this.Toast("小车1已连接", false, true);
                            }
                            else
                            {
                                this.Toast("小车1连接失败", false, true);
                            }
                        }
                        break;

                    case enum_Device.Marker:
                        if (iPEndPoint != this.tcpClient_Marker.Client.RemoteEndPoint)
                        {
                            this.tcpClient_Marker = new TcpClient();
                            this.tcpClient_Marker.Connect(iPEndPoint);

                            this.tcpClient_Marker = new TcpClient();
                            this.tcpClient_Marker.Connect(iPEndPoint);
                            if (this.tcpClient_Car1.Connected)
                            {
                                this.Toast("定位模块已连接", false, true);
                            }
                            else
                            {
                                this.Toast("定位装置连接失败", false, true);
                            }
                        }
                        break;
                    default:
                        break;

                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                this.Toast("TCP网络错误", true, true);
            }
            catch (System.ArgumentNullException e)
            {
                this.Toast("参数获取异常，请重启车", true, true);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                this.Toast(e.Source.ToString(), true, true);
            }
            catch
            {
                this.Toast("发生异常，不能联系到小车", true, true);
            }
        }

 

        private void InitializeTarget()
        {
            this.targets = Data.targets;
            picker_Target.Items.Add("请选择设备");
            foreach(var target in this.targets)
            {
                picker_Target.Items.Add(target.ToString());
            }
            picker_Target.SelectedIndex = 0;
        }

        private void ConfigureWebVideo(IPAddress targetIP)
        {
            if(targetIP != IPAddress.Any)
            {
                string url0 = "http://" + targetIP.ToString() + ":8080" + this.string_VideoUri;
                string url1 = "http://" + targetIP.ToString() + ":8081" + this.string_VideoUri;

                this.webView_Monitor.Source = new UrlWebViewSource
                {
                    Url = url0
                };
                this.webView_Map.Source = new UrlWebViewSource
                {
                    Url = url1
                };

            }
            else
            {
                var source = new HtmlWebViewSource();
                source.Html = this.string_NoDevice;
                this.webView_Map.Source = source;
                this.webView_Monitor.Source = source;
            }
        }

        private JObject MessageAssembler(enum_Command command, JObject args = null)
        {
            JObject jObject_Message = new JObject
            {
                {"Type" , "instruction"},
                {"FromIP", this.IPAddress_Local.ToString() },
                {"FromID", 0 },
                {"FromRole", "Controller" },
                {"Command", command.ToString() },
                {"Args", args }
            };
            return jObject_Message;
        }

        private void MessageEmitter(JObject message)
        {
            string msg = message.ToString();
            byte[] b = Encoding.UTF8.GetBytes(msg);

            TcpClient targetClient;
            IPEndPoint iPEndPoint;
            NetworkStream stream;
            switch (this.Device_CurrentTarget)
            {
                case enum_Device.Car0:
                    targetClient = this.tcpClient_Car0;
                    iPEndPoint = new IPEndPoint(this.IPAddress_Car0, this.Int_TCPPort);
                    break;

                case enum_Device.Car1:
                    targetClient = this.tcpClient_Car1;
                    iPEndPoint = new IPEndPoint(this.IPAddress_Car1, this.Int_TCPPort);
                    break;

                case enum_Device.Marker:
                    targetClient = this.tcpClient_Marker;
                    iPEndPoint = new IPEndPoint(this.IPAddress_Marker, this.Int_TCPPort);
                    break;
                default:
                    targetClient = new TcpClient();
                    iPEndPoint = new IPEndPoint(this.IPAddress_Local, this.Int_TCPPort);
                    break;
            }

            try
            {
                if (targetClient.Connected == false)
                {
                    targetClient.Connect(iPEndPoint);
                }
                stream = targetClient.GetStream();
                stream.Write(b, 0, b.Length);
                stream.Flush();
                //stream.Close();

            }
            catch(System.Exception e)
            {
                this.Toast("网络错误, 未能发送指令:\n" + e.Message, false);
            }

        }

    }

}
