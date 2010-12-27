using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public class OpponentBattlefield : Battlefield, IOpponentBattlefield
	{
		private readonly BattlefieldCellState[,] _battlefieldState = new BattlefieldCellState[Size, Size];
		private readonly List<Ship> _unsunkShips;
		private readonly int _initialUnsinkShipsCount;

		private readonly int[] _statistics = new int[2];
		private const int Hits = 0;
		private const int Misses = 1;

		private readonly Random _laDeaFortuna;

		public OpponentBattlefield() : this(new List<Ship>(5)
		                                           	{
		                                           		Navy.NewSubmarine(),
		                                           		Navy.NewPatrolBoat(),
		                                           		Navy.NewDestroyer(),
		                                           		Navy.NewBattleship(),
		                                           		Navy.NewAircraftcarrier()
		                                           	})
		{

		}

		public OpponentBattlefield(IEnumerable<Ship> unsinkShips)
		{
			_battlefieldState = new BattlefieldCellState[Size, Size];
			_unsunkShips = new List<Ship>(unsinkShips);
			_initialUnsinkShipsCount = _unsunkShips.Count;

			_laDeaFortuna = new Random();
		}

		public Point GetMaxEmptyCell(double[,] weights)
		{
			var cellsOrderedByWeightsDescending = CellsState()
				.Where(cellCellStatePair => cellCellStatePair.Value == BattlefieldCellState.Empty)
				.Select(cellStatePair =>  new KeyValuePair<Point, double>(cellStatePair.Key, weights[cellStatePair.Key.X, cellStatePair.Key.Y]))
				.OrderByDescending(cellWeightPair => cellWeightPair.Value);

			double maxWeight = cellsOrderedByWeightsDescending.ElementAt(0).Value;

			var cellsWithMaximumWeights = cellsOrderedByWeightsDescending.Where(pair => pair.Value == maxWeight);
			var count = cellsWithMaximumWeights.Count();

			return cellsWithMaximumWeights.ElementAt(_laDeaFortuna.Next(0, count)).Key;
		}


		public IEnumerable<KeyValuePair<int, Point>> EmptyCellsAlongTheDirection(Point startPointExcluded, int deltaX, int deltaY, int steps)
		{
			for (int distance = 1; distance <= steps; distance++)
			{
				var point = new Point(startPointExcluded.X + distance*deltaX, startPointExcluded.Y + distance*deltaY);

				if (GetState(point) != BattlefieldCellState.Empty)
				{
					continue;
				}

				yield return new KeyValuePair<int, Point>(distance, point);
			}
		}

		public IEnumerable<Ship> UnsinkShipsThatCouldBePlacedHere(Point location)
		{
			var boardSize = new Size(Size, Size);
			foreach (var ship in _unsunkShips)
			{
				foreach (var orientation in Enum.GetValues(typeof(ShipOrientation)))
				{
					var possiblePlacedship = ship.Clone();
					ship.Place(location, (ShipOrientation)orientation);
					if (possiblePlacedship.IsValid(boardSize) == false)
					{
						continue;
					}

					bool shipCellsAreEmptyOnTheBattlefield = possiblePlacedship.GetAllLocations().All(cell => GetState(cell) == BattlefieldCellState.Empty);
					if (shipCellsAreEmptyOnTheBattlefield == false)
					{
						continue;
					}

					yield return possiblePlacedship;
				}
			}
		}

		public IEnumerable<Point> CellsHitAndNotSink()
		{
			return CellsState().Where(pair => pair.Value == BattlefieldCellState.Hit).Select(pair => pair.Key);
		}

		public IEnumerable<int> UnsinkShipsLengthShorterThan(int maxLength)
		{
			return _unsunkShips.Where(ship => ship.Length <= maxLength).Select(ship => ship.Length);
		}

		public bool HasHitsOnUnsinkShips()
		{
			return CellsHitAndNotSink().Count() > 0;
		}

		public int CountAdjacentCellsHit(Point startPoint, int deltaX, int deltaY)
		{
			var states = new[] { BattlefieldCellState.Hit };
			return CountAdjacentCells(startPoint, deltaX, deltaY, states);
		}

		public int CountAdjacentCellsEmptyOrHit(Point startPoint, int deltaX, int deltaY)
		{
			var states = new [] {BattlefieldCellState.Empty, BattlefieldCellState.Hit};
			return CountAdjacentCells(startPoint, deltaX, deltaY, states);
		}

		public int CountAdjacentCellsEmpty(Point startPoint, int deltaX, int deltaY)
		{
			var states = new[] { BattlefieldCellState.Empty };
			return CountAdjacentCells(startPoint, deltaX, deltaY, states);
		}

		public void Hit(Point cell)
		{
			SetState(cell, BattlefieldCellState.Hit);
			_statistics[Hits] += 1;
		}

		public void HitAndSink(Ship sunkShip)
		{
			var sunkShipIndex = _unsunkShips.FindIndex(unsinkShip => unsinkShip.Length == sunkShip.Length);
			if (sunkShipIndex == -1)
			{
				throw new InvalidOperationException();
			}

			_unsunkShips.RemoveAt(sunkShipIndex);

			foreach (var cell in sunkShip.GetAllLocations())
			{
				SetState(cell, BattlefieldCellState.Sink);
			}
			_statistics[Hits] += 1;
		}

		public void Miss(Point cell)
		{
			SetState(cell, BattlefieldCellState.Miss);
			_statistics[Misses] += 1;
		}

		public void TellStatistics(OpponentBattlefieldStatisticsHandler statisticsHandler)
		{
			int totalShots = _statistics[Hits] + _statistics[Misses];
			int sinkShips = _initialUnsinkShipsCount - _unsunkShips.Count;
			statisticsHandler(totalShots, _statistics[Misses], _statistics[Hits], sinkShips, _unsunkShips.Count);
		}

		public void TellBattlefiels(OpponentBattlefieldBattlefieldHandler battlefieldHandler) 
		{
			battlefieldHandler((BattlefieldCellState[,])_battlefieldState.Clone());
		}

		private IEnumerable<KeyValuePair<Point, BattlefieldCellState>> CellsState()
		{
			for (int x = 0; x < Size; ++x)
			{
				for (int y = 0; y < Size; ++y)
				{
					yield return new KeyValuePair<Point, BattlefieldCellState>(new Point(x, y), _battlefieldState[x, y]);
				}
			}
		}

		private int CountAdjacentCells(Point startPoint, int deltaX, int deltaY, IEnumerable<BattlefieldCellState> states)
		{
			if (deltaX == 0 && deltaY == 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			int count = 0;
			for (int i = 1; IsInValidRange(startPoint.X + deltaX * i) && IsInValidRange(startPoint.Y + deltaY * i); ++i)
			{
				var cell = new Point(startPoint.X + deltaX * i, startPoint.Y + deltaY * i);
				var state = GetState(cell);
				if (states.Contains(state))
				{
					count += 1;
				}
				else
				{
					break;
				}
			}

			return count;
		}

		private BattlefieldCellState GetState(Point cell)
		{
			return _battlefieldState[cell.X, cell.Y];
		}

		private void SetState(Point cell, BattlefieldCellState state)
		{
			_battlefieldState[cell.X, cell.Y] = state;
		}

		private static bool IsInValidRange(int position)
		{
			return 0 <= position && position < Size;
		}
	}
}