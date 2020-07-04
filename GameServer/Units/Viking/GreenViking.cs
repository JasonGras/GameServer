using GameServer.Spells;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Spells.ISpellEffect;

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
            this.UnitVelocity = 200;
            this.UnitImage = "green_viking_img";
            this.UnitPrefab = "green_viking_01";
            this.UnitTribe = "Viking";

            BasicAttack UnitBasicAttack = new BasicAttack(
               "green_viking_basic_attack_id",
               "green_viking_basic_attack_name",
               "green_viking_basic_attack_img",
               1,
               UnitPower,
               1,
               SpellType.OFFENSE,
               SpellTarget.ENEMY,
               SpellTargetZone.ONE_UNIT,
               SpellTargetProperty.UNIT_HP);

            this.spellList = new List<ISpell>()
            {
                UnitBasicAttack,
            };
        }

    }
}




        
