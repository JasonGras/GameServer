using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

using Amazon.DynamoDBv2.DataModel;

using GameServer.Scenes;


namespace GameServer
{
    [DynamoDBTable("clients")]
    public class Player : IEquatable<Player>
    {
        /*public int id;
        public string username;
        public float level;
        public float levelxp;
        public float requiredLvlUpXp;*/


        
        public string client_id { get; set; }

        public string username { get; set; }

        public string email { get; set; }

        public string account_statut { get; set; }

        [DynamoDBHashKey]
        public string client_sub { get; set; }

        public float level { get; set; }

        public float level_xp { get; set; }

        public float required_levelup_xp { get; set; }

        public Scene currentScene;
        public Scene unloadScene;


        //public string currentScene;
        //public string oldScene;

        public Player()
        {
            currentScene = new HomePageScene();
            unloadScene = new AuthenticationScene();
            //oldScene = Constants.SCENE_NOSCENE;
            //currentScene = Constants.SCENE_HOMEPAGE;
            // Without i have an error :  System.InvalidOperationException: Type GameServer.Player is unsupported, it cannot be instantiated
        }

        public Player(string _clientId, string _username, string _email, string _accountStatut, string _clientSub, float _level, float _levelXp)//(int _id, string _username, float _level, float _levelxp, float _requiredLvlUpXp)
        {
            client_id = _clientId;
            username = _username;
            email = _email;
            account_statut = _accountStatut;
            client_sub = _clientSub;
            level = _level;
            level_xp = _levelXp;
            /*id = _id;
            username = _username;
            level = _level;
            levelxp = _levelxp;
            requiredLvlUpXp = _requiredLvlUpXp;*/
            //position = _spawnPosition;
            //rotation = Quaternion.Identity;
            currentScene = new HomePageScene();
            unloadScene = new AuthenticationScene();
            //ariane = 

            //inputs = new bool[4];
        }

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Player)obj);
        }

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

        /*public Player(int _id, string _username, Vector3 _spawnPosition, string _currentScene)
        {
            id = _id;
            username = _username;
            //position = _spawnPosition;
            //rotation = Quaternion.Identity;

            currentScene = _currentScene;

            //inputs = new bool[4];
        }*/

        /// <summary>Processes player input and moves the player.</summary>
        public void Update()
        {
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
        }

        /// <summary>Calculates the player's desired movement direction and moves him.</summary>
        /// <param name="_inputDirection"></param>
        /*private void Move(Vector2 _inputDirection)
        {
            Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

            Vector3 _moveDirection = _right * _inputDirection.X + _forward * _inputDirection.Y;
            position += _moveDirection * moveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        /// <summary>Updates the player input with newly received input.</summary>
        /// <param name="_inputs">The new key inputs.</param>
        /// <param name="_rotation">The new rotation.</param>
        public void SetInput(bool[] _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }*/
    }
}
