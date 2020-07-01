using GameServer.Spells;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Spells.ISpellEffect;

namespace GameServer.Units.Viking
{
    public class PurpleViking : Unit
    {
        public PurpleViking()
        {
            this.UnitName = "PurpleViking";
            this.UnitQuality = 3;
            this.UnitPower = 35;
            this.UnitMaxHp = 215;
            this.UnitHp = UnitMaxHp;
            this.UnitLevel = 1;
            this.UnitID = "purple_viking_id";
            this.UnitVelocity = 415;
            this.UnitImage = "purple_viking_img";
            this.UnitPrefab = "purple_viking_01";
            this.UnitTribe = "Viking";


            BasicAttack UnitBasicAttack = new BasicAttack(
               "purple_viking_basic_attack_id",
               "purple_viking_basic_attack_name",
               1,
               UnitPower,
               1,
               SpellType.OFFENSE,
               SpellTarget.ENEMY,
               SpellTargetZone.ONE_UNIT,
               SpellTargetProperty.UNIT_HP);

            Passive UnitPassive = new Passive(
               "purple_viking_passive_id",
               "purple_viking_passive_name",
               1,
               0.2f,
               1,
               SpellType.PASSIVE,
               SpellTarget.ALLY,
               SpellTargetZone.ALL_UNITS,
               SpellTargetProperty.UNIT_VELOCITY);

            Heal UnitHeal = new Heal(
               "purple_viking_heal_id",
               "purple_viking_heal_name",
               1,
               UnitPower,
               1,
               SpellType.BUFF,
               SpellTarget.ALLY,
               SpellTargetZone.ONE_UNIT,
               SpellTargetProperty.UNIT_HP);

            this.spellList = new List<ISpell>()
            {
                UnitBasicAttack,
                UnitHeal,
                UnitPassive,
            };
        }
    }
}




        
