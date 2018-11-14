﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using Android.OS;
using SkiaSharp;
using SkiaSharp.Views.Forms;

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
        Light,
        HumanDetect,
        FireDetect,
        SoundDetect,
        Track = 1000
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
		public double speed = 0.2;
        public double orientation = 0;
		public bool fireDetect = false;
        public enum_Command lastCommand = enum_Command.Stop;
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

        private Point point_CarCurrentLoc = new Point(0, 0);
        private Queue<SKPoint> points_historicalLoc0 = new Queue<SKPoint>(10);
        private Queue<SKPoint> points_historicalLoc1 = new Queue<SKPoint>(10);

		public MainPage()
		{
			InitializeComponent();
            InitializeNetwork();
            InitializeTarget();
            InitializeGravity();
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

        private void button_HumanDetect_Clicked(object sender, EventArgs e)
        {
            this.Toast("人活动检测", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.HumanDetect));
        }

        private void button_Fire_Clicked(object sender, EventArgs e)
        {
            this.Toast("反转灭火功能", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.FireDetect));
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

        private void Accelerometer_Changed(object sender, AccelerometerChangedEventArgs e)
        {
            enum_Command cmd = enum_Command.Stop;
            var data = e.Reading.Acceleration;
            if (Math.Abs(data.X) < 0.1f && data.Y < 0.2f)
            {
                cmd = enum_Command.Forward;
            }
            else if (Math.Abs(data.X) < 0.1f && data.Y > 0.61f)
            {
                cmd = enum_Command.Backward;
            }
            else if (data.X > 0.3f && data.Y > 0.4f)
            {
                cmd = enum_Command.LeftRotate;
            }
            else if (data.X < -0.3f && data.Y > 0.4f)
            {
                cmd = enum_Command.RightRotate;
            }
            else if (Math.Abs(data.Y) < 0.2f && data.X > 0.3f)
            {
                cmd = enum_Command.LeftShift;
            }
            else if (Math.Abs(data.Y) < 0.2f && data.X < -0.3f)
            {
                cmd = enum_Command.RightShift;
            }
            else
            {
                cmd = enum_Command.Stop;
            }
            if(cmd != this.lastCommand)
            {
                switch(cmd)
                {
                    case enum_Command.Stop:
                        this.button_Stop_Clicked(sender, e);
                        break;
                    case enum_Command.Forward:
                        this.button_Forward_Pressed(sender, e);
                        break;
                    case enum_Command.Backward:
                        this.button_Back_Pressed(sender, e);
                        break;
                    case enum_Command.LeftRotate:
                        this.button_LRotate_Pressed(sender, e);
                        break;
                    case enum_Command.RightRotate:
                        this.button_RRotate_Pressed(sender, e);
                        break;
                    case enum_Command.LeftShift:
                        this.button_LShift_Pressed(sender, e);
                        break;
                    case enum_Command.RightShift:
                        this.button_RShift_Pressed(sender, e);
                        break;
                }
                lastCommand = cmd;
                Thread.Sleep(100);
            }
        }

        private void picker_Mode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(picker_Mode.SelectedIndex)
            {
                case 1:
                    this.button_IRAvoidance_Clicked(sender, e);
                    break;
                case 2:
                    this.button_SonarAvoidance_Clicked(sender, e);
                    break;
                case 3:
                    this.button_Track_Clicked(sender, e);
                    break;
                case 4:
                    this.button_LightTrack_Clicked(sender, e);
                    break;
                case 5:
                    this.button_HumanDetect_Clicked(sender, e);
                    break;
                    break;
                default:
                    //    this.button_Stop_Clicked(sender, e);
                    break;
            }
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

                JObject jObject_Msg = null;
                bool canSetLoc = false;
                bool canSetOri = false;
                Point p = new Point(this.point_CarCurrentLoc.X, this.point_CarCurrentLoc.Y);
                double o = 0.0;

                if (jObject["Msg"].HasValues)
                {
                    jObject_Msg = jObject["Msg"] as JObject;
                    if (jObject_Msg.ContainsKey("position"))
                    {
                        JObject pos = jObject_Msg["position"] as JObject;
                        p = new Point(double.Parse( pos["X"].ToString()), double.Parse( pos["Y"].ToString()));
                        canSetLoc = true;
                    }
                    if(jObject_Msg.ContainsKey("orientation"))
                    {
                        o = double.Parse(jObject_Msg["orientation"].ToString());
                        canSetOri = true;
                    }
                }

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
                            if (this.Device_CurrentTarget == enum_Device.Car0)
                            {
                                if (canSetLoc)
                                {
                                    this.point_CarCurrentLoc = p;
                                    if (this.points_historicalLoc0.Count > 10)
                                    {
                                        this.points_historicalLoc0.Dequeue();
                                    }
                                    this.points_historicalLoc0.Enqueue(new SKPoint((float)p.X, (float)p.Y));
                                }
                                if(canSetOri)
                                {
                                    this.orientation = o;
                                }
                            }
                            break;
                        case 1:
                            if (tmpIPAddress.ToString() != this.IPAddress_Car1.ToString())
                            {
                                this.Toast("小车1已发现，IP：" + tmp, false, true);
                                this.IPAddress_Car1 = tmpIPAddress;
                                device = enum_Device.Car1;
                            }
                            if (this.Device_CurrentTarget == enum_Device.Car1)
                            {
                                if (canSetLoc)
                                {
                                    this.point_CarCurrentLoc = p;
                                    if (this.points_historicalLoc1.Count > 10)
                                    {
                                        this.points_historicalLoc1.Dequeue();
                                    }
                                    this.points_historicalLoc1.Enqueue(new SKPoint((float)p.X, (float)p.Y));
                                }
                                if(canSetOri)
                                {
                                    this.orientation = o;
                                }
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
                this.skCanvas.InvalidateSurface();
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
                            try
                            {
                                this.tcpClient_Car1 = new TcpClient();
                                this.tcpClient_Car1.Connect(iPEndPoint);
                            }
                            catch(System.Reflection.TargetInvocationException e)
                            {
                                this.Toast(e.Source.ToString(), true, true);
                            }

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
                this.Toast("指令流错误，请重启小车试试", true, true);
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
                this.Toast("未知异常", true, true);
            }
        }

 

        private void InitializeTarget()
        {
            this.targets = Data.targets;
            picker_Target.Items.Add("设备");
            foreach(var target in this.targets)
            {
                picker_Target.Items.Add(target.ToString());
            }
            picker_Target.SelectedIndex = 0;

            picker_Mode.Items.Add("遥控模式");
            picker_Mode.Items.Add("红外避障模式");
            picker_Mode.Items.Add("超声避障模式");
            picker_Mode.Items.Add("寻迹模式");
            picker_Mode.Items.Add("光跟踪模式");
            picker_Mode.Items.Add("人体活动检测模式");
            picker_Mode.SelectedIndex = 0;
        }

        private void InitializeGravity()
        {
            Accelerometer.ReadingChanged += this.Accelerometer_Changed;
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

            }
            else
            {
                var source = new HtmlWebViewSource();
                source.Html = this.string_NoDevice;
                // this.webView_Map.Source = source;
                this.webView_Monitor.Source = source;
            }
        }

        private JObject MessageAssembler(enum_Command command, JObject args = null)
        {
			JObject arg = new JObject
			{
				{ "Speed", this.speed },
				{ "Fire", this.fireDetect },
			};
            JObject jObject_Message = new JObject
            {
                {"Type" , "instruction"},
                {"FromIP", this.IPAddress_Local.ToString() },
                {"FromID", 0 },
                {"FromRole", "Controller" },
                {"Command", (int)command },
                {"Args", arg }
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
                case enum_Device.None:
                    return;

                default:
                    // Code below is useless, what's more, it causes crashing when trying to
                    // execute a command without control target selected.
                    // However, disabling the code commented below will cause
                    // "targetClient" used but not assigned. 
                    // So I have to return this function, after all, with an
                    // invalid device, nothing will be executed.

                    //targetClient = new TcpClient(IPAddress.None.ToString(), this.Int_TCPPort);
                    //iPEndPoint = new IPEndPoint(this.IPAddress_Local, this.Int_TCPPort);

                    return;
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
            catch(System.Net.Sockets.SocketException e)
            {
                this.Toast("似乎没有网络连接，可以重启小车和APP，并选择控制对象试试", true);
            }
            catch(System.ObjectDisposedException e)
            {
                this.Toast("似乎没有网络连接", false, false);
            }
            catch(System.Exception e)
            {
                this.Toast("网络错误, 未能发送指令，请重试\n" + e.Message, false);
                this.TCPConnectionManager(this.Device_CurrentTarget, iPEndPoint);
            }

        }

        private void button_SetttingsConfirm_Clicked(object sender, EventArgs e)
        {

        }

        private void SKCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            Point[] s = { new Point(1.5 / 6 * info.Width, 1.25 / 4 * info.Height),
                          new Point(1.5 / 6 * info.Width, 2.75 / 4 * info.Height),
                          new Point(3.0 / 6 * info.Width, 1.25 / 4 * info.Height),
                          new Point(3.0 / 6 * info.Width, 2.75 / 4 * info.Height),
                          new Point(4.5 / 6 * info.Width, 1.25 / 4 * info.Height),
                          new Point(4.5 / 6 * info.Width, 2.75 / 4 * info.Height) };
            string indicator = "V";

            canvas.Clear();

            // Draw base station
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.DarkGray,
                StrokeWidth = 4,
                TextSize = info.Height / 5,
                TextAlign = SKTextAlign.Center,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                FakeBoldText = false
            };
            foreach(Point p in s)
            {
                canvas.DrawCircle((float)p.X, (float)p.Y, 20, paint);
            }

            // Prepare to draw marker
            SKPoint[] points;
            switch(this.Device_CurrentTarget)
            {
                case enum_Device.Car1:
                    paint.Color = SKColors.Red;
                    points = this.points_historicalLoc1.ToArray();
                    break;
                default:
                    paint.Color = SKColors.Blue;
                    points = this.points_historicalLoc0.ToArray();
                    break;
            }
            SKRect sizeRect = new SKRect();
            double textWidth = paint.MeasureText(indicator, ref sizeRect);
            for(int a = 0; a < points.Length; a++)
            {
                points[a].X = info.Width * points[a].X;
                points[a].Y = info.Height * points[a].Y;
            }
            canvas.DrawPoints(SKPointMode.Polygon, points, paint);

            SKPoint pp = new SKPoint();
            pp.X = (float)(info.Width * this.point_CarCurrentLoc.X);
            pp.Y = (float)(info.Height * this.point_CarCurrentLoc.Y);
            //pp.X = (float)(pp.X * Math.Cos(this.orientation) + pp.Y * Math.Sin(this.orientation));
            //pp.Y = (float)(-pp.X * Math.Sin(this.orientation) + pp.Y * Math.Cos(this.orientation));
            canvas.RotateDegrees((float)this.orientation, pp.X, pp.Y);
            canvas.DrawText(indicator, pp.X, pp.Y, paint);
            // For test
            //canvas.RotateDegrees((float)this.orientation, (float)(info.Width / 2), (float)(info.Width / 2));
            //canvas.DrawText(indicator, (float)(info.Width / 2), (float)(info.Width / 2 - sizeRect.Height / 2), paint);
            canvas.RotateDegrees((float)(-this.orientation), pp.X, pp.Y);
        }

        private void slider_speed_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.speed = this.slider_speed.Value;
        }

        private void switch_Fire_Toggled(object sender, ToggledEventArgs e)
        {
            this.fireDetect = this.switch_Fire.IsToggled;
        }

        private void switch_Gravity_Toggled(object sender, ToggledEventArgs e)
        {
            if(this.switch_Gravity.IsToggled)           // Gravity On
            {
                Accelerometer.Start(SensorSpeed.UI);
            }
            else
            {
                Accelerometer.Stop();
            }
        }
    }

}
