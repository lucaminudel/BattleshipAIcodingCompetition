
using System.Drawing;

namespace Battleship.Opponents.Terminators
{
    class Matrix<T>
    {
        private readonly T[,] matrix = new T[Board.Size, Board.Size];

        public T this[int x, int y] 
        {
            get { return matrix[x, y]; }
            set { matrix[x, y] = value; }
        }

        public T this[Point p]
        {
            get { return matrix[p.X, p.Y];  }
            set { matrix[p.X, p.Y] = value; }
        }

        public void Clear(T value = default(T))
        {
            for (int i = 0; i < Board.Size; i++)
            {
                for (int j = 0; j < Board.Size; j++)
                {
                    matrix[i, j] = value;
                }
            }
        }

        public void Print()
        {
            MyDebug.Print(matrix);            
        }
    }
}
