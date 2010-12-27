using System;
using System.Collections.Generic;
using System.Drawing;
using Battleship.Opponents.Nebuchadnezzar.Offense;

namespace Battleship.Opponents.Nebuchadnezzar.Tests
{
	public class OpponentBattlefieldBuilder
	{
		private BattlefieldCellState _defaultCellState;
		private readonly Dictionary<BattlefieldCellState, IList<Point>> _definedStateCells;
		private Ship[] _unsinkShips = null;

		public OpponentBattlefieldBuilder()
		{
			_defaultCellState = BattlefieldCellState.Empty;

			_definedStateCells = new Dictionary<BattlefieldCellState, IList<Point>>();
			_definedStateCells[BattlefieldCellState.Empty] = new List<Point>();
			_definedStateCells[BattlefieldCellState.Hit] = new List<Point>();
			_definedStateCells[BattlefieldCellState.Miss] = new List<Point>();
			_definedStateCells[BattlefieldCellState.Sink] = new List<Point>();
		}

		private OpponentBattlefieldBuilder(BattlefieldCellState defaultCellState, Dictionary<BattlefieldCellState, IList<Point>> definedStateCells)
		{
			_defaultCellState = defaultCellState;
			_definedStateCells = definedStateCells;
		}

		public static OpponentBattlefieldBuilder AnOpponentBattlefield()
		{
			var battlefieldBuilder = new OpponentBattlefieldBuilder();
			return battlefieldBuilder;
		}

		public OpponentBattlefield Build()
		{
			OpponentBattlefield opponentBattlefield;
			if (_unsinkShips == null)
			{
				opponentBattlefield = new OpponentBattlefield();				
			}
			else
			{
				opponentBattlefield = new OpponentBattlefield(_unsinkShips);								
			}


			BuildDefaultCellsState(opponentBattlefield);

			BuildDefinedCellsState(opponentBattlefield);

			return opponentBattlefield;
		}

		public OpponentBattlefieldBuilder WithAllCellsMarkedAs(BattlefieldCellState defaultCellState)
		{
			if (defaultCellState == BattlefieldCellState.Sink)
			{
				throw new NotImplementedException();
			}

			_defaultCellState = defaultCellState;
			return this;
		}

		public OpponentBattlefieldBuilder WithTheseCellsMarkedAsHit(params Point[] cellsMarkedAsHit)
		{
			WithTheseCellsMarkedAsHit((ICollection<Point>)cellsMarkedAsHit);

			return this;
		}

		public OpponentBattlefieldBuilder WithTheseCellsMarkedAsHit(IEnumerable<Point> cellsMarkedAsHit)
		{
			foreach (var hit in cellsMarkedAsHit)
			{
				_definedStateCells[BattlefieldCellState.Hit].Add(hit);				
			}
			
			return this;
		}

		public OpponentBattlefieldBuilder WithTheseCellsMarkedAsMiss(params Point[] cellsMarkedAsMiss)
		{
			WithTheseCellsMarkedAsMiss((ICollection<Point>)cellsMarkedAsMiss);

			return this;
		}

		public OpponentBattlefieldBuilder WithTheseCellsMarkedAsMiss(IEnumerable<Point> cellsMarkedAsMiss)
		{
			foreach (var miss in cellsMarkedAsMiss)
			{
				_definedStateCells[BattlefieldCellState.Miss].Add(miss);
			}

			return this;
		}

		public OpponentBattlefieldBuilder WithTheseCellsMarkedAsEmpty(params Point[] cellsMarkedAsEmpty)
		{
			WithTheseCellsMarkedAsEmpty((ICollection<Point>)cellsMarkedAsEmpty);

			return this;
		}

		public OpponentBattlefieldBuilder WithTheseCellsMarkedAsEmpty(IEnumerable<Point> cellsMarkedAsEmpty)
		{
			foreach (var cellMarkedAsEmpty in cellsMarkedAsEmpty)
			{
				_definedStateCells[BattlefieldCellState.Empty].Add(cellMarkedAsEmpty);
			}
			return this;
		}

		public OpponentBattlefieldBuilder But()
		{
			var clone = new OpponentBattlefieldBuilder(
				_defaultCellState,
				new Dictionary<BattlefieldCellState, IList<Point>>(_definedStateCells));
			            	
			return clone;
		}

		private void BuildDefinedCellsState(OpponentBattlefield opponentBattlefield)
		{
			foreach (var cellStateCellsListKeyValuePair in _definedStateCells)
			{
				foreach (var cell in cellStateCellsListKeyValuePair.Value)
				{
					switch (cellStateCellsListKeyValuePair.Key)
					{
						case BattlefieldCellState.Hit:
							opponentBattlefield.Hit(cell);
							break;
						case BattlefieldCellState.Miss:
							opponentBattlefield.Miss(cell);
							break;

						case BattlefieldCellState.Empty:
							break;

						case BattlefieldCellState.Sink:
							throw new NotImplementedException();
					}
				}
			}
		}

		private void BuildDefaultCellsState(OpponentBattlefield opponentBattlefield)
		{
			if (_defaultCellState == BattlefieldCellState.Empty)
			{
				return;
			}

			foreach (var battlefieldCell in opponentBattlefield.Cells())
			{
				if (_definedStateCells[BattlefieldCellState.Empty].Contains(battlefieldCell))
				{
					continue;
				}

				switch (_defaultCellState)
				{
					case BattlefieldCellState.Hit:
						opponentBattlefield.Hit(battlefieldCell);
						break;

					case BattlefieldCellState.Miss:
						opponentBattlefield.Miss(battlefieldCell);
						break;
				}
			}
		}

		public OpponentBattlefieldBuilder WhithTheseUnsinkShips(params Ship[] unsinkShips)
		{
			_unsinkShips = unsinkShips;
			return this;
		}
	}
}
