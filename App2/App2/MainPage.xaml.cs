﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
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
        Stop = 0,
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
        Ridar,
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
        public bool isDebugMode = false;
        public bool isCalibrating = false;
        public double speed = 0.45;
        public double orientation = 0;
        private System.Numerics.Vector3 vector3_AccFiltered;
        private float accSensitivity = 1;
        public bool fireDetect = false;
        public enum_Command lastCommand = enum_Command.Stop;
        public readonly string string_VideoUri = "/stream_simple.html";
        public readonly string string_controllerUri = "/controller";
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

        private IPAddress[] carIPAddresses = new IPAddress[2];
        private IPAddress[] locatorIPAddresses = new IPAddress[2];
        private IPAddress IPAddress_Local = IPAddress.Any;

        private HttpClient httpClient = new HttpClient();


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
                try
                {
                    this.ConfigureWebVideo(this.carIPAddresses[(int)value - 1]);
                }
                catch (IndexOutOfRangeException)
                {
                    return;
                }
            }
        }
        private enum_Device _currentTarget;

        public Point point_CarCurrentLoc = new Point(0.001, 0.001);


        public MainPage()
        {
            InitializeComponent();
            InitializeNetwork();
            InitializeTarget();
            InitializeGravity();
        }

        private void button_Forward_Pressed(object sender, EventArgs e)
        {
            this.picker_Mode.SelectedIndex = 0;
            this.MessageEmitter(this.MessageAssembler(enum_Command.Forward));
        }

        private void button_Back_Pressed(object sender, EventArgs e)
        {
            this.picker_Mode.SelectedIndex = 0;
            this.MessageEmitter(this.MessageAssembler(enum_Command.Backward));
        }

        private void button_LShift_Pressed(object sender, EventArgs e)
        {
            this.picker_Mode.SelectedIndex = 0;
            this.MessageEmitter(this.MessageAssembler(enum_Command.LeftShift));
        }

        private void button_LRotate_Pressed(object sender, EventArgs e)
        {
            this.picker_Mode.SelectedIndex = 0;
            this.MessageEmitter(this.MessageAssembler(enum_Command.LeftRotate));
        }

        private void button_RShift_Pressed(object sender, EventArgs e)
        {
            this.picker_Mode.SelectedIndex = 0;
            this.MessageEmitter(this.MessageAssembler(enum_Command.RightShift));
        }

        private void button_RRotate_Pressed(object sender, EventArgs e)
        {
            this.picker_Mode.SelectedIndex = 0;
            this.MessageEmitter(this.MessageAssembler(enum_Command.RightRotate));
        }

        private void button_Stop_Clicked(object sender, EventArgs e)
        {
            this.picker_Mode.SelectedIndex = 0;
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

        private void button_SoundDetect_Clicked(object sender, EventArgs e)
        {
            this.Toast("声音检测", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.SoundDetect));
        }

        private void button_Ridar_Clicked(object sender, EventArgs e)
        {
            this.Toast("雷达避障模式", false);
            this.MessageEmitter(this.MessageAssembler(enum_Command.Ridar));
        }

        private void picker_Target_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (picker_Target.SelectedIndex != 0)
            {
                Target result = targets[picker_Target.SelectedIndex - 1];
                this.Device_CurrentTarget = result.device;
                this.label_connected.Text = this.Device_CurrentTarget.ToString();
                this.Toast("已选择控制" + result.ToString(), false);
            }
        }

        private void Accelerometer_Changed(object sender, AccelerometerChangedEventArgs e)
        {
            enum_Command cmd = enum_Command.Stop;
            var data = e.Reading.Acceleration;
            this.vector3_AccFiltered.X = this.accSensitivity * data.X + (1 - this.accSensitivity) * this.vector3_AccFiltered.X;
            this.vector3_AccFiltered.Y = this.accSensitivity * data.Y + (1 - this.accSensitivity) * this.vector3_AccFiltered.Y;
            this.vector3_AccFiltered.Z = this.accSensitivity * data.Z + (1 - this.accSensitivity) * this.vector3_AccFiltered.Z;

            if (Math.Abs(this.vector3_AccFiltered.X) < 0.1f && this.vector3_AccFiltered.Y < 0.2f)
            {
                cmd = enum_Command.Forward;
            }
            else if (Math.Abs(this.vector3_AccFiltered.X) < 0.1f && this.vector3_AccFiltered.Y > 0.61f)
            {
                cmd = enum_Command.Backward;
            }
            else if (this.vector3_AccFiltered.X > 0.3f && this.vector3_AccFiltered.Y > 0.4f)
            {
                cmd = enum_Command.LeftRotate;
            }
            else if (this.vector3_AccFiltered.X < -0.3f && this.vector3_AccFiltered.Y > 0.4f)
            {
                cmd = enum_Command.RightRotate;
            }
            else if (Math.Abs(this.vector3_AccFiltered.Y) < 0.2f && this.vector3_AccFiltered.X > 0.3f)
            {
                cmd = enum_Command.LeftShift;
            }
            else if (Math.Abs(this.vector3_AccFiltered.Y) < 0.2f && this.vector3_AccFiltered.X < -0.3f)
            {
                cmd = enum_Command.RightShift;
            }
            else
            {
                cmd = enum_Command.Stop;
            }
            if (cmd != this.lastCommand)
            {
                switch (cmd)
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
                this.lastCommand = cmd;
                Thread.Sleep(100);
            }
        }

        private void picker_Mode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (picker_Mode.SelectedIndex)
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
                case 6:
                    this.button_SoundDetect_Clicked(sender, e);
                    break;
                case 7:
                    this.button_Ridar_Clicked(sender, e);
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
            this.carIPAddresses[0] = IPAddress.Any;
            this.carIPAddresses[1] = IPAddress.Any;
            this.locatorIPAddresses[0] = IPAddress.Any;
            this.locatorIPAddresses[1] = IPAddress.Any;

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

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                rawRecv = this.udpClient.Receive(ref sender);
                encRecv = Encoding.UTF8.GetString(rawRecv);

                JObject jObject = JObject.Parse(encRecv);
                if (jObject.ContainsKey("Type"))
                {
                    int? id = null;
                    id = (int)jObject["FromID"];
                    if (id is null)
                    {
                        Thread.Sleep(300);
                        continue;
                    }

                    string type = jObject["Type"].ToString();
                    switch (type)
                    {
                        case "heartbeat":
                            try
                            {
                                if (!this.carIPAddresses[id ?? 99].Equals(sender.Address))
                                {
                                    this.Toast($"AGV{id}已发现，IP：{sender.Address}", false, true);
                                    this.carIPAddresses[id ?? 99] = sender.Address;
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                Thread.Sleep(300);
                                continue;
                            }
                            break;
                        case "locate":
                            if (!this.locatorIPAddresses[id ?? 99].Equals(sender.Address))
                            {
                                this.locatorIPAddresses[id ?? 99] = sender.Address;
                            }
                            if (jObject["Msg"].HasValues)
                            {
                                JObject msg = jObject["Msg"] as JObject;
                                if (msg.ContainsKey("position"))
                                {
                                    try
                                    {
                                        if (this.Device_CurrentTarget == (enum_Device)((id + 1) ?? 3))
                                        {
                                            Point gotP = new Point(double.Parse(msg["position"]["X"].ToString()), double.Parse(msg["position"]["Y"].ToString()));
                                            this.point_CarCurrentLoc = gotP;
                                        }
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        Thread.Sleep(100);
                                        continue;
                                    }
                                }
                                if (msg.ContainsKey("orientation"))
                                {
                                    try
                                    {
                                        if(this.Device_CurrentTarget == (enum_Device)((id + 1) ?? 3))
                                        {
                                            this.orientation = double.Parse(msg["orientation"].ToString());
                                        }
                                    }
                                    catch(IndexOutOfRangeException)
                                    {
                                        Thread.Sleep(100);
                                        continue;
                                    }
                                }
                            }
                            break;
                        default:
                            Thread.Sleep(300);
                            continue;
                    }
                }

                this.skCanvas.InvalidateSurface();
            }
        }

        private void InitializeTarget()
        {
            this.targets = Data.targets;
            picker_Target.Items.Add("设备");
            foreach (var target in this.targets)
            {
                picker_Target.Items.Add(target.ToString());
            }
            picker_Target.SelectedIndex = 0;

            picker_Mode.Items.Add("遥控模式");
            picker_Mode.Items.Add("红外避障模式");
            picker_Mode.Items.Add("超声避障模式");
            picker_Mode.Items.Add("寻迹模式");
            picker_Mode.Items.Add("光检测模式");
            picker_Mode.Items.Add("人体活动检测模式");
            picker_Mode.Items.Add("声音检测模式");
            picker_Mode.Items.Add("激光雷达");
            picker_Mode.SelectedIndex = 0;
        }

        private void InitializeGravity()
        {
            Accelerometer.ReadingChanged += this.Accelerometer_Changed;
        }

        private void ConfigureWebVideo(IPAddress targetIP)
        {
            if (targetIP != IPAddress.Any)
            {
                string url0 = "http://" + targetIP.ToString() + ":8080" + this.string_VideoUri;

                this.webView_Monitor.Source = new UrlWebViewSource
                {
                    Url = url0
                };

            }
            else
            {
                var source = new HtmlWebViewSource();
                source.Html = this.string_NoDevice;
                this.webView_Monitor.Source = source;
            }
        }

        private JObject MessageAssembler(enum_Command command, JObject args = null)
        {
            double speed = this.speed;
            JObject arg = new JObject
            {
                { "Speed", speed },
                { "Fire", this.fireDetect },
                { "Debug", this.isDebugMode }
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

            string uri = "http://";
            string ip = "";
            switch (this.Device_CurrentTarget)
            {
                case enum_Device.Car0:
                    ip = this.carIPAddresses[0].ToString();
                    break;

                case enum_Device.Car1:
                    ip = this.carIPAddresses[1].ToString();
                    break;
                case enum_Device.None:
                    return;

                default:
                    return;
            }

            try
            {
                uri = "http://" + ip + ":" + this.Int_TCPPort.ToString() + this.string_controllerUri;
                string requestData = message.ToString();
                this.httpClient.PostAsync(uri, new StringContent(requestData, Encoding.UTF8, "application/json"));
            }
            catch (System.Exception e)
            {
                this.Toast("网络错误, 未能发送指令，请重试\n" + e.Message, false);
            }

        }

        private void SKCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            Point[] s = { new Point(2.0 / 6 * info.Width, 1.25 / 4 * info.Height),
                          new Point(2.0 / 6 * info.Width, 2.75 / 4 * info.Height),
                          new Point(3.0 / 6 * info.Width, 2.75 / 4 * info.Height),
                          new Point(3.0 / 6 * info.Width, 1.25 / 4 * info.Height),
                          new Point(4.0 / 6 * info.Width, 1.25 / 4 * info.Height),
                          new Point(4.0 / 6 * info.Width, 2.75 / 4 * info.Height) };
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
            foreach (Point p in s)
            {
                canvas.DrawCircle((float)p.X, (float)p.Y, 20, paint);
            }

            // Prepare to draw marker
            switch (this.Device_CurrentTarget)
            {
                case enum_Device.Car1:
                    paint.Color = SKColors.HotPink;
                    break;
                case enum_Device.Car0:
                    paint.Color = SKColors.LightBlue;
                    break;
                default:
                    paint.Color = SKColors.Green;
                    break;
            }
            SKRect sizeRect = new SKRect();
            double textWidth = paint.MeasureText(indicator, ref sizeRect);
            switch (this.Device_CurrentTarget)
            {
                case enum_Device.Car1:
                    paint.Color = SKColors.Red;
                    break;
                default:
                    paint.Color = SKColors.Blue;
                    break;
            }

            SKPoint pp = new SKPoint();
            pp.X = (float)(info.Width * this.point_CarCurrentLoc.X / 6.0f);
            pp.Y = (float)(info.Height * this.point_CarCurrentLoc.Y / 4.0f);
            canvas.RotateDegrees((float)this.orientation, pp.X, pp.Y);
            canvas.DrawText(indicator, pp.X, pp.Y, paint);
            canvas.RotateDegrees((float)(-this.orientation), pp.X, pp.Y);
            this.label_XLoc.Text = (this.point_CarCurrentLoc.X ).ToString("F2");
            this.label_YLoc.Text = (this.point_CarCurrentLoc.Y).ToString("F2");
            this.label_Angl.Text = this.orientation.ToString("F2");
        }

        private void slider_speed_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.speed = this.slider_speed.Value * 0.75;
            this.MessageEmitter(this.MessageAssembler(enum_Command.None));
        }

        private void slider_sensitivity_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.accSensitivity = (float)(this.slider_sensitivity.Value * 0.3);
        }

        private void switch_Fire_Toggled(object sender, ToggledEventArgs e)
        {
            this.fireDetect = this.switch_Fire.IsToggled;
            this.MessageEmitter(this.MessageAssembler(enum_Command.None));
            this.Toast("灭火功能：" + (this.fireDetect ? "开" : "关"), false);
        }

        private void switch_Gravity_Toggled(object sender, ToggledEventArgs e)
        {
            if (this.switch_Gravity.IsToggled)           // Gravity On
            {
                Accelerometer.Start(SensorSpeed.UI);
            }
            else
            {
                Accelerometer.Stop();
            }
        }

        private async void Switch_DebugMode_Toggled(object sender, ToggledEventArgs e)
        {
            this.isDebugMode = switch_DebugMode.IsToggled;
            if (this.switch_DebugMode.IsToggled)
            {
                bool action = await DisplayAlert("调试模式", "点击“继续”将启动调试界面，届时将关闭定位功能，在原图传界面显示定位灯的调试信息。" +
                    "\r\n此模式和“显示地图”是冲突的。", "继续", "取消");
                if (!action)
                {
                    return;
                }
                this.switch_ShowMap.IsToggled = false;
            }
            try
            {
                int device = 99;
                device = (int)this.Device_CurrentTarget - 1;
                this.ConfigureWebVideo(this.isDebugMode ? this.locatorIPAddresses[device]
                                                        : this.carIPAddresses[device]);
                byte[] bytes;
                if (this.isDebugMode)
                {
                    bytes = Encoding.ASCII.GetBytes("ON");
                }
                else
                {
                    bytes = Encoding.ASCII.GetBytes("OFF");
                }
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Broadcast, 6688);
                for (int i = 0; i < 25; i++)
                {
                    this.udpClient.Send(bytes, bytes.Length, iPEndPoint);
                    Thread.Sleep(10);
                }
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
        }

        private async void Button_compassCalibrate_Clicked(object sender, EventArgs e)
        {
            bool action = await DisplayAlert("罗盘校准步骤", "点击“继续”将执行罗盘校准，届时请使AGV原地旋转。校准完成后会有提示。" +
                "\r\n注意：校准期间请务必使AGV原地旋转，并保持远离金属物，否则将会影响校准效果，进而影响其他功能。", "继续", "取消");
            if (action)
            {
                action = await DisplayAlert("开始校准罗盘", "现在已经进入罗盘校准模式，请遥控AGV使其原地旋转。", "继续", "取消");
                if (!action)
                {
                    return;
                }

                string ip;
                switch (this.Device_CurrentTarget)
                {
                    case enum_Device.Car0:
                        ip = this.locatorIPAddresses[0].ToString();
                        break;

                    case enum_Device.Car1:
                        ip = this.locatorIPAddresses[1].ToString();
                        break;
                    case enum_Device.None:
                        return;

                    default:
                        return;
                }
                string uri = "http://" + ip + ":6688/calib/";
                try
                {
                    var res = await this.httpClient.GetAsync(uri);
                    if (await res.Content.ReadAsStringAsync() == "DONE")
                    {
                        await DisplayAlert("罗盘校准", "罗盘校准结束，请重启该AGV", "完成");
                        return;
                    }

                    else
                    {
                        await DisplayAlert("错误", res.Content.ToString(), "OK");
                        return;
                    }
                }
                catch(HttpRequestException)
                {
                    await DisplayAlert("错误", "出现了网络错误", "取消");
                }
            }
        }

        private void Switch_ShowMap_Toggled(object sender, ToggledEventArgs e)
        {
            if (this.switch_ShowMap.IsToggled)
            {
                this.switch_DebugMode.IsToggled = false;
            }
        }
    }

}
