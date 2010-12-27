using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.USSMissouri
{
	// An Attack is the data on the ship we are currently working on sinking.
	// It consists of a set of points, horizontal and vertical, from a central point.
	// And can be extended in any direction.
	public class Attack
	{
		public Attack(USSMissouri root, Point p)
		{
			Player = root;
			hit = p;
			horzExtent = new Extent(p.X, p.X);
			vertExtent = new Extent(p.Y, p.Y);
		}

		public Extent HorizontalExtent
		{
			get { return horzExtent; }
		}
		public Extent VerticalExtent
		{
			get { return vertExtent; }
		}
		public Point FirstHit
		{
			get { return hit; }
		}

		public void AddHit(Point p)
		{
			if (hit.X == p.X) // New hit in the vertical direction
			{
				vertExtent.Min = Math.Min(vertExtent.Min, p.Y);
				vertExtent.Max = Math.Max(vertExtent.Max, p.Y);
			}
			else if (hit.Y == p.Y)
			{
				horzExtent.Min = Math.Min(horzExtent.Min, p.X);
				horzExtent.Max = Math.Max(horzExtent.Max, p.X);
			}
		}
		public Point[] GetNextTargets()
		{
			List<Point> bors = new List<Point>();

			Point p;

			p = new Point(hit.X, vertExtent.Min - 1);
			while (p.Y >= 0 && Player.theShotBoard[p] == Shot.Hit)
			{
				if (Player.theShotBoard[p] == Shot.Miss)
				{
					break; // Don't add p to the List 'bors.  
				}
				--p.Y;
			}
			if (p.Y >= 0 && Player.theShotBoard[p] == Shot.None) // Add next-target only if there is no shot here yet.
			{
				bors.Add(p);
			}

			//-------------------

			p = new Point(hit.X, vertExtent.Max + 1);
			while (p.Y < Player.theBoardSize.Height && Player.theShotBoard[p] == Shot.Hit)
			{
				if (Player.theShotBoard[p] == Shot.Miss)
				{
					break; // Don't add p to the List 'bors.  
				}
				++p.Y;
			}
			if (p.Y < Player.theBoardSize.Height && Player.theShotBoard[p] == Shot.None)
			{
				bors.Add(p);
			}

			//-------------------

			p = new Point(horzExtent.Min - 1, hit.Y);
			while (p.X >= 0 && Player.theShotBoard[p] == Shot.Hit)
			{
				if (Player.theShotBoard[p] == Shot.Miss)
				{
					break; // Don't add p to the List 'bors.  
				}
				--p.X;
			}
			if (p.X >= 0 && Player.theShotBoard[p] == Shot.None)
			{
				bors.Add(p);
			}

			//-------------------

			p = new Point(horzExtent.Max + 1, hit.Y);
			while (p.X < Player.theBoardSize.Width && Player.theShotBoard[p] == Shot.Hit)
			{
				if (Player.theShotBoard[p] == Shot.Miss)
				{
					break; // Don't add p to the List 'bors.  
				}
				++p.X;
			}
			if (p.X < Player.theBoardSize.Width && Player.theShotBoard[p] == Shot.None)
			{
				bors.Add(p);
			}

			return bors.ToArray();
		}

		private Point hit;
		private Extent horzExtent;
		private Extent vertExtent;
		private USSMissouri Player;
	}
}