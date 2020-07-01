using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Units.Viking
{
    class GreyBrotherViking : Unit
    {
        public GreyBrotherViking()
        {
            this.UnitName = "GreyBrotherViking";
            this.UnitQuality = 0;
            this.UnitPower = 21;
            this.UnitMaxHp = 201;
            this.UnitHp = UnitMaxHp;
            this.UnitLevel = 1;
            this.UnitID = "grey_brother_viking_id";
            this.UnitVelocity = 401;
            this.UnitImage = "grey_brother_viking_img";
            this.UnitPrefab = "grey_brother_viking_01";
            this.UnitTribe = "Viking";

        }

    }
}




        
