using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using GameServer.Units;
using NLog.Common;
using NLog;
using GameServer.Scenes;
using System.Collections;
using GameServer.Spells;
using GameServer.IA;

namespace GameServer.TurnBasedFights
{
    public enum BattleState { INIT_FIGHT, INIT_OVER, PLAYER_TURN, ENEMY_TURN, UPDATE_TURN, WON, LOST }

    public class NewFight
    {
        public Dictionary<int, Unit> _playerCollection;
        public Dictionary<int, Unit> _enemyCollection;

        // Used Frequently so turnmeter is handle by this instead of Unit
        private Dictionary<int, float> _playerUnitTurnmeter;
        private Dictionary<int, float> _enemyUnitTurnmeter;

        public int PlayingUnitID;
        private float TurnMeterMaxValue = 400; // Each turn cost 400 Velocity

        private bool isInitDone = false; // Used to Init the Fight only Once

        public bool isClientInitOver = false;

        public BattleState currentBattleState;

        public NewFight()
        {
            Console.WriteLine("NewFight Constructor");
            currentBattleState = BattleState.INIT_FIGHT;
        }

        public void Update()
        {
            switch (currentBattleState)
            {
                case BattleState.INIT_FIGHT:
                        if (!isInitDone)
                        {
                            // Set _playerUnitTurnmeter & _enemyUnitTurnmeter & Switch to UPDATE_TURN
                            InitUpdateAllTurnmeter();
                            isInitDone = true;
                            Console.WriteLine("Server Init Over");
                        }
                        if (isClientInitOver & isInitDone) //Waiting for FIGHT_READY from Client
                        {
                            Console.WriteLine("Server & Client Init Over");
                            // Switch to Update_Turn => Status who ll find who's turn it is.
                            currentBattleState = BattleState.UPDATE_TURN;
                        }
                    break;
                case BattleState.UPDATE_TURN:
                        // Who's turn is is with the current TurnMeter ? 
                        ChangeTurnFromTurnmeter();
                    break;
                case BattleState.PLAYER_TURN:
                    Console.WriteLine("Player Turn");
                    if (true) // PlayerPlayed
                        {
                            // Update All TurnMeters
                            UpdateAllTurnmeter(PlayingUnitID, BattleState.PLAYER_TURN);
                        }
                    break;
                case BattleState.ENEMY_TURN:
                    Console.WriteLine("Enemy Turn");

                    EnemyTurn(); 
                        // 
                        // Update All TurnMeters
                        UpdateAllTurnmeter(PlayingUnitID, BattleState.ENEMY_TURN);
                    break;
                case BattleState.WON:
                    // Fid a Reward
                    // Send to the player the Reward = End of Fight
                    break;
                case BattleState.LOST:
                    // Send to the player End of Fight
                    break;
            }

        }

        private void EnemyTurn()
        {
            EnemyIA EnemyChoice = new EnemyIA(_playerCollection, _enemyCollection, PlayingUnitID);
            // Analyse Situation
            EnemyChoice.UnitsAnalyse();

            // Choose a Spell Memorise it as "BestSpell"
            EnemyChoice.CheckUnitSpells(_enemyCollection[PlayingUnitID].spellList);

            //Find Player Target
            var Target = EnemyChoice.FindUnitTarget();

            //Define the Spell Target Zone
            var TargetZone = EnemyChoice.FindUnitTargetZone();

            // Do Action
        }       


        /// <summary>Set _enemyCollection for the Scene Fight from definied Scene Units</summary>
        /// <param name="_clientId">The Client Id, trace client for the Logs</param>
        /// UPDATED 01/07/2020
        public void InstantiateEnemyUnitsCrew(int _clientId, Scene currentScene)
        {

            //NeokyCollection_EnemyCrew = new Dictionary<int, NeokyCollection>();
            _enemyCollection = new Dictionary<int, Unit>();

            if (currentScene.enemyUnitCrewMember != null)
            {
                // Foreach to Set Up the Enemy Crew
                foreach (var item in currentScene.enemyUnitCrewMember)
                {
                    var l_UnitID = UnitManager.FindUnitByID(item.Value.UnitID);

                    switch (item.Key)
                    {
                        case 1:
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data                                    
                                    l_UnitID.isSpawned = true;
                                    _enemyCollection.Add(1, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _enemyCollection.Add(2, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _enemyCollection.Add(3, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _enemyCollection.Add(4, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _enemyCollection.Add(5, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _enemyCollection.Add(6, l_UnitID);
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
            ServerSend.SpawnEnemyAllUnitsCrew(_clientId, _enemyCollection.Count, _enemyCollection);
        }

        /// <summary>Set _playerCollection for the Scene Fight from Player DB</summary>
        /// <param name="_clientId">The Client Id, trace client for the Logs</param>
        /// UPDATED 01/07/2020
        public void InstantiatePlayerUnitsCrew(int _clientId, Dictionary<string, string> PlayerCrew)
        {
            _playerCollection = new Dictionary<int, Unit>();

            if (PlayerCrew != null)
            {
                // Foreach to Set Up the Player Crew
                foreach (var item in PlayerCrew)
                {
                    var l_UnitID = UnitManager.FindUnitByID(item.Value);

                    switch (item.Key)
                    {
                        case "Member_01":
                            // In case the 1st Member of the crew is Set 
                            if (item.Value != null)
                            {   // If the value "Exists" / is a Real Value
                                try
                                {
                                    //Try Update the Collection Data                                   
                                    l_UnitID.isSpawned = true;
                                    _playerCollection.Add(1, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _playerCollection.Add(2, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _playerCollection.Add(3, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _playerCollection.Add(4, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _playerCollection.Add(5, l_UnitID);
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
                                    l_UnitID.isSpawned = true;
                                    _playerCollection.Add(6, l_UnitID);
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
            ServerSend.SpawnPlayerAllUnitCrew(_clientId, _playerCollection.Count, _playerCollection);
        }

        /*/// <summary>Sort All Units By TurnMeter</summary>  
        /// UPDATED 01/07/2020
        private List<Unit> GetAllUnitsSortedByTurnMeter()
        {
            List<Unit> FullList = new List<Unit>();

            FullList.AddRange(_playerCollection.Values.ToList());
            FullList.AddRange(_enemyCollection.Values.ToList());

            FullList.Sort((x, y) => x.turnMeter.CompareTo(y.turnMeter));

            return FullList;
        }*/

        /*/// <summary>Sort Units By TurnMeter</summary>  
        /// UPDATED 01/07/2020
        private Dictionary<int, float> GetUnitsSortedByTurnMeter(Dictionary<int, Unit> _Units)
        {
            var OrderedDic = _Units.OrderByDescending(u => u.Value.turnMeter).ToDictionary(u => u.Key, u => u.Value.turnMeter);

            return OrderedDic;            
        }*/

        private Dictionary<int, float> GetUnitsSortedByTurnMeter(Dictionary<int, float> _Units)
        {
            var OrderedDic = _Units.OrderByDescending(u => u.Value).ToDictionary(u => u.Key, u => u.Value);

            return OrderedDic;
        }

        /// <summary>Set Player or Enemy Turn and Set PlayingUnitID</int></summary>  
        /// UPDATED 01/07/2020
        private void ChangeTurnFromTurnmeter()
        {
            Random random = new Random();

            // If turnMeter Enemy = TurnMeter Player, Random Pick Next player
            if (_playerUnitTurnmeter.First().Value == _enemyUnitTurnmeter.First().Value)
            {
                var n = random.Next(0, 2);

                //Set the NextPlayer 
                //Set NextUnitID

                if (n == 0)
                {
                    SetPlayerTurn();   
                }
                else
                {
                    SetEnemyTurn();
                }
            }

            if (_playerUnitTurnmeter.First().Value > _enemyUnitTurnmeter.First().Value)
            {
                SetPlayerTurn();
            }
            else
            {
                SetEnemyTurn();
            }
        }

        private void SetEnemyTurn()
        {
            PlayingUnitID = _enemyUnitTurnmeter.First().Key;
            currentBattleState = BattleState.ENEMY_TURN;            
        }

        private void SetPlayerTurn()
        {
            // Send to Player the Unit to Play
            PlayingUnitID = _playerUnitTurnmeter.First().Key;
            currentBattleState = BattleState.PLAYER_TURN;
        }

        private void UpdateAllTurnmeter(int _PlayingUnitID, BattleState _battleState)
        {
            UpdatePlayerTurnMeter(_PlayingUnitID, _battleState);
            UpdateEnemyTurnMeter(_PlayingUnitID, _battleState);
        }
        private void InitUpdateAllTurnmeter()
        {
            _playerUnitTurnmeter = new Dictionary<int, float>();
            _enemyUnitTurnmeter = new Dictionary<int, float>();

            // Update _playerUnitTurnmeter from PlayerCollection
            foreach (var UnitPlayerCollection in _playerCollection)
            {
                _playerUnitTurnmeter[UnitPlayerCollection.Key] = _playerCollection[UnitPlayerCollection.Key].UnitVelocity;
            }
            _playerUnitTurnmeter = GetUnitsSortedByTurnMeter(_playerUnitTurnmeter);
            // Update _enemyUnitTurnmeter from EnemyCollection
            foreach (var UnitEnemyCollection in _enemyCollection)
            {
                _enemyUnitTurnmeter[UnitEnemyCollection.Key] = _enemyCollection[UnitEnemyCollection.Key].UnitVelocity;
            }
            _enemyUnitTurnmeter = GetUnitsSortedByTurnMeter(_enemyUnitTurnmeter);            
        }

        /// <summary>Update Player TurnMeter and Sort It</int></summary>  
        /// UPDATED 01/07/2020
        private void UpdatePlayerTurnMeter(int _PlayingUnitID, BattleState _battleState)
        {
            foreach (var UnitPlayerCollection in _playerCollection)
            {
                //_playerCollection[UnitPlayerCollection.Key].turnMeter += _playerCollection[UnitPlayerCollection.Key].UnitVelocity;
                _playerUnitTurnmeter[UnitPlayerCollection.Key] += _playerCollection[UnitPlayerCollection.Key].UnitVelocity;

                if (_battleState.Equals(BattleState.PLAYER_TURN))
                {
                    // If it's a player Turn & the Unit is the Current Playing Unit
                    if (_playerUnitTurnmeter[UnitPlayerCollection.Key].Equals(_PlayingUnitID))
                    {
                        // Decrease the turnMeter by the turnMeterMaxValues.
                        //_playerCollection[UnitPlayerCollection.Key].turnMeter -= TurnMeterMaxValue;
                        _playerUnitTurnmeter[UnitPlayerCollection.Key] -= TurnMeterMaxValue;
                        //Sort my new _playerUnitTurnmeter by Hiest Turnmeter
                        _playerUnitTurnmeter = GetUnitsSortedByTurnMeter(_playerUnitTurnmeter);
                        currentBattleState = BattleState.UPDATE_TURN;
                    }
                }
            }
        }

        /// <summary>Update Enemy TurnMeter and Sort It</int></summary>  
        /// UPDATED 01/07/2020
        private void UpdateEnemyTurnMeter(int _PlayingUnitID, BattleState _battleState)
        {
            foreach (var UnitEnemyCollection in _enemyCollection)
            {
                //_enemyUnitTurnmeter[UnitEnemyCollection.Key].turnMeter += _enemyCollection[UnitEnemyCollection.Key].UnitVelocity;
                _enemyUnitTurnmeter[UnitEnemyCollection.Key] += _enemyCollection[UnitEnemyCollection.Key].UnitVelocity;

                if (_battleState.Equals(BattleState.ENEMY_TURN))
                {
                    // If it's a enemy Turn & the Unit is the Current Playing Unit
                    if (_enemyUnitTurnmeter[UnitEnemyCollection.Key].Equals(_PlayingUnitID))
                    {
                        // Decrease the turnMeter by the turnMeterMaxValues.
                        //_playerCollection[UnitEnemyCollection.Key].turnMeter -= TurnMeterMaxValue;
                        _enemyUnitTurnmeter[UnitEnemyCollection.Key] -= TurnMeterMaxValue;
                        //Sort my new _enemyUnitTurnmeter by Hiest Turnmeter
                        _enemyUnitTurnmeter = GetUnitsSortedByTurnMeter(_enemyUnitTurnmeter);
                        currentBattleState = BattleState.UPDATE_TURN;
                    }
                }
            }            
        }

        /*private void InitPlayerTurnMeter()
        {
            foreach (var Unit in _playerCollection)
            {
                //Unit.Value.turnMeter = Unit.Value.UnitVelocity;
                _playerUnitTurnmeter[Unit.Key] = Unit.Value.UnitVelocity;
            }
            //_playerUnitTurnmeter = GetUnitsSortedByTurnMeter(_playerCollection);
            _playerUnitTurnmeter = GetUnitsSortedByTurnMeter(_playerUnitTurnmeter);
        }

        private void InitEnemyTurnMeter()
        {
            foreach (var Unit in _enemyCollection)
            {
               // Unit.Value.turnMeter = Unit.Value.UnitVelocity;
                _enemyUnitTurnmeter[Unit.Key] = Unit.Value.UnitVelocity;
            }
            //_enemyUnitTurnmeter = GetUnitsSortedByTurnMeter(_enemyCollection);
            _enemyUnitTurnmeter = GetUnitsSortedByTurnMeter(_enemyUnitTurnmeter);

        }*/

    }
}
