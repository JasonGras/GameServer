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

            // List of Scenes wich user can come from to claim access HomePage
            this.oldScenes = new List<string>();
            oldScenes.Add(Constants.SCENE_AUTHENTICATION);            
            oldScenes.Add(Constants.SCENE_COLLECTION);
            oldScenes.Add(Constants.SCENE_LOOTS);
            oldScenes.Add("DJ_01_02");            
        }
    }
}
