﻿using System;
using System.Net;
using ServerCore;
using Server.Game;
using System.Threading;
using Server.Data;
using System.Collections.Generic;
using Server.DB;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

        static void TickRoom(GameRoom room, int tick = 100) {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => { room.Update(); } );
            timer.AutoReset = true;
            timer.Enabled = true;

            _timers.Add(timer);
        }

        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            GameRoom room = RoomManager.Instance.Add(1);
            TickRoom(room, 50);

            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Inst.Generate(); });

            //JobTimer.Inst.Push(FlushRoom);

            while (true)
            {
                //JobTimer.Inst.Flush();
                Thread.Sleep(100);
            }
        }

    }

}
