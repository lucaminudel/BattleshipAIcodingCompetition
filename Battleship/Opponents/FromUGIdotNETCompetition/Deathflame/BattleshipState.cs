using System.Drawing;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public abstract class BattleshipState {
		public BattleshipState NextState { get; set; }
		public abstract Shot NextShot();
		public abstract void ShotHit( Point shot );
		public abstract void ShotHitAndSink( Point shot, Ship sunkShip );
		public abstract void ShotMiss( Point shot );
		//internal abstract BattleshipState NextState();
	}
}