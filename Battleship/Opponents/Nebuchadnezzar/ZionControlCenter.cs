using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar
{
	public class ZionControlCenter : IBattleshipOpponent {

		private Size _gameSize;
		private IOffenseStrategy _offense;
		private IDefenseStrategy _defense;

		public string Name {
			get 
			{
				return "Nebuchadnezzar (luKa)";
			}
		}

		public Version Version
		{
			get
			{
				return new Version(0, 8);
			}
		}
	
		public void NewMatch(string opponent) { }
		public void NewGame(Size size, TimeSpan timeSpan) {
			if (size != _gameSize)
			{
				_offense = new Offense.ProbabilityBasedOffenseStrategy();
				_defense = new Defense.UniformDistributionDefenceStrategy();
				_gameSize = size;
			}
		}
	
		public void PlaceShips(ReadOnlyCollection<Ship> ships) {
			_offense.StartGame();
			IList<Ship> placement = _defense.StartGame();
	  

			CopyPlacementOverToReturnedCollection(placement, ships);
		}


		public Point GetShot() {
			Point p = _offense.GetShot();
			return p;
		}
	
		public void OpponentShot(Point shot) {
			_defense.Shot(shot);
		}

		public void ShotHit(Point shot)
		{
			_offense.ShotHit(shot);
		}

		public void ShotHitAndSink(Point shot, Ship sunkShip)
		{
			_offense.ShotSunk(shot, sunkShip);
		}
	
		public void ShotMiss(Point shot) {
			_offense.ShotMiss(shot);
		}
	
		public void GameWon() {
			_offense.EndGame();
			_defense.EndGame();
		}
	
		public void GameLost() {
			_offense.EndGame();
			_defense.EndGame();
		}
	
		public void MatchOver() { }

		private static void CopyPlacementOverToReturnedCollection(IEnumerable<Ship> placement, IEnumerable<Ship> ships)
		{
			foreach (Ship s in placement)
			{
				foreach (Ship t in ships)
				{
					if (t.IsPlaced == false && t.Length == s.Length)
					{
						t.Place(s.Location, s.Orientation);
						break;
					}
				}
			}
		}

	}
}