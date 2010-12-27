using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Terminator
{
    static class Util
    {
        public static readonly List<Ship> AllShips2;
        public static readonly List<Ship> AllShips3;
        public static readonly List<Ship> AllShips4;
        public static readonly List<Ship> AllShips5;

        static Util()
        {
            AllShips2 = GenerateAllPossibleShips(2);
            AllShips3 = GenerateAllPossibleShips(3);
            AllShips4 = GenerateAllPossibleShips(4);
            AllShips5 = GenerateAllPossibleShips(5);
        }

        public static List<Ship> GetAllShipWithLength(int length)
        {
            if (length == 2) return AllShips2;
            if (length == 3) return AllShips3;
            if (length == 4) return AllShips4;
            if (length == 5) return AllShips5;

            return null;
        }
        
        private static readonly Random Random = new Random((int)DateTime.Now.Ticks);

        public static int GetRandomIntFromInterval(int minInclusiveValue, int maxInclusiveValue)
        {
            return Random.Next(minInclusiveValue, maxInclusiveValue + 1);
        }

        public static int GetRandomBoardIndex()
        {
            return GetRandomIntFromInterval(0, Board.Size - 1);
        }

        public static ShipOrientation GetRandomShipOrientation()
        {
            return (ShipOrientation) GetRandomIntFromInterval(0, 1);
        }

        public static Point GetRandomPoint()
        {
            return new Point(GetRandomBoardIndex(), GetRandomBoardIndex());
        }

        public static Point Sum(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point Opposite(Point p)
        {
            return new Point(-p.X, -p.Y);
        }

        public static double Max(params double[] v)
        {
            double max = double.MinValue;

            foreach (double t in v)
            {
                if (t > max)
                {
                    max = t;
                }
            }

            return max;
        }

        public static List<Ship> GenerateAllPossibleShips(int length)
        {
            var ships = new List<Ship>();

            for (int x = 0; x <= Board.Size - length; ++x)
            {
                for (int y = 0; y < Board.Size; ++y)
                {
                    var hship = new Ship(length);
                    hship.Place(new Point(x, y), ShipOrientation.Horizontal);

                    var vship = new Ship(length);
                    vship.Place(new Point(y, x), ShipOrientation.Vertical);

                    ships.Add(hship);
                    ships.Add(vship);
                }
            }

            return ships;
        }

        public static void Clear(double[,] matrix, double value = 0)
        {
            for (int i = 0; i < Board.Size; ++i)
            {
                for (int j = 0; j < Board.Size; ++j)
                {
                    matrix[i, j] = value;
                }                
            }
        }
    }
}
