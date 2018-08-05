using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Newtonsoft.Json.Linq;
using Microsoft.CSharp;
using Microsoft.CSharp.RuntimeBinder;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XController.Droid;
using Xamarin.Forms;


[assembly: Dependency(typeof(HeartBeatListener))]
namespace XController.Droid
{
    class HeartBeatListener : IHeartbeatListener
    {
        private IPEndPoint senderEndPoint;
        private UdpClient udpClient;
        private Handler handler_ToastHandler;

        public IPAddress ipAddress_Car0 { private set; get; }
        public IPAddress ipAddress_Car1 { private set; get; }
        public IPAddress ipAddress_Marker { private set; get; }

        public void StartHeartbeatListener(int localPort)
        {
            var local = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            Toast.MakeText(Android.App.Application.Context, "local IP: " + local.ToString(), ToastLength.Long).Show();
            //IPEndPoint endPoint = new IPEndPoint(local, localPort);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), localPort);
            UdpClient client = new UdpClient(endPoint);
            this.udpClient = client;

            this.handler_ToastHandler = new Handler((Message msg) =>
            {
                Android.Widget.Toast.MakeText(Android.App.Application.Context, msg.Obj.ToString(), ToastLength.Short).Show();
            });
            
            ThreadStart threadStart = new ThreadStart(this.Run);
            Thread thread_HeartbeatListener = new Thread(threadStart);
            thread_HeartbeatListener.Start();
        }

        public void Run()
        {
            byte[] rawRecv;
            string encRecv;

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            this.senderEndPoint = sender;

            while (true)
            {
                rawRecv = udpClient.Receive(ref sender);
                encRecv = Encoding.UTF8.GetString(rawRecv);
                Message m = new Message();

                JObject jo = JObject.Parse(encRecv);
                if(jo["Type"].ToString() == "heartbeat")
                {
                    if(jo["FromRole"].ToString() == "car")
                    {
                        switch((int)jo["FromID"])
                        {
                            case 0:
                                this.ipAddress_Car0 = IPAddress.Parse(jo["FromIP"].ToString());
                                break;
                            case 1:
                                this.ipAddress_Car0 = IPAddress.Parse(jo["FromIP"].ToString());
                                break;
                        }

                    }
                    if(jo["FromRole"].ToString() == "marker")
                    {
                        this.ipAddress_Marker = IPAddress.Parse(jo["FromIP"].ToString());
                    }
                    m.Obj = "Heartbeat Listener:\n" + jo["FromRole"].ToString() + jo["FromID"].ToString() + ", IP: " + jo["FromIP"].ToString();

                    handler_ToastHandler.SendMessage(m);

                }
            }
        }

        public IPAddress GetCar0IPAddress()
        {
            return this.ipAddress_Car0;
        }

        public IPAddress GetCar1IPAddress()
        {
            return this.ipAddress_Car1;
        }

        public IPAddress GetMarkerIPAddress()
        {
            return this.ipAddress_Marker;
        }
    }
}