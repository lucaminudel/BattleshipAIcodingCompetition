using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Terminators
{
    class Board
    {
        public const int Size = 10;
        private readonly ShotInfo[,] shotInfo = new ShotInfo[Size, Size];

        public ShotInfo this[Point p]
        {
            get { return shotInfo[p.X, p.Y]; }
            set { shotInfo[p.X, p.Y] = value; }
        }

        public ShotInfo this[int x, int y]
        {
            get { return shotInfo[x, y]; }
            set { shotInfo[x, y] = value; }
        }

        public static bool Contains(Point p)
        {
            return (0 <= p.X && p.X < Size && 0 <= p.Y && p.Y < Size);
        }

        public List<Point> GetAllPositions(ShotInfo status)
        {
            var points = new List<Point>();

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (shotInfo[x, y] == status)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            return points;
        }
    }
}
