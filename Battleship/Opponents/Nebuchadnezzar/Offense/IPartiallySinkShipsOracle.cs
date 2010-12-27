using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public interface IPartiallySinkShipsOracle
	{
		Point GuessTheBestShotOnAPartiallySinkShip(double[,] weights);
	}
}