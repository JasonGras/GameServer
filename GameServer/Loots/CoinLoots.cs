using GameServer.Units;
using NLog;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Loots
{

    public class CoinLoots
    {
        public enum Coin
        {
            Viking,
            Massai,
        }

        public enum UnitAverageQuality
        {
            Grey,
            Green,
            Blue,
            Purple,
            Gold
        }
        public enum CoinAverageQuality
        {
            Green,
            Blue,
            Purple,
            Gold
        }


/*        private List<Coin> CoinList = new List<Coin>()
        {
            Coin.Massai,
            Coin.Viking
        };*/

        private Dictionary<CoinAverageQuality, Dictionary<UnitAverageQuality, double>> CoinUnitLootAverage = new Dictionary<CoinAverageQuality, Dictionary<UnitAverageQuality, double>>()
        {
            {CoinAverageQuality.Green, new Dictionary<UnitAverageQuality, double>()
                {
                    {UnitAverageQuality.Grey,75},
                    {UnitAverageQuality.Green,23.6},
                    {UnitAverageQuality.Blue,1.4},
                    {UnitAverageQuality.Purple,0},
                    {UnitAverageQuality.Gold,0}
                }
            },
            {CoinAverageQuality.Blue, new Dictionary<UnitAverageQuality, double>()
                {
                    {UnitAverageQuality.Grey,0},
                    {UnitAverageQuality.Green,10},
                    {UnitAverageQuality.Blue,81.5},
                    {UnitAverageQuality.Purple,8},
                    {UnitAverageQuality.Gold,0.5}
                }
            },
            {CoinAverageQuality.Purple, new Dictionary<UnitAverageQuality, double>()
                {
                    {UnitAverageQuality.Grey,0},
                    {UnitAverageQuality.Green,0},
                    {UnitAverageQuality.Blue,91.5},
                    {UnitAverageQuality.Purple,8},
                    {UnitAverageQuality.Gold,0.5}
                }
            },
            {CoinAverageQuality.Gold, new Dictionary<UnitAverageQuality, double>()
                {
                    {UnitAverageQuality.Grey,0},
                    {UnitAverageQuality.Green,0},
                    {UnitAverageQuality.Blue,0},
                    {UnitAverageQuality.Purple,95.5},
                    {UnitAverageQuality.Gold,4.5}
                }
            }
        };


        private static readonly Random random = new Random();
        //Sum of all % of the LootTable
        public double weight;

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        private static int RandomNumberBetween(int minValue, int maxValue)
        {
            var next = random.Next(minValue, maxValue);

            return next;
        }

        public Unit GetRandomLoot(CoinAverageQuality _qualityCoin)
        {
            // Tire un Nombre aléatoire entre 0 et 101
            double calc_RandomLoot = RandomNumberBetween((double) 0, (double) 100);

            // Récupère les % de chances de Loot pour la Capsule ouverte
            CoinUnitLootAverage.TryGetValue(_qualityCoin, out Dictionary<UnitAverageQuality, double> lootTable);
            weight = 0;
            foreach (var UnitAverageQuality in lootTable)
            {
                //Console.WriteLine("Quality : "+UnitAverageQuality.Key.ToString() + " | Quality Value : "+ UnitAverageQuality.Value.ToString()+ " Random Number is : " + calc_RandomLoot.ToString());
                if(UnitAverageQuality.Value != 0)
                {
                    weight += UnitAverageQuality.Value;
                    //Console.WriteLine("Current Weight : " + weight.ToString());
                    if (calc_RandomLoot <= weight)
                    {
                        // This is it
                        //Console.WriteLine("The Quality for the Random Loot is this One : "+ UnitAverageQuality.Key);                        
                        weight = 0;
                        return GetRandomUnit(UnitAverageQuality.Key);                        
                    }                    
                }
            }
            return null;
        }

        public Unit GetRandomUnit(CoinLoots.UnitAverageQuality _UnitQuality)
        {
            try
            {
                //Try Update the Collection Data
                //var dSTCollectionByQuality = Server.dynamoDBServer.ScanAllNeokyCollectionByQuality((int) _UnitQuality);
                
                // Tire un Nombre aléatoire entre 0 et 101
                int calc_RandomUnit = RandomNumberBetween(0, UnitManager.UnitHandler.Count);

                //Console.WriteLine("Return the Result Random Unit : "+ calc_RandomUnit);
                return UnitManager.UnitHandler[calc_RandomUnit];//dSTCollectionByQuality.Result[calc_RandomUnit];

            }
            catch (Exception e)
            {                
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Warn, "GetRandomLoot", "GetRandomUnit Failed"), NlogClass.exceptions.Add));
                return null;
            }
        }

    }
}
