using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar
{
	public interface IOffenseStrategy {
		void StartGame();
		Point GetShot();
		void ShotMiss(Point p);
		void ShotHit(Point p);
		void ShotSunk(Point p, Ship ship);
		void EndGame();
	}
}