using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

using GameServer.Dungeon;

using Amazon.DynamoDBv2.DataModel;

using GameServer.Scenes;
using NLog.Common;
using NLog;

namespace GameServer
{
    [DynamoDBTable("clients")]
    public class Player : IEquatable<Player>
    {        
        public string client_id { get; set; }

        public string username { get; set; }

        public string email { get; set; }

        public string account_statut { get; set; }

        [DynamoDBHashKey]
        public string client_sub { get; set; }

        public float level { get; set; }

        public float golds { get; set; }

        public float diams { get; set; }

        public float level_xp { get; set; }

        public float required_levelup_xp { get; set; }

        public Dictionary<string, string> PlayerCrew { get; set; }

        public Dictionary<string, int> box { get; set; }

        public Scene currentScene;
        public Scene unloadScene;

        // SetFight Variables
        public Dictionary<int, NeokyCollection> NeokyCollection_PlayerCrew; // Crew defined by the player for the Fights
        public Dictionary<int, NeokyCollection> NeokyCollection_EnemyCrew; // Crew defined by the Dungeon for the Scene Fight

        //public string currentScene;
        //public string oldScene;

        public Player()
        {
            currentScene = new HomePageScene();
            unloadScene = new AuthenticationScene();
        }

        /*public Player(string _clientId, string _username, string _email, string _accountStatut, string _clientSub, float _level, float _levelXp)
        {
            client_id = _clientId;
            username = _username;
            email = _email;
            account_statut = _accountStatut;
            client_sub = _clientSub;
            level = _level;
            level_xp = _levelXp;
            currentScene = new HomePageScene();
            unloadScene = new AuthenticationScene();

        }*/

        /// <summary>Mandatory for a DynamoDB Interface class</summary>
        /// UPDATED 13/06/2020
        public bool Equals(Player other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return client_id == other.client_id
                && string.Equals(username, other.username)
                && string.Equals(email, other.email)
                && string.Equals(account_statut, other.account_statut)
                && string.Equals(client_sub, other.client_sub)
                && float.Equals(level, other.level)
                && float.Equals(level_xp, other.level_xp)
                && float.Equals(required_levelup_xp, other.required_levelup_xp);
        }

        /// <summary>Mandatory for a DynamoDB Interface class</summary>
        /// UPDATED 13/06/2020
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Player)obj);
        }

        /// <summary>Mandatory for a DynamoDB Interface class</summary>
        /// UPDATED 13/06/2020
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = client_id != null ? client_id.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (username != null ? username.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (email != null ? email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (account_statut != null ? account_statut.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (client_sub != null ? client_sub.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (level != 0 ? level.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (level_xp != 0 ? level_xp.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (required_levelup_xp != 0 ? required_levelup_xp.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>Check if player is legit to request access to the Scene, An accessible scene must be on the Scene.OldScenes, if he is, return true.</summary>
        /// <param name="_desiredScene">Client Scene Desired</param>
        /// UPDATED 13/06/2020
        public bool playerCheckAccessDesiredScene(Scene _desiredScene)
        {
            // Search in oldScenes of the desiredScene if the currentScene is there
            foreach (string _oldScene in _desiredScene.oldScenes)
            {
                if (_oldScene == currentScene.sceneName)
                {

                    return true;
                }
            }
            return false;
        }

        /// <summary>Check if player is legit to request access to the Dungeon, and if he is, send him to the Spawn Scene</summary>
        /// <param name="_clientID">The client ID requesting access to the Dungeon</param>
        /// <param name="_desiredDungeon">The desired Dungeon Name</param>
        /// UPDATED 13/06/2020
        public void PlayerEnterDungeon(int _clientID, string _desiredDungeon)
        {
            Dungeon.Dungeon _dungeonFound = DungeonManager.FindDungeonByName(_desiredDungeon);

            if (_dungeonFound != null)
            {   // Dungeon Exists                
                if (_dungeonFound.playerCanAccessDungeon(level))
                {   //Player have the right level for this Dungeon                    
                    if (_dungeonFound.playerCanAccessDesiredDungeonMap(_dungeonFound,currentScene.sceneName))
                    {   // Dungeon Spawn Scene exist for this Dungeon & Player is on the Right CurrentScene to Access it

                        ServerSend.SwitchToScene(_clientID, _dungeonFound.spawnScene, currentScene);
                        // Update Current Scene
                        currentScene = _dungeonFound.spawnScene;
                    }
                    else
                    {
                        NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "PlayerEnterDungeon", "SpawnDungeonScene does not exist for this Dungeon OR Player [" + username + "] is trying to access to the Scene from a bad Entry Point. [CurrentScene : " + currentScene.sceneName + "]"), NlogClass.exceptions.Add));
                    }
                }
                else
                {
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "PlayerEnterDungeon", "Player [" + username + "] dont have the level required to Access this Dungeon. [Lvl : " + level + "]"), NlogClass.exceptions.Add));
                }
            }
            else
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "PlayerEnterDungeon", "Player [" + username + "] try to access to a Dungeon wich does not exist ["+ _desiredDungeon + "]"), NlogClass.exceptions.Add));
            }
        }

        /// <summary>Processes player input and moves the player.</summary>
        //public void Update()
        //{
        /*Vector2 _inputDirection = Vector2.Zero;
        if (inputs[0])
        {
            _inputDirection.Y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.Y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.X += 1;
        }
        if (inputs[3])
        {
            _inputDirection.X -= 1;
        }

        Move(_inputDirection);*/
        //}

        /// <summary>Set the PlayerCrew and EnemyCrew for the Scene Fight</summary>
        /// <param name="_clientId">The Client Id, trace client for the Logs</param>
        /// UPDATED 13/06/2020
        public void SetFight(int _clientId)
        {       
            SetEnemyCrew(_clientId);
            SetPlayerCrew(_clientId);            
        }

        /// <summary>Set the NeokyCollection_EnemyCrew for the Scene Fight</summary>
        /// <param name="_clientId">The Client Id, trace client for the Logs</param>
        /// UPDATED 13/06/2020
        private void SetEnemyCrew(int _clientId)
        {
            NeokyCollection_EnemyCrew = new Dictionary<int, NeokyCollection>();

            if (currentScene.enemyCrewMember != null)
            {
                // Foreach to Set Up the Enemy Crew
                foreach (var item in currentScene.enemyCrewMember)
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
                                    var dSTByID = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    dSTByID.Result.isSpawned = true;
                                    NeokyCollection_EnemyCrew.Add(1, dSTByID.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Enemy Unit N° 1 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Unit N° 1 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            }
                            break;
                        case 2:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dSTByID = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    dSTByID.Result.isSpawned = true;
                                    NeokyCollection_EnemyCrew.Add(2, dSTByID.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Enemy Unit N° 2 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Unit N° 2 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            }
                            break;
                        case 3:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dSTByID = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    dSTByID.Result.isSpawned = true;
                                    NeokyCollection_EnemyCrew.Add(3, dSTByID.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Enemy Unit N° 3 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Unit N° 3 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            }
                            break;
                        case 4:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dSTByID = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    dSTByID.Result.isSpawned = true;
                                    NeokyCollection_EnemyCrew.Add(4, dSTByID.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Enemy Unit N° 4 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Unit N° 4 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            }
                            break;
                        case 5:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dSTByID = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    dSTByID.Result.isSpawned = true;
                                    NeokyCollection_EnemyCrew.Add(5, dSTByID.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Enemy Unit N° 5 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Unit N° 5 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            }
                            break;
                        case 6:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data
                                    var dSTByID = Server.dynamoDBServer.ScanForNeokyCollectionUsingCollectionID(item.Value.collection_id);
                                    dSTByID.Result.isSpawned = true;
                                    NeokyCollection_EnemyCrew.Add(6, dSTByID.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Enemy Unit N° 6 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Unit N° 6 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            }
                            break;
                        default:
                            NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Unit N° is not between [1 and 6] for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            break;
                    }
                }
            }
            else
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Enemy Crew Member is Empty for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
            }
            ServerSend.SpawnEnemyAllCrew(_clientId, NeokyCollection_EnemyCrew.Count, NeokyCollection_EnemyCrew);
        }

        /// <summary>Set the NeokyCollection_PlayerCrew for the Scene Fight</summary>
        /// <param name="_clientId">The Client Id, trace client for the Logs</param>
        /// UPDATED 13/06/2020
        private void SetPlayerCrew(int _clientId)
        {
            NeokyCollection_PlayerCrew = new Dictionary<int, NeokyCollection>();

            if (PlayerCrew != null)
            {
                // Foreach to Set Up the Player Crew
                foreach (var item in PlayerCrew)
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
                                    dynamoScanTask.Result.isSpawned = true;
                                    NeokyCollection_PlayerCrew.Add(1, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)                                   
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Player Unit N° 1 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Player Unit N° 1 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
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
                                    dynamoScanTask.Result.isSpawned = true;
                                    NeokyCollection_PlayerCrew.Add(2, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Player Unit N° 2 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Player Unit N° 2 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
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
                                    dynamoScanTask.Result.isSpawned = true;
                                    NeokyCollection_PlayerCrew.Add(3, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Player Unit N° 3 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Player Unit N° 3 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
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
                                    dynamoScanTask.Result.isSpawned = true;
                                    NeokyCollection_PlayerCrew.Add(4, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Player Unit N° 4 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Player Unit N° 4 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
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
                                    dynamoScanTask.Result.isSpawned = true;
                                    NeokyCollection_PlayerCrew.Add(5, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Player Unit N° 5 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Player Unit N° 5 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
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
                                    dynamoScanTask.Result.isSpawned = true;
                                    NeokyCollection_PlayerCrew.Add(6, dynamoScanTask.Result); // 1st Member , Neoky Collection Updated (Id, Stats)
                                }
                                catch (Exception e)
                                {
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Failed the ScanForNeokyCollectionUsingCollectionID for Player Unit N° 6 of the Fight for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName + " | Exception : " + e), NlogClass.exceptions.Add));
                                    // Abort the Fight
                                }
                            }
                            else
                            {
                                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | Player Unit N° 6 of the Fight is Null for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            }
                            break;
                        default:
                            NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | PlayerCrew N° is not between [1 and 6] for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
                            break;
                    }

                }
            }
            else
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SetFight", "Client Name : " + Server.clients[_clientId].myUser.Username + " | PlayerCrew is Empty for the Scene : " + Server.clients[_clientId].player.currentScene.sceneName), NlogClass.exceptions.Add));
            }
            ServerSend.SpawnPlayerAllCrew(_clientId, NeokyCollection_PlayerCrew.Count, NeokyCollection_PlayerCrew);
        }

        public void UnitPlayerAttack(int _clientId, int _unitPosition, int _unitTarget)
        {
            NeokyCollection _CurrentUnit = new NeokyCollection();
            NeokyCollection _CurrentTarget = new NeokyCollection();

            if (NeokyCollection_PlayerCrew.TryGetValue(_unitPosition, out _CurrentUnit))
            {
                if (NeokyCollection_EnemyCrew.TryGetValue(_unitTarget, out _CurrentTarget))
                {
                    if (_CurrentTarget.isSpawned && _CurrentUnit.isSpawned)
                    {
                        // Target & Player Unit are Spawned
                        if (_CurrentTarget.lifePoints > 0 && _CurrentUnit.lifePoints > 0)
                        {
                            // Target & Player Unit are Alive
                            _CurrentTarget.lifePoints -= _CurrentUnit.attackDamages;
                            // Does target Die ? 
                            // Does Fight is Win ? 
                            Console.WriteLine(_CurrentUnit.collection_name + " deal " + _CurrentUnit.attackDamages.ToString() + " to the Enemy unit " + _CurrentTarget.collection_name);
                            ServerSend.AttackUnit(_clientId, _unitPosition, _unitTarget);
                        }
                    }
                }
            }
        }
    }
}
