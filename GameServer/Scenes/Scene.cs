using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Scenes
{
    public class Scene
    {
        public string sceneName { get; set; }

        // List of Scenes wich user can come from to claim access HomePage
        public List<string> oldScenes { get; set; }

        public Dictionary<int, NeokyCollection> enemyCrewMember { get; set; }

    }


}
