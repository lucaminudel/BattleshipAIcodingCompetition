using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public interface IEmptyCellsOracle
	{
		Point GuessTheBestShotOnAnEmptyCell(double[,] weights);
	}
}