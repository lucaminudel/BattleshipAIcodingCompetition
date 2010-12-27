#region

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public class SinkingState : BattleshipState {
		private readonly Grid _grid;

		private readonly List<Shot> _hitShots = new List<Shot>();
		private readonly SearchingState _searchingState;

		public SinkingState( SearchingState searchingState, Grid grid, Point firstShot ) {
			_searchingState = searchingState;
			_grid = grid;
			_grid.Fired( firstShot );
			_hitShots.Add( _grid[ firstShot ] );
			NextState = this;
		}

		public override Shot NextShot() {
			return Candidates().First();
		}

		public override void ShotHit( Point shot ) {
			_grid.Fired( shot );
			_hitShots.Add( _grid[ shot ] );
		}

		public override void ShotHitAndSink( Point shot, Ship sunkShip ) {
			_grid.Fired( shot );
			_hitShots.Add( _grid[ shot ] );
			_grid.Sunk( sunkShip );

			sunkShip.GetAllLocations()
				.Do( point => _hitShots.Remove( _grid[ point ] ) );

			if ( !_hitShots.Any() ) {
				NextState = _searchingState;
			}
		}

		public override void ShotMiss( Point shot ) {
			_grid.Fired( shot );
		}

		public IEnumerable<Shot> Candidates() {
			var allCandidates = _hitShots.SelectMany( shot => shot.GetNeighbours( _grid ) );

			var validCandidates =
				allCandidates.Where( candidate => candidate.IsAvailable && !_hitShots.Contains( candidate ) );

			if ( _hitShots.Count == 1 ) {
				return validCandidates;
			}

			//TODO: Refactor this!
			var row = _hitShots.First().Position.Y;
			var haveSameRow = _hitShots.All( shot => shot.Position.Y == row );
			if ( haveSameRow ) {
				var result = validCandidates.Where( shot => shot.Position.Y == row );
				return result.Count() == 0 ? validCandidates : result;
			}

			var column = _hitShots.First().Position.X;
			var haveSameColumn = _hitShots.All( shot => shot.Position.X == column );
			if ( haveSameColumn ) {
				var result = validCandidates.Where( shot => shot.Position.X == column );
				return result.Count() == 0 ? validCandidates : result;
			}

			return validCandidates;
		}
	}
}