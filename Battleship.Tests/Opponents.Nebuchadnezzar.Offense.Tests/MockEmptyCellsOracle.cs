using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	class MockEmptyCellsOracle : IEmptyCellsOracle
	{
		private int _callsesCount = 0;

		public Point GuessTheBestShotOnAnEmptyCell(double[,] weights)
		{
			++_callsesCount;
			return Point.Empty;
		}

		public int GuessTheBestShotOnAnEmptyCellCallsCount
		{
			get
			{
				return _callsesCount;
			}
		}

	}
}