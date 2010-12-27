// The part of the Dreadnought battleship program which handles
// defense - the placement of ships on the board to avoid the
// shots from the opponent.

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.Dreadnought
{
	public class Defense : IDefense {
		int w;
		int h;
		Random rand = new Random();
    
		bool place_notouching = false;   // if true, no generated boards have touching ships.
		bool standard_touching = false;  // if true, touching is ignored
		// otherwise, we thin out touching to about 1/4 of generated boards.

		// statistics kept about opponent's behavior
		int nshots_in_game;
		int[,] opponent_shots;

		public Defense(Size size, List<String> options) {
			w = size.Width;
			h = size.Height;
			place_notouching = options.Exists(x => x == "place_notouching");
			standard_touching = options.Exists(x => x == "standard_touching");
			opponent_shots = new int[w, h];
		}
    
		public List<Ship> startGame(int[] ship_sizes) {
			List<Ship> placement = placeShips(ship_sizes);
			print_placement(placement);
			nshots_in_game = 0;
			return placement;
		}
		public void shot(Point p) {
			opponent_shots[p.X, p.Y] += Math.Max(1, 50 - nshots_in_game);
			nshots_in_game++;
		}
		public void endGame() {
		}
    
		private List<Ship> placeShips(int[] ship_sizes) {
			int max_opp_shots = 0;
			foreach (int x in opponent_shots) max_opp_shots = Math.Max(max_opp_shots, x);
			max_opp_shots++;  // prevent divide by 0

#if DEBUG_DREADNOUGHT
			Console.WriteLine("square shot scores");
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					Console.Write("{0,-4} ", 1000 * opponent_shots[x, y] / max_opp_shots);
				}
				Console.WriteLine();
			}
#endif

			// generate 100 valid placements
			const int N = 100;
			List<List<Ship>> allocations = new List<List<Ship>>();
			for (int n = 0; n < N; n++) {
				List<Ship> allocation = new List<Ship>();
				foreach (int size in ship_sizes) {
					Ship s = new Ship(size);
					while (true) {
						int x = rand.Next(w);
						int y = rand.Next(h);
						int orient = rand.Next(2);
						s.Place(new Point(x, y), (ShipOrientation)orient);
						if (!s.IsValid(new Size(w, h))) continue;
            
						bool ok = true;
						foreach (Ship t in allocation) {
							if (s.ConflictsWith(t)) { ok = false; break; }
						}
						if (ok) break;
					}
					allocation.Add(s);
				}
				allocations.Add(allocation);
			}
      
			// score the allocations, pick best one
			int minscore = 1000000000;
			List<Ship> min_allocation = null;
			foreach (List<Ship> allocation in allocations) {
				int score = 0;
				foreach (Ship s in allocation) {
					foreach (Point p in s.GetAllLocations()) {
						score += 100 * opponent_shots[p.X, p.Y] / max_opp_shots;
					}
					foreach (Ship t in allocation) {
						if (!standard_touching && shipsAdjacent(s, t)) score += 20;
						if (place_notouching && shipsAdjacent(s, t)) score += 1000000;
					}
				}
				score += rand.Next(15); // some inherent randomness
				if (score < minscore) {
					minscore = score;
					min_allocation = allocation;
				}
			}
			return min_allocation;
		}


		// returns true if these ships touch.
		private bool shipsAdjacent(Ship s, Ship t) {
			foreach (Point p in s.GetAllLocations()) {
				if (t.IsAt(p + new Size(1,0))) return true;
				if (t.IsAt(p + new Size(-1,0))) return true;
				if (t.IsAt(p + new Size(0,1))) return true;
				if (t.IsAt(p + new Size(0,-1))) return true;
			}
			return false;
		}
    
		private void print_placement(List<Ship> ships) {
			int adj = 0;
			for (int i = 0; i < ships.Count; i++) {
				Ship s = ships[i];
				for (int j = i+1; j < ships.Count; j++) {
					Ship t = ships[j];
					if (shipsAdjacent(s,t)) adj++;
				}
			}
      
			char[,] placement = new char[w,h];
			for (int x = 0; x < w; x++) {
				for (int y = 0; y < h; y++) {
					placement[x,y] = '.';
				}
			}
			foreach (Ship s in ships) {
				foreach (Point p in s.GetAllLocations()) {
					placement[p.X, p.Y] = (char)('0' + s.Length);
				}
			}
#if DEBUG_DREADNOUGHT
			Console.WriteLine("placement {0}:", adj);
			for (int y = 0; y < h; y++) {
				Console.Write("  ");
				for (int x = 0; x < w; x++) {
					Console.Write(placement[x,y]);
				}
				Console.WriteLine();
			}
#endif
		}
	}
}