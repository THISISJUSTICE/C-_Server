﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    class Projectile : GameObject
    {
        public Data.Skill Data { get; set; }
        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public override void Update() { 

        }

    }
}
