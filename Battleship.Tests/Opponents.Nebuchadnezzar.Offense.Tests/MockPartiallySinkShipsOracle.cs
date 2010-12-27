using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	class MockPartiallySinkShipsOracle :IPartiallySinkShipsOracle
	{
		private int _callsCount = 0;

		public Point GuessTheBestShotOnAPartiallySinkShip(double[,] weights)
		{
			++_callsCount;
			return Point.Empty;
		}

		public int GuessTheBestShotOnAPartiallySinkShipCallsCount
		{
			get
			{
				return _callsCount;
			}
		}

	}
}