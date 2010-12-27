namespace Battleship
{
    using System;
    using System.Collections.ObjectModel;
    using System.Drawing;

    public interface IBattleshipOpponent
    {
		string Name
		{
			get;
		}

		Version Version
		{
			get;
		}

		void NewMatch(string opponent);

		void NewGame(Size size, TimeSpan timeSpan);

		void PlaceShips(ReadOnlyCollection<Ship> ships);

		Point GetShot();

		void OpponentShot(Point shot);

		void ShotHit(Point shot);

		void ShotHitAndSink(Point shot, Ship sunkShip);

		void ShotMiss(Point shot);

		void GameWon();

		void GameLost();

		void MatchOver();
	}
}
