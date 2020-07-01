using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Units.Viking
{
    class BlueViking : Unit
    {
        public BlueViking()
        {            
            this.UnitName = "BlueViking";
            this.UnitQuality = 2;
            this.UnitPower = 30;
            this.UnitMaxHp = 210;
            this.UnitHp = UnitMaxHp;            
            this.UnitLevel = 1;
            this.UnitID = "blue_viking_id";
            this.UnitVelocity = 405;
            this.UnitImage = "blue_viking_img";
            this.UnitPrefab = "blue_viking_01";
            this.UnitTribe = "Viking";
        }

    }
}




        
