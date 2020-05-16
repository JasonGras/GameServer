using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        public int id;
        public string username;
        //public string ariane;

        //public Vector3 position;
        //public Quaternion rotation;

        //private float moveSpeed = 5f / Constants.TICKS_PER_SEC;
        //private bool[] inputs;
        
        public string currentScene;
        public string oldScene;

        public Player(int _id)
        {
            id = _id;
            //username = _username;
            //position = _spawnPosition;
            //rotation = Quaternion.Identity;
            oldScene = Constants.SCENE_AUTHENTICATION;
            currentScene = Constants.SCENE_HOMEPAGE;
            //ariane = 

            //inputs = new bool[4];
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
