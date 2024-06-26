﻿using System;
using System.Net;
using ServerCore;
using Server.Game;
using System.Threading;
using Server.Data;
using System.Collections.Generic;
using Server.DB;
using System.Linq;

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

            // Test Code
            using (AppDbContext db = new AppDbContext()) {
                PlayerDb player = db.Players.FirstOrDefault();
                if (player != null) {
                    db.Items.Add(new ItemDb(){
                        TemplateId = 1,
                        Count = 1,
                        Slot = 0,
                        Owner = player
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 100,
                        Count = 1,
                        Slot = 1,
                        Owner = player
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 101,
                        Count = 1,
                        Slot = 2,
                        Owner = player
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 200,
                        Count = 10,
                        Slot = 5,
                        Owner = player
                    });

                    db.SaveChanges();
                }
            }

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
                DBTransaction.Instance.Flush();
                
            }
        }

    }

}
