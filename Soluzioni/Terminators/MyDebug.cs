using System.Diagnostics;
using System.Text;

namespace Battleship.Opponents.Terminators
{
    class MyDebug
    {
        public static void Print<T>(T[,] matrix)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Board.Size; ++i)
            {
                for (int j = 0; j < Board.Size; ++j)
                {
                    sb.AppendFormat("{0} ", matrix[i, j]);
                }

                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine();

            Debug.Print(sb.ToString());
        }
    }
}
