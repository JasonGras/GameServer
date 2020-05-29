using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Scenes
{
    class CollectionPageScene : Scene
    {
        // If you create a New Class Scene dont forget to add it to SceneManager

        public CollectionPageScene()
        {
            this.sceneName = "CollectionPage";
            this.oldScenes = new List<string>();
            this.oldScenes.Add("HomePage");
        }
    }
}

