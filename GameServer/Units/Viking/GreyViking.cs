using Amazon.Runtime.Internal;
using GameServer.Spells;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Spells.ISpellEffect;

namespace GameServer.Units.Viking
{
    public class GreyViking : Unit
    {
        public GreyViking()
        {
            this.UnitName = "GreyViking";
            this.UnitQuality = 0;
            this.UnitPower = 20;
            this.UnitMaxHp = 200;
            this.UnitHp = UnitMaxHp;
            this.UnitLevel = 1;
            this.UnitID = "grey_viking_id";
            this.UnitVelocity = 150;
            this.UnitImage = "grey_viking_img";
            this.UnitPrefab = "grey_viking_01";
            this.UnitTribe = "Viking";

            BasicAttack UnitBasicAttack = new BasicAttack(
               "grey_viking_basic_attack_id",
               "grey_viking_basic_attack_name",
               "grey_viking_basic_attack_img",
               1,
               UnitPower,
               1,
               SpellType.OFFENSE,
               SpellTarget.ENEMY,
               SpellTargetZone.ONE_UNIT,
               SpellTargetProperty.UNIT_HP);

            Heal UnitHeal = new Heal(
               "grey_viking_heal_id",
               "grey_viking_heal_name",
               "grey_viking_heal_img",
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
            };


            //st<ISpell> Spell;
            //Spell.Add()

            /**public ISpell(string _SpellID, string _SpellName, float _SpellRecastTime, List<SpellEffect> _SpellEffect)
            {
                SpellID = _SpellID;
                SpellName = _SpellName;
                SpellRecastTime = _SpellRecastTime;
                spellEffects = _SpellEffect;
            }*/

            /*this.spellList = new List<Spell>()
            {
                new Spell("AutoAttack_ID","Auto Attack",1,new List<SpellEffect>()
                {
                    new SpellEffect(10f,1,SpellType.OFFENSE,SpellTarget.ENEMY,SpellTargetZone.ONE_UNIT,SpellTargetProperty.UNIT_HP),
                    new SpellEffect(0.1f,1,SpellType.OFFENSE,SpellTarget.ENEMY,SpellTargetZone.LINE_UNITS,SpellTargetProperty.UNIT_HP),
                }),
                new Spell("Heal_ID","Heal",3,new List<SpellEffect>()
                {
                    new SpellEffect(10f,1,SpellType.BUFF,SpellTarget.SELF,SpellTargetZone.ONE_UNIT,SpellTargetProperty.UNIT_HP),
                })
            };*/



        }

        
    }
}




        
