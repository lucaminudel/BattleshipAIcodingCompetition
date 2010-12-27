#region

using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame.Tests
{
	public class FillHolesShotProviderFixture {
		private Grid _grid;

		private FillHolesShotProvider CreateFillHolesShotProvider( int maxShipSize ) {
			return new FillHolesShotProvider( _grid, maxShipSize );
		}

		[SetUp]
		public void SetUp() {
			_grid = new Grid( 5, 5 );
		}

		[TearDown]
		public void TearDown() {}

		[Test]
		public void NoShotAreAvailable_WhenMaxSize_IsBiggerThanHole() {
			/*
			  0  1  2  3  4  
			  -- -- -- -- --  
			0|XX|XX|XX|XX|XX|
			  -- -- -- -- --  
			1|XX|XX|  |XX|XX|
			 -- -- -- -- --  
			2|XX|XX|  |XX|XX|
			 -- -- -- -- --  
			3|XX|XX|  |XX|XX|
			 -- -- -- -- --  
			4|XX|XX|XX|XX|XX|
			 -- -- -- -- --   
			*/

			FillGrid();
			_grid.At( 2, 1 ).IsAvailable = true;
			_grid.At( 2, 2 ).IsAvailable = true;
			_grid.At( 2, 3 ).IsAvailable = true;

			var maxShipSize = 4;

			var provider = CreateFillHolesShotProvider( maxShipSize );

			CollectionAssert.IsEmpty( provider.Shots(), "If hole is smaller than maxSize, no points should be available" );
		}

		//NOT TRUE!
		//[Test]
		//public void MidPointShouldBePreferred() {
		//    /*
		//      0  1  2  3  4  
		//      -- -- -- -- --  
		//    0|XX|XX|XX|XX|XX|
		//      -- -- -- -- --  
		//    1|XX|XX|  |XX|XX|
		//     -- -- -- -- --  
		//    2|XX|XX|OO|XX|XX|
		//     -- -- -- -- --  
		//    3|XX|XX|  |XX|XX|
		//     -- -- -- -- --  
		//    4|XX|XX|XX|XX|XX|
		//     -- -- -- -- --   
		//    */

		//    FillAllGrid();
		//    _grid.At( 2, 1 ).IsAvailable = true;
		//    _grid.At( 2, 2 ).IsAvailable = true;
		//    _grid.At( 2, 3 ).IsAvailable = true;

		//    var maxShipSize = 3;

		//    var provider = CreateFillHolesShotProvider( _grid, maxShipSize );

		//    var actual = provider.Shots().ToList();

		//    Assert.That( actual.Count, Is.EqualTo( 1 ) );
		//    Assert.That( actual[ 0 ], Is.SameAs( _grid.At( 2, 2 ) ), "Midpoint should be preferred" );
		//}

		[Test]
		public void MidPointsShouldBePreferred_IfChoicesAreEquivalent() {
			/*
			  0  1  2  3  4  
			  -- -- -- -- --  
			0|XX|XX|  |XX|XX|
			  -- -- -- -- --  
			1|XX|XX|OO|XX|XX|
			 -- -- -- -- --  
			2|XX|XX|OO|XX|XX|
			 -- -- -- -- --  
			3|XX|XX|  |XX|XX|
			 -- -- -- -- --  
			4|XX|XX|XX|XX|XX|
			 -- -- -- -- --   
			*/

			FillGrid();
			_grid.At( 2, 0 ).IsAvailable = true;
			_grid.At( 2, 1 ).IsAvailable = true;
			_grid.At( 2, 2 ).IsAvailable = true;
			_grid.At( 2, 3 ).IsAvailable = true;

			var maxShipSize = 3;

			var provider = CreateFillHolesShotProvider( maxShipSize );

			var actual = provider.Shots().ToList();

			var expected = new[] {
			                     	_grid.At( 2, 1 ),
			                     	_grid.At( 2, 2 )
			                     };

			Assert.That( actual.Count, Is.EqualTo( 2 ) );
			CollectionAssert.AreEquivalent( expected, actual );
		}

		[Test]
		public void ShotAtCrossBetweenTwoDirection_ShouldBePreferred() {
			/*
			  0  1  2  3  4  
			  -- -- -- -- --  
			0|XX|XX|  |XX|XX|
			  -- -- -- -- --  
			1|XX|  |OO|  |  |
			 -- -- -- -- --  
			2|XX|XX|  |XX|XX|
			 -- -- -- -- --  
			3|XX|XX|  |XX|XX|
			 -- -- -- -- --  
			4|XX|XX|XX|XX|XX|
			 -- -- -- -- --   
			*/

			FillGrid();
			_grid.At( 2, 0 ).IsAvailable = true;
			_grid.At( 2, 1 ).IsAvailable = true;
			_grid.At( 2, 2 ).IsAvailable = true;
			_grid.At( 2, 3 ).IsAvailable = true;

			_grid.At( 1, 1 ).IsAvailable = true;
			_grid.At( 3, 1 ).IsAvailable = true;

			var maxShipSize = 3;

			var provider = CreateFillHolesShotProvider( maxShipSize );

			var actual = provider.Shots().ToList();

			var expected = new[] {
			                     	_grid.At( 2, 1 )
			                     };

			CollectionAssert.AreEquivalent( expected, actual, "Crosspoint should be preferred" );
		}

		[Test]
		public void ShotAtCrossBetweenTwoDirection_WithSameLenght_ShouldBePreferred_() {
			/*
			  0  1  2  
			  -- -- -- 
			0|  |  |XX|
			  -- -- -- 
			1|XX|  |XX|
			 -- -- -- -
			2|XX|XX|XX|
			 -- -- -- -
			*/

			_grid = new Grid( 3, 3 );
			FillGrid();
			_grid.At( 0, 0 ).IsAvailable = true;
			_grid.At( 1, 0 ).IsAvailable = true;
			_grid.At( 1, 1 ).IsAvailable = true;

			var maxShipSize = 2;

			var provider = CreateFillHolesShotProvider( maxShipSize );

			var actual = provider.Shots().ToList();

			var expected = new[] {
			                     	_grid.At( 1, 0 )
			                     };

			CollectionAssert.AreEquivalent( expected, actual );
		}

		[Test]
		public void AllBestPoints_ShouldBeReturned_WhenDistanceIsMoreThenMaxShipSize() {
			/*
			  0  1  2  3  4  
			  -- -- -- -- --  
			0|  |OO|  |XX|XX|
			  -- -- -- -- --  
			1|XX|XX|XX|XX|XX|
			 -- -- -- -- --  
			2|XX|XX|  |XX|XX|
			 -- -- -- -- --  
			3|XX|XX|OO|XX|XX|
			 -- -- -- -- --  
			4|XX|XX|  |XX|XX|
			 -- -- -- -- --   
			*/

			FillGrid();
			_grid.At( 0, 0 ).IsAvailable = true;
			_grid.At( 1, 0 ).IsAvailable = true;
			_grid.At( 2, 0 ).IsAvailable = true;

			_grid.At( 2, 2 ).IsAvailable = true;
			_grid.At( 2, 3 ).IsAvailable = true;
			_grid.At( 2, 4 ).IsAvailable = true;

			var maxShipSize = 3;

			var provider = CreateFillHolesShotProvider( maxShipSize );

			var actual = provider.Shots().ToList();

			Assert.That( actual.Count, Is.GreaterThanOrEqualTo( 2 ) );

			Assert.IsTrue( actual.Any( shot => shot.Position.Y == 0 ) );
			Assert.IsTrue( actual.Any( shot => shot.Position.X == 2 ) );
		}

		[Test]
		public void FewPoint_ShouldBeUsed() {
			/*
			  0  1  2  3  4  5  6  7 
			  -- -- -- -- -- -- -- --
			0|  |  |XX|  |  |XX|  |  |
			  -- -- -- -- -- -- -- --
			*/

			_grid = new Grid( 8, 1 );

			var maxShipSize = 3;

			var provider = CreateFillHolesShotProvider( maxShipSize );

			var actual = provider.Shots().ToList();

			var expected = new[] {
			                     	_grid.At( 2, 0 ),
			                     	_grid.At( 5, 0 )
			                     };

			CollectionAssert.AreEquivalent( expected, actual );
		}

		private void FillGrid() {
			foreach ( var shot in _grid ) {
				shot.Fired();
			}
		}
	}
}