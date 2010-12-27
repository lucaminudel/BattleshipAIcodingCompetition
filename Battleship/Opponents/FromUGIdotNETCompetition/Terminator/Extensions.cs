using System.Collections.Generic;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Terminator
{
    static class Extensions
    {
        public static List<T> Shuffle<T>(this List<T> points)
        {
            int len = points.Count();

            for (int i = 0; i < len; ++i)
            {
                int j = Util.GetRandomIntFromInterval(i, len - 1);
                
                T temp = points[i];
                points[i] = points[j];
                points[j] = temp;
            }

            return points;
        }

        public static T ChooseOneAtRandom<T>(this IList<T> collection)
        {
            return collection[Util.GetRandomIntFromInterval(0, collection.Count - 1)];            
        }
    }
}
