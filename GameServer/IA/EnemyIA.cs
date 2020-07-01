using GameServer.Spells;
using GameServer.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.IA
{
    public class EnemyIA
    {
        public Dictionary<int, Unit> _playerCollection;
        public Dictionary<int, Unit> _enemyCollection;

        public Unit CurrentEnemyUnit;
        public int PlayingUnitID;

        public int BestPlayerUnitTarget = 1; // = The ID of The Weakest Player Unit to Focus Attack | Start to 1 coz ID of Units begins to 1
        public int BestEnemyUnitTarget = 1; // = The ID of The Weakest IA Unit Needing Help | Start to 1 coz ID of Units begins to 1

        public Queue<ISpellEffect.SpellType> spellTypeQueue;

        public ISpell BestSpell;

        public EnemyIA(Dictionary<int, Unit>  PlayerCollection, Dictionary<int, Unit>  EnemyCollection,int _PlayingUnitID)
        {
            _playerCollection = PlayerCollection;
            _enemyCollection = EnemyCollection;
            PlayingUnitID = _PlayingUnitID;
            CurrentEnemyUnit = _enemyCollection[_PlayingUnitID];
        }

        public void CheckUnitSpells(List<ISpell> _UnitSpells)
        {
            // Priorise Spell
            spellTypeQueue = new Queue<ISpellEffect.SpellType>();
            spellTypeQueue.Enqueue(ISpellEffect.SpellType.DEFENSE);
            spellTypeQueue.Enqueue(ISpellEffect.SpellType.OFFENSE);
            spellTypeQueue.Enqueue(ISpellEffect.SpellType.BUFF);

            // Find Best Spell       
            foreach (var SpellType in spellTypeQueue)
            {
                ISpell NewSpell = FindSpellType(_UnitSpells, SpellType); // Null if no spell Found
                if (NewSpell != null)
                {
                    if(BestSpell != null)
                    {
                         var NewSpellTotalPower = NewSpell.SpellEffectLast * NewSpell.SpellAmount;
                         var BestSpellTotalPower = BestSpell.SpellEffectLast * BestSpell.SpellAmount;

                        if(NewSpellTotalPower > BestSpellTotalPower)
                        {
                            BestSpell = NewSpell;
                        }
                    }
                    else
                    {
                        BestSpell = NewSpell;
                    }        
                }
            }

        }

        public int FindUnitTarget()
        {
            switch (BestSpell.spellTarget)
            {
                case ISpellEffect.SpellTarget.ALLY:
                    return BestPlayerUnitTarget;
                case ISpellEffect.SpellTarget.ENEMY:
                    return BestEnemyUnitTarget;
                case ISpellEffect.SpellTarget.SELF:
                default:
                    return PlayingUnitID;                    
            }
        }

        public List<int> FindUnitTargetZone(int Target)
        {
            List<int> UnitTargetZone = new List<int>();
            switch (BestSpell.spellTargetZone)
            {                
                case ISpellEffect.SpellTargetZone.ALL_UNITS:
                    switch (BestSpell.spellTarget)
                    {
                        case ISpellEffect.SpellTarget.ALLY:
                            for (int i = 1; i <= _enemyCollection.Count; i++)
                            {
                                UnitTargetZone.Add(i);
                            }
                            return UnitTargetZone;
                        case ISpellEffect.SpellTarget.ENEMY:
                            for (int i = 1; i <= _playerCollection.Count; i++)
                            {
                                UnitTargetZone.Add(i);
                            }
                            return UnitTargetZone;
                        case ISpellEffect.SpellTarget.SELF:
                        default:
                             UnitTargetZone.Add(PlayingUnitID);
                             return UnitTargetZone;
                    }
                case ISpellEffect.SpellTargetZone.LINE_UNITS:
                    if(Target <= 3)
                    {
                        UnitTargetZone.Add(1);
                        UnitTargetZone.Add(2);
                        UnitTargetZone.Add(3);
                    }
                    else
                    {
                        UnitTargetZone.Add(4);
                        UnitTargetZone.Add(5);
                        UnitTargetZone.Add(6);
                    }                
                    return UnitTargetZone;
                case ISpellEffect.SpellTargetZone.COLUMN_UNITS:
                    break;
                case ISpellEffect.SpellTargetZone.ZONE_UNITS:
                    break;
                case ISpellEffect.SpellTargetZone.ONE_UNIT:                    
                default:
                    UnitTargetZone.Add(BestEnemyUnitTarget);
                    return UnitTargetZone;
            }
        }

        public void UnitsAnalyse()
        {            
            // Find The Best Target ? // Lowest HP target
            foreach (var PlayerUnit in _playerCollection)
            {
                if(PlayerUnit.Value.UnitHp <= _playerCollection[BestPlayerUnitTarget].UnitHp)
                {
                    BestPlayerUnitTarget = PlayerUnit.Key;
                }
            }

            foreach (var EnemyUnit in _enemyCollection)
            {
                if (EnemyUnit.Value.UnitHp <= _enemyCollection[BestEnemyUnitTarget].UnitHp)
                {
                    BestEnemyUnitTarget = EnemyUnit.Key;
                }
            }

            // Offensive or Defensive Decision

            /*if (UnitHaveHealSpell())
            {
                if(_playerCollection[BestEnemyUnitTarget].UnitHp <= _playerCollection[BestEnemyUnitTarget].UnitMaxHp - GetHealAmount())
                {

                }
            }*/
            

        }

        /*private bool UnitHaveHealSpell() 
        {
            FindSpellType(_playerCollection[PlayingUnitID].spellList, ISpellEffect.SpellType.BUFF)
            {

            }
        }*/

        private float GetHealAmount()
        {
            return _playerCollection[PlayingUnitID].UnitPower; // Heal Amount should be calculated Form Spell
        }

        private ISpell FindSpellType(List<ISpell> SpellList, ISpellEffect.SpellType _spellType)
        {
            ISpell result = SpellList.Find(
                delegate (ISpell sp)
                {
                    return sp.spellType == _spellType;
                }
            );

            return result;

        }

        

    }    
}
