using System.Drawing;
using System.Linq;
using Battleship.Opponents.Nebuchadnezzar.Tests;
using NUnit.Framework;

namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	[TestFixture]
	public class PartiallySinkShipsOracleTest
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
		public void When_there_is_an_hit_with_2_adjacent_empty_cells_GetSHot_should_hit_the_most_adjacent_empty_cell()
		{
			var hit = Cell.At(4, 6);
			var emptyCellsNextToTheHit = new[] { Cell.At(5, 6), Cell.At(6, 6) };
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
							.But()
							.WithTheseCellsMarkedAsHit(hit)
							.WithTheseCellsMarkedAsEmpty(emptyCellsNextToTheHit)
							.Build();
			var target = new PartiallySinkShipsOracle(opponentBattlefield);

			var shot = target.GuessTheBestShotOnAPartiallySinkShip((_allWeightsZero));

			Assert.AreEqual(Cell.At(5, 6), shot, "shot cell");
		}

		[Test]
		public void When_there_is_an_hit_with_adjacent_empty_cells_left_and_right_and_up_and_down_GetShot_should_hit_the_most_adjacent_cell_on_the_longest_side()
		{
			var hit = Cell.At(6, 5);
			var emptyCellsNextToTheHitLeftAndRight = new[] { Cell.At(5, 5), Cell.At(7, 5) };
			var emptyCellsNextToTheHitUpANdDown = new[] { Cell.At(6, 4), Cell.At(6, 6), Cell.At(6, 7) };
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
							.But()
							.WithTheseCellsMarkedAsHit(hit)
							.WithTheseCellsMarkedAsEmpty(emptyCellsNextToTheHitLeftAndRight)
							.WithTheseCellsMarkedAsEmpty(emptyCellsNextToTheHitUpANdDown)
							.Build();
			var target = new PartiallySinkShipsOracle(opponentBattlefield);

			var shot = target.GuessTheBestShotOnAPartiallySinkShip(_allWeightsZero);

			Assert.AreEqual(Cell.At(6, 6), shot, "shot cell");
		}

		[Test]
		public void With_two_adjacent_hits_GetShot_should_still_hit_the_adjacent_cell_on_the_longest_side()
		{
			var adjacentHits = new[] { Cell.At(6, 5), Cell.At(7, 5) };
			var emptyCellsNextToTheHit = new[] { Cell.At(5, 5), Cell.At(8, 5), Cell.At(9, 5) };
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
							.But()
							.WithTheseCellsMarkedAsHit(adjacentHits)
							.WithTheseCellsMarkedAsEmpty(emptyCellsNextToTheHit)
							.Build();
			var target = new PartiallySinkShipsOracle(opponentBattlefield);


			var shot = target.GuessTheBestShotOnAPartiallySinkShip(_allWeightsZero);


			Assert.AreEqual(Cell.At(8, 5), shot, "shot cell");
		}

		[Test]
		public void With_two_alligned_adjacent_hits_and_possible_shots_both_horrizontal_and_vertica_GetShot_should_try_to_continue_with_a_third_alligned_shot()
		{
			var adjacentHits = new[] { Cell.At(5, 6), Cell.At(5, 7) };
			var verticalEmptyCellsNextToTheHit = new[] { Cell.At(5, 5), Cell.At(5, 8), Cell.At(5, 9) };
			var horizontalEmptyCellsNextToTheHit = new[] { Cell.At(4, 6), Cell.At(6, 6), Cell.At(7, 6), Cell.At(8, 6) };
			var moreHorizontalEmptyCellsNextToTheHit = new[] { Cell.At(1, 7), Cell.At(2, 7), Cell.At(3, 7), Cell.At(4, 7) };
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
							.But()
							.WithTheseCellsMarkedAsHit(adjacentHits)
							.WithTheseCellsMarkedAsEmpty(verticalEmptyCellsNextToTheHit)
							.WithTheseCellsMarkedAsEmpty(horizontalEmptyCellsNextToTheHit)
							.WithTheseCellsMarkedAsEmpty(moreHorizontalEmptyCellsNextToTheHit)
							.Build();
			var target = new PartiallySinkShipsOracle(opponentBattlefield);

			var shot = target.GuessTheBestShotOnAPartiallySinkShip(_allWeightsZero);



			CollectionAssert.Contains(new[] { Cell.At(5, 5), Cell.At(5, 8) }, shot, "shot cell");
		}

		[Test]
		public void When_linear_shot_search_fails_GetShot_should_use_touching_ships_search()
		{
			var adjacentHits = new[] { Cell.At(5, 6), Cell.At(5, 7) };
			var horizontalEmptyCellsNextToTheHit = new[] { Cell.At(4, 6), Cell.At(6, 6), Cell.At(7, 6), Cell.At(8, 6) };
			var moreHorizontalEmptyCellsNextToTheHit = new[] { Cell.At(1, 7), Cell.At(2, 7), Cell.At(3, 7), Cell.At(4, 7) };
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
							.But()
							.WithTheseCellsMarkedAsHit(adjacentHits)
							.WithTheseCellsMarkedAsEmpty(horizontalEmptyCellsNextToTheHit)
							.WithTheseCellsMarkedAsEmpty(moreHorizontalEmptyCellsNextToTheHit)
							.Build();
			var target = new PartiallySinkShipsOracle(opponentBattlefield);


			var shot = target.GuessTheBestShotOnAPartiallySinkShip(_allWeightsZero);

			Assert.AreEqual(Cell.At(4, 7), shot, "shot cell");
		}



		[Test]
		public void GetShot_should_hit_adjacent_cell_with_higher_weight_multiplied_probability()
		{
			var freeCellWithLowWeight = Cell.At(2, 3);
			const int lowWeight = 5;
			_weights[2, 3] = lowWeight;

			var freeCellWithHighWeight = Cell.At(4, 3);
			const int highWeight = 15;
			_weights[4, 3] = highWeight;

			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
								.But()
								.WithTheseCellsMarkedAsHit(Cell.At(3, 3))
								.WithTheseCellsMarkedAsEmpty(freeCellWithLowWeight, freeCellWithHighWeight)
								.Build();

			var target = new PartiallySinkShipsOracle(opponentBattlefield);


			var shotCell = target.GuessTheBestShotOnAPartiallySinkShip(_weights);


			Assert.AreEqual(freeCellWithHighWeight, shotCell, "shot cell");
		}

		[Test]
		public void GetShot_should_hit_adjacent_cell_with_higher_weight_multiplied_probability_even_whet_it_is_not_the_most_adjacent()
		{
			var freeCellWithLowWeight = Cell.At(4, 3);
			const int lowWeight = 5;
			_weights[4, 3] = lowWeight;

			var freeCellWithHighWeight = Cell.At(5, 3);
			const int highWeight = 14 + 1;
			_weights[5, 3] = highWeight;

			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
								.But()
								.WithTheseCellsMarkedAsHit(Cell.At(3, 3))
								.WithTheseCellsMarkedAsEmpty(freeCellWithLowWeight, freeCellWithHighWeight)
								.Build();

			var target = new PartiallySinkShipsOracle(opponentBattlefield);


			var shotCell = target.GuessTheBestShotOnAPartiallySinkShip(_weights);


			Assert.AreEqual(freeCellWithHighWeight, shotCell, "shot cell");
		}

		[Test]
		public void When_only_ships_with_lenght_2_are_available_adjacent_cell_3_places_away_is_not_shot_even_when_have_the_highest_weight_per_probability()
		{
			var hit = Cell.At(4, 5);
			var horizontalEmptyCellsNextToTheHit = new[] { Cell.At(3, 5), Cell.At(5, 5) };

			var thirdHorizontalCellWithByFarHighestWeightPerProbability = Cell.At(6, 5);
			const int byFarHighestWeightPerProbability = 1000;
			_weights[6, 5] = byFarHighestWeightPerProbability;

			var verticalEmptyCellsNextToTheHit = new[] { Cell.At(4, 4), Cell.At(4, 6) };
			var opponentBattlefield = _anOpponentBattlefieldWithAllShotsMissed
				.But()
				.WhithTheseUnsinkShips(Navy.NewPatrolBoat(), Navy.NewPatrolBoat())
				.WithTheseCellsMarkedAsHit(hit)
				.WithTheseCellsMarkedAsEmpty(verticalEmptyCellsNextToTheHit)
				.WithTheseCellsMarkedAsEmpty(horizontalEmptyCellsNextToTheHit)
				.WithTheseCellsMarkedAsEmpty(thirdHorizontalCellWithByFarHighestWeightPerProbability)
				.Build();

			var target = new PartiallySinkShipsOracle(opponentBattlefield);


			var shot = target.GuessTheBestShotOnAPartiallySinkShip(_weights);


			var expectedShots = horizontalEmptyCellsNextToTheHit.Concat(verticalEmptyCellsNextToTheHit);
			CollectionAssert.Contains(expectedShots, shot, "shot cell");
		}


		[Test, Ignore("feature da definire e verificare")]
		public void With_two_adjacent_non_consecutive_hits_define_the_expected_behaviour()
		{
			Assert.Fail();
		}

	}
}
