using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.Dreadnought
{
	public class GameState {
		SeaState[,] state;
		List<int> orig_ship_sizes;
		Size boardSize;

		Dictionary<int,int> size_counts;
    
		// possible positions for ships.  Keyed by ship size.
		Dictionary<int,List<Ship>> positions;
    
		// list of possibilities for ships with at least one hit.
		public List<List<Ship>> ship_possibilities;

		public GameState(int w, int h, int[] ship_sizes) {
			boardSize = new Size(w, h);
			state = new SeaState[w, h];
			orig_ship_sizes = new List<int>();
			size_counts = new Dictionary<int,int>();
			foreach (int s in ship_sizes) {
				orig_ship_sizes.Add(s);
				if (!size_counts.ContainsKey(s)) size_counts[s] = 0;
				size_counts[s]++;
			}
			ship_possibilities = new List<List<Ship>>();
			ship_possibilities.Add(new List<Ship>());
      
			int max_size = 0;
			foreach (int size in orig_ship_sizes) if (size > max_size) max_size = size;

			positions = new Dictionary<int,List<Ship>>();
			foreach (int len in size_counts.Keys) {
				positions[len] = new List<Ship>();
				for (int x = 0; x < w; x++) {
					for (int y = 0; y < h; y++) {
						for (int orient = 0; orient < 2; orient++) {
							int dy = orient;
							int dx = 1 - dy;
							if (x + len * dx > w) continue;
							if (y + len * dy > h) continue;
							Ship s = new Ship(len);
							s.Place(new Point(x, y), (ShipOrientation)orient);
							positions[len].Add(s);
						}
					}
				}
			}
		}
    
		public bool valid() {
			// no place for some ship
			foreach (int size in size_counts.Keys) {
				if (positions[size].Count < size_counts[size]) return false;
			}
			// something inconsistent about possibilities
			if (ship_possibilities.Count == 0) return false;
			return true;
		}
    
		public SeaState get(int x, int y) {
			if (!valid()) throw new ApplicationException("get on bad state");
			return state[x, y];
		}
		public SeaState get(Point p) {
			return get(p.X, p.Y);
		}
    
		public List<int> remaining_ship_sizes() {
			if (!valid()) throw new ApplicationException("get on bad state");
			int max_size = 0;
			foreach (int size in orig_ship_sizes) if (size > max_size) max_size = size;
			int[] orig_histo = new int[max_size + 1];
			foreach (int size in orig_ship_sizes) orig_histo[size]++;
      
			int[] max_histo = new int[max_size + 1];
			int[] histo = new int[max_size + 1];
			foreach (List<Ship> list in ship_possibilities) {
				for (int i = 0; i <= max_size; i++) histo[i] = orig_histo[i];
				foreach (Ship s in list) {
					histo[s.Length]--;
				}
				for (int i = 0; i <= max_size; i++) {
					if (histo[i] > max_histo[i]) max_histo[i] = histo[i];
				}
			}
			List<int> sizes = new List<int>();
			for (int i = 0; i <= max_size; i++) {
				for (int j = 0; j < max_histo[i]; j++) {
					sizes.Add(i);
				}
			}
#if DEBUG_DREADNOUGHT
			Console.Write("remaining sizes:");
			foreach (int s in sizes) Console.Write(" {0}", s);
			Console.WriteLine();
#endif
			return sizes;
		}
    
		public void addMiss(Point p) {
			if (state[p.X, p.Y] != SeaState.CLEAR) {
				ship_possibilities.Clear();
				return;
			}
			state[p.X, p.Y] = SeaState.MISS;
			updatePossibilitiesMiss(p);
      
			// delete any possible ship positions containing the miss
			foreach (List<Ship> list in positions.Values) {
				int j = 0;
				for (int i = 0; i < list.Count; i++) {
					Ship s = list[i];
					if (!s.IsAt(p)) list[j++] = s;
				}
				list.RemoveRange(j, list.Count - j);
			}
		}
		public void addHit(Point p) {
			if (state[p.X, p.Y] != SeaState.CLEAR) {
				ship_possibilities.Clear();
				return;
			}
			state[p.X, p.Y] = SeaState.HIT;
			updatePossibilitiesHit(p);
		}
		public void addSunk(Point p) {
			if (state[p.X, p.Y] != SeaState.CLEAR) {
				ship_possibilities.Clear();
				return;
			}
			state[p.X, p.Y] = SeaState.SUNK;
			updatePossibilitiesSunk(p);
		}

		public void print() {
			for (int y = 0; y < boardSize.Height; y++) {
				for (int x = 0; x < boardSize.Width; x++) {
					char c = ' ';
					if (state[x,y] == SeaState.CLEAR) c = '.';
					if (state[x,y] == SeaState.MISS) c = '!';
					if (state[x,y] == SeaState.HIT) c = 'H';
					if (state[x,y] == SeaState.SUNK) c = 'S';
					Console.Write("{0}", c);
				}
				Console.WriteLine();
			}
			Console.WriteLine("ship possibilities: {0}", ship_possibilities.Count);
			if (ship_possibilities.Count <= 10) {
				foreach (List<Ship> list in ship_possibilities) {
					Console.Write("   ");
					foreach (Ship s in list) {
						Console.Write(" {0}({1},{2},{3})", s.Length, s.Location.X, s.Location.Y, s.Orientation);
					}
					Console.WriteLine();
				}
			}
		}

		private static Size[] dirs = {new Size(1, 0), new Size(-1, 0),
		                              new Size(0, 1), new Size(0, -1)};
    
		private IEnumerable<Point> getNeighbors(Point p) {
			foreach (Size d in dirs) {
				Point q = p + d;
				if (q.X >= 0 && q.X < boardSize.Width && q.Y >= 0 && q.Y < boardSize.Height) yield return q;
			}
		}

		public bool adjacent(Ship s, Ship t) {
			foreach (Point p in s.GetAllLocations()) {
				foreach (Point q in getNeighbors(p)) {
					if (t.IsAt(q)) return true;
				}
			}
			return false;
		}
    
		public bool isSunk(Ship s) {
			if (!valid()) throw new ApplicationException("isSunk on bad state");
			foreach (Point p in s.GetAllLocations()) {
				if (state[p.X, p.Y] == SeaState.SUNK) return true;
			}
			return false;
		}
    
		public bool allSunk(List<Ship> list) {
			foreach (Ship s in list) {
				if (!isSunk(s)) return false;
			}
			return true;
		}

		private bool isAt(List<Ship> list, Point p) {
			foreach (Ship s in list) {
				if (s.IsAt(p)) return true;
			}
			return false;
		}
    
		public double probability(List<Ship> list) {
			double r = 1;
			foreach (Ship s in list) {
				r *= probability(s);
			}
			foreach (Ship s in list) {
				foreach (Ship t in list) {
					if (s == t) continue;
					if (adjacent(s, t)) r *= 0.5;
				}
			}
			return r;
		}
    
		// probability that a given ship configuration appears
		// in a configuration.
		// indexed by (ship length, # of clears)
		private static double[][] probs = new double[][] {
		                                                 	new double[]{},
		                                                 	new double[]{},
		                                                 	new double[]{1, 1.0/16},   // size 2
		                                                 	new double[]{1, 1.0/4, 1.0/32}, // size 3
		                                                 	new double[]{1, 1.0/4, 1.0/8, 1.0/32},  // size 4
		                                                 	new double[]{1, 1.0/4, 1.0/8, 1.0/16, 1.0/32}, // size 5
		                                                 };
		public double probability(Ship s) {
			int clear_cnt = 0;
			foreach (Point p in s.GetAllLocations()) {
				if (state[p.X, p.Y] == SeaState.CLEAR) clear_cnt++;
			}
			return probs[s.Length][clear_cnt];
		}

		void updatePossibilitiesMiss(Point p) {
			int j = 0;
			for (int i = 0; i < ship_possibilities.Count; i++) {
				List<Ship> list = ship_possibilities[i];
				if (!isAt(list, p)) ship_possibilities[j++] = list;
			}
			ship_possibilities.RemoveRange(j, ship_possibilities.Count - j);
		}
    
		void updatePossibilitiesSunk(Point p) {
			// We take advantage of the fact that no ships are length 1, so
			// any sinking must be of a ship already hit, and thus already
			// in the possibilities array.
			int j = 0;
			for (int i = 0; i < ship_possibilities.Count; i++) {
				List<Ship> list = ship_possibilities[i];
        
				// find ship that was sunk
				Ship hit_ship = null;
				foreach (Ship s in list) {
					if (s.IsAt(p)) {
						hit_ship = s;
					}
				}
				if (hit_ship == null) continue;  // sink location wasn't on a ship
        
				// make sure the whole ship was hit (except for the SINK just registered)
				bool valid = true;
				foreach (Point q in hit_ship.GetAllLocations()) {
					if (q == p) continue; // the new sunk
					if (state[q.X, q.Y] != SeaState.HIT) {
						valid = false;
						break;
					}
				}
				if (!valid) continue;
        
				ship_possibilities[j++] = list;
			}
			ship_possibilities.RemoveRange(j, ship_possibilities.Count - j);
		}
    
		void updatePossibilitiesHit(Point p) {
			// This is the hard one.  If a hit was on a ship in the list,
			// check a few things.  Otherwise, we need to add to the list
			// all possible ships/positions that can cover the new hit.
			List<List<Ship>> new_possibilities = new List<List<Ship>>();
			foreach (List<Ship> list in ship_possibilities) {
				// find ship that was hit
				Ship hit_ship = null;
				foreach (Ship s in list) {
					if (s.IsAt(p)) {
						hit_ship = s;
					}
				}
				if (hit_ship != null) {
					// make sure the whole ship wasn't hit, because then this would have been a sink
					foreach (Point q in hit_ship.GetAllLocations()) {
						if (state[q.X, q.Y] == SeaState.CLEAR) {
							new_possibilities.Add(list);
							break;
						}
					}
					continue;
				}
        
				// Hit outside any current ship in the list.  Add all possible
				// new positions of a ship that intersects this point.
				List<int> t = new List<int>(orig_ship_sizes);
				foreach (Ship s in list) t.Remove(s.Length);
				List<int> possible_sizes = new List<int>();
				foreach (int v in t) { // remove duplicates
					if (!possible_sizes.Contains(v)) possible_sizes.Add(v);
				}
        
				foreach (int size in possible_sizes) {
					for (int offset = 0; offset < size; offset++) {
						for (int orient = 0; orient < 2; orient++) {
							int dy = orient;
							int dx = 1 - dy;
							int x = p.X - offset * dx;
							int y = p.Y - offset * dy;
							if (x < 0 || y < 0) continue;
							if (x + size * dx > boardSize.Width) continue;
							if (y + size * dy > boardSize.Height) continue;
							bool valid = true;
							for (int i = 0; i < size; i++) {
								if (i == offset) continue;  // the new hit
								if (state[x + i*dx, y + i*dy] != SeaState.CLEAR) {
									valid = false;
									break;
								}
							}
							if (!valid) continue;
							Ship s = new Ship(size);
							s.Place(new Point(x, y), (ShipOrientation)orient);
							foreach (Ship w in list) {
								if (s.ConflictsWith(w)) {
									valid = false;
									break;
								}
							}
							if (!valid) continue;
							List<Ship> new_list = new List<Ship>(list);
							new_list.Add(s);
							new_possibilities.Add(new_list);
						}
					}
				}
			}
			ship_possibilities = new_possibilities;
		}
	}
}