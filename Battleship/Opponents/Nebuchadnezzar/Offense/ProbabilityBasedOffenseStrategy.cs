using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public class ProbabilityBasedOffenseStrategy : IOffenseStrategy
	{
		private IOpponentBattlefield _opponentBattlefield;
		private IPartiallySinkShipsOracle _partiallySinkShipsOracle;
		private IEmptyCellsOracle _defaultOracle;
		private int _gamesCountStatistic = 0;
#if DEBUG_NEBUCHADNEZZAR
		private int _gameWinsStatistic = 0;
#endif
		private readonly IProbabilityBasedOffenseStrategyFactory _factory;
		private readonly int[,] _hits;
		private readonly int[,] _misses;
		private readonly double[,] _initialProbability;

		public ProbabilityBasedOffenseStrategy()
			: this(new ProbabilityBasedOffenseStrategyFactory(), new int[Battlefield.Size, Battlefield.Size], new int[Battlefield.Size, Battlefield.Size])
		{
		}


		public ProbabilityBasedOffenseStrategy(IProbabilityBasedOffenseStrategyFactory factory, int[,] hits, int[,] misses)
		{
			CheckStatisticParameter(hits, "hits");
			CheckStatisticParameter(misses, "misses");

			_factory = factory;
			_hits = hits;
			_misses = misses;


			_initialProbability = new[,] // The sum is 17, the total number of cells of ships of the navy.
			{
				{0.079998864, 0.1149213, 0.143477034, 0.158654918, 0.166711471, 0.166711471, 0.158654918, 0.143477034, 0.1149213, 0.079998864}, 
				{0.1149213, 0.142630673, 0.165505561, 0.177721122, 0.184202804, 0.184202804, 0.177721122, 0.165505561, 0.142630673, 0.1149213}, 
				{0.143477034, 0.165505561, 0.18407125, 0.194094264, 0.199432565, 0.199432565, 0.194094264, 0.18407125, 0.165505561, 0.143477034}, 
				{0.158654918, 0.177721122, 0.194094264, 0.203413072, 0.208422349, 0.208422349, 0.203413072, 0.194094264, 0.177721122, 0.158654918}, 
				{0.166711471, 0.184202804, 0.199432565, 0.208422349, 0.213599364, 0.213599364, 0.208422349, 0.199432565, 0.184202804, 0.166711471}, 
				{0.166711471, 0.184202804, 0.199432565, 0.208422349, 0.213599364, 0.213599364, 0.208422349, 0.199432565, 0.184202804, 0.166711471}, 
				{0.158654918, 0.177721122, 0.194094264, 0.203413072, 0.208422349, 0.208422349, 0.203413072, 0.194094264, 0.177721122, 0.158654918}, 
				{0.143477034, 0.165505561, 0.18407125, 0.194094264, 0.199432565, 0.199432565, 0.194094264, 0.18407125, 0.165505561, 0.143477034}, 
				{0.1149213, 0.142630673, 0.165505561, 0.177721122, 0.184202804, 0.184202804, 0.177721122, 0.165505561, 0.142630673, 0.1149213}, 
				{0.079998864, 0.1149213, 0.143477034, 0.158654918, 0.166711471, 0.166711471, 0.158654918, 0.143477034, 0.1149213, 0.079998864}
			};
		}

		public void StartGame()
		{
			_factory.CreateDependencies(out _opponentBattlefield, out _partiallySinkShipsOracle, out _defaultOracle);
		}

		public Point GetShot()
		{
			Point shot;
			if (_opponentBattlefield.HasHitsOnUnsinkShips() == false)
			{
				shot = _defaultOracle.GuessTheBestShotOnAnEmptyCell(EstimatedProbabilities());
			}
			else
			{
				shot = _partiallySinkShipsOracle.GuessTheBestShotOnAPartiallySinkShip(EstimatedProbabilities());
			}

			
			_opponentBattlefield.TellBattlefiels(
				delegate(BattlefieldCellState[,] battlefield)
				{
#if DEBUG_NEBUCHADNEZZAR_OFFENSE
					for (int y = battlefield.GetLength(1) -1; y >= 0 ; y--)
					{
						for (int x = 0; x < battlefield.GetLength(0); ++x)
						{
							if (shot.X == x && shot.Y == y)
							{
								Console.Write("0");
								continue;
							}

							switch (battlefield[x, y])
							{
								case BattlefieldCellState.Empty:
									Console.Write("·");
									break;
								case BattlefieldCellState.Hit:
									Console.Write("X");
									break;
								case BattlefieldCellState.Sink:
									Console.Write("#");
									break;
								case BattlefieldCellState.Miss:
									Console.Write("+");
									break;
							}
						}
					Console.WriteLine();
					}
					Console.WriteLine();
#endif
				});
			
			return shot;
		}

		private double[,] EstimatedProbabilities()
		{
			// Estimation/approximation:
			// Here the probabilities are calculated as if each ship could be placed on the battlefield
			// indipendently on the other ships. So the probability is calculated simply as the sum
			// of the independent probability of every single ship.
			// In reality when a ship is placed on the battlefield, it constraint how other ships 
			// can be placed on the battlefield, the probability depend on all possible ship 
			// combinations. And the number of combinations is huge and take hours to compute them all.
			
			var calculatedProbability = new double[Battlefield.Size,Battlefield.Size];
			for (int x = 0; x < Battlefield.Size; x++)
			{
				for (int y = 0; y < Battlefield.Size; y++)
				{

					IEnumerable<Ship> unsinkShipsThatCouldBePlacedHere =_opponentBattlefield.UnsinkShipsThatCouldBePlacedHere(new Point(x, y));

					foreach (var shipThatCouldBePlacedHere in unsinkShipsThatCouldBePlacedHere)
					{
						foreach (var cell in shipThatCouldBePlacedHere.GetAllLocations())
						{
							int hits = _hits[cell.X, cell.Y];
							int misses = _misses[cell.X, cell.Y];
							int shots = hits + misses;

							const double simulatedShotsCount = 10;
							double simulatedHits = simulatedShotsCount * _initialProbability[cell.X, cell.Y];
							double probability = (hits + simulatedHits) / (shots + simulatedShotsCount);
							//const double fakeShots = 10;
							//double fakeHits = fakeShots * Navy.NavyTotalLengh/(Battlefield.Size*Battlefield.Size);
							//double probability = (hits + fakeHits) / (shots + fakeShots);
							calculatedProbability[cell.X, cell.Y] += probability; 
						}
					}
				}				
			}

			return calculatedProbability;
		}

		public void ShotMiss(Point p)
		{
			_opponentBattlefield.Miss(p);
			_misses[p.X, p.Y] += 1;
		}

		public void ShotHit(Point p)
		{
			_opponentBattlefield.Hit(p);
			_hits[p.X, p.Y] += 1;
		}

		public void ShotSunk(Point p, Ship ship)
		{
			_opponentBattlefield.HitAndSink(ship);
			_hits[p.X, p.Y] += 1;
		}

		public void EndGame()
		{
			_gamesCountStatistic += 1;
			_opponentBattlefield.TellStatistics(delegate(int totalShots, int missShots, int hitShots, int sinkShips, int unsinkShips)
			                                    	{
#if DEBUG_NEBUCHADNEZZAR

														string result;

														if (unsinkShips == 0)
														{
			                                    			result =  "Win!   ";
															_gameWinsStatistic += 1;
														}
														else
														{
			                                    			result =  "Lost :(";
															
														}

														result += string.Format(" {0,2}:{1,-2}", _gameWinsStatistic, _gamesCountStatistic - _gameWinsStatistic);
			                                    		int opponentHitsPercentage = Navy.NavyTotalLengh * 100 / totalShots;
			                                    		Console.WriteLine(
			                                    			string.Format(
																"{0} - Shots: {1}, Hits: {2:00}%, Miss: {3:00}%, Ships unsink: {4} - Winning Opponent Hits: {5:00}%, Miss: {6:00}%",
																result, totalShots, hitShots * 100 / totalShots, missShots * 100 / totalShots, unsinkShips, opponentHitsPercentage, 100 - opponentHitsPercentage)
			                                    		);
#endif
			                                    	});

		}

		private static void CheckStatisticParameter(int[,] parameter, string parameterName)
		{
			if (parameter == null)
			{
				throw new ArgumentNullException(parameterName);
			}

			if (parameter.GetLength(0) != Battlefield.Size || parameter.GetLength(0) != Battlefield.Size)
			{
				throw new ArgumentOutOfRangeException(parameterName);
			}
		}

	}
}
