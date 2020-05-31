using System;
using System.Collections.Generic;
using System.Text;

using GameServer.Scenes;

namespace GameServer.Dungeon
{
    public class Dungeon
    {
        public string instanceName { get; set; }
        public Scene spawnScene { get; set; }
        public List<Scene> dungeonMaps { get; set; }
        public float minLvlAccess { get;  set; }

        public bool playerCanAccessDungeon(float playerLvl)
        {
            if (playerLvl >= minLvlAccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Check if a player is allowed to Spawn in the Dungeon Map.
        public bool playerCanAccessDesiredDungeonMap(Dungeon desiredDungeon, string currentScene)
        {
            // Search if the desiredMap is a Scene beeing part of dungeonMaps
            foreach (Scene _scene in desiredDungeon.dungeonMaps)
            {
                // if the the Desired Spawn Name is part of dungeonMaps
                if (desiredDungeon.spawnScene.sceneName == _scene.sceneName)
                {
                    // Search in oldScenes of the desiredDungeon if the currentScene is a Scene Allowed to Access to the desired Map
                    foreach (string _oldScene in _scene.oldScenes)
                    {
                        //If My current Scene is a Scene Allowed to Access to the desired Map
                        if (currentScene == _oldScene)
                        {
                            //Player is allowed to Spawn in this Dungeon Map
                            return true;
                        }
                    }
                }
            }
            //Player is not allowed to Spawn in this Dungeon Map
            return false;
        }

    }
}
