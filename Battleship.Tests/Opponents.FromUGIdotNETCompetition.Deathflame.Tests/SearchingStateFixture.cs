#region

using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame.Tests
{
	[TestFixture]
	public class SearchingStateFixture {
		#region Setup/Teardown
		[SetUp]
		public void SetUp() {
			_grid = new Grid( 10, 10 );
			_state = new SearchingState( _grid );
		}
		#endregion

		private SearchingState _state;
		private Grid _grid;

		[Test]
		public void NextState_ShouldBe_SinkingState_After_HitShot() {
			var aShot = new Point();
			_state.ShotHit( aShot );

			Assert.That( _state.NextState, Is.TypeOf( typeof ( SinkingState ) ) );
		}

		[Test]
		public void NextState_ShouldRemain_Searching_After_MissShot() {
			var aShot = new Point();
			_state.ShotMiss( aShot );

			Assert.That( _state.NextState, Is.TypeOf( typeof ( SearchingState ) ) );
		}
	}
}