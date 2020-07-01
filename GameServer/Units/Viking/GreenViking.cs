using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Units.Viking
{
    class GreenViking : Unit
    {
        public GreenViking()
        {
            this.UnitName = "GreenViking";
            this.UnitQuality = 1;
            this.UnitPower = 25;
            this.UnitMaxHp = 205;
            this.UnitHp = UnitMaxHp;
            this.UnitLevel = 1;
            this.UnitID = "green_viking_id";
            this.UnitVelocity = 400;
            this.UnitImage = "green_viking_img";
            this.UnitPrefab = "green_viking_01";
            this.UnitTribe = "Viking";
        }

    }
}




        
