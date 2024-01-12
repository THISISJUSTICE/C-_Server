using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();

        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        //[UNUSED(1)][TYPE(7)][ID(24)]
        //[00000000] [00000000] [00000000] [00000000]
        int _counter = 0; //TODO

        public T Add<T>() where T :GameObject, new()
        {
            T gameObejct = new T();

            lock (_lock) {
                gameObejct.id = GenerateID(gameObejct.ObjectType);

                if (gameObejct.ObjectType == GameObjectType.Player) {
                    _players.Add(gameObejct.id, gameObejct as Player);
                }
            }

            return gameObejct;
        }

        int GenerateID(GameObjectType type) {
            lock (_lock) {
                return ((int)type << 24) | (_counter++);
            }
        }

        public static GameObjectType GetObjectTypeByID(int id) {
            //타입 id를 오른쪽으로 이동 후 7비트의 마스크
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public bool Remove(int objectID)
        {
            GameObjectType objectType = GetObjectTypeByID(objectID);
            
            lock (_lock)
            {
                if(objectType == GameObjectType.Player)
                    return _players.Remove(objectID);
            }

            return false;
        }

        public Player Find(int objectID)
        {
            GameObjectType objectType = GetObjectTypeByID(objectID);

            lock (_lock)
            {
                if (objectType == GameObjectType.Player) {
                    Player player = null;

                    if (_players.TryGetValue(objectID, out player))
                    {
                        return player;
                    }
                }
            }
            return null;
        }

    }
}
