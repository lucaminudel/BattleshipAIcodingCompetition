#region

using System;
using System.Collections.Generic;
using System.Drawing;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public class UniformDistributedShotProvider : IShotProvider {
		private readonly Grid _grid;
		public Point Offset { get; private set; }
		private readonly int _step;

		public UniformDistributedShotProvider( Grid grid, int step )
			: this( grid, step, Point.Empty ) {}

		public UniformDistributedShotProvider( Grid grid, int step, Point offset ) {
			if ( ( step < 1 ) || step >= Math.Max( grid.Size.Height, grid.Size.Width ) ) {
				throw new ArgumentException( "Step", "step" );
			}
			_grid = grid;
			Offset = new Point( offset.X % step, offset.Y % step );
			_step = step;
		}

		public IEnumerable<Shot> Shots() {
			for ( var rowIndex = Offset.Y; rowIndex < _grid.Size.Height; rowIndex += _step ) {
				for ( var columnIndex = Offset.X; columnIndex < _grid.Size.Width; columnIndex += _step ) {
					yield return new Shot( columnIndex, rowIndex );
				}
			}
		}
	}
}