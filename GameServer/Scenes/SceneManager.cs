using GameServer.Scenes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameServer.Scenes
{
    public static class SceneManager 
    {
        public static List<Scene> sceneHandlers;

        // Return the Scene if finded, or Return Null
        public static Scene FindSceneByName(string _desiredScene)
        {
            foreach (Scene _scene in sceneHandlers)
            {
                if (_scene.sceneName == _desiredScene)
                {
                    return _scene;
                }
            }
            return null;
        }

        // Initialize all the Scenes
        public static void InitializeSceneData()
        {        
            sceneHandlers = new List<Scene>()
            {
                { new HomePageScene() },
                { new CollectionPageScene() },
                { new NewScene() },
            };
            Console.WriteLine("Initialized Scenes.");
        }
    }
}
