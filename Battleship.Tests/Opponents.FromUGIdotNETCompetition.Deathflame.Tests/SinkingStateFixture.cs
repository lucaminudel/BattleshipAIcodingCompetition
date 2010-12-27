#region

using System.Drawing;
using System.Linq;
using NUnit.Framework;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame.Tests
{
	[TestFixture]
	public class SinkingStateFixture {
		private SinkingState _sinkingState;

		private SinkingState CreateSinkingState( SearchingState searchingState, Grid grid, Point position ) {
			return new SinkingState( searchingState, grid, position );
		}

		private SinkingState CreateSinkingState( Grid grid, Point position ) {
			return CreateSinkingState( null, grid, position );
		}

		[Test]
		public void AfterFirstShot_CandidatesAre_AllNighbours() {
			/*
			  -- -- -- 
			 |  |CC|  |
			  -- -- -- 
			 |CC|XX|CC|
			  -- -- -- 
			 |  |CC|  |
			  -- -- -- 
			 */
			var grid = new Grid( 3, 3 );
			var firstShot = new Shot( 1, 1 );
			_sinkingState = CreateSinkingState( grid, firstShot.Position );

			var expectedCandidates = new[] {
			                               	new Shot( 1, 0 ),
			                               	new Shot( 0, 1 ),
			                               	new Shot( 2, 1 ),
			                               	new Shot( 1, 2 )
			                               };

			var actual = _sinkingState.Candidates().ToList();

			CollectionAssert.AreEquivalent( expectedCandidates, actual );
		}

		[Test]
		public void AfterSecondShot_NighboursAreFiltered_BasedOnDirection_Horizontal() {
			/*
			  -- -- -- -- --  
			 |  |  |  |  |  | 
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |CC|XX|XX|CC|  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --   
			 */
			var grid = new Grid( 5, 5 );
			var firstShot = new Shot( 2, 2 );
			_sinkingState = CreateSinkingState( grid, firstShot.Position );

			_sinkingState.ShotHit( new Point( 1, 2 ) );
			var expectedCandidates = new[] {
			                               	new Shot( 0, 2 ),
			                               	new Shot( 3, 2 )
			                               };

			var actual = _sinkingState.Candidates().ToList();

			CollectionAssert.AreEquivalent( expectedCandidates, actual );
		}

		[Test]
		public void AfterSecondShot_NighboursAreFiltered_BasedOnDirection_Vertical() {
			/*
			  -- -- -- -- --  
			 |  |  |CC|  |  | 
			  -- -- -- -- --  
			 |  |  |XX|  |  |
			  -- -- -- -- --  
			 |  |  |XX|  |  |
			  -- -- -- -- --  
			 |  |  |CC|  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --   
			 */
			var grid = new Grid( 5, 5 );
			var firstShot = new Shot( 2, 2 );
			_sinkingState = CreateSinkingState( grid, firstShot.Position );

			_sinkingState.ShotHit( new Point( 2, 1 ) );
			var expectedCandidates = new[] {
			                               	new Shot( 2, 0 ),
			                               	new Shot( 2, 3 )
			                               };

			var actual = _sinkingState.Candidates().ToList();

			CollectionAssert.AreEquivalent( expectedCandidates, actual );
		}

		[Test]
		public void AfterSunk_IfSomeHitShotIsLeft_DoNOTSwitchTo_SearchingState() {
			/*
			  -- -- -- -- --  
			 |  |  |22|  |  | 
			  -- -- -- -- --  
			 |  |  |22|  |  |
			  -- -- -- -- --  
			 |  |??|XX|??|  |
			  -- -- -- -- --  
			 |  |  |??|  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --   
			 */
			var grid = new Grid( 5, 5 );
			var ship = new Ship( 2 );
			ship.Place( new Point( 2, 0 ), ShipOrientation.Vertical );

			var firstShot = new Shot( 2, 2 );

			var searchingState = new SearchingState( grid );

			_sinkingState = CreateSinkingState( searchingState, grid, firstShot.Position );

			_sinkingState.ShotHit( new Point( 2, 1 ) );
			_sinkingState.ShotHitAndSink( new Point( 2, 0 ), ship );

			Assert.AreSame( _sinkingState, _sinkingState.NextState );
		}

		[Test]
		public void AfterSunk_SwitchTo_SearchingState() {
			/*
			  -- -- -- -- --  
			 |  |  |22|  |  | 
			  -- -- -- -- --  
			 |  |  |22|  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --   
			 */
			var grid = new Grid( 5, 5 );
			var ship = new Ship( 2 );
			ship.Place( new Point( 2, 0 ), ShipOrientation.Vertical );

			var firstShot = new Shot( 2, 0 );

			var searchingState = new SearchingState( grid );

			_sinkingState = CreateSinkingState( searchingState, grid, firstShot.Position );

			_sinkingState.ShotHitAndSink( new Point( 2, 1 ), ship );

			Assert.AreSame( searchingState, _sinkingState.NextState );
		}

		[Test]
		public void WhenDirectionDoesNotSunkAShip_OtherCandidatesShouldReturnActive() {
			/*
			  -- -- -- -- --  
			 |  |  |OO|  |  | 
			  -- -- -- -- --  
			 |  |CC|XX|CC|  |
			  -- -- -- -- --  
			 |  |CC|XX|CC|  |
			  -- -- -- -- --  
			 |  |  |OO|  |  |
			  -- -- -- -- --  
			 |  |  |  |  |  |
			  -- -- -- -- --   
			 */
			var grid = new Grid( 5, 5 );
			var firstShot = new Shot( 2, 2 );
			_sinkingState = CreateSinkingState( grid, firstShot.Position );

			_sinkingState.ShotHit( new Point( 2, 1 ) );
			_sinkingState.ShotMiss( new Point( 2, 0 ) );
			_sinkingState.ShotMiss( new Point( 2, 3 ) );

			var expectedCandidates = new[] {
			                               	new Shot( 1, 1 ),
			                               	new Shot( 3, 1 ),
			                               	new Shot( 1, 2 ),
			                               	new Shot( 3, 2 )
			                               };

			var actual = _sinkingState.Candidates().ToList();

			CollectionAssert.AreEquivalent( expectedCandidates, actual );
		}
	}
}