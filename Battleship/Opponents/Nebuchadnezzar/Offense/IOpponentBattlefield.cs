using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public interface IOpponentBattlefield
	{
		Point GetMaxEmptyCell(double[,] weights);
		bool HasHitsOnUnsinkShips();
		void Hit(Point cell);
		void HitAndSink(Ship ship);
		void Miss(Point cell);
		int CountAdjacentCellsEmptyOrHit(Point startPoint, int deltaX, int deltaY);
		int CountAdjacentCellsHit(Point startPoint, int deltaX, int deltaY);
		int CountAdjacentCellsEmpty(Point startPoint, int deltaX, int deltaY);
		IEnumerable<int> UnsinkShipsLengthShorterThan(int maxLength);
		IEnumerable<Point> Cells();
		IEnumerable<Point> CellsHitAndNotSink();
		IEnumerable<KeyValuePair<int, Point>> EmptyCellsAlongTheDirection(Point startPointExcluded, int deltaX, int deltaY, int steps);
		IEnumerable<Ship> UnsinkShipsThatCouldBePlacedHere(Point point);
		void TellStatistics(OpponentBattlefieldStatisticsHandler statisticsHandler);
		void TellBattlefiels(OpponentBattlefieldBattlefieldHandler battlefieldHandler);
	}
}