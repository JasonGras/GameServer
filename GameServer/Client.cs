using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;

using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;

using GameServer.Scenes;
using NLog;
using NLog.Common;

using GameServer.Dungeon;

using System.ComponentModel.DataAnnotations;
using Amazon.CognitoIdentityProvider.Model;
using GameServer.Loots;
using System.Diagnostics;

namespace GameServer
{

    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public CognitoUser myUser;
        public UserSession currentUserSession;
        public Player player;
        //public bool CoinOpeningAvailable = true;
        public Task RemoveCoinTask = null;

        public TCP tcp;
        public UDP udp;

        public Dictionary<NeokyCollection, Dictionary<string, int>> PlayerCollection; // Detailed Unit Collection + Souls + Level .. (Characteristics)
        public Dictionary<string, int> UnitsDetails;
      

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
        public void GetRandomLoot(int _coin, int _coinQuality, UserSession _currentUserSession)
        {
            CoinLoots coin = new CoinLoots();            

            OnSessionExpiredRenewTokens(_currentUserSession);

            if (myUser.SessionTokens.IsValid())
            {
                try
                {
                    //Try Update the Collection Data
                    var dSTCollectionBySub = Server.dynamoDBServer.ScanForNeokyClientsUsingUsername(player.username);

                    switch (_coin)
                    {
                        case 0: // CoinLoots.Coin.Viking
                            dSTCollectionBySub.Result.coin.TryGetValue(Constants.DB_COINTYPE_01, out int _currentVikingCoins);
                            if (_currentVikingCoins > 0)
                            {
                                Console.WriteLine("Open " + Constants.DB_COINTYPE_01 + " Nb Coin : " + _currentVikingCoins);
                                var NeokyUnit = coin.GetRandomLoot((CoinLoots.CoinAverageQuality)_coinQuality);
                                AddLootToPlayer(NeokyUnit);
                                RemovePlayerCoin(_coin);
                                Console.WriteLine("Open Done" + Constants.DB_COINTYPE_01);
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetRandomLoot", "Client Username : " + myUser.Username + " | Dont have enough Coins for claimed " + Constants.DB_COINTYPE_01 + " CoinOpening | CoinOpeiningAvailable : " + RemoveCoinTask), NlogClass.exceptions.Add));
                            }
                            break;
                        case 1: //CoinLoots.Coin.Massai
                            dSTCollectionBySub.Result.coin.TryGetValue(Constants.DB_COINTYPE_02, out int _currentMassaiCoins);
                            if (_currentMassaiCoins > 0)
                            {
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetRandomLoot", "Client Username : " + myUser.Username + " | Dont have enough Coins for claimed " + Constants.DB_COINTYPE_02 + " CoinOpening"), NlogClass.exceptions.Add));
                            }
                            break;
                        default:
                            NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetRandomLoot", "Client Username : " + myUser.Username + " | Unknown type of Coin"), NlogClass.exceptions.Add));
                            break;
                    }
                }
                catch (Exception)
                {
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "GetRandomLoot", "Database scan to Check Player have Coins [ Player claimed name : " + player.username + "]"), NlogClass.exceptions.Add));
                }
            }
            else
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetRandomLoot", "Client Username : " + myUser.Username + " | myUser.SessionTokens is still not Valid"), NlogClass.exceptions.Add));
            }
        }

        // Not Async Coz it causes problem Opening several Coins instead of 1
        private void RemovePlayerCoin(int _coin)
        {
            switch (_coin)
            {
                case 0:  // CoinLoots.Coin.Viking

                    try
                    {
                        //Try Update the Collection Data
                        var dSTCollectionByUsername = Server.dynamoDBServer.ScanForNeokyClientsUsingUsername(player.username);
                        dSTCollectionByUsername.Result.coin[Constants.DB_COINTYPE_01]--;
                        //Console.WriteLine("Remove Coin to player !");
                        //RemoveCoinTask = Task.Run(async () => await Server.dynamoDBServer.SaveOrUpdatNeokyClients(dSTCollectionByUsername.Result));
                        //RemoveCoinTask.Status
                        Server.dynamoDBServer.SaveOrUpdatNeokyClients(dSTCollectionByUsername.Result).RunSynchronously();     
                    }
                    catch (Exception)
                    {
                        NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "RemovePlayerCoin", "Database remove coin to player Failed [ Player claimed name : " + player.username + "] | Coin Used : "+ _coin), NlogClass.exceptions.Add));
                    }     
                    break;
                default:
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "RemovePlayerCoin", "Client Username : " + myUser.Username + " | Unknown type of Coin to remove to player"), NlogClass.exceptions.Add));
                    break;
            }
        }
        
        private async void  AddLootToPlayer(NeokyCollection NeokyUnit)
        {
            try
            {
                //Try Update the Collection Data
                var dSTCollectionBySub = Server.dynamoDBServer.ScanForPlayerCollectionUsingSub(player.client_sub);

                if (dSTCollectionBySub.Result.PlayerCollection.TryGetValue(NeokyUnit.collection_id , out var UnitDetails))
                {
                    // The unit Already Exist on the account
                    // You should add a soul to that Unit
                    UnitDetails[Constants.DB_COLLECTION_SOULS]++;
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "AddLootToPlayer", "Client Username : " + myUser.Username + " | New player Soul : "+ NeokyUnit.collection_id), NlogClass.exceptions.Add));
                    await Server.dynamoDBServer.SaveOrUpdatNeokCollection(dSTCollectionBySub.Result);

                }
                else
                {
                    // The unit does not Exist on the account
                    // You should create that Unit
                    dSTCollectionBySub.Result.PlayerCollection.Add(NeokyUnit.collection_id, new Dictionary<string, int>
                            {
                                { Constants.DB_COLLECTION_ID_LVL , 1 },
                                { Constants.DB_COLLECTION_SOULS , 1 }
                            });
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "AddLootToPlayer", "Client Username : " + myUser.Username + " | New player Unit Creation : " + NeokyUnit.collection_id), NlogClass.exceptions.Add));
                    await Server.dynamoDBServer.SaveOrUpdatNeokCollection(dSTCollectionBySub.Result);
                }               
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "AddLootToPlayer", "Database add Loot to player Failed [ Player claimed name : " + player.username + "]"), NlogClass.exceptions.Add));
            }
        }

            /// <summary>Sends the client into the game and informs other clients of the new player.</summary>
            /// <param name="_playerName">The username of the new player.</param>
        public void SendIntoGame(string _playerName, string _sceneToUnload)
        {

            if (myUser.SessionTokens.IsValid())
            {
                try
                {
                    //Get Player Data From Database (Table "clients")
                    var task = Server.dynamoDBServer.ScanForNeokyClientsUsingUsername(_playerName);
                    var taskPlayerCollection = Server.dynamoDBServer.ScanForPlayerCollectionUsingSub(task.Result.client_sub);


                    //Set Client Player Data with Database Return
                    player = task.Result;
                    //playerCollection = taskPlayerCollection.Result; // Initialize player Collection

                    // Get Table XP from DB and find the required Lvl Up from the Lvl
                    player.required_levelup_xp = player.level * 100; // <= Temporary

                    // Set the Scene to Unload depending from where you loged in (AUTHENTICATION / REDEFINE_PWD ..)
                    //player.oldScene = _sceneToUnload;

                    // Ask the Client to Spawn the Player
                    ServerSend.SpawnPlayer(id, player);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Client.cs | Client Scan Failed : " + e);
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "SendIntoGame", "Database scan to initialize player failed for  [ Player claimed name : " + _playerName + "]"), NlogClass.exceptions.Add));
                }
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


        /// <summary>Send to the player a new Scene if Loadable from his current scene.</summary>
        /// <param name="_desiredScene">Desired Scene Name recieved from the Client.</param>
        /// <param name="_currentUserSession">The Client SessionTokens, permit to check if he is still Authenticated</param>
        /// UPDATED 13/06/2020
        public void SwitchScene(string _desiredScene, UserSession _currentUserSession)
        {
            OnSessionExpiredRenewTokens(_currentUserSession);

            // For Valid Sessions, Send Collection to Player
            if (myUser.SessionTokens.IsValid())
            {
                Scene _sceneFound = SceneManager.FindSceneByName(_desiredScene);

                if (_sceneFound != null)
                {
                    if (player.playerCheckAccessDesiredScene(_sceneFound))
                    {
                        ServerSend.SwitchToScene(id, _sceneFound, player.currentScene);
                        // Update Current Scene
                        player.currentScene = _sceneFound;
                    }
                    else
                    {
                        NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "SwitchScene", "PLayer [" + player.username + "] is trying to Access to a Scene wich is not Accessible. [Current Scene : " + player.currentScene.sceneName + " | Desired Scene : " + _desiredScene + "]"), NlogClass.exceptions.Add));
                    }
                }
                else
                {
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "SwitchScene", "Scene [" + _desiredScene + "] requested by [" + player.username + "] does not exist !"), NlogClass.exceptions.Add));
                }
            }
            else
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SwitchScene", "Client Username : " + myUser.Username + " | myUser.SessionTokens is still not Valid"), NlogClass.exceptions.Add));
            }
        }

        /// <summary>Send to the player his Updated Collection from DB.</summary>
        /// <param name="_currentUserSession">The Client SessionTokens, permit to check if he is still Authenticated</param>
        /// UPDATED 13/06/2020
        public void UpdatePlayerCollection(UserSession _currentUserSession)
        {
            PlayerCollection = new Dictionary<NeokyCollection, Dictionary<string, int>>();

            OnSessionExpiredRenewTokens(_currentUserSession);

            // For Valid Sessions, Send Collection to Player
            if (myUser.SessionTokens.IsValid())
            {
                // Foreach to Set Up the Enemy Crew
                try
                {
                    //Try Update the Collection Data
                    var dSTCollectionBySub = Server.dynamoDBServer.ScanForPlayerCollectionUsingSub(player.client_sub);
                    foreach (var UnitCollection in dSTCollectionBySub.Result.PlayerCollection)
                    {
                        UnitsDetails = new Dictionary<string, int>();
                        try
                        {
                            //Try Update the Collection Data
                            var dSTCollectionByID = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(UnitCollection.Key);

                            foreach (var UnitCharacteristic in UnitCollection.Value)
                            {
                                UnitsDetails.Add(UnitCharacteristic.Key, UnitCharacteristic.Value); // 1st Member , Neoky Collection Updated (Id, Stats)       
                            }
                            PlayerCollection.Add(dSTCollectionByID.Result, UnitsDetails);
                        }
                        catch (Exception e)
                        {
                            NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "UpdatePlayerCollection", "Client Username : " + myUser.Username + " | ScanForNeokyCollectionUsingCollectionID failed | Exception : " + e), NlogClass.exceptions.Add));
                        }
                    }
                    ServerSend.SendPlayerCollection(id, PlayerCollection.Count, UnitsDetails.Count, PlayerCollection); // the .count are needed to read the Packet on Client Side
                }
                catch (Exception e)
                {
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "UpdatePlayerCollection", "Client Username : " + myUser.Username + " | ScanForPlayerCollectionUsingSub failed | Exception : " + e), NlogClass.exceptions.Add));
                }
            }
            else
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "UpdatePlayerCollection", "Client Username : " + myUser.Username + " | myUser.SessionTokens is still not Valid"), NlogClass.exceptions.Add));
            }
        }

        /// <summary>Send the client to the Spawn scene of the claimed Dungeon</summary>
        /// <param name="_currentUserSession">The Client SessionTokens, permit to check if he is still Authenticated</param>
        /// <param name="_desiredDungeon">The Client claimed Dungeon Name/param>
        /// UPDATED 13/06/2020
        public void EnterDungeon(UserSession _currentUserSession, string _desiredDungeon)
        {
            OnSessionExpiredRenewTokens(_currentUserSession);
            // For Valid Sessions, Send Collection to Player
            if (myUser.SessionTokens.IsValid())
            {
                player.PlayerEnterDungeon(id,_desiredDungeon);
            }
        }

        /// <summary>Instantiate PlayerCrew and EnemyCrew</summary>
        /// <param name="_currentUserSession">The Client SessionTokens, permit to check if he is still Authenticated</param>
        /// UPDATED 13/06/2020
        public void setPlayerFight(UserSession _currentUserSession)
        {
            OnSessionExpiredRenewTokens(_currentUserSession);
            player.SetFight(id);
        }

        public void UnitAttack(UserSession _currentUserSession, int _unitPosition, int _unitTarget)
        {
            OnSessionExpiredRenewTokens(_currentUserSession);
            player.UnitPlayerAttack(id,_unitPosition,_unitTarget);
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
                    //Server.clients[_fromClient].player.
                    myUser = new CognitoUser(_username, Constants.CLIENTAPP_ID, Server.cognitoManagerServer.userPool, Server.cognitoManagerServer.provider, Constants.NeokySecret);
                    await SignInClients.SignInClientToCognito(_username, _password, id);
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

        /// <summary>Update myUser SessionTokens with RefreshToken</summary>
        /// <param name="_RefreshToken">The CognitoUser Session RefreshToken, used to get New IdToken and AccessToken</param>
        /// UPDATED 13/06/2020
        public async Task GetNewValidTokensAsync(string _RefreshToken)
        {
            // The  "Refresh token expiration (days)" (Cognito->UserPool->General Settings->App clients->Show Details) is the
            // amount of time since the last login that you can use the refresh token to get new tokens.
            // After that period the refresh will fail
            // Using DateTime.Now.AddHours(1) is a workaround for https://github.com/aws/aws-sdk-net-extensions-cognito/issues/24
            myUser.SessionTokens = new CognitoUserSession(myUser.SessionTokens.IdToken, myUser.SessionTokens.AccessToken, _RefreshToken, DateTime.Now, DateTime.Now.AddHours(1));

            try
            {
                AuthFlowResponse context = await myUser.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
                {
                    AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH

                })
                .ConfigureAwait(false);
                myUser.SessionTokens.RefreshToken = _RefreshToken; // Problem is StartWithRefreshTokenAuthAsync return a Null RefreshToken so i will keep my current One.
                currentUserSession = new UserSession(myUser.SessionTokens.AccessToken, myUser.SessionTokens.IdToken, _RefreshToken);
                ServerSend.SendTokens(id, currentUserSession);
            }
            catch (NotAuthorizedException)
            {
                //https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-tokens-with-identity-providers.html
                // refresh tokens will expire - user must login manually every x days (see user pool -> app clients -> details)
                Console.WriteLine("Ask to Client to Login Manually");
                //return new SignInContext(CognitoResult.RefreshNotAuthorized) { ResultMessage = ne.Message };
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetNewValidTokensAsync", "Client Name : " + myUser.Username + " | RefreshToken Create New Session failed | Exception : " + e), NlogClass.exceptions.Add));
            }
        }

        /// <summary>Check if the Client is requesting with a UserSession from another client.</summary>
        /// <param name="_userSession">The Client Claimed UserSession</param>
        /// UPDATED 13/06/2020
        public bool CheckSessionClaimedByUser(UserSession _userSession)
        {
            if (_userSession.Refresh_Token == myUser.SessionTokens.RefreshToken)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>Check if the UserSession is Valid, if Not Renew Tokens </summary>
        /// <param name="_currentUserSession">The Client Claimed UserSession</param>
        /// UPDATED 13/06/2020
        private async void OnSessionExpiredRenewTokens(UserSession _currentUserSession)
        {
            if (!myUser.SessionTokens.IsValid())
            {
                await GetNewValidTokensAsync(_currentUserSession.Refresh_Token); // If not possible the Function ll send a Client Packet Return to LoginPage
            }
        }

        /// <summary>Disconnects the client and stops all network traffic.</summary>
        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            player = null;
            myUser = null;
            currentUserSession = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
