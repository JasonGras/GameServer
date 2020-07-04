using GameServer.Spells;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Spells.ISpellEffect;

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
            this.UnitVelocity = 151;
            this.UnitImage = "grey_brother_viking_img";
            this.UnitPrefab = "grey_brother_viking_01";
            this.UnitTribe = "Viking";

            BasicAttack UnitBasicAttack = new BasicAttack(
               "grey_brother_viking_basic_attack_id",
               "grey_brother_viking_basic_attack_name",
               "grey_brother_viking_basic_attack_img",
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




        
