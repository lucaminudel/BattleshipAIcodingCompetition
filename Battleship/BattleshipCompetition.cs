namespace Battleship
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;

    public class BattleshipCompetition
    {
		private readonly IBattleshipOpponent _op1;
		private readonly IBattleshipOpponent _op2;
        private readonly TimeSpan _timePerGame;
        private readonly int _wins;
        private readonly bool _playOut;
        private readonly Size _boardSize;
        private readonly List<int> _shipSizes;

		public BattleshipCompetition(IBattleshipOpponent op1, IBattleshipOpponent op2, TimeSpan timePerGame, int wins, bool playOut, Size boardSize, params int[] shipSizes)
        {
            if (op1 == null)
            {
                throw new ArgumentNullException("op1");
            }

            if (op2 == null)
            {
                throw new ArgumentNullException("op2");
            }

            if (timePerGame.TotalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException("timePerGame");
            }

            if (wins <= 0)
            {
                throw new ArgumentOutOfRangeException("wins");
            }

            if (boardSize.Width <= 2 || boardSize.Height <= 2)
            {
                throw new ArgumentOutOfRangeException("boardSize");
            }

            if (shipSizes == null || shipSizes.Length < 1)
            {
                throw new ArgumentNullException("shipSizes");
            }

            if (shipSizes.Where(s => s <= 0).Any())
            {
                throw new ArgumentOutOfRangeException("shipSizes");
            }

            if (shipSizes.Sum() >= (boardSize.Width * boardSize.Height))
            {
                throw new ArgumentOutOfRangeException("shipSizes");
            }

            _op1 = op1;
            _op2 = op2;
            _timePerGame = timePerGame;
            _wins = wins;
            _playOut = playOut;
            _boardSize = boardSize;
            _shipSizes = new List<int>(shipSizes);
        }

        public Dictionary<IBattleshipOpponent, int> RunCompetition()
        {
            var rand = new Random();

			var opponents = new Dictionary<int, IBattleshipOpponent>();
            var scores = new Dictionary<int, int>();
            var times = new Dictionary<int, Stopwatch>();
            var ships = new Dictionary<int, List<Ship>>();
            var shots = new Dictionary<int, List<Point>>();


			var first = 0;
			InitializeOpponentMatch(_op1, first, opponents, scores, times, shots);

        	var second = 1;
			InitializeOpponentMatch(_op2, second, opponents, scores, times, shots);

            if (rand.NextDouble() >= 0.5)
            {
                var swap = first;
                first = second;
                second = swap;
            }

            opponents[first].NewMatch(opponents[second].Name + " " + opponents[second].Version.ToString());
            opponents[second].NewMatch(opponents[first].Name + " " + opponents[first].Version.ToString());

            while (true)
            {
                if ((!_playOut && scores.Where(p => p.Value >= _wins).Any()) || (_playOut && scores.Sum(s => s.Value) >= (_wins * 2 - 1)))
                {
                    break;
                }

                {
                    var swap = first;
                    first = second;
                    second = swap;
                }

                times[first].Reset();
                times[second].Reset();
                shots[first].Clear();
                shots[second].Clear();

                StartNewGame(first, opponents, times);
				if (times[first].Elapsed > _timePerGame) { RecordTimeoutWin(second, first, scores, opponents); continue; }

				StartNewGame(second, opponents, times);
				if (times[second].Elapsed > _timePerGame) { RecordTimeoutWin(first, second, scores, opponents); continue; }

                PlaceShipsAndVerifyConflictsAndValidity(first, opponents, times, ships);
				if (times[first].Elapsed > _timePerGame) { RecordTimeoutWin(second, first, scores, opponents); continue; }

				PlaceShipsAndVerifyConflictsAndValidity(second, opponents, times, ships);
				if (times[second].Elapsed > _timePerGame) { RecordTimeoutWin(first, second, scores, opponents); continue; }

                var current = first;
                while (true)
                {
                    times[current].Start();
                    Point shot = opponents[current].GetShot();
                    times[current].Stop();
					if (times[current].Elapsed > _timePerGame) { RecordTimeoutWin(1 - current, current, scores, opponents); break; }

                    if (shots[current].Where(s => s.X == shot.X && s.Y == shot.Y).Any())
                    {
                        continue;
                    }

                    shots[current].Add(shot);

                    times[1 - current].Start();
                    opponents[1 - current].OpponentShot(shot);
                    times[1 - current].Stop();
					if (times[1 - current].Elapsed > _timePerGame) { RecordTimeoutWin(current, 1 - current, scores, opponents); break; }

                    var ship = (from s in ships[1 - current]
                                where s.IsAt(shot)
                                select s).SingleOrDefault();

                    if (ship != null)
                    {
                        var sunk = ship.IsSunk(shots[current]);

                        times[current].Start();
						if (sunk)
						{
							opponents[current].ShotHitAndSink(shot, ship);														
						}
						else
						{
							opponents[current].ShotHit(shot);							
						}
                        times[current].Stop();
						if (times[current].Elapsed > _timePerGame) { RecordTimeoutWin(1 - current, current, scores, opponents); break; }
                    }
                    else
                    {
                        times[current].Start();
                        opponents[current].ShotMiss(shot);
                        times[current].Stop();
						if (times[current].Elapsed > _timePerGame) { RecordTimeoutWin(1 - current, current, scores, opponents); break; }
                    }

                	int key = current;
					var unsunk = (from s in ships[1 - key]
								  where !s.IsSunk(shots[key])
								  select s);

                    if (!unsunk.Any()) { RecordWin(current, 1 - current, scores, opponents); break; }

                    current = 1 - current;
                }
            }

            opponents[first].MatchOver();
            opponents[second].MatchOver();

            return scores.Keys.ToDictionary(s => opponents[s], s => scores[s]);
        }

    	private static void InitializeOpponentMatch(IBattleshipOpponent opponent, int bibNumber, IDictionary<int, IBattleshipOpponent> opponents, IDictionary<int, int> scores, IDictionary<int, Stopwatch> times, IDictionary<int, List<Point>> shots)
    	{
    		opponents[bibNumber] = opponent;
    		scores[bibNumber] = 0;
    		times[bibNumber] = new Stopwatch();
    		shots[bibNumber] = new List<Point>();
    	}

		private void PlaceShipsAndVerifyConflictsAndValidity(int bibNumber, IDictionary<int, IBattleshipOpponent> opponents, IDictionary<int, Stopwatch> times, IDictionary<int, List<Ship>> ships)
		{
			bool success = false;

			do
			{
				ships[bibNumber] = (from s in _shipSizes
									select new Ship(s)).ToList();

				times[bibNumber].Start();
				opponents[bibNumber].PlaceShips(ships[bibNumber].AsReadOnly());
				times[bibNumber].Stop();
				if (times[bibNumber].Elapsed > _timePerGame) { break; }

				bool allPlacedValidly = true;
				for (int i = 0; i < ships[bibNumber].Count; i++)
				{
					if (!ships[bibNumber][i].IsPlaced || !ships[bibNumber][i].IsValid(_boardSize))
					{
						allPlacedValidly = false;
						break;
					}
				}

				if (!allPlacedValidly)
				{
					continue;
				}

				bool noneConflict = true;
				for (int i = 0; i < ships[bibNumber].Count; i++)
				{
					for (int j = i + 1; j < ships[bibNumber].Count; j++)
					{
						if (ships[bibNumber][i].ConflictsWith(ships[bibNumber][j]))
						{
							noneConflict = false;
							break;
						}
					}

					if (!noneConflict)
					{
						break;
					}
				}

				if (!noneConflict)
				{
					continue;
				}
				else
				{
					success = true;
				}
			} while (!success);
		}

		private void StartNewGame(int bibNumber, IDictionary<int, IBattleshipOpponent> opponents, IDictionary<int, Stopwatch> times)
		{
			times[bibNumber].Start();
			opponents[bibNumber].NewGame(_boardSize, _timePerGame);
			times[bibNumber].Stop();
		}

		private static void RecordTimeoutWin(int winner, int loser, IDictionary<int, int> scores, IDictionary<int, IBattleshipOpponent> opponents)
		{
#if DEBUG_FRAMEWORK
			Console.Write("({0} {1} time-out) ", opponents[loser].Name, opponents[loser].Version);
#endif
			RecordWin(winner, loser, scores, opponents);
		}

    	private static void RecordWin(int winner, int loser, IDictionary<int, int> scores, IDictionary<int, IBattleshipOpponent> opponents)
		{
			scores[winner]++;
            opponents[winner].GameWon();
            opponents[loser].GameLost();
#if DEBUG_FRAMEWORK
    		char[] winnerChar = new char[] {'<', '>'};
			Console.WriteLine("{0} {1} Vs {2} {3}{4}  {5,2}:{6,-2} ", opponents[0].Name, opponents[0].Version, opponents[1].Name, opponents[1].Version, winnerChar[winner], scores[0], scores[1]);
#endif
		}
    }
}
