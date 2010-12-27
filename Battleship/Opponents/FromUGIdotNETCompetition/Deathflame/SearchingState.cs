#region

using System;
using System.Drawing;
using System.Linq;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public class SearchingState : BattleshipState {
		private readonly Grid _grid;

		public SearchingState( Grid grid) {
			_grid = grid;
			NextState = this;
		}

		
		private IShotProvider CreateFillHolesShotProvider( int maxShipSize ) {
			return new FillHolesShotProvider( _grid, maxShipSize );
		}
		
		public override Shot NextShot() {
			var shotProvider = CreateFillHolesShotProvider( GetMaxShipSize() );
			return shotProvider.Shots().ToList().Shuffle().First();
		}

		private int GetMaxShipSize() {
			var sunkShips = _grid.SunkShips;

			return sunkShips.Any( ship => ship.Length == 2) ? 3 : 2;
		}

		public override void ShotHit( Point shot ) {
			_grid.Fired( shot );
			NextState = new SinkingState( this, _grid, shot );
		}

		public override void ShotHitAndSink( Point shot, Ship sunkShip ) {
			//This MUST be impossible!
			throw new InvalidOperationException( "Cannot sink during searching state!" );
		}

		public override void ShotMiss( Point shot ) {
			_grid.Fired( shot );
			NextState = this;
		}
	}
}