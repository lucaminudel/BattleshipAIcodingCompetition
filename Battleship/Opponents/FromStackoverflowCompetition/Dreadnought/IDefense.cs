using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.Dreadnought
{
	public interface IDefense {
		List<Ship> startGame(int[] ship_sizes);
		void shot(Point p);
		void endGame();
	}
}