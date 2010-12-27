using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Defense
{
	public class UniformDistributionDefenceStrategy : IDefenseStrategy
	{
		private readonly int[,] _shipsKnownPositionsStatistic = new int[Battlefield.Size, Battlefield.Size];
		private int _totalHits = 0;
		private IList<Ship> _currentGameBattlefield;

		public IList<Ship> StartGame()
		{
			const float mutationRate = 10f / 100;
			int[,] weights = _shipsKnownPositionsStatistic;

			var fintessStrategy = new CalculateFitnessWithMinimumWeightStrategy(weights);
			var battelfieldPopulation = new BattelfieldPopulation(mutationRate, fintessStrategy);


			bool targetReached = false;
			//for (int i = 0; i < 250 && targetReached == false; i++) // with 2 seconds time per game
			for (int i = 0; i < 1100 && targetReached == false; i++) // with 4 seconds time per game
			{
				battelfieldPopulation.ProcreateTheNewGeneration(out targetReached);
			}

			battelfieldPopulation.VisitPopulation(delegate(int generation, float bestFitness, float avarageFitness, float totalFitness, IList<Ship> battleField)
			{
#if DEBUG_NEBUCHADNEZZAR_DEFENSE
				double average = GetAverage(weights);
				double stdev = GetStdev(weights);
				double stdevPercentage = GetStdev(weights) * 100 / average;
				float bestFitnessPerCell = bestFitness / Navy.NavyTotalLengh;
				System.Console.WriteLine(string.Format("Defense Fitness: {0:0.00} - Avg: {1:0.00} - Stdev: {2:0.00} = {3:0.00}% Fitness dev: {4:0.00}%", bestFitnessPerCell, average, stdev, stdevPercentage, (bestFitnessPerCell - average) * 100 / average));
#endif
				_currentGameBattlefield = battleField;
			});

			return _currentGameBattlefield;
		}

		public void Shot(Point p)
		{

			foreach(Ship ship in _currentGameBattlefield)
			{
				if (ship.IsAt(p))
				{
					_shipsKnownPositionsStatistic[p.X, p.Y] += 1;
					_totalHits += 1;
					break;
				}
			}
		}

		public void EndGame()
		{
			_currentGameBattlefield = null;
		}

#if DEBUG_NEBUCHADNEZZAR_DEFENSE
		private static double GetVariance(int[,] data)
		{
			int len = data.Length;
			
			double avg = GetAverage(data);

			double sum = 0;
			foreach (int value in data)
			{
				sum += System.Math.Pow((value - avg), 2);				
			}

			return sum / len;
		}

		private static double GetStdev(int[,] data)
		{
			return System.Math.Sqrt(GetVariance(data));
		}

		private static double GetAverage(int[,] data)
		{
			int len = data.Length;
			if (len == 0)
				throw new System.Exception("No data");

			double sum = 0;
			foreach (int value in data)
			{
				sum += value;				
			}

			return sum / len;
		}
#endif
	}
}
