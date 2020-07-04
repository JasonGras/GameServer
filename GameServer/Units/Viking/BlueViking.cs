using GameServer.Spells;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Spells.ISpellEffect;

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
            this.UnitVelocity = 250;
            this.UnitImage = "blue_viking_img";
            this.UnitPrefab = "blue_viking_01";
            this.UnitTribe = "Viking";

            BasicAttack UnitBasicAttack = new BasicAttack(
               "blue_viking_basic_attack_id",
               "blue_viking_basic_attack_name",
               "blue_viking_basic_attack_img",
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




        
