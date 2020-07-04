using GameServer.Spells;
using GameServer.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.IA
{
    public class EnemyIA
    {
        public Dictionary<int, Unit> _ASide;
        public Dictionary<int, Unit> _BSide;
        public Unit _CurrentSide;

        public Unit CurrentIAUnit;

        public int PlayingUnitID;
        public int IATargetUnitID;

        public int BestASideUnitTarget = 1; // = The ID of The Weakest Player Unit to Focus Attack | Start to 1 coz ID of Units begins to 1
        public int BestBSideUnitTarget = 1; // = The ID of The Weakest IA Unit Needing Help | Start to 1 coz ID of Units begins to 1

        public Queue<ISpellEffect.SpellType> spellTypeQueue;

        public ISpell BestSpell;

        public EnemyIA(Dictionary<int, Unit>  PlayerCollection, Dictionary<int, Unit>  EnemyCollection,int _PlayingUnitID)
        {
            _ASide = PlayerCollection;
            _BSide = EnemyCollection;
            PlayingUnitID = _PlayingUnitID;
            CurrentIAUnit = _BSide[_PlayingUnitID];
        }

        public void UseSpell(List<int> _UnitsTargeted)
        {
            
            switch (BestSpell.spellTarget)
            {
                
                case ISpellEffect.SpellTarget.ENEMY:
                    foreach (var ASideUnits in _UnitsTargeted)
                    {
                        if (_ASide.ContainsKey(ASideUnits))
                        {                            
                            _ASide.TryGetValue(ASideUnits, out _CurrentSide);

                            _ASide[ASideUnits] = BestSpell.Play(_CurrentSide);
                        }
                    }
                    break;
                case ISpellEffect.SpellTarget.ALLY:
                case ISpellEffect.SpellTarget.SELF:
                default:
                    foreach (var BsideUnits in _UnitsTargeted)
                    {
                        if (_BSide.ContainsKey(BsideUnits))
                        {
                            _BSide.TryGetValue(BsideUnits, out _CurrentSide);

                            _BSide[BsideUnits] = BestSpell.Play(_CurrentSide);                            
                        }
                    }
                    break;
            }
        }

        /*switch (BestSpell.spellType)
                            {
                                case ISpellEffect.SpellType.DEFENSE:
                                    switch (BestSpell.spellTargetProperty)
                                    {
                                        case ISpellEffect.SpellTargetProperty.UNIT_HP:
                                            _CurrentSide.UnitHp
                                            break;
                                        case ISpellEffect.SpellTargetProperty.UNIT_MANA:
                                            break;
                                        case ISpellEffect.SpellTargetProperty.UNIT_SHIELD:
                                            break;
                                        case ISpellEffect.SpellTargetProperty.UNIT_DAMAGES:
                                            break;
                                        case ISpellEffect.SpellTargetProperty.UNIT_VELOCITY:
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case ISpellEffect.SpellType.OFFENSE:
                                    break;
                                case ISpellEffect.SpellType.BUFF:
                                    break;
                                case ISpellEffect.SpellType.DEBUFF:
                                    break;
                                case ISpellEffect.SpellType.INCREMENTAL:
                                    break;
                                case ISpellEffect.SpellType.PASSIVE:
                                    break;
                                default:
                                    break;
                            }*/

        public void CheckUnitSpells(List<ISpell> _UnitSpells)
        {

            bool isSpellFound = false;

            // Priorise Spell
            spellTypeQueue = new Queue<ISpellEffect.SpellType>();
            spellTypeQueue.Enqueue(ISpellEffect.SpellType.DEFENSE);
            spellTypeQueue.Enqueue(ISpellEffect.SpellType.OFFENSE);
            spellTypeQueue.Enqueue(ISpellEffect.SpellType.BUFF);

            // Find Best Spell       
            foreach (var SpellType in spellTypeQueue)
            {
                List<ISpell> NewSpell = FindSpellType(_UnitSpells, SpellType); // Null if no spell Found 
                if (NewSpell.Count != 0 && !isSpellFound) // 
                {
                    foreach (var Spell in NewSpell)
                    {
                        if (BestSpell != null)
                        {
                            var NewSpellTotalPower = Spell.SpellEffectLast * Spell.SpellAmount;
                            var BestSpellTotalPower = BestSpell.SpellEffectLast * BestSpell.SpellAmount;

                            if (NewSpellTotalPower > BestSpellTotalPower)
                            {
                                BestSpell = Spell;
                            }
                            
                        }
                        else
                        {
                            BestSpell = Spell;
                        }
                    }
                    isSpellFound = true;
                }                
            }
        }

        public int FindUnitTarget()
        {
            switch (BestSpell.spellTarget)
            {
                case ISpellEffect.SpellTarget.ALLY:
                    return BestBSideUnitTarget;
                case ISpellEffect.SpellTarget.ENEMY:
                    return BestASideUnitTarget;
                case ISpellEffect.SpellTarget.SELF:
                default:
                    return PlayingUnitID;                    
            }
        }

        public List<int> FindUnitTargetZone(int Target, ISpellEffect.SpellTarget _spellTarget)
        {
            IATargetUnitID = Target;
            List<int> UnitTargetZone = new List<int>();            

            switch (BestSpell.spellTargetZone)
            {                
                case ISpellEffect.SpellTargetZone.ALL_UNITS:
                    switch (_spellTarget)
                    {
                        case ISpellEffect.SpellTarget.ALLY:
                            foreach (var BSideUnit in _BSide)
                            {
                                UnitTargetZone.Add(BSideUnit.Key);
                            }
                            return UnitTargetZone;
                        case ISpellEffect.SpellTarget.ENEMY:
                            foreach (var ASideUnit in _ASide)
                            {
                                UnitTargetZone.Add(ASideUnit.Key);
                            }
                            return UnitTargetZone;
                        case ISpellEffect.SpellTarget.SELF:
                        default:
                             UnitTargetZone.Add(PlayingUnitID);
                             return UnitTargetZone;
                    }
                case ISpellEffect.SpellTargetZone.LINE_UNITS:
                    switch (_spellTarget)
                    {                        
                        case ISpellEffect.SpellTarget.ENEMY:
                            if (Target <= 3) // From B Side Enemy it's a BACK Lane
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key <= 3)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key > 3)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            return UnitTargetZone;

                        case ISpellEffect.SpellTarget.ALLY:
                        case ISpellEffect.SpellTarget.SELF:
                        default:                        
                            if (Target <= 3) // From B Side Ally it's a FRONT Lane
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key <= 3)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key > 3)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            return UnitTargetZone;
                    }              
                case ISpellEffect.SpellTargetZone.COLUMN_UNITS:
                    switch (_spellTarget)
                    {
                       
                        case ISpellEffect.SpellTarget.ENEMY:
                            
                            if (Target == 1 || Target == 4)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 1 || ASideUnit.Key == 4)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 2 || Target == 5)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 2 || ASideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 3 || Target == 6)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 3 || ASideUnit.Key == 6)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            return UnitTargetZone;

                        case ISpellEffect.SpellTarget.ALLY:
                        case ISpellEffect.SpellTarget.SELF:
                        default:
                            if (Target == 1 || Target == 4)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 1 || BSideUnit.Key == 4)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 2 || Target == 5)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 2 || BSideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 3 || Target == 6)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 3 || BSideUnit.Key == 6)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            return UnitTargetZone;
                    }
                case ISpellEffect.SpellTargetZone.ZONE_UNITS:
                    switch (_spellTarget)
                    {                        
                        case ISpellEffect.SpellTarget.ENEMY:
                            if (Target == 1)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 1 || ASideUnit.Key == 2 || ASideUnit.Key == 4)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 2)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 1 || ASideUnit.Key == 2 || ASideUnit.Key == 3 || ASideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 3)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 3 || ASideUnit.Key == 2 || ASideUnit.Key == 6)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 4)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 4 || ASideUnit.Key == 1 || ASideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 5)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 5 || ASideUnit.Key == 2 || ASideUnit.Key == 4 || ASideUnit.Key == 6)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 6)
                            {
                                foreach (var ASideUnit in _ASide)
                                {
                                    if (ASideUnit.Key == 6 || ASideUnit.Key == 3 || ASideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(ASideUnit.Key);
                                    }
                                }
                            }
                            return UnitTargetZone;
                        
                        case ISpellEffect.SpellTarget.ALLY:
                        case ISpellEffect.SpellTarget.SELF:
                        default:
                            if (Target == 1)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 1 || BSideUnit.Key == 2 || BSideUnit.Key == 4)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 2)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 1 || BSideUnit.Key == 2 || BSideUnit.Key == 3 || BSideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 3)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 3 || BSideUnit.Key == 2 || BSideUnit.Key == 6)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 4)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 4 || BSideUnit.Key == 1 || BSideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 5)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 5 || BSideUnit.Key == 2 || BSideUnit.Key == 4 || BSideUnit.Key == 6)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            if (Target == 6)
                            {
                                foreach (var BSideUnit in _BSide)
                                {
                                    if (BSideUnit.Key == 6 || BSideUnit.Key == 3 || BSideUnit.Key == 5)
                                    {
                                        UnitTargetZone.Add(BSideUnit.Key);
                                    }
                                }
                            }
                            return UnitTargetZone;
                    }
                case ISpellEffect.SpellTargetZone.ONE_UNIT:                    
                default:
                    UnitTargetZone.Add(FindUnitTarget());
                    return UnitTargetZone;
            }
        }

        public void UnitsAnalyse()
        {            
            // Find The Best Target ? // Lowest HP target
            foreach (var PlayerUnit in _ASide)
            {
                if(PlayerUnit.Value.UnitHp <= _ASide[BestASideUnitTarget].UnitHp)
                {
                    BestASideUnitTarget = PlayerUnit.Key;
                }
            }

            foreach (var EnemyUnit in _BSide)
            {
                if (EnemyUnit.Value.UnitHp <= _BSide[BestBSideUnitTarget].UnitHp)
                {
                    BestBSideUnitTarget = EnemyUnit.Key;
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
            return _ASide[PlayingUnitID].UnitPower; // Heal Amount should be calculated Form Spell
        }

        private List<ISpell> FindSpellType(List<ISpell> SpellList, ISpellEffect.SpellType _spellType)
        {

            List<ISpell> result = SpellList.FindAll(
                delegate (ISpell sp)
                {
                    return sp.spellType == _spellType;
                }
            );

            return result;

        }       
    }
}
