using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar
{
	public interface IDefenseStrategy {
		IList<Ship> StartGame();
		void Shot(Point p);
		void EndGame();
	}
}