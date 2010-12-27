using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.Dreadnought
{
	public class Dreadnought : IBattleshipOpponent {
	
		public string Name {
			get {
				String s = "Dreadnought";
				foreach (String opt in options) {
					s += "," + opt;
				}
				return s;
			}
		}
		public Version Version { get { return new Version(1, 2); } }
	
		Size gameSize;
		IOffense offense;
		IDefense defense;
		public List<String> options = new List<String>();

		public void setOption(String option) {
			options.Add(option);
		}
	
		public void NewMatch(string opponent) { }
		public void NewGame(Size size, TimeSpan timeSpan) {
			if (size != gameSize) {
				offense = new Offense(size, options);
				defense = new Defense(size, options);
				gameSize = size;
			}
		}
	
		public void PlaceShips(ReadOnlyCollection<Ship> ships) {
			int[] ship_sizes = new int[ships.Count];
			for (int i = 0; i < ships.Count; i++) ship_sizes[i] = ships[i].Length;
			offense.startGame(ship_sizes);
			List<Ship> placement = defense.startGame(ship_sizes);
	  
			// copy placement over to collection we're returning
			foreach (Ship s in placement) {
				foreach (Ship t in ships) {
					if (!t.IsPlaced && t.Length == s.Length) {
						t.Place(s.Location, s.Orientation);
						break;
					}
				}
			}
		}
	
		public Point GetShot() {
			Point p = offense.getShot();
#if DEBUG_DREADNOUGHT
			Console.WriteLine("shoot at {0},{1}", p.X, p.Y);
#endif
			return p;
		}
	
		public void OpponentShot(Point shot)
		{
#if DEBUG_DREADNOUGHT
			Console.WriteLine("opponent shot {0},{1}", shot.X, shot.Y);
#endif
			defense.shot(shot);
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
#if DEBUG_DREADNOUGHT
			Console.WriteLine("shot at {0},{1} hit{2}", shot.X, shot.Y, sunk ?  " and sunk" : "");
#endif
			if (sunk) offense.shotSunk(shot);
			else offense.shotHit(shot);
		}
	
		public void ShotMiss(Point shot) {
#if DEBUG_DREADNOUGHT
			Console.WriteLine("shot at {0},{1} missed", shot.X, shot.Y);
#endif
			offense.shotMiss(shot);
		}
	
		public void GameWon() {
#if DEBUG_DREADNOUGHT
			Console.WriteLine("game won");
#endif
			offense.endGame();
			defense.endGame();
		}
	
		public void GameLost()
		{
#if DEBUG_DREADNOUGHT
			Console.WriteLine("game lost");
#endif
			offense.endGame();
			defense.endGame();
		}
	
		public void MatchOver() { }
	}
}