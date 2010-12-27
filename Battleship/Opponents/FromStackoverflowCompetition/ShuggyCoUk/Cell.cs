using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromStackoverflowCompetition.ShuggyCoUk
{
	class Cell<T>
	{
		private readonly BoardView<T> view;
		public readonly int X;
		public readonly int Y;
		public T Data;
		public double Bias { get; set; }

		public Cell(BoardView<T> view, int x, int y)
		{
			this.view = view; this.X = x; this.Y = y; this.Bias = 1.0;
		}

		public Point Location
		{
			get { return new Point(X, Y); }
		}

		public IEnumerable<U> FoldAll<U>(U acc, Func<Cell<T>, U, U> trip)
		{
			return new[] { Compass.North, Compass.East, Compass.South, Compass.West }
				.Select(x => FoldLine(x, acc, trip));
		}

		public U FoldLine<U>(Compass direction, U acc, Func<Cell<T>, U, U> trip)
		{
			var cell = this;
			while (true)
			{
				switch (direction)
				{
					case Compass.North:
						cell = cell.North; break;
					case Compass.East:
						cell = cell.East; break;
					case Compass.South:
						cell = cell.South; break;
					case Compass.West:
						cell = cell.West; break;
				}
				if (cell == null)
					return acc;
				acc = trip(cell, acc);
			}
		}

		public Cell<T> North
		{
			get { return view.SafeLookup(X, Y - 1); }
		}

		public Cell<T> South
		{
			get { return view.SafeLookup(X, Y + 1); }
		}

		public Cell<T> East
		{
			get { return view.SafeLookup(X + 1, Y); }
		}

		public Cell<T> West
		{
			get { return view.SafeLookup(X - 1, Y); }
		}

		public IEnumerable<Cell<T>> Neighbours()
		{
			if (North != null)
				yield return North;
			if (South != null)
				yield return South;
			if (East != null)
				yield return East;
			if (West != null)
				yield return West;
		}
	}
}
