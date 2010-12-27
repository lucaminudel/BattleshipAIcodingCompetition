using System;
using System.Collections.Generic;
using System.Drawing;
using Battleship.Opponents.Nebuchadnezzar.Tests;
using NUnit.Framework;

namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	[TestFixture]
	public class OpponentBattlefieldTest
	{
		[Test]
		public void After_HitAndSink_the_ship_should_be_removed()
		{
			var unsinkShipsList = new List<Ship>()
			                  	{
			                  		Navy.NewDestroyer(),
			                  		Navy.NewSubmarine(),
			                  		Navy.NewAircraftcarrier()
			                  	};
			var shipToSink =
				NavyBuilder.AShip()
				.ThatIsLikeA(Navy.NewSubmarine())
				.AtLocation(3, 4).WithOrientation(ShipOrientation.Horizontal)
				.Build();
			var target = new OpponentBattlefield(unsinkShipsList);


			target.HitAndSink(shipToSink);


			target.TellStatistics(delegate(int totalShots, int missShots, int hitShots, int sinkShips, int unsinkShips)
			                      	{
										Assert.AreEqual(1, sinkShips, "Sink ships count");
										Assert.AreEqual(2, unsinkShips, "Unsink ships count");
			                      	});
		}

		[Test]
		public void After_HitAndSink_the_ship_should_be_removed_even_when_there_are_2_similar_ships()
		{
			var unsinkShipsList = new List<Ship>()
			                  	{
			                  		Navy.NewSubmarine(),
			                  		Navy.NewSubmarine(),
			                  		Navy.NewAircraftcarrier()
			                  	};
			var submarineToSink =
				NavyBuilder.AShip()
				.ThatIsLikeA(Navy.NewSubmarine())
				.AtLocation(3, 4).WithOrientation(ShipOrientation.Horizontal)
				.Build();
			var target = new OpponentBattlefield(unsinkShipsList);


			target.HitAndSink(submarineToSink);


			target.TellStatistics(delegate(int totalShots, int missShots, int hitShots, int sinkShips, int unsinkShips)
			{
				Assert.AreEqual(1, sinkShips, "Sink ships count");
				Assert.AreEqual(2, unsinkShips, "Unsink ships count");
			});
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void The_sink_of_a_non_existing_ship_should_raise_an_exception()
		{
			var unsinkShipsList = new List<Ship>()
			                  	{
			                  		Navy.NewBattleship(),
			                  		Navy.NewAircraftcarrier()
			                  	};
			var submarineToSink =
				NavyBuilder.AShip()
				.ThatIsLikeA(Navy.NewSubmarine())
				.AtLocation(3, 4).WithOrientation(ShipOrientation.Horizontal)
				.Build();
			var target = new OpponentBattlefield(unsinkShipsList);


			target.HitAndSink(submarineToSink);
		}

		[Test]
		public void bug_CountAdjacentCellsEmptyOrHit_should_not_raise_IndexOutOfRangeException_when_dx_or_dy_is_negative()
		{
			var target = new OpponentBattlefield();

			int count;

			count = target.CountAdjacentCellsEmptyOrHit(new Point(5, 5), -1, 0);
			Assert.AreEqual(5, count, "horrizontal adjacent cells count");

			count = target.CountAdjacentCellsEmptyOrHit(new Point(5, 5), 0, -1);
			Assert.AreEqual(5, count, "vertical adjacent cells count");
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CountAdjacentCellsHit_should_raise_ArgumentOutOfRangeException_when_dx_and_dy_are_both_zero()
		{
			var target = new OpponentBattlefield();

			target.CountAdjacentCellsHit(new Point(5, 5), 0, 0);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CountAdjacentCellsEmptyOrHit_should_raise_ArgumentOutOfRangeException_when_dx_and_dy_are_both_zero()
		{
			var target = new OpponentBattlefield();

			target.CountAdjacentCellsEmptyOrHit(new Point(5, 5), 0, 0);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CountAdjacentCellsEmpty_should_raise_ArgumentOutOfRangeException_when_dx_and_dy_are_both_zero()
		{
			var target = new OpponentBattlefield();

			target.CountAdjacentCellsEmpty(new Point(5, 5), 0, 0);
		}


		[Test, Ignore("funzione da testare")]
		public void UnsinkShipsThatCouldBePlacedHere_test()
		{
			
		}
	}
}
