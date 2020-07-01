using GameServer.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Spells
{
    
    public interface ISpell : ISpellEffect
    {
        public string SpellID { get; set; }
        public string SpellName { get; set; }
        public float SpellRecastTime { get; set; } // 1 = Every Turn | 5 = Every 5 turns    

        void Play(Dictionary<int, Unit> _PlayerUnits, Dictionary<int, Unit> _EnemyUnits);        
        
        //public List<ISpellEffect> spellEffects { get; set; }

        /*public Spell(string _SpellID, string _SpellName, float _SpellRecastTime, List<SpellEffect> _SpellEffect)
        {
            SpellID = _SpellID;
            SpellName = _SpellName;
            SpellRecastTime = _SpellRecastTime;
            spellEffects = _SpellEffect;
        }*/
        //void Play();
    }

    public interface ISpellEffect
    {
        public float SpellAmount { get; set; } // +50 Damage, -500 Heal, 0.2 %     
        public float SpellEffectLast { get; set; } // 1 Turn = Burst | > 1 Turn = Damage Over Time // Heal Over Time .. 
        public SpellType spellType { get; set; }
        public SpellTarget spellTarget { get; set; }
        public SpellTargetZone spellTargetZone { get; set; }
        public SpellTargetProperty spellTargetProperty { get; set; }


        public enum SpellType
        {
            DEFENSE, // Shield, Armor
            OFFENSE, // Spell, Attack
            BUFF, // Heal, +Armor
            DEBUFF, // Remove, -Armor
            INCREMENTAL, //Turn 1 += 10 Damage , Turn 2 += 10 Damage ... 
            PASSIVE,
        }

        public enum SpellTarget
        {
            ALLY,
            ENEMY,
            SELF,
        }
        public enum SpellTargetZone
        {
            ONE_UNIT,
            ALL_UNITS,
            LINE_UNITS,
            COLUMN_UNITS,
            ZONE_UNITS,
        }
        public enum SpellTargetProperty
        {
            UNIT_HP,
            UNIT_MANA,
            UNIT_SHIELD,
            UNIT_DAMAGES,
            UNIT_VELOCITY,
        }

        /*public SpellEffect(float _SpellAmount, float _SpellEffectLast,SpellType _SpellType, SpellTarget _SpellTarget, SpellTargetZone _SpellTargetZone, SpellTargetProperty _SpellTargetProperty)
        {
            SpellAmount = _SpellAmount;
            SpellEffectLast = _SpellEffectLast;
            spellType = _SpellType;
            spellTarget = _SpellTarget;
            spellTargetZone = _SpellTargetZone;
            spellTargetProperty = _SpellTargetProperty;
        }*/

        
    }
}
