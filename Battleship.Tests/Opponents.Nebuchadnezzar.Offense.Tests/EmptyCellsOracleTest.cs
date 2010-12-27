using System;
using System.Drawing;
using Battleship.Opponents.Nebuchadnezzar.Tests;
using NUnit.Framework;

namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	[TestFixture]
	public class EmptyCellsOracleTest
	{
		private double[,] _weights;
		private double[,] _allWeightsZero;
		private OpponentBattlefieldBuilder _anOpponentBattlefieldWithAllShotsMissed;

		[SetUp]
		public void SetUp()
		{
			_weights = new double[Battlefield.Size, Battlefield.Size];
			_allWeightsZero = new double[Battlefield.Size, Battlefield.Size];
			_anOpponentBattlefieldWithAllShotsMissed =
				OpponentBattlefieldBuilder.AnOpponentBattlefield().WithAllCellsMarkedAs(BattlefieldCellState.Miss);
		}

		[Test]
		public void GetShot_should_return_the_only_empty_cell()
		{
			var freeCell = new Point(5, 8);
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed.But().WithTheseCellsMarkedAsEmpty(freeCell).Build();
			var target = new EmptyCellsOracle(opponentBattlefield);

			var shotCell = target.GuessTheBestShotOnAnEmptyCell(_allWeightsZero);

			Assert.AreEqual(freeCell, shotCell, "shot cell");
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void GetShot_with_no_more_empty_cells_should_fire_proper_exception()
		{
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed.Build();
			var target = new EmptyCellsOracle(opponentBattlefield);

			target.GuessTheBestShotOnAnEmptyCell(_allWeightsZero);
		}

		[Test]
		public void GetShot_should_return_the_empty_cell_with_higher_weight()
		{
			var freeCellWithLowWeight = new Point(3, 4);
			const int lowWeight = 5;
			_weights[3, 4] = lowWeight;

			var freeCellWithHighWeight = new Point(7, 8);
			const int highWeight = 15;
			_weights[7, 8] = highWeight;

			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
										.But()
										.WithTheseCellsMarkedAsEmpty(freeCellWithLowWeight, freeCellWithHighWeight)
										.Build();

			var target = new EmptyCellsOracle(opponentBattlefield);


			var shotCell = target.GuessTheBestShotOnAnEmptyCell(_weights);


			Assert.AreEqual(freeCellWithHighWeight, shotCell, "shot cell");
		}

	}
}
