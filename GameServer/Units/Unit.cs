using Amazon.DynamoDBv2.DataModel;
using GameServer.Loots;
using GameServer.Spells;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Units
{
    //[DynamoDBTable("CollectionUnits")]
    public class Unit
    {        
        //[DynamoDBHashKey]
        public string UnitID { get; set; }
        public string UnitName { get; set; }        
        public string UnitTribe { get; set; }        
        public float UnitLevel { get; set; }
        public int UnitQuality { get; set; }
        public float UnitPower { get; set; }
        public float UnitHp { get; set; }
        public float UnitMaxHp { get; set; }
        public float UnitVelocity { get; set; }
        public bool isSpawned { get; set; } = false;
        
        public float turnMeter { get; set; } = 0; // Used on Init of Fights to Sort 
        
        public List<ISpell> spellList { get; set; }

        // 
        //public string DBUnitID { get; set; }
        public string UnitImage { get; set; }
        public string UnitPrefab { get; set; }
    }

    public class DBUnit
    {
        [DynamoDBHashKey]
        public string DBUnitID { get; set; }
        public string UnitImage { get; set; }
        public string UnitPrefab { get; set; }
    }
}
