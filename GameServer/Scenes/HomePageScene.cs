using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameServer.Scenes
{
    public class HomePageScene : Scene
    {
        // If you create a New Class Scene dont forget to add it to SceneManager
        public HomePageScene()
        {            
            sceneName = "HomePage";     
            this.oldScenes = new List<string>();
            oldScenes.Add("Authentication");            
            oldScenes.Add("CollectionPage");            
        }
    }
}
