using System.Diagnostics;
using System.Text;

namespace Battleship.Opponents.Terminators
{
    class ProbabilisticMap : Matrix<double>
    {
        public new void Print()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Board.Size; ++i)
            {
                for (int j = 0; j < Board.Size; ++j)
                {
                    sb.AppendFormat("{0:0.000} ", this[i, j]);
                }

                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine();
        }
    }
}
