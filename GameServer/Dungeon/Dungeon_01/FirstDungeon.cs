using System;
using System.Collections.Generic;
using System.Text;

using GameServer.Scenes;
using GameServer.Dungeon;
using GameServer.Units;

namespace GameServer.Dungeon.Dungeon_01
{
    class FirstDungeon : Dungeon
    {
       public Dictionary<string, Dictionary<int, Unit>> dungeonWaves { get; set; }
        

        public FirstDungeon()
        {
            // Set an Instance Name
            this.instanceName = "instance_01_name";
            this.dungeonWaves = new Dictionary<string, Dictionary<int, Unit>>();
            // This First Dungeon is Accessible from lvl 5.
            this.minLvlAccess = 1;

            dungeonMaps = new List<Scene>();

            // First Scene
            Scene DJ_01_01 = new Scene();

            // I want to Spwan on the Scene DJ_01_01
            this.spawnScene = DJ_01_01;


            DJ_01_01.sceneName = "DJ_01_01";
            DJ_01_01.enemyUnitCrewMember = new Dictionary<int, Unit>();
            DJ_01_01.enemyUnitCrewMember.Add(1, new Unit { UnitID = "grey_viking_id" });
            DJ_01_01.enemyUnitCrewMember.Add(2, new Unit { UnitID = "grey_brother_viking_id" });
            DJ_01_01.enemyUnitCrewMember.Add(3, new Unit { UnitID = "green_viking_id" });
            DJ_01_01.enemyUnitCrewMember.Add(4, new Unit { UnitID = "blue_viking_id" });
            DJ_01_01.enemyUnitCrewMember.Add(5, new Unit { UnitID = "purple_viking_id" });
            DJ_01_01.enemyUnitCrewMember.Add(6, new Unit { UnitID = "gold_viking_id" });
            dungeonWaves.Add(DJ_01_01.sceneName, DJ_01_01.enemyUnitCrewMember);
            DJ_01_01.oldScenes = new List<string>();
            DJ_01_01.oldScenes.Add("HomePage"); // Seems to be accessible from HomePage
            this.dungeonMaps.Add(DJ_01_01);

            Scene DJ_01_02 = new Scene();
            DJ_01_02.sceneName = "DJ_01_02";
            DJ_01_02.enemyUnitCrewMember = new Dictionary<int, Unit>();
            DJ_01_02.enemyUnitCrewMember.Add(1, new Unit { UnitID = "grey_viking_id" });
            DJ_01_02.enemyUnitCrewMember.Add(2, new Unit { UnitID = "grey_viking_id" });
            dungeonWaves.Add(DJ_01_02.sceneName, DJ_01_02.enemyUnitCrewMember);
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
