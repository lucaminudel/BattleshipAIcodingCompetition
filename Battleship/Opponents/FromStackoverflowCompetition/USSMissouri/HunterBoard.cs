using System;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.USSMissouri
{
	public class HunterBoard
	{
		public HunterBoard(USSMissouri root, Size boardsize, Ship target)
		{
			size = boardsize;
			grid = new int[size.Width, size.Height];
			Array.Clear(grid, 0, size.Width * size.Height);

			Player = root;
			Target = target;
			Initialize();
		}

		public void Initialize()
		{
			int x, y, i;

			for (y = 0; y < size.Height; ++y)
			{
				for (x = 0; x < size.Width - Target.Length + 1; ++x)
				{
					for (i = 0; i < Target.Length; ++i)
					{
						grid[x + i, y]++;
					}
				}
			}

			for (y = 0; y < size.Height - Target.Length + 1; ++y)
			{
				for (x = 0; x < size.Width; ++x)
				{
					for (i = 0; i < Target.Length; ++i)
					{
						grid[x, y + i]++;
					}
				}
			}
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

		public void ShotMiss(Point p)
		{
			int x, y;
			int min, max;

			min = Math.Max(p.X - Target.Length + 1, 0);
			max = Math.Min(p.X, size.Width - Target.Length);
			for (x = min; x <= max; ++x)
			{
				DecrementRow(p.Y, x, x + Target.Length - 1);
			}

			min = Math.Max(p.Y - Target.Length + 1, 0);
			max = Math.Min(p.Y, size.Height - Target.Length);
			for (y = min; y <= max; ++y)
			{
				DecrementColumn(p.X, y, y + Target.Length - 1);
			}

			grid[p.X, p.Y] = 0;
		}

		public void ShotHit(Point p)
		{
		}

		public override String ToString()
		{
			String output = String.Format("Target size is {0}\n", Target.Length);
			String horzDiv = "   +----+----+----+----+----+----+----+----+----+----+\n";
			int x, y;

			output += "      A    B    C    D    E    F    G    H    I    J    \n" + horzDiv;
			for (y = 0; y < size.Height; ++y)
			{
				output += String.Format("{0} ", y + 1).PadLeft(3);
				for (x = 0; x < size.Width; ++x)
				{
					output += String.Format("| {0} ", grid[x, y].ToString().PadLeft(2));
				}
				output += "|\n" + horzDiv;
			}
			return output;
		}

		// If we shoot at point P, how does that affect the potential of the board?
		public Int32 GetWeightAt(Point p)
		{
			int x, y;
			int potential = 0;
			int min, max;

			min = Math.Max(p.X - Target.Length + 1, 0);
			max = Math.Min(p.X, size.Width - Target.Length);
			for (x = min; x <= max; ++x)
			{
				if (Player.theShotBoard.isMissInRow(p.Y, x, x + Target.Length - 1) == false)
				{
					++potential;
				}
			}

			min = Math.Max(p.Y - Target.Length + 1, 0);
			max = Math.Min(p.Y, size.Height - Target.Length);
			for (y = min; y <= max; ++y)
			{
				if (Player.theShotBoard.isMissInColumn(p.X, y, y + Target.Length - 1) == false)
				{
					++potential;
				}
			}

			return potential;
		}

		public void DecrementRow(int row, int rangeA, int rangeB)
		{
			int x;
			for (x = rangeA; x <= rangeB; ++x)
			{
				grid[x, row] = (grid[x, row] == 0) ? 0 : grid[x, row] - 1;
			}
		}
		public void DecrementColumn(int col, int rangeA, int rangeB)
		{
			int y;
			for (y = rangeA; y <= rangeB; ++y)
			{
				grid[col, y] = (grid[col, y] == 0) ? 0 : grid[col, y] - 1;
			}
		}

		private Ship Target = null;
		private USSMissouri Player;
		private Int32[,] grid;
		private Size size;
	}
}