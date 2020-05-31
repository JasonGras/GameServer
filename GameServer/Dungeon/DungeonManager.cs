using System;
using System.Collections.Generic;
using System.Text;

using GameServer.Scenes;

namespace GameServer.Dungeon
{
    class DungeonManager
    {
        public static List<Dungeon> sceneHandlers;

        // Return the Scene if finded, or Return Null
        public static Dungeon FindDungeonByName(string _desiredDungeon)
        {
            foreach (Dungeon _dungeon in sceneHandlers)
            {
                if (_dungeon.instanceName == _desiredDungeon)
                {
                    return _dungeon;
                }
            }
            return null;
        }

        // Initialize all the Scenes
        public static void InitializeDungeonData()
        {
            sceneHandlers = new List<Dungeon>()
            {
                { new Dungeon_01.FirstDungeon() },
            };
            Console.WriteLine("Initialized Dungeons.");
        }

    }
}
