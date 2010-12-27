using System.Drawing;
using Battleship.Opponents.Nebuchadnezzar.Tests;
using NUnit.Framework;

namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	[TestFixture]
	public class ProbabilityBasedOffenseStrategyTest
	{
		private StubOpponentBattlefieldBuilder _stubOpponentBattlefieldBuilder;
		private MockPartiallySinkShipsOracle _mockPartiallySinkShipsOracle;
		private MockEmptyCellsOracle _mockEmptyCellsOracle;
		private StubProbabilityBasedOffenseStrategyFactory _stubProbabilityBasedOffenseStrategyFactory;
		private int[,] _hits;
		private int[,] _misses;
		private ProbabilityBasedOffenseStrategy _target;

		[SetUp]
		public void SetUp()
		{
			_stubOpponentBattlefieldBuilder = new StubOpponentBattlefieldBuilder();
			_mockPartiallySinkShipsOracle = new MockPartiallySinkShipsOracle();
			_mockEmptyCellsOracle = new MockEmptyCellsOracle();
			_stubProbabilityBasedOffenseStrategyFactory = new StubProbabilityBasedOffenseStrategyFactory();
			_stubProbabilityBasedOffenseStrategyFactory.SetCreateDependenciesReturnValues(_stubOpponentBattlefieldBuilder,
			                                                                          _mockPartiallySinkShipsOracle,
			                                                                          _mockEmptyCellsOracle);
			_hits = new int[Battlefield.Size, Battlefield.Size];
			_misses = new int[Battlefield.Size, Battlefield.Size];

			_target = new ProbabilityBasedOffenseStrategy(_stubProbabilityBasedOffenseStrategyFactory, _hits, _misses);
			_target.StartGame();
		}

		[Test]
		public void When_there_is_a_cell_with_an_hit_GetShot_should_search_the_next_shot_with_the_PartiallySinkShipsOracle()
		{
			_stubOpponentBattlefieldBuilder.SetHasHitsOnUnsinkShipsReturnValue(true);

			_target.GetShot();

			Assert.AreEqual(_mockPartiallySinkShipsOracle.GuessTheBestShotOnAPartiallySinkShipCallsCount, 1, "expected calls to PartiallySinkShipsOracle");
			Assert.AreEqual(_mockEmptyCellsOracle.GuessTheBestShotOnAnEmptyCellCallsCount, 0, "expected calls to EmptyCellsOracle");
		}

		[Test]
		public void When_there_are_no_hits_GetShot_should_search_the_next_shot_with_the_EmptyCellsOracle()
		{
			_stubOpponentBattlefieldBuilder.SetHasHitsOnUnsinkShipsReturnValue(false);

			_target.GetShot();

			Assert.AreEqual(_mockPartiallySinkShipsOracle.GuessTheBestShotOnAPartiallySinkShipCallsCount, 0, "expected calls to PartiallySinkShipsOracle");
			Assert.AreEqual(_mockEmptyCellsOracle.GuessTheBestShotOnAnEmptyCellCallsCount, 1, "expected calls to EmptyCellsOracle");
		}

		[Test]
		public void bug_ShotSunk_should_increase_the_hits()
		{
			_target.ShotSunk(new Point(2,2), NavyBuilder.AShip().ThatIsLikeA(Navy.NewBattleship()).Build());

			Assert.AreEqual(1, _hits[2, 2], "hits increment");
		}




	}
}
