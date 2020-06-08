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

namespace GameServer
{

    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public CognitoUser myUser;
        public UserSession currentUserSession;
        public Player player;
        public NeokyPlayerCollection playerCollection;
        public TCP tcp;
        public UDP udp;
        public Dictionary<int, NeokyCollection> PlayerCrew;
        public Dictionary<int, NeokyCollection> EnemyCrew;
        public Dictionary<NeokyCollection, Dictionary<string, int>> PlayerCollection;
        public Dictionary<string, int> UnitsDetails;
        public int UnitDetailCount; // Permet de savir combien d'éléments il y a dans le Dictionary<string,int> de PlayerCollection

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
                    playerCollection = taskPlayerCollection.Result;

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

        // Principalement utilisé par le UIMenu du Client
        public void SwitchScene(string _desiredScene)
        {

            Scene _sceneFound = SceneManager.FindSceneByName(_desiredScene);
            //string _oldScene = Server.clients[id].player.currentScene.sceneName;
            if (_sceneFound != null)
            {
                if (Server.clients[id].player.playerCheckAccessDesiredScene(_sceneFound))
                {
                    ServerSend.SwitchToScene(id, _sceneFound, Server.clients[id].player.currentScene);
                    // Update Current Scene
                    Server.clients[id].player.currentScene = _sceneFound;
                }
                else
                {
                    //Console.WriteLine("User Cant access to this Scene !");
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "SwitchScene", "PLayer [" + Server.clients[id].player.username + "] is trying to Access to a Scene wich is not Accessible. [Current Scene : " + Server.clients[id].player.currentScene.sceneName + " | Desired Scene : " + _desiredScene + "]"), NlogClass.exceptions.Add));
                }
            }
            else
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "SwitchScene", "Scene [" + _desiredScene + "] requested by [" + Server.clients[id].player.username + "] does not exist !"), NlogClass.exceptions.Add));
            }
        }

        public void UpdatePlayerCollection(string _userSub)
        {
            //Dictionary<string, int>  Units = new Dictionary<string, int>();
            PlayerCollection = new Dictionary<NeokyCollection, Dictionary<string, int>>();
            

            if (Server.clients[id].playerCollection != null)
            {
                
                // Foreach to Set Up the Enemy Crew
                try
                {
                    
                    //Try Update the Collection Data
                    var dynamoScanTask = Server.dynamoDBServer.ScanForPlayerCollectionUsingSub(_userSub);                    
                    foreach (var UnitCollection in dynamoScanTask.Result.PlayerCollection)
                    {
                        UnitsDetails = new Dictionary<string, int>();
                        UnitDetailCount = 0;

                        try
                        {
                            //Try Update the Collection Data
                            var dynamoScanTsk = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(UnitCollection.Key);    

                            foreach (var UnitCharacteristic in UnitCollection.Value)
                            {
                                UnitDetailCount += 1;
                                UnitsDetails.Add(UnitCharacteristic.Key, UnitCharacteristic.Value); // 1st Member , Neoky Collection Updated (Id, Stats)       
                            
                            }

                            PlayerCollection.Add(dynamoScanTsk.Result, UnitsDetails);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("UpdatePlayerCollection | Scan NeokyCollection Failed");
                            throw;
                        }
                        //UnitsDetails.Clear();
                        Console.WriteLine("UpdatePlayerCollection | Added New Unit Collection" + UnitCollection.Key);
                    }
                    Console.WriteLine("UpdatePlayerCollection | All Unit Collection Added");

                    ServerSend.SendPlayerCollection(id, PlayerCollection.Count, UnitDetailCount, PlayerCollection);
                }
                catch (Exception e)
                {
                    Console.WriteLine("UpdatePlayerCollection | Scan Player Collection Failed");
                }
                //ServerSend.SendPlayerCollection(id, PlayerCollection.Count, PlayerCollection);
            }
            else
            {
                Console.WriteLine("UpdatePlayerCollection | User Player Collection is Null !");
            }
        }





        public void EnterDungeon(string _desiredDungeon)
        {
            Dungeon.Dungeon _dungeonFound = DungeonManager.FindDungeonByName(_desiredDungeon);


            if (_dungeonFound != null)
            {   // Dungeon Exists                
                if (_dungeonFound.playerCanAccessDungeon(Server.clients[id].player.level))
                {   //Player have the right level for this Dungeon                    
                    if (_dungeonFound.playerCanAccessDesiredDungeonMap(_dungeonFound, Server.clients[id].player.currentScene.sceneName))
                    {   // Dungeon Spawn Scene exist for this Dungeon & Player is on the Right CurrentScene to Access it

                        ServerSend.SwitchToScene(id, _dungeonFound.spawnScene, Server.clients[id].player.currentScene);
                        // Update Current Scene
                        Server.clients[id].player.currentScene = _dungeonFound.spawnScene;
                    }
                    else
                    {
                        Console.WriteLine("SpawnScene does not exist for this Dungeon OR Player is trying to access to the Scene from a bad Entry Point.");
                        NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "EnterDungeon", "SpawnDungeonScene does not exist for this Dungeon OR Player [" + Server.clients[id].player.username + "] is trying to access to the Scene from a bad Entry Point. [CurrentScene : " + Server.clients[id].player.currentScene.sceneName + "]"), NlogClass.exceptions.Add));

                    }
                }
                else
                {
                    Console.WriteLine("Player dont have the level required to Access this Dungeon.");
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "EnterDungeon", "Player [" + Server.clients[id].player.username + "] dont have the level required to Access this Dungeon. [Lvl : " + Server.clients[id].player.level + "]"), NlogClass.exceptions.Add));
                }
            }
        }

        public void setFight ()
        {
            PlayerCrew = new Dictionary<int, NeokyCollection>();
            EnemyCrew = new Dictionary<int, NeokyCollection>();


            if (Server.clients[id].player.currentScene.enemyCrewMember != null)
            {
                // Foreach to Set Up the Enemy Crew
                foreach (var item in Server.clients[id].player.currentScene.enemyCrewMember)
                {
                
                    switch (item.Key)
                    {
                        case 1:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    EnemyCrew.Add(1, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case 2:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    EnemyCrew.Add(2, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case 3:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    EnemyCrew.Add(3, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case 4:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    EnemyCrew.Add(4, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case 5:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    EnemyCrew.Add(5, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case 6:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    EnemyCrew.Add(6, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        default:
                            Console.WriteLine("setFight | We found a Key we cant read.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("setFight | enemyCrewMember seems Empty.");
            }

            if (Server.clients[id].player.PlayerCrew != null)
            {
                // Foreach to Set Up the Player Crew
                foreach (var item in Server.clients[id].player.PlayerCrew)
                {
                    switch (item.Key)
                    {
                        case "Member_01":
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value);
                                    PlayerCrew.Add(1, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case "Member_02":
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value);
                                    PlayerCrew.Add(2, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case "Member_03":
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value);
                                    PlayerCrew.Add(3, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case "Member_04":
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value);
                                    PlayerCrew.Add(4, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case "Member_05":
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value);
                                    PlayerCrew.Add(5, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        case "Member_06":
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dynamoScanTask = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value);
                                    PlayerCrew.Add(6, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                    Console.WriteLine("setFight | Scan Collection Data Success");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("setFight | Scan Collection to Update Collection Data Failed");
                                    //throw;
                                }
                            }
                            else
                            {
                                Console.WriteLine("setFight | Player have not set a Member_01 value is Null.");
                            }
                            break;
                        default:
                            Console.WriteLine("setFight | We found a Key we cant read.");
                            break;
                    }

                }
            }
            else
            {
                Console.WriteLine("setFight | Player Crew seems Empty.");
            }

            // Foreach to send the EnemyCrew to the player
            /*foreach (var _enemyCrewMember in EnemyCrew)
            {
                if (_enemyCrewMember.Value != null)
                {
                    // Set the Member crew as Spawned
                    _enemyCrewMember.Value.isSpawned = true;
                    //ServerSend.SpawnEnemyMemberCrew(id, _enemyCrewMember.Key, _enemyCrewMember.Value);
                    
                }
            }*/


            ServerSend.SpawnEnemyAllCrew(id, EnemyCrew.Count, EnemyCrew);
            ServerSend.SpawnPlayerAllCrew(id, PlayerCrew.Count, PlayerCrew);
            // Foreach to send the crew to the player
            /*foreach (var _crewMember in ClientCollection)
            {
                if (_crewMember.Value != null)
                {
                    // Set the Member crew as Spawned
                    _crewMember.Value.isSpawned = true;
                    ServerSend.SpawnMemberCrew(id, _crewMember.Key, _crewMember.Value);                    
                }
            }*/
        }

        public async void SignUptoCognito(string _username, string _password, string _email)
        {
            // If the REgEx Formats are Respected, we proceed to Adhesion OR We Return an Error Format
            if (SecurityCheck.CheckUserPattern(_username))
            {
                if (SecurityCheck.CheckPasswordPattern(_password))
                {
                    if (SecurityCheck.CheckEmailPattern(_email))
                    {
                        await SignUpClients.SignUpClientToCognito(id, _username, _password, _email);
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
                    Server.clients[id].myUser = new CognitoUser(_username, Constants.CLIENTAPP_ID, Server.cognitoManagerServer.userPool, Server.cognitoManagerServer.provider, Constants.NeokySecret);
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

        public async void AccessHomepage(string _clientToken)
        {
            // Check Token is still valid
            // Current Scene = Homepage
            // Ask Client to Load Homepage
            // Et on confirme au client qu'il peut Switch de Scene
            //ServerSend.SwitchToScene(id, Constants.SCENE_HOMEPAGE, Constants.SCENE_NOSCENE);
        }

        public async Task GetNewValidTokensAsync(string _RefreshToken)
        {

            //var CompareTokens = Server.clients[id].myUser.SessionTokens.IdToken;
            Server.clients[id].myUser.SessionTokens = new CognitoUserSession(null, null, _RefreshToken, DateTime.Now, DateTime.Now.AddHours(1));

            InitiateRefreshTokenAuthRequest refreshRequest = new InitiateRefreshTokenAuthRequest()
            {
                AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
            };           

            try
            {
                Console.WriteLine("Client.cs | GetNewValidTokensAsync | StartWithRefreshTokenAuthAsync try lunched !");
                AuthFlowResponse authResponse = await Server.clients[id].myUser.StartWithRefreshTokenAuthAsync(refreshRequest).ConfigureAwait(false);
                currentUserSession = new UserSession(Server.clients[id].myUser.SessionTokens.AccessToken, Server.clients[id].myUser.SessionTokens.IdToken, Server.clients[id].myUser.SessionTokens.RefreshToken);

                Console.WriteLine("Client.cs | GetNewValidTokensAsync | SessionUpdated sended to Client");
                ServerSend.SendTokens(id, currentUserSession);
            }
            catch (Exception e)
            {
                Console.WriteLine("Client.cs | GetNewValidTokensAsync | StartWithRefreshTokenAuthAsync failed to Refresh Session");
                switch (e.GetType().ToString())
                {
                    default:
                        Console.WriteLine("Client.cs | GetNewValidTokensAsync | Unknown Exception | " + e.GetType().ToString() + " | " + e);
                        //ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_KO);
                        break;
                }
            }
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
