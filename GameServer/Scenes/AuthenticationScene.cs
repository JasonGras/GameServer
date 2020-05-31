using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Scenes
{
    class AuthenticationScene : Scene
    {
        // If you create a New Class Scene dont forget to add it to SceneManager
        public AuthenticationScene()
        {
            this.sceneName = "Authentication";

            // List of Scenes wich user can come from to claim access HomePage
            this.oldScenes = new List<string>();
            this.oldScenes.Add("NOSCENE");
        }
    }
}
