// Part of the Dreadnought battleship program which handles
// offense - the choice of shots to sink the opponent's ships.

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.Dreadnought
{
	public class Offense : IOffense {
		private int w;
		private int h;
		private Random rand = new Random();
		public GameState state;
		private int apriori_types = 2;
		private int apriori_type;
		private int total_ship_sizes;
    
		// option flags
		public bool fully_resolve_hits;
		public bool assume_notouching;

		// statistics kept about opponent's layout behavior
#if DEBUG_DREADNOUGHT
		int shots_in_game;
#endif
		int[,] statistics_shot_hit;
		int[,] statistics_shot_miss;

		public Offense(Size size, List<String> options) {
			w = size.Width;
			h = size.Height;
			statistics_shot_hit = new int[w, h];
			statistics_shot_miss = new int[w, h];
#if DEBUG_DREADNOUGHT
			print_apriori();
#endif
			apriori_type = rand.Next(apriori_types);
			apriori_type = 0; // TODO: remove
			fully_resolve_hits = options.Exists(x => x == "fully_resolve_hits");
			assume_notouching = options.Exists(x => x == "assume_notouching");
		}
    
		public void startGame(int[] ship_sizes) {
			state = new GameState(w, h, ship_sizes);
			total_ship_sizes = 0;
			foreach (int i in ship_sizes) total_ship_sizes += i;
#if DEBUG_DREADNOUGHT
			shots_in_game = 0;
#endif
		}
		public void shotMiss(Point shot) {
			state.addMiss(shot);
			statistics_shot_miss[shot.X, shot.Y]++;
		}
		public void shotHit(Point shot) {
			state.addHit(shot); 
			statistics_shot_hit[shot.X, shot.Y]++;
		}
		public void shotSunk(Point shot) {
			state.addSunk(shot);
			if (assume_notouching) {
				foreach (Point p in getSquares()) {
					if (state.get(p) == SeaState.HIT || state.get(p) == SeaState.SUNK) {
						foreach (Point q in neighbors(p)) {
							if (state.get(q) == SeaState.CLEAR) {
								state.addMiss(q);
								break;
							}
						}
					}
				}
			}
			statistics_shot_hit[shot.X, shot.Y]++;
		}
		public void endGame()
		{
#if DEBUG_DREADNOUGHT
			Console.WriteLine("history probability");
			for (int y = 0; y < h; y++) {
				Console.Write("   ");
				for (int x = 0; x < w; x++) {
					double p = history_probability(x, y);
					Console.Write(" {0,-4}", (int)(1000*p));
				}
				Console.WriteLine();
			}
#endif
		}
    
		public Point getShot()
		{
#if DEBUG_DREADNOUGHT
			Console.WriteLine("getting shot {0}", shots_in_game++);
			state.print();
#endif
			List<Point> choices = getShot_ExtendShips();
			if (choices.Count > 0)
			{
#if DEBUG_DREADNOUGHT
				Console.Write("extendships ");
				foreach (Point p in choices) Console.Write("{0} ", p);
				Console.WriteLine();
#endif
				return choices[rand.Next(choices.Count)];
			}
			Point r = getShot_Random();
#if DEBUG_DREADNOUGHT
			foreach (Point q in neighbors(r)) {
				if (state.get(q) == SeaState.HIT || state.get(q) == SeaState.SUNK) Console.WriteLine("adjacent to ship!");
			}
#endif
			return r;
		}
    
		// returns all squares on the board.
		private IEnumerable<Point> getSquares() {
			for (int x = 0; x < w; x++) {
				for (int y = 0; y < h; y++) {
					yield return new Point(x, y);
				}
			}
		}

		private static Size[] dirs = {new Size(1, 0), new Size(-1, 0),
		                              new Size(0, 1), new Size(0, -1)};
    
		// returns the <= 4 neighbor squares of the given point
		//  0
		// 1*2
		//  3
		private IEnumerable<Point> neighbors(Point p) {
			foreach (Size d in dirs) {
				Point q = p + d;
				if (q.X >= 0 && q.X < w && q.Y >= 0 && q.Y < h) yield return q;
			}
		}
    
		// public for testing
		public List<Point> getShot_ExtendShips() {
			List<Point> choices = new List<Point>();
      
			if (!fully_resolve_hits) {
				foreach (List<Ship> list in state.ship_possibilities) {
					if (state.allSunk(list)) return choices;
				}
			}
      
			// algorithm: choose spot which, if a miss, maximizes the
			// number of ship layout possibilities (weighted by probability)
			// which we eliminate.
			double[,] weight = new double[w,h];
			foreach (List<Ship> list in state.ship_possibilities) {
				double wt = state.probability(list);
				foreach (Ship s in list) {
					foreach (Point p in s.GetAllLocations()) {
						if (state.get(p) == SeaState.CLEAR) {
							weight[p.X, p.Y] += wt;
						}
					}
				}
			}
#if DEBUG_DREADNOUGHT
			Console.WriteLine("weights:");
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					Console.Write("{0,-4} ", (int)(1000*weight[x, y]));
				}
				Console.WriteLine();
			}
#endif

			// return maximum weight squares
			double maxw = 0.0;
			foreach (Point p in getSquares()) {
				if (weight[p.X, p.Y] > maxw) {
					maxw = weight[p.X, p.Y];
					choices.Clear();
				}
				if (weight[p.X, p.Y] == maxw) {
					choices.Add(p);
				}
			}
			return choices;
		}
    
		private Point getShot_Random() {

			// find out which hits are definitely sunk and which might still be
			// on live ships.
			bool[,] possible_unsunk_hits = new bool[w, h];
			foreach (List<Ship> list in state.ship_possibilities) {
				foreach (Ship s in list) {
					if (state.isSunk(s)) continue;
					foreach (Point p in s.GetAllLocations()) {
						if (state.get(p) == SeaState.HIT) possible_unsunk_hits[p.X, p.Y] = true;
					}
				}
			}
#if DEBUG_DREADNOUGHT
			Console.WriteLine("possible unsunk hits");
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					Console.Write("{0}", possible_unsunk_hits[x,y] ? 'H' : '.');
				}
				Console.WriteLine();
			}
#endif

			// find out which squares could hold the remaining ships, and if so
			// what the probability of each square is.
			double[,] ship_prob = new double[w, h];
			foreach (int len in state.remaining_ship_sizes()) {
				double[,] aposteriori_prob = new double[w, h];
				for (int x = 0; x < w; x++) {
					for (int y = 0; y < h; y++) {
						for (int orient = 0; orient < 2; orient++) {
							int dy = orient;
							int dx = 1 - dy;
							if (x + len * dx > w) continue;
							if (y + len * dy > h) continue;
							bool good = true;
							for (int i = 0; i < len; i++) {
								SeaState st = state.get(x + i*dx, y + i*dy);
								if (!(st == SeaState.CLEAR ||
								      (!fully_resolve_hits && st == SeaState.HIT && possible_unsunk_hits[x + i*dx, y + i*dy]))) {
								      	good = false;
								      	break;
								      }
							}
							if (!good) continue;
							double p = apriori_prob(len, new Point(x, y), (ShipOrientation) orient);
							bool next_to_other_ship = false;
							for (int i = 0; i < len; i++) {
								foreach (Point n in neighbors(new Point(x + i*dx, y + i*dy))) {
									if (state.get(n) == SeaState.HIT || state.get(n) == SeaState.SUNK) next_to_other_ship = true;
								}
							}
							if (next_to_other_ship) p *= .2;  // TODO: set to .2?
							for (int i = 0; i < len; i++) {
								if (state.get(x + i*dx, y + i*dy) == SeaState.HIT) continue;
								double wt = history_probability(x + i*dx, y + i*dy);
								aposteriori_prob[x + i*dx, y + i*dy] += p * wt;
							}
						}
					}
				}
				// normalize aposteriori probability
				if (!normalize_prob(aposteriori_prob)) {
					// this condition triggers when there is no place
					// to put a ship of a particular size.
					continue;
				}
        
				// TODO: weight sum of probabilities, for example
				// to prioritize the finding of the 2-ship?
				for (int x = 0; x < w; x++) {
					for (int y = 0; y < h; y++) {
						ship_prob[x,y] += aposteriori_prob[x,y];
					}
				}
			}
      
			double max_prob = 0;
			foreach (double p in ship_prob) if (p > max_prob) max_prob = p;

#if DEBUG_DREADNOUGHT
			Console.WriteLine("random choice probabilities");
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					Console.Write("{0,-3}{1} ", (int)(ship_prob[x,y] * 1000), ship_prob[x,y] == max_prob ? "*" : " ");
				}
				Console.WriteLine();
			}
#endif

			// pick random one of the prob maximizing spots
			List<Point> max_points = new List<Point>();
			for (int x = 0; x < w; x++) {
				for (int y = 0; y < h; y++) {
					if (ship_prob[x, y] == max_prob) {
						max_points.Add(new Point(x, y));
					}
				}
			}
      
			return max_points[rand.Next(max_points.Count)];
		}
    
		double apriori_prob(int len, Point p, ShipOrientation orient) {
			if (apriori_type == 0) {
				// uniform distribution
				if (orient == ShipOrientation.Horizontal) return 1.0 / (w - len + 1) / h;
				else return 1.0 / (h - len + 1) / w;
			} else if (apriori_type == 1) {
				// weighted towards edge
				double scale = Math.Sqrt(2.0) - 1; // factor of 2.0 from center to edge
				if (orient == ShipOrientation.Horizontal) {
					int min = 0;
					int max = w - len;
					double mid = (max - min) / 2.0;
					double r = 1.0 + scale * Math.Abs(p.X - mid) / (max - mid);
          
					min = 0;
					max = h - 1;
					mid = (max - min) / 2.0;
					r *= 1.0 + scale * Math.Abs(p.Y - mid) / (max - mid);
					return r;
				} else {
					int min = 0;
					int max = w - 1;
					double mid = (max - min) / 2.0;
					double r = 1.0 + scale * Math.Abs(p.X - mid) / (max - mid);
          
					min = 0;
					max = h - len;
					mid = (max - min) / 2.0;
					r *= 1.0 + scale * Math.Abs(p.Y - mid) / (max - mid);
					return r;
				}
			} else {
				return 0.0;
			}
		}
    
		// normalizes entries in prob so they total 1.0.
		private bool normalize_prob(double[,] prob) {
			double total_prob = 0.0;
			foreach (double x in prob) total_prob += x;
			if (total_prob == 0) return false;
      
			for (int x = 0; x < prob.GetLength(0); x++) {
				for (int y = 0; y < prob.GetLength(1); y++) {
					prob[x,y] /= total_prob;
				}
			}
			return true;
		}

		// returns the fraction of shots at this square that resulted
		// in a hit (smoothed somewhat).
		private double history_probability(int x, int y) {
			// These estimates are hard to do in general, as we don't pick
			// our sampling points randomly.  But we don't need exact answers
			// here, so we assume our samples were chosen randomly.  We
			// use a simple frequency-based approach.
      
			int hits = statistics_shot_hit[x, y];
			int misses = statistics_shot_miss[x, y];
			int shots = hits + misses;
      
			// # of samples to weight prior (17/100) and current data 50/50.
			double fake_shots = 25;
			double fake_hits = fake_shots * total_ship_sizes / (w * h);
      
			double res = (hits + fake_hits) / (shots + fake_shots);
			return res;
		}
    
		void print_apriori() {
			for (apriori_type = 0; apriori_type < apriori_types; apriori_type++) {
				Console.WriteLine("apriori type: {0}", apriori_type);
				for (int size = 2; size <= 5; size++) {
					for (int orient = 0; orient < 2; orient++) {
						Console.WriteLine("  {0}{1}:", size, orient == 0 ? "H" : "V");
						double sum = 0.0;
						for (int y = 0; y < h; y++) {
							Console.Write("   ");
							for (int x = 0; x < w; x++) {
								double p;
								if ((orient == 0 && x + size > w) || (orient == 1 && y + size > h)) {
									p = 0.0;
								} else {
									p = apriori_prob(size, new Point(x, y), (ShipOrientation)orient);
								}
								Console.Write(" {0,-4}", (int)(10000*p));
								sum += p;
							}
							Console.WriteLine();
						}
						Console.WriteLine("    sum: {0}", sum);
					}
				}
			}
		}
	}
}