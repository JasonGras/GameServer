using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using System.Threading.Tasks;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public CognitoUser myUser;
        public UserSession currentUserSession;
        public Player player;
        public TCP tcp;
        public UDP udp;

        public Client(int _clientId)
        {
            id = _clientId;
            //accessToken = null;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            /// <summary>Initializes the newly connected client's TCP-related info.</summary>
            /// <param name="_socket">The TcpClient instance of the newly connected client.</param>
            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            /// <summary>Sends data to the client via TCP.</summary>
            /// <param name="_packet">The packet to send.</param>
            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null); // Send data to appropriate client
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
                }
            }

            /// <summary>Reads incoming data from the stream.</summary>
            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data)); // Reset receivedData if all data was handled
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}");
                    Server.clients[id].Disconnect();
                }
            }

            /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
            /// <param name="_data">The recieved data.</param>
            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    // If client's received data contains a packet
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                        }
                    });

                    _packetLength = 0; // Reset packet length
                    if (receivedData.UnreadLength() >= 4)
                    {
                        // If client's received data contains another packet
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            // If packet contains no data
                            return true; // Reset receivedData instance to allow it to be reused
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true; // Reset receivedData instance to allow it to be reused
                }

                return false;
            }

            /// <summary>Closes and cleans up the TCP connection.</summary>
            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            /// <summary>Initializes the newly connected client's UDP-related info.</summary>
            /// <param name="_endPoint">The IPEndPoint instance of the newly connected client.</param>
            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            /// <summary>Sends data to the client via UDP.</summary>
            /// <param name="_packet">The packet to send.</param>
            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
            /// <param name="_packetData">The packet containing the recieved data.</param>
            public void HandleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                    }
                });
            }

            /// <summary>Cleans up the UDP connection.</summary>
            public void Disconnect()
            {
                endPoint = null;
            }
        }

        /// <summary>Sends the client into the game and informs other clients of the new player.</summary>
        /// <param name="_playerName">The username of the new player.</param>
        public void SendIntoGame()
        {
            if (myUser.SessionTokens.IsValid())
            {
                player = new Player(id);                

                ServerSend.SpawnPlayer(id, player.currentScene, player.oldScene); // player param is useless
            }
            


            // Send all players to the new player
            /* foreach (Client _client in Server.clients.Values)
             {
                 if (_client.player != null)
                 {
                     if (_client.id != id)
                     {
                         ServerSend.SpawnPlayer(id, _client.player, player.currentScene);
                     }
                 }
             }*/

            // Send the new player to all players (including himself)
            /*foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(_client.id, player, player.currentScene);
                }
            }*/
        }

        public void SwitchScene(string _desiredScene)
        {
            
            string _oldScene = Server.clients[id].player.currentScene;

            //L'objectif de ce IF est de s'assurer que le client soit sur NOSCENE et qu'il ne peut accèder qu'aux pages désignes
            // Homepage && Collection

            /*if(_desiredScene == Server.clients[id].player.currentScene)
            {
                ActualiseScene(_desiredScene, Constants.SCENE_SAMESCENE);
                Console.WriteLine("SAMESCENE | Current Scene : " + Server.clients[id].player.currentScene + " | Old Scene : " + Server.clients[id].player.oldScene);
            }
            else
            {
                ActualiseScene(_desiredScene, Server.clients[id].player.currentScene);
                Console.WriteLine("NO SAMESCENE |Current Scene : " + Server.clients[id].player.currentScene + " | Old Scene : " + Server.clients[id].player.oldScene);
            }*/

            /* if (Server.clients[id].player.currentScene == Constants.SCENE_NOSCENE) // Sur la HomePage
             {               
                 if (_desiredScene == Constants.SCENE_COLLECTION) // is Collection Scene
                 {
                     ActualiseScene(_desiredScene, Constants.SCENE_NOSCENE);
                 }
                 if (_desiredScene == Constants.SCENE_HOMEPAGE) // is Home Scene
                 {
                     ActualiseScene(_desiredScene, Constants.SCENE_NOSCENE);
                 }
             } */
            if (_desiredScene == Server.clients[id].player.currentScene || Constants.SCENE_NOSCENE == Server.clients[id].player.currentScene)
            {
                ActualiseScene(_desiredScene, Constants.SCENE_SAMESCENE);
                Console.WriteLine("SAMESCENE | Current Scene : " + Server.clients[id].player.currentScene + " | Old Scene : " + Server.clients[id].player.oldScene);
            }
            // Si le client est sur la homePage 
            if (Server.clients[id].player.currentScene == Constants.SCENE_HOMEPAGE)
            {
                //Si la scene désirée est bien la Collection
                if(_desiredScene == Constants.SCENE_COLLECTION)
                {
                    ActualiseScene(_desiredScene, _oldScene);
                    Console.WriteLine("Home => Coll | Current Scene : " + Server.clients[id].player.currentScene + " | Old Scene : " + Server.clients[id].player.oldScene);
                }                
            }
            if (Server.clients[id].player.currentScene == Constants.SCENE_COLLECTION)
            {
                //Si la scene désirée est bien la Collection
                if (_desiredScene == Constants.SCENE_HOMEPAGE)
                {
                    ActualiseScene(_desiredScene, _oldScene);
                    Console.WriteLine("Coll => Home | Current Scene : " + Server.clients[id].player.currentScene + " | Old Scene : " + Server.clients[id].player.oldScene);
                }
            }
        }

        private void ActualiseScene(string _desiredScene, string _oldScene)
        {
            // Alors sur le serveur on Assigne sa Current Scene a SCENE_HOMEPAGE
            Server.clients[id].player.currentScene = _desiredScene;

            // j'actualise ma OldScene
            Server.clients[id].player.oldScene = _oldScene;

            // Et on confirme au client qu'il peut Switch de Scene
            ServerSend.SwitchToScene(id, _desiredScene, _oldScene);
        }

        public async void SignUptoCognito(string _username, string _password, string _email, string _clientAppId)
        {
            // If the REgEx Formats are Respected, we proceed to Adhesion OR We Return an Error Format
            if (SecurityCheck.CheckUserPattern(_username))
            {
                if (SecurityCheck.CheckPasswordPattern(_password))
                {
                    if (SecurityCheck.CheckEmailPattern(_email))
                    {
                        await SignUpClients.SignUpClientToCognito(id, _username, _password, _email, _clientAppId);
                    }
                    else
                    {
                        ServerSend.SignUpStatusReturn(id, Constants.ADHESION_FORMAT_EMAIL_KO);
                    }
                }
                else
                {
                    ServerSend.SignUpStatusReturn(id, Constants.ADHESION_FORMAT_PASSWORD_KO);
                }
            }
            else
            {
                ServerSend.SignUpStatusReturn(id, Constants.ADHESION_FORMAT_USERNAME_KO);
            }


            //Console.WriteLine("SignUpToCognito Return :"+_signUpReturn);

        }
        public async void SignInToCognito(string _username, string _password)
        {
            
            //Console.WriteLine("SignUpToCognito Return :"+_signUpReturn);
            if (SecurityCheck.CheckUserPattern(_username))
            {
                if (SecurityCheck.CheckPasswordPattern(_password))
                {
                    // This will SET a UserCognito with Valid Tokens on my Client.
                    // And send the tokens to the Client who has Sign In
                    await SignInClients.SignInClientToCognito(_username, _password, id);
                    SendIntoGame();


                }
                else
                {
                    ServerSend.AuthenticationStatus(id,Constants.AUTHENTIFICATION_FORMAT_PASSWORD_KO);
                }
            }
            else
            {
                ServerSend.AuthenticationStatus(id,Constants.AUTHENTIFICATION_FORMAT_USERNAME_KO);
            }

        }

        public async void AccessHomepage(string _clientToken)
        {
            // Check Token is still valid
            // Current Scene = Homepage
            // Ask Client to Load Homepage
            // Et on confirme au client qu'il peut Switch de Scene
            ServerSend.SwitchToScene(id, Constants.SCENE_HOMEPAGE, Constants.SCENE_NOSCENE);
        }

        public async Task GetNewValidTokensAsync()
        {

            //var CompareTokens = Server.clients[id].myUser.SessionTokens.IdToken;
            Server.clients[id].myUser.SessionTokens = new CognitoUserSession(null, null, Server.clients[id].myUser.SessionTokens.RefreshToken, DateTime.Now, DateTime.Now.AddHours(1));

            InitiateRefreshTokenAuthRequest refreshRequest = new InitiateRefreshTokenAuthRequest()
            {
                AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
            };

            AuthFlowResponse authResponse = await Server.clients[id].myUser.StartWithRefreshTokenAuthAsync(refreshRequest).ConfigureAwait(false);
            UserSession refreshedUserSession = new UserSession(Server.clients[id].myUser.SessionTokens.AccessToken, Server.clients[id].myUser.SessionTokens.IdToken, Server.clients[id].myUser.SessionTokens.RefreshToken);
            ServerSend.SendTokens(id, refreshedUserSession);
            //authResponse.AuthenticationResult.
            //CompareTokens += " | " + Server.clients[id].myUser.SessionTokens.IdToken;
            //Console.WriteLine(CompareTokens);
        }

        /// <summary>Disconnects the client and stops all network traffic.</summary>
        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
