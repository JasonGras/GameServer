using System;
using System.Collections.Generic;
using System.Text;

using GameServer.Scenes;
using GameServer.Dungeon;

namespace GameServer.Dungeon.Dungeon_01
{
    class FirstDungeon : Dungeon
    {
       public Dictionary<string, Dictionary<int, NeokyCollection>> dungeonWaves { get; set; }
        

        public FirstDungeon()
        {
            // Set an Instance Name
            this.instanceName = "instance_01_name";
            this.dungeonWaves = new Dictionary<string, Dictionary<int, NeokyCollection>>();
            // This First Dungeon is Accessible from lvl 5.
            this.minLvlAccess = 1;

            dungeonMaps = new List<Scene>();

            // First Scene
            Scene DJ_01_01 = new Scene();

            // I want to Spwan on the Scene DJ_01_01
            this.spawnScene = DJ_01_01;


            DJ_01_01.sceneName = "DJ_01_01";
            DJ_01_01.enemyCrewMember = new Dictionary<int, NeokyCollection>();
            DJ_01_01.enemyCrewMember.Add(1, new NeokyCollection { collection_id = "sphere_id", collection_name = "Sphere", attackDamages = 5, attackSpeed = 20, collection_prefab = "sphere_prefab", lifePoints = 500 });
            DJ_01_01.enemyCrewMember.Add(2, new NeokyCollection { collection_id = "sphere_id", collection_name = "Sphere", attackDamages = 5, attackSpeed = 20, collection_prefab = "sphere_prefab", lifePoints = 500 });
            DJ_01_01.enemyCrewMember.Add(3, new NeokyCollection { collection_id = "sphere_id", collection_name = "Sphere", attackDamages = 5, attackSpeed = 20, collection_prefab = "sphere_prefab", lifePoints = 500 });
            DJ_01_01.enemyCrewMember.Add(4, new NeokyCollection { collection_id = "sphere_id", collection_name = "Sphere", attackDamages = 5, attackSpeed = 20, collection_prefab = "sphere_prefab", lifePoints = 500 });
            DJ_01_01.enemyCrewMember.Add(5, new NeokyCollection { collection_id = "sphere_id", collection_name = "Sphere", attackDamages = 5, attackSpeed = 20, collection_prefab = "sphere_prefab", lifePoints = 500 });
            dungeonWaves.Add(DJ_01_01.sceneName, DJ_01_01.enemyCrewMember);
            DJ_01_01.oldScenes = new List<string>();
            DJ_01_01.oldScenes.Add("HomePage"); // Seems to be accessible from HomePage
            this.dungeonMaps.Add(DJ_01_01);

            Scene DJ_01_02 = new Scene();
            DJ_01_02.sceneName = "DJ_01_02";
            DJ_01_02.enemyCrewMember = new Dictionary<int, NeokyCollection>();
            DJ_01_02.enemyCrewMember.Add(1, new NeokyCollection { collection_id = "sphere_id", collection_name = "Sphere", attackDamages = 5, attackSpeed = 20, collection_prefab = "sphere_prefab", lifePoints = 500 });
            DJ_01_02.enemyCrewMember.Add(2, new NeokyCollection { collection_id = "sphere_id", collection_name = "Sphere", attackDamages = 5, attackSpeed = 20, collection_prefab = "sphere_prefab", lifePoints = 500 });
            dungeonWaves.Add(DJ_01_02.sceneName, DJ_01_02.enemyCrewMember);
            DJ_01_02.oldScenes = new List<string>();
            DJ_01_02.oldScenes.Add("DJ_01_01"); // Seems Only Accessible from DJ_01_01
            this.dungeonMaps.Add(DJ_01_02);

            /*
            Scene DJ_01_03 = new Scene();
            DJ_01_03.sceneName = "DJ_01_03";

            DJ_01_03.oldScenes = new List<string>();
            DJ_01_03.oldScenes.Add("DJ_01_02");// Seems Only Accessible from DJ_01_02
            this.dungeonMaps.Add(DJ_01_03);*/
        }
    }
}
