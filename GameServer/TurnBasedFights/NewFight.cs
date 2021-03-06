﻿using System;
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
using System.Security.Cryptography.X509Certificates;

namespace GameServer.TurnBasedFights
{
    public enum BattleState { INIT_FIGHT, INIT_OVER, PLAYER_TURN, IA_TURN, UPDATE_TURN, WON, LOST }

    public class NewFight
    {
        public Dictionary<int, Unit> _playerCollection;
        public Dictionary<int, Unit> _IACollection;

        // Used Frequently so turnmeter is handle by this instead of Unit
        private Dictionary<int, float> _playerUnitTurnmeter;
        private Dictionary<int, float> _IAUnitTurnmeter;

        public int PlayingUnitID;
        private float TurnMeterMaxValue = 400; // Each turn cost 400 Velocity

        private bool isInitDone = false; // Used to Init the Fight only Once
        public bool isIAAttacking = false;// Control that Unit only Attack Once

        public bool isClientInitOver = false;
        public bool isPlayerTurnOver = false;
        public bool isIATurnOver = false;
        public bool isTurnUpdated = false;

        public BattleState currentBattleState;

        public int ClientID;

        public NewFight(int _clientId)
        {
            ClientID = _clientId;
            Console.WriteLine("NewFight Constructor");
            currentBattleState = BattleState.INIT_FIGHT;
        }

        public void Update()
        {
            /*var message = "";
            var massage = "";
            var Retenue = "";*/
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
                        //isTurnOver = true;
                        currentBattleState = BattleState.UPDATE_TURN;
                        }
                    break;
                case BattleState.UPDATE_TURN:
                    
                    // Who's turn is is with the current TurnMeter ? 
                    ChangeTurnFromTurnmeter();
                    /*if (isTurnOver)
                    {
                        Console.WriteLine("Changing Turn");
                        
                        isTurnOver = false;
                    }*/
                    break;
                case BattleState.PLAYER_TURN:

                    /*foreach (var item in _playerUnitTurnmeter)
                    {
                        if (PlayingUnitID == item.Key)
                        {
                            Retenue = " (-" + TurnMeterMaxValue + ")";
                        }
                        else
                        {
                            Retenue = "";
                        }
                        message += " | Item : " + item.Key + " Value : " + item.Value + Retenue;
                    };
                    foreach (var item in _enemyUnitTurnmeter)
                    {
                        massage += " | Item : " + item.Key + " Value : " + item.Value + Retenue;
                    };*/

                    
                    if (isPlayerTurnOver) // Wait for Client Turn_Over Packet
                    {
                        isTurnUpdated = false; // Update Turn is Finished Waiting for new Update Turn
                        Console.WriteLine("Update Player TurnMeter " + PlayingUnitID);
                        // Update All TurnMeters
                        UpdateAllTurnmeter(PlayingUnitID, BattleState.PLAYER_TURN);
                        isPlayerTurnOver = false;
                    }


                    break;
                case BattleState.IA_TURN:

                    /*foreach (var item in _playerUnitTurnmeter)
                    {
                        message += " | Item : " + item.Key + " Value : " + item.Value + Retenue;
                    };
                    foreach (var item in _enemyUnitTurnmeter)
                        {
                            if(PlayingUnitID == item.Key)
                            {
                                 Retenue = " (-"+ TurnMeterMaxValue + ")";
                            }
                            else
                            {
                                Retenue = "";
                            }

                            massage += " | Item : " + item.Key  + " Value : " + item.Value + Retenue;
                        };*/
                    //Console.WriteLine("Enemy Turn Unit ID : " + PlayingUnitID + massage + " | Player Units : "+ message);

                    
                    if (!isIAAttacking)
                    {
                        isTurnUpdated = false; // Update Turn is Finished Waiting for new Update Turn
                        Console.WriteLine("IA Turn " + PlayingUnitID);
                        EnemyTurn();
                        isIAAttacking = true;
                    }

                    if (isIATurnOver) // Wait for Client Turn_Over Packet
                    {                        
                        // Update All TurnMeters 
                        UpdateAllTurnmeter(PlayingUnitID, BattleState.IA_TURN);
                        isIATurnOver = false;
                        isIAAttacking = false;
                    }
                    
                                           
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

        public void isTurnOverPacketRecieved(string _myUpdateTurn, int EndingUnitIDTurn)
        { 
           //return FindSpellFromSpellListByID(_playerCollection[PlayingUnitID].spellList,_SpellID);
           if(_myUpdateTurn == "IA_TURN_OVER")
            {
                Console.WriteLine("Requested IA Spell OVER on Client Side | Set isIATurnOver TRUE :" + EndingUnitIDTurn);
                isIATurnOver =  true;
            }
            else
            {
                isIATurnOver =  false;
            }
            if (_myUpdateTurn == "PLAYER_TURN_OVER")
            {
                Console.WriteLine("Requested Player Spell OVER on Client Side | Set isPlayerTurnOver TRUE :" + EndingUnitIDTurn);
                isPlayerTurnOver = true;
            }
            else
            {
                isPlayerTurnOver = false;
            }
            //return _myUpdateTurn;
        }

    private ISpell FindSpellFromSpellListByID(List<ISpell> SpellList, string SpellID)
        {

            ISpell result = SpellList.Find(
                delegate (ISpell sp)
                {
                    return sp.SpellID == SpellID;
                }
            );

            return result;
        }

        public void PlayerRequestUsingSpell(string _SpellID, int _UnitTarget, int _PlayingUnitNumber)
        {            
            ISpell Spell = FindSpellFromSpellListByID(_playerCollection[_PlayingUnitNumber].spellList, _SpellID);
            if (Spell != null)
            {               
                Spell.Play(_IACollection[_UnitTarget]);
                Console.WriteLine("Player Turn Spell Recieved " + _PlayingUnitNumber);
                ServerSend.AttackIAUnits(ClientID, PlayingUnitID, _UnitTarget, Spell.spellTarget, Spell.SpellID);
                //isTurnOver = true;
            }
        }


        private void EnemyTurn()
        {
            EnemyIA EnemyChoice = new EnemyIA(_playerCollection, _IACollection, PlayingUnitID);
            // Analyse Situation
            EnemyChoice.UnitsAnalyse();

            // Choose a Spell Memorise it as "BestSpell"
            EnemyChoice.CheckUnitSpells(_IACollection[PlayingUnitID].spellList);

            //Find Player Target
            var Target = EnemyChoice.FindUnitTarget();

            //Define the Spell Target Zone
            var TargetZone = EnemyChoice.FindUnitTargetZone(Target, EnemyChoice.BestSpell.spellTarget);

            // Do Action
            EnemyChoice.UseSpell(TargetZone);

            // Send to Client the IA Attack
            ServerSend.AttackPlayersUnits(ClientID, EnemyChoice.PlayingUnitID, EnemyChoice.IATargetUnitID, EnemyChoice.BestSpell.spellTarget, EnemyChoice.BestSpell.SpellID);
        }      

        /// <summary>Set _enemyCollection for the Scene Fight from definied Scene Units</summary>
        /// <param name="_clientId">The Client Id, trace client for the Logs</param>
        /// UPDATED 01/07/2020
        public void InstantiateEnemyUnitsCrew(int _clientId, Scene currentScene)
        {

            //NeokyCollection_EnemyCrew = new Dictionary<int, NeokyCollection>();
            _IACollection = new Dictionary<int, Unit>();

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
                                    _IACollection.Add(1, l_UnitID);
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
                                    _IACollection.Add(2, l_UnitID);
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
                                    _IACollection.Add(3, l_UnitID);
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
                                    _IACollection.Add(4, l_UnitID);
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
                                    _IACollection.Add(5, l_UnitID);
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
                                    _IACollection.Add(6, l_UnitID);
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
            ServerSend.SpawnEnemyAllUnitsCrew(_clientId, _IACollection.Count, _IACollection);
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
            if(_playerUnitTurnmeter.First().Value >= TurnMeterMaxValue  || _IAUnitTurnmeter.First().Value >= TurnMeterMaxValue)
            {
                if (!isTurnUpdated) // Update the Turn Only Once, so it dont send the Player Turn Twice
                {
                    // If turnMeter Enemy = TurnMeter Player, Random Pick Next player
                    if (_playerUnitTurnmeter.First().Value == _IAUnitTurnmeter.First().Value)
                    {
                        var n = random.Next(0, 2);

                        //Set the NextPlayer 
                        //Set NextUnitID

                        if (n == 0)
                        {
                            Console.WriteLine("Random Player Turn Set");
                            SetPlayerTurn();
                            isTurnUpdated = true;
                        }
                        else
                        {
                            Console.WriteLine("Random IA Turn Set");
                            SetEnemyTurn();
                            isTurnUpdated = true;
                        }
                    }

                    if (_playerUnitTurnmeter.First().Value > _IAUnitTurnmeter.First().Value)
                    {
                        Console.WriteLine("ChangeTurn Player Turn Set");
                        SetPlayerTurn();
                        isTurnUpdated = true;
                    }
                    if (_playerUnitTurnmeter.First().Value < _IAUnitTurnmeter.First().Value)
                    {
                        Console.WriteLine("ChangeTurn IA Turn Set");
                        SetEnemyTurn();
                        isTurnUpdated = true;
                    }
                }
            }
            else
            {
                Console.WriteLine("Update Turn");
                UpdateAllTurnmeterEmptyTurn();
            }
        }

        private void SetEnemyTurn()
        {
            PlayingUnitID = _IAUnitTurnmeter.First().Key;
            currentBattleState = BattleState.IA_TURN;            
        }

        private void SetPlayerTurn()
        {            
            PlayingUnitID = _playerUnitTurnmeter.First().Key;
            // Send to Player the Unit to Play
            ServerSend.NewPlayerUnitTurn(ClientID, PlayingUnitID);
            currentBattleState = BattleState.PLAYER_TURN;
        }

        private void UpdateAllTurnmeter(int _PlayingUnitID, BattleState _battleState)
        {
            UpdatePlayerTurnMeter(_PlayingUnitID, _battleState);
            UpdateEnemyTurnMeter(_PlayingUnitID, _battleState);
        }
        private void UpdateAllTurnmeterEmptyTurn()
        {
            foreach (var UnitPlayerCollection in _playerCollection)
            {
                //_playerCollection[UnitPlayerCollection.Key].turnMeter += _playerCollection[UnitPlayerCollection.Key].UnitVelocity;
                _playerUnitTurnmeter[UnitPlayerCollection.Key] += (_playerCollection[UnitPlayerCollection.Key].UnitVelocity / ((float)_IAUnitTurnmeter.Count + (float)_playerUnitTurnmeter.Count));                
            }
            //Sort my new _playerUnitTurnmeter by Hiest Turnmeter
            _playerUnitTurnmeter = GetUnitsSortedByTurnMeter(_playerUnitTurnmeter);

            foreach (var UnitEnemyCollection in _IACollection)
            {
                //_enemyUnitTurnmeter[UnitEnemyCollection.Key].turnMeter += _enemyCollection[UnitEnemyCollection.Key].UnitVelocity;
                _IAUnitTurnmeter[UnitEnemyCollection.Key] += (_IACollection[UnitEnemyCollection.Key].UnitVelocity / ((float)_IAUnitTurnmeter.Count + (float)_playerUnitTurnmeter.Count));                
            }
            //Sort my new _enemyUnitTurnmeter by Hiest Turnmeter
            _IAUnitTurnmeter = GetUnitsSortedByTurnMeter(_IAUnitTurnmeter);
        }
        private void InitUpdateAllTurnmeter()
        {
            _playerUnitTurnmeter = new Dictionary<int, float>();
            _IAUnitTurnmeter = new Dictionary<int, float>();

            // Update _playerUnitTurnmeter from PlayerCollection
            foreach (var UnitPlayerCollection in _playerCollection)
            {
                _playerUnitTurnmeter[UnitPlayerCollection.Key] = _playerCollection[UnitPlayerCollection.Key].UnitVelocity;
            }
            _playerUnitTurnmeter = GetUnitsSortedByTurnMeter(_playerUnitTurnmeter);
            // Update _enemyUnitTurnmeter from EnemyCollection
            foreach (var UnitEnemyCollection in _IACollection)
            {
                _IAUnitTurnmeter[UnitEnemyCollection.Key] = _IACollection[UnitEnemyCollection.Key].UnitVelocity;
            }
            _IAUnitTurnmeter = GetUnitsSortedByTurnMeter(_IAUnitTurnmeter);            
        }

        /// <summary>Update Player TurnMeter and Sort It</int></summary>  
        /// UPDATED 01/07/2020
        private void UpdatePlayerTurnMeter(int _PlayingUnitID, BattleState _battleState)
        {
            foreach (var UnitPlayerCollection in _playerCollection)
            {
                //_playerCollection[UnitPlayerCollection.Key].turnMeter += _playerCollection[UnitPlayerCollection.Key].UnitVelocity;
                _playerUnitTurnmeter[UnitPlayerCollection.Key] += (_playerCollection[UnitPlayerCollection.Key].UnitVelocity / ((float)_IAUnitTurnmeter.Count + (float)_playerUnitTurnmeter.Count));
             
                if (_battleState.Equals(BattleState.PLAYER_TURN))
                {
                    // If it's a player Turn & the Unit is the Current Playing Unit
                    if (UnitPlayerCollection.Key.Equals(_PlayingUnitID))
                    {
                        // Decrease the turnMeter by the turnMeterMaxValues.
                        //_playerCollection[UnitPlayerCollection.Key].turnMeter -= TurnMeterMaxValue;
                        _playerUnitTurnmeter[UnitPlayerCollection.Key] -= TurnMeterMaxValue;                        
                    }                    
                    currentBattleState = BattleState.UPDATE_TURN;
                }
            }
            //Sort my new _playerUnitTurnmeter by Hiest Turnmeter
            _playerUnitTurnmeter = GetUnitsSortedByTurnMeter(_playerUnitTurnmeter);
        }

        /// <summary>Update Enemy TurnMeter and Sort It</int></summary>  
        /// UPDATED 01/07/2020
        private void UpdateEnemyTurnMeter(int _PlayingUnitID, BattleState _battleState)
        {
            foreach (var UnitEnemyCollection in _IACollection)
            {
                //_enemyUnitTurnmeter[UnitEnemyCollection.Key].turnMeter += _enemyCollection[UnitEnemyCollection.Key].UnitVelocity;
                _IAUnitTurnmeter[UnitEnemyCollection.Key] += (_IACollection[UnitEnemyCollection.Key].UnitVelocity / ((float)_IAUnitTurnmeter.Count + (float)_playerUnitTurnmeter.Count));

                if (_battleState.Equals(BattleState.IA_TURN))
                {
                    // If it's a enemy Turn & the Unit is the Current Playing Unit
                    if (UnitEnemyCollection.Key.Equals(_PlayingUnitID))
                    {
                        // Decrease the turnMeter by the turnMeterMaxValues.
                        //_playerCollection[UnitEnemyCollection.Key].turnMeter -= TurnMeterMaxValue;
                        _IAUnitTurnmeter[UnitEnemyCollection.Key] -= TurnMeterMaxValue;                        
                    }                    
                    currentBattleState = BattleState.UPDATE_TURN;
                }
            }
            //Sort my new _enemyUnitTurnmeter by Hiest Turnmeter
            _IAUnitTurnmeter = GetUnitsSortedByTurnMeter(_IAUnitTurnmeter);
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
