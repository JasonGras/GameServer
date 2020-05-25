using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        // Toutes les méthodes pour définir TOUT les paquets qu'on souhiate envoyer au client.

        /// <summary>Sends a packet to a client via TCP.</summary>
        /// <param name="_toClient">The client to send the packet the packet to.</param>
        /// <param name="_packet">The packet to send to the client.</param>
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        /// <summary>Sends a packet to a client via UDP.</summary>
        /// <param name="_toClient">The client to send the packet the packet to.</param>
        /// <param name="_packet">The packet to send to the client.</param>
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        /// <summary>Sends a packet to all clients via TCP.</summary>
        /// <param name="_packet">The packet to send.</param>
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        /// <summary>Sends a packet to all clients except one via TCP.</summary>
        /// <param name="_exceptClient">The client to NOT send the data to.</param>
        /// <param name="_packet">The packet to send.</param>
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        /// <summary>Sends a packet to all clients via UDP.</summary>
        /// <param name="_packet">The packet to send.</param>
        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
        /// <summary>Sends a packet to all clients except one via UDP.</summary>
        /// <param name="_exceptClient">The client to NOT send the data to.</param>
        /// <param name="_packet">The packet to send.</param>
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #region Packets
        /// <summary>Sends a welcome message to the given client.</summary>
        /// <param name="_toClient">The client to send the packet to.</param>
        /// <param name="_msg">The message to send.</param>
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome)) // Définition du Nouveau Packet
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        /// <summary>Tells a client to spawn a player.</summary>
        /// <param name="_toClient">The client that should spawn the player.</param>
        /// <param name="_player">The player to spawn.</param>
        public static void SpawnPlayer(int _toClient,Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_toClient);
                _packet.Write(_player.username);
                _packet.Write(_player.level);
                _packet.Write(_player.level_xp);
                _packet.Write(_player.required_levelup_xp);
                _packet.Write(_player.currentScene);
                _packet.Write(_player.oldScene);
                //_packet.Write(_player.rotation);

                SendTCPData(_toClient, _packet);
            }
        }

        /// <summary>Tells a client to spawn a player.</summary>
        /// <param name="_newScene">The validated Scene to switch to</param>
        public static void SwitchToScene(int _toClient, string _newScene, string _oldScene)
        {
            using (Packet _packet = new Packet((int)ServerPackets.switchToScene))
            {                
                _packet.Write(_newScene);
                _packet.Write(_oldScene);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        /// <summary>After Authentication of a player if Challenge is NEW password REquired : Ask client to change his password</summary>
        public static void ClientNewPasswordRequired(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.redefinePassword)) // Définition du Nouveau Packet
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        /// <summary>Tells a client if he Sign Up Successfully</summary>
        /// <param name="_newScene">The validated Scene to switch to</param>
        public static void SignUpStatusReturn(int _toClient, string _returnStatus)
        {
            using (Packet _packet = new Packet((int)ServerPackets.signUpStatus))
            {
                _packet.Write(_returnStatus);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void AuthenticationStatus(int _toClient,string _returnStatus)
        {
            using (Packet _packet = new Packet((int)ServerPackets.signInStatus))
            {
                _packet.Write(_returnStatus);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }
        public static void SendTokens(int _toClient, UserSession _clientTokens)
        {
            using (Packet _packet = new Packet((int)ServerPackets.signInToken))
            {
                _packet.Write(_clientTokens);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }
        /// <summary>Sends a player's updated position to all clients.</summary>
        /// <param name="_player">The player whose position to update.</param>
        /*public static void PlayerPosition(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);

                SendUDPDataToAll(_packet);
            }
        }

        /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
        /// <param name="_player">The player whose rotation to update.</param>
        public static void PlayerRotation(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);

                SendUDPDataToAll(_player.id, _packet);
            }
        }*/
        #endregion
    }
}
