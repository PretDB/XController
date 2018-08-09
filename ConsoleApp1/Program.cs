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
            Program.MessageWithSubmessage();


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

            dynamic jObject = JObject.Parse(string_RawJsonString) as dynamic;
            Console.WriteLine(jObject);
            IPAddress ip = IPAddress.Parse(jObject.FromIP.ToString());
            Console.WriteLine(ip.ToString());
            Console.WriteLine("Name" + jObject.Msg.Name);
            Console.Read();
        }

        static string MessageWithSubmessage()
        {
            JObject jObject = new JObject
            {
                { "Name", "张三" },
                {"Age", 15 },
                {"Height", 177 },
                {"Other", null }
            };
            IPAddress iPAddress = IPAddress.Parse("192.168.1.1");
            JObject jObject_Message = new JObject
            {
                {"Type" , "instruction"},
                {"FromIP",  iPAddress.ToString()},
                {"FromID", 0 },
                {"FromRole", "Controller" },
                {"Msg", jObject }
            };
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
