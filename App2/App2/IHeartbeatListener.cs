using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace XController
{
    interface IHeartbeatListener
    {
        void StartHeartbeatListener(int localPort);
        IPAddress GetCar0IPAddress();
        IPAddress GetCar1IPAddress();
        IPAddress GetMarkerIPAddress();
    }
}
