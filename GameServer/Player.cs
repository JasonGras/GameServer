using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

using GameServer.Dungeon;

using Amazon.DynamoDBv2.DataModel;

using GameServer.Scenes;
using NLog.Common;
using NLog;
using System.Linq;
using NLog.Targets;
using GameServer.Units;
using GameServer.TurnBasedFights;

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

        public Dictionary<string, int> coin { get; set; }

        public Scene currentScene;
        public Scene unloadScene;

        // SetFight Variables
        public Dictionary<int, Unit> Unit_PlayerCrew; // Crew defined by the player for the Fights
        public Dictionary<int, Unit> Unit_EnemyCrew; // Crew defined by the player for the Fights

        public NewFight FightManager;

        //public Dictionary<int, NeokyCollection> NeokyCollection_PlayerCrew; // Crew defined by the player for the Fights
        //public Dictionary<int, NeokyCollection> NeokyCollection_EnemyCrew; // Crew defined by the Dungeon for the Scene Fight

        public List<Unit> sortedUnits;

        public bool isInGame = false;

        public enum BattleState { INIT_FIGHT, PLAYER_TURN, ENEMY_TURN, WON, LOST }

        public BattleState currentBattleState;

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
        
        /*/// <summary>Processes player inputs on Fights.</summary>
        public void Update()
        {

        }*/
        
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
            FightManager = new NewFight();
            isInGame = true;

            FightManager.InstantiateEnemyUnitsCrew(_clientId, currentScene);
            FightManager.InstantiatePlayerUnitsCrew(_clientId, PlayerCrew);    
            
        }       

        public void UnitPlayerAttack(int _clientId, int _unitPosition, int _unitTarget)
        {                    

            Unit _CurrentUnit = new Unit();
            Unit _CurrentTarget = new Unit();

            //Console.WriteLine("Unit Player Attack");
            if (currentBattleState == BattleState.PLAYER_TURN)
            {                
                if (Unit_PlayerCrew.TryGetValue(_unitPosition, out _CurrentUnit))
                {
                    if (Unit_EnemyCrew.TryGetValue(_unitTarget, out _CurrentTarget))
                    {
                        if (_CurrentTarget.isSpawned && _CurrentUnit.isSpawned)
                        {                           
                            // Target & Player Unit are Spawned
                            if (_CurrentTarget.UnitHp > 0 && _CurrentUnit.UnitHp > 0)
                            {
                                Console.WriteLine(_CurrentUnit.UnitName + " deal " + _CurrentUnit.UnitPower.ToString() + " to the Enemy unit " + _CurrentTarget.UnitName);
                                ServerSend.AttackUnit(_clientId, _unitPosition, _unitTarget);
                                // Target & Player Unit are Alive

                                _CurrentTarget.UnitHp -= _CurrentUnit.UnitPower;
                                if (_CurrentTarget.UnitHp <= 0)
                                {
                                    //TargetDied
                                    // Check If Win
                                    foreach (var Unit in Unit_EnemyCrew)
                                    {
                                        if (Unit.Value.UnitHp > 0)
                                        {

                                        }
                                        else
                                        {
                                            currentBattleState = BattleState.WON;
                                        }
                                    }
                                }
                                else
                                {
                                    currentBattleState = BattleState.ENEMY_TURN;
                                    // EndPlayerTurn
                                }

                            }
                        }
                    }
                }
                //sortedUnits = GetUnitsSortedByTurnMeter();
                // Update units Parameters (Hp, TurnMeter..)
            }



        }

    }
}
