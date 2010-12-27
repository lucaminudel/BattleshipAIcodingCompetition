#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	internal class DFBattleshipOpponent : IBattleshipOpponent {
		private readonly Random _random = new Random();
		private readonly bool _verbose;
		private Size _gameSize;
		private Grid _grid;
		private BattleshipState _state;

		private MatchStats _stats;

		public DFBattleshipOpponent() : this( false ) {}

		public DFBattleshipOpponent( bool verbose ) {
			_verbose = verbose;
		}

		#region IBattleshipOpponent Members
		public string Name {
			get { return "Deathflame"; }
		}

		public Version Version {
			get { return new Version( 0, 1, 0, 1 ); }
		}

		public void NewMatch( string opponent ) {
			_stats = new MatchStats();
		}

		public void NewGame( Size size, TimeSpan timeSpan ) {
			_stats.NewGame();
			_gameSize = size;
			_grid = new Grid( size );
			_state = new SearchingState( _grid );
		}

		public void PlaceShips( ReadOnlyCollection<Ship> ships ) {
			PlaceShipsRandomly( ships );
		}

		public Point GetShot() {
			var shot = _state.NextShot();
			_stats.Shot();
			//Print( "GetShot from {0}(): {1} ", _state.GetType().Name, shot );
			return shot.Position;
		}

		public void OpponentShot( Point shot ) {
			// It's possible to record if opponent has some pattern or something. Nothing for now
		}

		public void ShotHit( Point shot ) {
			//Print( "ShotHit(): " + shot );
			_state.ShotHit( shot );
			_state = _state.NextState;
		}

		public void ShotHitAndSink( Point shot, Ship sunkShip ) {
			//Print( "ShotHitAndSink(): {0}, {1}", shot, sunkShip );
			_state.ShotHitAndSink( shot, sunkShip );
			_state = _state.NextState;
		}

		public void ShotMiss( Point shot ) {
			//Print( "ShotMiss(): " + shot );
			_state.ShotMiss( shot );
			_state = _state.NextState;
		}

		public void GameWon() {
			_stats.GamWon();
			PrintGameStat( "WON" );
		}

		public void GameLost() {
			Print( _grid.ToString() );
			_stats.GameLost();
			PrintGameStat( "LOST" );
		}

		public void MatchOver() {}
		#endregion

		private void PrintGameStat( string wonOrLost ) {
			Print( "{0}: " + wonOrLost + " with {1} shots -- Total: W {2} / L {3}", _stats.GameCount, _stats.ShotCount,
			       _stats.WonCount, _stats.LostCount );
		}

		private void Print( string format, params object[] parameters ) {
			if ( _verbose ) {
				Console.WriteLine( format, parameters );
			}
		}

		private void PlaceShipsRandomly( IEnumerable<Ship> ships ) {
			foreach ( var ship in ships ) {
				ship.Place(
					new Point(
						_random.Next( _gameSize.Width ),
						_random.Next( _gameSize.Height ) ),
					(ShipOrientation) _random.Next( 2 ) );
			}
		}
	}
}