using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Terminator
{
    class CountMap : Matrix<int>
    {
        public ProbabilisticMap ToProbabilisticMap()
        {
            int total = TotalSum();

            var probabilisticMap = new ProbabilisticMap();

            if (total == 0)
            {
                return probabilisticMap;
            }

            for (int i = 0; i < Board.Size; i++)
            {
                for (int j = 0; j < Board.Size; j++)
                {
                    probabilisticMap[i, j] = this[i, j]/(double) total;
                }
            }

            return probabilisticMap;
        }
        
        public int Sum(IEnumerable<Point> points)
        {
            return points.Sum(point => this[point]);
        }

        public int TotalSum()
        {
            int total = 0;

            for (int i = 0; i < Board.Size; i++)
            {
                for (int j = 0; j < Board.Size; j++)
                {
                    total += this[i, j];
                }
            }

            return total;
        }
    }
}
