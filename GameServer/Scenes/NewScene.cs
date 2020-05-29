using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Scenes
{
    class NewScene : Scene
    {
        // If you create a New Class Scene dont forget to add it to SceneManager
        public NewScene()
        {
            this.sceneName = "NewScene";
            this.oldScenes = new List<string>();
            this.oldScenes.Add("Authentication");
            this.oldScenes.Add("Blabla");
        }
    }
}
