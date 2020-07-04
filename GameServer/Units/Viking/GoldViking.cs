using GameServer.Spells;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Spells.ISpellEffect;

namespace GameServer.Units.Viking
{
    class GoldViking : Unit
    {
        public GoldViking()
        {
            this.UnitName = "GoldViking";
            this.UnitQuality = 4;
            this.UnitPower = 40;
            this.UnitMaxHp = 220;
            this.UnitHp = UnitMaxHp;
            this.UnitLevel = 1;
            this.UnitID = "gold_viking_id";
            this.UnitVelocity = 390;
            this.UnitImage = "gold_viking_img";
            this.UnitPrefab = "gold_viking_01";
            this.UnitTribe = "Viking";


            BasicAttack UnitBasicAttack = new BasicAttack(
               "gold_viking_basic_attack_id",
               "gold_viking_basic_attack_name",
               "gold_viking_basic_attack_img",
               1,
               UnitPower,
               1,
               SpellType.OFFENSE,
               SpellTarget.ENEMY,
               SpellTargetZone.ONE_UNIT,
               SpellTargetProperty.UNIT_HP);

            BasicAttack UnitMegaAttack = new BasicAttack(
               "gold_viking_mega_attack_id",
               "gold_viking_mega_attack_name",
               "gold_viking_mega_attack_img",
               1,
               UnitPower*2,
               1,
               SpellType.OFFENSE,
               SpellTarget.ENEMY,
               SpellTargetZone.ONE_UNIT,
               SpellTargetProperty.UNIT_HP);

            Passive UnitPassive = new Passive(
               "gold_viking_passive_id",
               "gold_viking_passive_name",
               "gold_viking_passive_img",
               1,
               0.2f,
               1,
               SpellType.PASSIVE,
               SpellTarget.ALLY,
               SpellTargetZone.ALL_UNITS,
               SpellTargetProperty.UNIT_VELOCITY);

            Heal UnitHeal = new Heal(
               "gold_viking_heal_id",
               "gold_viking_heal_name",
               "gold_viking_heal_img",
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
                UnitMegaAttack,
                UnitHeal,
                UnitPassive,
            };
        }
    }
}
