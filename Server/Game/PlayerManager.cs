using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class PlayerManager
    {
        public static PlayerManager Instance { get; } = new PlayerManager();

        object _lock = new object();

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        int _playerID = 1; //TODO

        public Player Add()
        {
            Player player = new Player();

            lock (_lock)
            {
                player.Info.PlayerID = _playerID;
                _players.Add(_playerID, player);
                _playerID++;
            }

            return player;
        }

        public bool Remove(int playerID)
        {
            lock (_lock)
            {
                return _players.Remove(playerID);
            }
        }

        public Player Find(int playerID)
        {
            lock (_lock)
            {
                Player player = null;

                if (_players.TryGetValue(playerID, out player))
                {
                    return player;
                }
                return null;
            }
        }

    }
}
