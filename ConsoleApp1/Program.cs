using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    class Program
    {
        enum Dev
        {
            NetCard,
            MotherBoard,
            Mem,
            CPU,
            Disk
        }
        static void Main(string[] args)
        {
            Program.ParseStringToJsonObject();


            Console.Read();
            
        }

        static void TrySocket()
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(new IPEndPoint(IPAddress.Parse("10.10.10.18"), 6688));
                if (client.Connected == false)
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse("10.10.10.18"), 6688));
                }
                NetworkStream stream = client.GetStream();
                byte[] buff = new byte[1024];
                buff = Encoding.UTF8.GetBytes("hello");
                stream.Write(buff, 0, buff.Length);
                stream.Flush();
                stream.Close();
                client.Close();
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("emmmm");
            }
        }

        static void EnumString()
        {
            Console.WriteLine(Dev.CPU.ToString());
        }

        static void Exc()
        {
            while(true)
            {
                try
                {
                    string b = null;
                    Console.WriteLine(b);
                }
                catch(NullReferenceException e)
                {
                    Console.WriteLine("emmm" + e.ToString());
                    Thread.Sleep(1000);
                }
                finally
                {
                    Console.WriteLine("finally");
                    Thread.Sleep(1000);
                }
            }
        }

        static void SyncTest()
        {
            Program.SyncFunc();
            Console.WriteLine("out");
        }

        static async void SyncFunc()
        {
            Console.WriteLine(await Program.GetHere());
        }

        static Task<string> GetHere()
        {
            return Task.Run(() =>
            {
                Thread.Sleep(1000);
                return "Here";
            });
        }

        static void SystemJson()
        {
            string string_RawJsonString = "{" +
                "\"Type\"" + " : " + "\"heartbeat\"," +
                "\"FromIP\" : " + "\"10.10.10.18\"," +
                "\"Msg\" : \"None\"" +
                "}";
            JsonObject jv = JsonObject.Parse(string_RawJsonString) as JsonObject;
            Console.WriteLine(jv["Type"].ToString());
            Console.Read();
        }

        static void ParseStringToJsonObject()
        {
            string string_RawJsonString = Program.MessageWithSubmessage();

            JObject jObject = JObject.Parse(string_RawJsonString) as dynamic;
            Console.WriteLine("Original json content: ");
            Console.WriteLine(jObject);

            Console.WriteLine("original has msg? " + jObject["Msg"].HasValues);
            if(jObject["Msg"].HasValues)
            {
                Console.WriteLine("original Msg: " + jObject["Msg"]);
                Console.WriteLine("Msg type: " + jObject["Msg"].GetType());

                JObject msg = jObject["Msg"] as JObject;
                Console.WriteLine("Msg: ");
                Console.WriteLine(msg);
                Console.WriteLine("msg has positin? " + msg.ContainsKey("position"));
                Console.WriteLine("Msg has position? " + msg["position"].HasValues);
                if(msg["position"].HasValues)
                {
                    JObject pos = msg["position"] as JObject;
                    Console.WriteLine("tag: " + pos["tag"] + "\tX: " + (double)pos["X"] + "\tY: " + (double)pos["Y"]);
                }
            }
            else
            {
                JObject msg = null;
                Console.WriteLine("msg is null:"  + (msg == null));
            }

            Console.Read();
        }

        static string MessageWithSubmessage()
        {
            JObject loc = new JObject
            {
                {"tag", "0" },
                {"X", 1.908 },
                {"Y", 3.44 }
            };
            JObject msg = new JObject
            {
                {"position", loc }
            };
            IPAddress iPAddress = IPAddress.Parse("192.168.1.1");
            JObject jObject_Message = new JObject
            {
                {"Type" , "heartbeat"},
                {"FromIP",  iPAddress.ToString()},
                {"FromID", 0 },
                {"FromRole", "Car" },
                {"Msg", msg }
            };
            Console.WriteLine("String:");
            Console.WriteLine(jObject_Message.ToString());
            return jObject_Message.ToString();
        }

        static void CreateJsonObject()
        {
            Newtonsoft.Json.Linq.JObject jObject_JsonObject = new Newtonsoft.Json.Linq.JObject
            {
                { "Enterd", DateTime.Now }
            };
            dynamic album = jObject_JsonObject;

            album.AlbumName = "Dirty Deeds Don Dirt Cheap";
            album.Artist = "AC/DC/DVD";
            album.YearReleased = DateTime.Now.Year;
            album.Songs = new Newtonsoft.Json.Linq.JArray() as dynamic;

            dynamic song = new Newtonsoft.Json.Linq.JObject();
            song.SongName = "Dirty Deeds Done Dirt Cheap";
            song.SongLength = new TimeSpan(0, 4, 5);
            new DateTime();
            album.Songs.Add(song);

            song = new Newtonsoft.Json.Linq.JObject();
            song.SongName = "Love at First Fell";
            song.SongLength = new TimeSpan(0, 3, 1);
            album.Songs.Add(song);

            song = new Newtonsoft.Json.Linq.JObject();
            song.SongName = "小苹果";
            song.songLength = "03:32";
            album.Songs.Add(song);

            Console.WriteLine(album.ToString());

        }

        static void IPTest()
        {
            IPAddress localIP = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint end = new IPEndPoint(localIP, 9999);

            UdpClient client = new UdpClient(end);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                Console.WriteLine("waiting for package");
                byte[] rawData = client.Receive(ref sender);
                Console.Write("sender: " + sender.ToString());
                Console.WriteLine("data: " + Encoding.UTF8.GetString(rawData));
                Thread.Sleep(1000);
            }
        }
    }
}
