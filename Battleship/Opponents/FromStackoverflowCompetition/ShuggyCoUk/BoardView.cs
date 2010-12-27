using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Battleship.Opponents.FromStackoverflowCompetition.ShuggyCoUk
{
	class BoardView<T> : IEnumerable<Cell<T>>
	{
		public readonly Size Size;
		private readonly int Columns;
		private readonly int Rows;

		private Cell<T>[] history;

		public BoardView(Size size)
		{
			this.Size = size;
			Columns = size.Width;
			Rows = size.Height;
			this.history = new Cell<T>[Columns * Rows];
			for (int y = 0; y < Rows; y++)
			{
				for (int x = 0; x < Rows; x++)
					history[x + y * Columns] = new Cell<T>(this, x, y);
			}
		}

		public T this[int x, int y]
		{
			get { return history[x + y * Columns].Data; }
			set { history[x + y * Columns].Data = value; }
		}

		public T this[Point p]
		{
			get { return history[SafeCalc(p.X, p.Y, true)].Data; }
			set { this.history[SafeCalc(p.X, p.Y, true)].Data = value; }
		}

		private int SafeCalc(int x, int y, bool throwIfIllegal)
		{
			if (x < 0 || y < 0 || x >= Columns || y >= Rows)
			{
				if (throwIfIllegal)
					throw new ArgumentOutOfRangeException("[" + x + "," + y + "]");
				else
					return -1;
			}
			return x + y * Columns;
		}

		public void Set(T data)
		{
			foreach (var cell in this.history)
				cell.Data = data;
		}

		public Cell<T> SafeLookup(int x, int y)
		{
			int index = SafeCalc(x, y, false);
			if (index < 0)
				return null;
			return history[index];
		}

		#region IEnumerable<Cell<T>> Members

		public IEnumerator<Cell<T>> GetEnumerator()
		{
			foreach (var cell in this.history)
				yield return cell;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public BoardView<U> Transform<U>(Func<T, U> transform)
		{
			var result = new BoardView<U>(new Size(Columns, Rows));
			for (int y = 0; y < Rows; y++)
			{
				for (int x = 0; x < Columns; x++)
				{
					result[x, y] = transform(this[x, y]);
				}
			}
			return result;
		}

		public void WriteAsGrid(TextWriter w)
		{
			WriteAsGrid(w, "{0}");
		}

		public void WriteAsGrid(TextWriter w, string format)
		{
			WriteAsGrid(w, x => string.Format(format, x.Data));
		}

		public void WriteAsGrid(TextWriter w, Func<Cell<T>, string> perCell)
		{
			for (int y = 0; y < Rows; y++)
			{
				for (int x = 0; x < Columns; x++)
				{
					if (x != 0)
						w.Write(",");
					w.Write(perCell(this.SafeLookup(x, y)));
				}
				w.WriteLine();
			}
		}

		#endregion
	}
}
