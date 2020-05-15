using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            //string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            //Server.clients[_fromClient].SendIntoGame();
        }

        public static void DesiredPlayerScene(int _fromClient, Packet _packet)
        {
            // Getting the parameter from Client Packet
            int _clientIdCheck = _packet.ReadInt();
            string _desiredscene = _packet.ReadString();


            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} want to switch to {_desiredscene}.");
            Server.clients[_fromClient].SwitchScene(_desiredscene);
        }

        public async static void SignUpClientRequest(int _fromClient, Packet _packet)
        {
            // Getting the parameter from Client Packet
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            string _password = _packet.ReadString();
            string _email = _packet.ReadString();
            Server.clients[_fromClient].SignUptoCognito(_username, _password, _email, Constants.CLIENTAPP_ID);
            //string _signUpReturn = await SignUpClients.SignUpClientToCognito(_username, _password, _email, Constants.CLIENTAPP_ID);
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} want to switch to {_desiredscene}.");
            /*SignUpClients Sign = new SignUpClients();
            Sign.*/
            //Server.clients[_fromClient].SwitchScene(_desiredscene);
        }

        public async static void SignIpClientRequest(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            string _password = _packet.ReadString();

            Server.clients[_fromClient].SignInToCognito(_username, _password);
        }

        public async static void AccessHomePageClientRequest(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _clientToken = _packet.ReadString();

            Server.clients[_fromClient].AccessHomepage(_clientToken);
        }


        /*public static void PlayerMovement(int _fromClient, Packet _packet)
        {
            bool[] _inputs = new bool[_packet.ReadInt()];
            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = _packet.ReadBool();
            }
            Quaternion _rotation = _packet.ReadQuaternion();

            Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
        }

        public static void PlayerShoot(int _fromClient, Packet _packet)
        {
            Vector3 _playerShoot = _packet.ReadVector3();

            //Server.clients[_fromClient].WantToShoot(_playerShoot);
        }*/
    }
}
