#region

using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame.Tests
{
	public class ShotFixture {
		[SetUp]
		public void SetUp() {}

		[TearDown]
		public void TearDown() {}

		[Test]
		public void GetNeighbours_Returns_4Neighbours_WhenShotIsNotOnTheEdge() {
			/*
			 -- -- -- 
			|  |NN|  |
			 -- -- -- 
			|NN|XX|NN|
			 -- -- -- 
			|  |NN|  |
			 -- -- --
			 */
			var shot = new Shot( 1, 1 );
			var grid = new Grid( 3, 3 );

			var neighbours = shot.GetNeighbours( grid );

			var expected = new[] {
			                     	grid.At( 1, 0 ),
			                     	grid.At( 0, 1 ),
			                     	grid.At( 2, 1 ),
			                     	grid.At( 1, 2 )
			                     };

			CollectionAssert.AreEquivalent( expected, neighbours );
		}

		[Test]
		public void GetNeighbours_Returns_2Neighbours_WhenShotIsOnACorner() {
			/*
			 -- -- -- 
			|  |  |  |
			 -- -- -- 
			|NN|  |  |
			 -- -- -- 
			|XX|NN|  |
			 -- -- --
			 */
			var shot = new Shot( 0, 2 );
			var grid = new Grid( 3, 3 );

			var neighbours = shot.GetNeighbours( grid );

			var expected = new[] {
			                     	grid.At( 0, 1 ),
			                     	grid.At( 1, 2 )
			                     };

			CollectionAssert.AreEquivalent( expected, neighbours );
		}

		[Test]
		public void GetNeighbours_Return_SameInstancesOfShotInsideGrid() {
			var shot = new Shot( 0, 0 );
			var grid = new Grid( 2, 1 );

			var neighbours = shot.GetNeighbours( grid );
			var expected = grid.At( 1, 0 );
			Assert.AreSame( expected, neighbours.Single() );
		}

		[Test]
		public void GetDistance_ReturnsMaxDistance_FromNotAvailableShot() {
			/*
			  0  1  2  3  4  
			  -- -- -- -- --  
			0|  |  |XX|  |  |
			  -- -- -- -- --  
			1|  |  |OO|  |XX|
			 -- -- -- -- --  
			2|  |  |  |  |  |
			 -- -- -- -- --  
			3|  |  |  |  |  |
			 -- -- -- -- --  
			4|  |  |XX|  |  |
			 -- -- -- -- --   
			*/

			var grid = new Grid( 5, 5 );
			var shot = new Shot( 2, 1 );

			grid.At( 2, 0 ).Fired();
			grid.At( 4, 1 ).Fired();
			grid.At( 2, 4 ).Fired();

			var distance = shot.GetDistance( grid );

			Assert.That( distance.Horizontal, Is.EqualTo( 4 ) );
			Assert.That( distance.Vertical, Is.EqualTo( 3 ) );
		}

		[Test]
		public void GetDistance_ReturnsOne_IfShotIsSurroundedByNotAvailable() {
			/*
			  0  1  2  3  4  
			  -- -- -- -- --  
			0|OO|XX|  |  |  |
			  -- -- -- -- --  
			1|XX|  |  |  |  |
			 -- -- -- -- --  
			2|  |  |  |  |  |
			 -- -- -- -- --  
			3|  |  |  |  |  |
			 -- -- -- -- --  
			4|  |  |  |  |  |
			 -- -- -- -- --   
			*/

			var grid = new Grid( 5, 5 );
			var shot = new Shot( 0, 0 );

			grid.At( 1, 0 ).Fired();
			grid.At( 0, 1 ).Fired();

			var distance = shot.GetDistance( grid );

			Assert.That( distance.Horizontal, Is.EqualTo( 1 ) );
			Assert.That( distance.Vertical, Is.EqualTo( 1 ) );
		}
	}
}