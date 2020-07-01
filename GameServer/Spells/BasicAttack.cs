using GameServer.Units;
using System;
using System.Collections.Generic;
using System.Text;
using static GameServer.Spells.ISpellEffect;

namespace GameServer.Spells
{
    public class BasicAttack : ISpell
    {
        /* Spell */
        public string SpellID { get ; set ; }
        public string SpellName { get ; set ; }
        public float SpellRecastTime { get ; set ; }

        /* SpellEffect */
        public float SpellAmount { get ; set ; }
        public float SpellEffectLast { get ; set ; }
        public SpellType spellType { get ; set ; }
        public SpellTarget spellTarget { get ; set ; }
        public SpellTargetZone spellTargetZone { get ; set ; }
        public SpellTargetProperty spellTargetProperty { get ; set ; }

        public BasicAttack(string _SpellID, string _SpellName, float _SpellRecastTime, float _SpellAmount , float _SpellEffectLast, SpellType _spellType, SpellTarget _spellTarget, SpellTargetZone _spellTargetZone, SpellTargetProperty _spellTargetProperty)
        {
            SpellID = _SpellID;
            SpellName = _SpellName;
            SpellRecastTime = _SpellRecastTime;
            SpellAmount = _SpellAmount;
            SpellEffectLast = _SpellEffectLast;
            spellType = _spellType;
            spellTarget = _spellTarget;
            spellTargetZone = _spellTargetZone;
            spellTargetProperty = _spellTargetProperty;
        }

        public void Play(Dictionary<int, Unit> _playerCollection, Dictionary<int, Unit> _enemyCollection)
        {
            switch (spellTargetZone)
            {
                case SpellTargetZone.ONE_UNIT:
                    // Focus the Target
                    break;
                case SpellTargetZone.ALL_UNITS:
                    // Hit All targets
                    break;
                case SpellTargetZone.LINE_UNITS:
                    // Hit Line of Target
                    break;
                case SpellTargetZone.COLUMN_UNITS:
                    // Hit Target Behind or In Front of Target
                    break;
                case SpellTargetZone.ZONE_UNITS:
                    // Hit +1 Side Targets
                    break;
            }
        }

    }
}
