using System;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.USSMissouri
{
	public class Board  // The potential-Board, which measures the full potential of each square.
	{
		// A Board is the status of many things.
		public Board(Size boardsize)
		{
			size = boardsize;
			grid = new int[size.Width, size.Height];
			Array.Clear(grid, 0, size.Width * size.Height);
		}

		public int this[int c, int r]
		{
			get { return grid[c, r]; }
			set { grid[c, r] = value; }
		}
		public int this[Point p]
		{
			get { return grid[p.X, p.Y]; }
			set { grid[p.X, p.Y] = value; }
		}

		public Point GetWeightedRandom(double r)
		{
			Int32 sum = 0;
			foreach (Int32 i in grid)
			{
				sum += i;
			}

			Int32 index = (Int32)(r * sum);

			Int32 x = 0, y = 0;
			for (y = 0; y < size.Height; ++y)
			{
				for (x = 0; x < size.Width; ++x)
				{
					if (grid[x, y] == 0) continue; // Skip any zero-cells
					index -= grid[x, y];
					if (index < 0) break;
				}
				if (index < 0) break;
			}

			if (x == 10 || y == 10)
				throw new Exception("WTF");

			return new Point(x, y);
		}

		public Point GetBestShot()
		{
			int max = grid[0, 0];
			for (int y = 0; y < size.Height; ++y)
			{
				for (int x = 0; x < size.Width; ++x)
				{
					max = (grid[x, y] > max) ? grid[x, y] : max;
				}
			}

			for (int y = 0; y < size.Height; ++y)
			{
				for (int x = 0; x < size.Width; ++x)
				{
					if (grid[x, y] == max)
					{
						return new Point(x, y);
					}
				}
			}
			return new Point(0, 0);
		}

		public bool IsZero()
		{
			foreach (Int32 p in grid)
			{
				if (p > 0)
				{
					return false;
				}
			}
			return true;
		}

		public override String ToString()
		{
			String output = "";
			String horzDiv = "   +----+----+----+----+----+----+----+----+----+----+\n";
			String disp;
			int x, y;

			output += "      A    B    C    D    E    F    G    H    I    J    \n" + horzDiv;

			for (y = 0; y < size.Height; ++y)
			{
				output += String.Format("{0} ", y + 1).PadLeft(3);
				for (x = 0; x < size.Width; ++x)
				{
					switch (grid[x, y])
					{
						case (int)Shot.None: disp = ""; break;
						case (int)Shot.Hit: disp = "#"; break;
						case (int)Shot.Miss: disp = "."; break;
						case (int)Shot.Unresolved: disp = "?"; break;
						default: disp = "!"; break;
					}

					output += String.Format("| {0} ", disp.PadLeft(2));
				}
				output += "|\n" + horzDiv;
			}

			return output;
		}

		protected Int32[,] grid;
		protected Size size;
	}
}