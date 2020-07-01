using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class GameLogic
    {
        /// <summary>Runs all game logic.</summary>
        public static void Update()
        {
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null) // Si le client existe et si il a lancé une game
                {
                    if (_client.player.isInGame)
                    {
                        _client.player.FightManager.Update();
                    }                    
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
