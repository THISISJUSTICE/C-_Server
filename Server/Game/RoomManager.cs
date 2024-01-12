using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomID = 1;

        public GameRoom Add(int mapID) {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Init(mapID);

            lock (_lock) {
                gameRoom.RoomID = _roomID;
                _rooms.Add(_roomID, gameRoom);
                _roomID++;
            }

            return gameRoom;
        }

        public bool Remove(int roomID) {
            lock (_lock) {
                return _rooms.Remove(roomID);
            }
        }

        public GameRoom Find(int roomID) {
            lock (_lock) {
                GameRoom room = null;

                if (_rooms.TryGetValue(roomID, out room)) {
                    return room;
                }
                return null;
            }
        }

    }
}
