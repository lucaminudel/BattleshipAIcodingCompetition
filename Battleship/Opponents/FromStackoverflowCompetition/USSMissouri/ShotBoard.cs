using System;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.USSMissouri
{
	public class ShotBoard
	{
		public ShotBoard(Size boardsize)
		{
			size = boardsize;
			grid = new Shot[size.Width, size.Height];

			for (int y = 0; y < size.Height; ++y)
			{
				for (int x = 0; x < size.Width; ++x)
				{
					grid[x, y] = Shot.None;
				}
			}
		}

		public Shot this[int c, int r]
		{
			get { return grid[c, r]; }
			set { grid[c, r] = value; }
		}
		public Shot this[Point p]
		{
			get { return grid[p.X, p.Y]; }
			set { grid[p.X, p.Y] = value; }
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
						case Shot.None: disp = ""; break;
						case Shot.Hit: disp = "#"; break;
						case Shot.Miss: disp = "."; break;
						case Shot.Unresolved: disp = "?"; break;
						default: disp = "!"; break;
					}

					output += String.Format("| {0} ", disp.PadLeft(2));
				}
				output += "|\n" + horzDiv;
			}
			return output;
		}

		// Functions to find shots on the board, at a specific point, or in a row or column, within a range
		public bool ShotAt(Point p)
		{
			return !(this[p] == Shot.None);
		}
		public bool isMissInColumn(int col, int rangeA, int rangeB)
		{
			for (int y = rangeA; y <= rangeB; ++y)
			{
				if (grid[col, y] == Shot.Miss)
				{
					return true;
				}
			}
			return false;
		}
		public bool isMissInRow(int row, int rangeA, int rangeB)
		{
			for (int x = rangeA; x <= rangeB; ++x)
			{
				if (grid[x, row] == Shot.Miss)
				{
					return true;
				}
			}
			return false;
		}
		protected Shot[,] grid;
		protected Size size;
	}
}