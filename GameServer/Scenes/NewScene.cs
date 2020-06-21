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
            this.sceneName = "SceneLoots";

            // List of Scenes wich user can come from to claim access HomePage
            this.oldScenes = new List<string>();
            this.oldScenes.Add(Constants.SCENE_HOMEPAGE);
            this.oldScenes.Add(Constants.SCENE_COLLECTION);
        }
    }
}
