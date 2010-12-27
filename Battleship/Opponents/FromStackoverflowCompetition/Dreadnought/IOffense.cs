using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.Dreadnought
{
	public interface IOffense {
		void startGame(int[] ship_sizes);
		Point getShot();
		void shotMiss(Point p);
		void shotHit(Point p);
		void shotSunk(Point p);
		void endGame();
	}
}