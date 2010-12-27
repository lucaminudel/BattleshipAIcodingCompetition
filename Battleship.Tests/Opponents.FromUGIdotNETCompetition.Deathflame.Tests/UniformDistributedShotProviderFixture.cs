#region

using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame.Tests
{
	public class UniformDistributedShotProviderFixture {
		private Grid _grid;

		[SetUp]
		public void SetUp() {
			_grid = new Grid( 16, 9 );
		}

		[TearDown]
		public void TearDown() {}

		[Test]
		public void Offset_IsZero_IfNotSpecified() {
			var provider = new UniformDistributedShotProvider( _grid, 2 );
			Assert.That( provider.Offset, Is.EqualTo( new Point( 0, 0 ) ) );
		}

		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void Pace_ShouldBe_GreaterThan_Zero() {
			new UniformDistributedShotProvider( _grid, 0 );
		}

		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void Pace_ShouldBe_LowerThan_MaxGridSize() {
			new UniformDistributedShotProvider( _grid, 16 );
		}

		[Test]
		public void Shots_ContainsAllGrid_WhenPaceIs_One() {
			_grid = new Grid( 4, 2 );
			var provider = new UniformDistributedShotProvider( _grid, 1 );

			var shots = provider.Shots().ToList();

			CollectionAssert.Contains( shots, new Shot( 0, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 1, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 2, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 3, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 0, 1 ) );
			CollectionAssert.Contains( shots, new Shot( 1, 1 ) );
			CollectionAssert.Contains( shots, new Shot( 2, 1 ) );
			CollectionAssert.Contains( shots, new Shot( 3, 1 ) );
		}

		[Test]
		public void Shots_ContainsFourCorners_WhenPaceIs_MaxSize() {
			/*
			  -- -- -- -- --  
			 |XX|  |  |  |XX| 
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |XX|  |  |  |XX|
			  -- -- -- -- --   
			 */
			_grid = new Grid( 5, 5 );
			var provider = new UniformDistributedShotProvider( _grid, 4 );

			var shots = provider.Shots().ToList();

			CollectionAssert.Contains( shots, new Shot( 0, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 4, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 0, 4 ) );
			CollectionAssert.Contains( shots, new Shot( 4, 4 ) );
		}

		[Test]
		public void Offset_ShiftShots() {
			/*
			  -- -- -- -- --  
			 |  |XX|  |XX|  | 
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |  |XX|  |XX|  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |  |XX|  |XX|  |
			  -- -- -- -- --   
			 */
			_grid = new Grid( 5, 5 );
			var provider = new UniformDistributedShotProvider( _grid, 2, new Point( 1, 2 ) );

			var shots = provider.Shots().ToList();

			CollectionAssert.Contains( shots, new Shot( 1, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 3, 0 ) );
			CollectionAssert.Contains( shots, new Shot( 1, 2 ) );
			CollectionAssert.Contains( shots, new Shot( 3, 2 ) );
			CollectionAssert.Contains( shots, new Shot( 1, 4 ) );
			CollectionAssert.Contains( shots, new Shot( 3, 4 ) );
		}
	}
}