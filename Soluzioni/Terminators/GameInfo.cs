using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Terminators
{
    class GameInfo
    {
        public static readonly Size GameSize = new Size(10, 10);

        public Board MyBoard { get; set; }
        public Board OpponentBoard { get; set; }

        public HashSet<Point> MyShots { get; set; }
        public HashSet<Point> OpponentShots { get; set; }

        public int MyNumberOfShots { get; set; }
        public int OpponentNumberOfShots { get; set; }

        public GameInfo()
        {
            MyBoard = new Board();
            OpponentBoard = new Board();

            MyShots = new HashSet<Point>();
            OpponentShots = new HashSet<Point>();

            MyNumberOfShots = 0;
            OpponentNumberOfShots = 0;
        }
    }
}
