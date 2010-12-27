namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	internal class MatchStats {
		public int WonCount { get; private set; }

		public int LostCount { get; private set; }

		public int ShotCount { get; private set; }

		public int GameCount {
			get { return WonCount + LostCount; }
		}

		public void GamWon() {
			WonCount++;
		}

		public void GameLost() {
			LostCount++;
		}

		public void Shot() {
			ShotCount++;
		}

		public void NewGame() {
			ShotCount = 0;
		}
	}
}