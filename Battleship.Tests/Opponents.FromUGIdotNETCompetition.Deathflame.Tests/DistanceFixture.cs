using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame.Tests
{
	public class DistanceFixture {

		[SetUp]
		public void SetUp() {}

		[TearDown]
		public void TearDown() {}

		[Test]
		public void Quality_InTheMiddle_CouldBeEquals() {
			var d1 = new Shot.Distance( 0, 3, 0, 0 );
			var d2 = new Shot.Distance( 1, 2, 0, 0 );
			var d3 = new Shot.Distance( 2, 1, 0, 0 );
			var d4 = new Shot.Distance( 3, 0, 0, 0 );

			var maxHoleSize = 3;
			Assert.That( d2.GetQuality( maxHoleSize ), Is.EqualTo( d3.GetQuality( maxHoleSize ) ) );
			Assert.That( d1.GetQuality( maxHoleSize ), Is.EqualTo( d4.GetQuality( maxHoleSize ) ) );
			Assert.That( d2.GetQuality( maxHoleSize ), Is.GreaterThan( d1.GetQuality( maxHoleSize ) ) );
		}

		[Test]
		public void Quality_ShouldPrefer_EquallySpaced() {
			/*
			  1  2  3  4  5  6  7  8
			  -- -- -- -- -- -- -- --
			 |  |  |XX|  |  |XX|  |  |
			  -- -- -- -- -- -- -- --
			*/
			
			var d1 = new Shot.Distance( 0, 7, 0, 0 );
			var d2 = new Shot.Distance( 1, 6, 0, 0 );
			var d3 = new Shot.Distance( 2, 5, 0, 0 );
			var d4 = new Shot.Distance( 3, 4, 0, 0 );
			var d5 = new Shot.Distance( 4, 3, 0, 0 );
			var d6 = new Shot.Distance( 5, 2, 0, 0 );
			var d7 = new Shot.Distance( 6, 1, 0, 0 );
			var d8 = new Shot.Distance( 7, 0, 0, 0 );

			var maxHoleSize = 3;
			Assert.That( d3.GetQuality( maxHoleSize ), Is.EqualTo( d6.GetQuality( maxHoleSize ) ) );

			Assert.That( d3.GetQuality( maxHoleSize ), Is.GreaterThan( d1.GetQuality( maxHoleSize ) ) );
			Assert.That( d3.GetQuality( maxHoleSize ), Is.GreaterThan( d2.GetQuality( maxHoleSize ) ) );
			Assert.That( d3.GetQuality( maxHoleSize ), Is.GreaterThan( d4.GetQuality( maxHoleSize ) ) );
			Assert.That( d3.GetQuality( maxHoleSize ), Is.GreaterThan( d5.GetQuality( maxHoleSize ) ) );
			Assert.That( d3.GetQuality( maxHoleSize ), Is.GreaterThan( d7.GetQuality( maxHoleSize ) ) );
			Assert.That( d3.GetQuality( maxHoleSize ), Is.GreaterThan( d8.GetQuality( maxHoleSize ) ) );

		}

		[Test]
		public void Quality_ShouldPreferACorner_IfPossible() {
			/*			    
			  - - 
			 |1|2|
			  - - 
			 |3|X|
			  - - 
			*/


			var d1 = new Shot.Distance( 0, 1, 0, 1 );
			var d2 = new Shot.Distance( 1, 0, 0, 0 );
			var d3 = new Shot.Distance( 0, 0, 1, 0 );

			var maxHoleSize = 2;

			Assert.That( d1.GetQuality( maxHoleSize ), Is.GreaterThan( d2.GetQuality( maxHoleSize ) ) );
			Assert.That( d1.GetQuality( maxHoleSize ), Is.GreaterThan( d3.GetQuality( maxHoleSize ) ) );

		}
		
	}
}