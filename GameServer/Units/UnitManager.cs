using GameServer.Loots;
using GameServer.Units.Viking;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Units
{
    public class UnitManager 
    {

        public static List<Unit> UnitHandler;

        // Return the Unit if finded, or Return Null
        public static Unit FindUnitByName(string _desiredUnit)
        {
            foreach (Unit _unit in UnitHandler)
            {
                if (_unit.UnitName == _desiredUnit)
                {
                    return _unit;
                }
            }
            return null;
        }

        // Return the Unit if finded, or Return Null
        public static Unit FindUnitByID(string _desiredUnit)
        {
            foreach (Unit _unit in UnitHandler)
            {
                if (_unit.UnitID == _desiredUnit)
                {
                    return _unit;
                }
            }
            return null;
        }

        // Initialize all the Scenes
        public static void InitializeUnitsData()
        {
            UnitHandler = new List<Unit>()
            {
                { new Viking.GreyViking() },
                { new Viking.GreyBrotherViking() },
                { new Viking.GreenViking() },
                { new Viking.BlueViking() },
                { new Viking.PurpleViking() },
                { new Viking.GoldViking() },
            };
            Console.WriteLine("Initialized Units.");
        }
    }
}
