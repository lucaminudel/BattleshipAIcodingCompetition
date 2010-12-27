using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework.SyntaxHelpers;

namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	class StubOpponentBattlefieldBuilder : IOpponentBattlefield
	{
		private bool _cannedResponse = false;

		public void SetHasHitsOnUnsinkShipsReturnValue(bool cannedResponse)
		{
			_cannedResponse = cannedResponse;	
		}

		public bool HasHitsOnUnsinkShips()
		{
			return _cannedResponse;
		}


		public Point GetMaxEmptyCell(double[,] weights)
		{
			return Point.Empty;
		}

		public IEnumerable<KeyValuePair<int, Point>> EmptyCellsAlongTheDirection(Point startPointExcluded, int deltaX, int deltaY, int steps)
		{
			return null;
		}

		public IEnumerable<Ship> UnsinkShipsThatCouldBePlacedHere(Point point)
		{
			return new List<Ship>();
		}

		public IEnumerable<Point> CellsHitAndNotSink()
		{
			return null;
		}

		public IEnumerable<int> UnsinkShipsLengthShorterThan(int maxLength)
		{
			return null;
		}

		public void Hit(Point cell)
		{
		}

		public void HitAndSink(Ship ship)
		{
		}

		public void Miss(Point cell)
		{
		}

		public int CountAdjacentCellsHit(Point startPoint, int deltaX, int deltaY)
		{
			return 0;
		}

		public int CountAdjacentCellsEmptyOrHit(Point startPoint, int deltaX, int deltaY)
		{
			return 0;
		}

		public int CountAdjacentCellsEmpty(Point startPoint, int deltaX, int deltaY)
		{
			return 0;
		}

		public IEnumerable<Point> Cells()
		{
			return null;
		}

		public void TellStatistics(OpponentBattlefieldStatisticsHandler statisticsHandler)
		{
		}

		public void TellBattlefiels(OpponentBattlefieldBattlefieldHandler battlefieldHandler)
		{
		}
	}
}