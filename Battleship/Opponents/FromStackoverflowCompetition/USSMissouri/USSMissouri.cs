using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.USSMissouri
{
	// The Empire of Japan surrendered on the deck of the USS Missouri on Sept. 2, 1945
	public class USSMissouri : IBattleshipOpponent
	{
		public String Name { get { return name; } }
		public Version Version { get { return ver; } }

		#region IBattleship Interface
		// IBattleship::NewGame
		public void NewGame(Size gameSize, TimeSpan timeSpan)
		{
			size = gameSize;
			shotBoard = new ShotBoard(size);
			attackVector = new Stack<Attack>();
		}

		// IBattleship::PlaceShips
		public void PlaceShips(ReadOnlyCollection<Ship> ships)
		{
			HunterBoard board;
			targetBoards = new List<HunterBoard>();
			shotBoard = new ShotBoard(size);
			foreach (Ship s in ships)
			{
				board = new HunterBoard(this, size, s);
				targetBoards.Add(board);

				// REWRITE: to ensure valid board placement.
				s.Place(
					new Point(
						rand.Next(size.Width),
						rand.Next(size.Height)),
					(ShipOrientation)rand.Next(2));
			}
		}

		// IBattleship::GetShot
		public Point GetShot()
		{
			Point p = new Point();

			if (attackVector.Count > 0)
			{
				p = ExtendShot();
				return p;
			}

			// Contemplate a shot at every-single point, and measure how effective it would be.
			Board potential = new Board(size);
			for (p.Y = 0; p.Y < size.Height; ++p.Y)
			{
				for (p.X = 0; p.X < size.Width; ++p.X)
				{
					if (shotBoard.ShotAt(p))
					{
						potential[p] = 0;
						continue;
					}

					foreach (HunterBoard b in targetBoards)
					{
						potential[p] += b.GetWeightAt(p);
					}
				}
			}

			// Okay, we have the shot potential of the board.
			// Lets pick a weighted-random spot.
			Point shot;
			shot = potential.GetWeightedRandom(rand.NextDouble());

			shotBoard[shot] = Shot.Unresolved;

			return shot;
		}

		public Point ExtendShot()
		{
			// Lets consider North, South, East, and West of the current shot.
			// and measure the potential of each
			Attack attack = attackVector.Peek();

			Board potential = new Board(size);

			Point[] points = attack.GetNextTargets();
			foreach (Point p in points)
			{
				if (shotBoard.ShotAt(p))
				{
					potential[p] = 0;
					continue;
				}

				foreach (HunterBoard b in targetBoards)
				{
					potential[p] += b.GetWeightAt(p);
				}
			}

			Point shot = potential.GetBestShot();
			shotBoard[shot] = Shot.Unresolved;
			return shot;
		}

		// IBattleship::NewMatch
		public void NewMatch(string opponent)
		{
		}
		public void OpponentShot(Point shot)
		{
		}
		public void ShotHit(Point shot)
		{
			ShotHit(shot, false);
		}
		public void ShotHitAndSink(Point shot, Ship sunkShip)
		{
			ShotHit(shot, true);
		}
		public void ShotHit(Point shot, bool sunk)
		{
			shotBoard[shot] = Shot.Hit;

			if (!sunk)
			{
				if (attackVector.Count == 0) // This is a first hit, open an attackVector
				{
					attackVector.Push(new Attack(this, shot));
				}
				else
				{
					attackVector.Peek().AddHit(shot);    // Add a hit to our current attack.
				}
			}

			// What if it is sunk?  Close the top attack, which we've been pursuing.
			if (sunk)
			{
				if (attackVector.Count > 0)
				{
					attackVector.Pop();
				}
			}
		}
		public void ShotMiss(Point shot)
		{
			shotBoard[shot] = Shot.Miss;

			foreach (HunterBoard b in targetBoards)
			{
				b.ShotMiss(shot);  // Update the potential map.
			}
		}
		public void GameWon()
		{
			Trace.WriteLine("I won the game!");
		}
		public void GameLost()
		{
			Trace.WriteLine("I lost the game!");
		}
		public void MatchOver()
		{
			Trace.WriteLine("This match is over.");
		}

		#endregion

		public ShotBoard theShotBoard
		{
			get { return shotBoard; }
		}
		public Size theBoardSize
		{
			get { return size; }
		}

		private Random rand = new Random();
		private Version ver = new Version(6, 3); // USS Missouri is BB-63, hence version 6.3
		private String name = "USS Missouri (abelenky@alum.mit.edu)";
		private Size size;
		private List<HunterBoard> targetBoards;
		private ShotBoard shotBoard;
		private Stack<Attack> attackVector;
	}
}
