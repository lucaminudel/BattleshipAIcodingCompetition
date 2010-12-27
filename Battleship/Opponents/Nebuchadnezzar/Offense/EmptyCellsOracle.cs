using System;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public class EmptyCellsOracle : IEmptyCellsOracle
	{
		private readonly IOpponentBattlefield _opponentBattlefield;

		public EmptyCellsOracle(IOpponentBattlefield opponentBattlefield)
		{
			_opponentBattlefield = opponentBattlefield;
		}

		public Point GuessTheBestShotOnAnEmptyCell(double[,] weights)
		{
			Point shot;
			try
			{
				shot = _opponentBattlefield.GetMaxEmptyCell(weights);
				//shot = _opponentBattlefield.GetXxxxxxx(weights, 50);
			}
			catch (ArgumentOutOfRangeException)
			{

				throw new InvalidOperationException();
			}

			return shot;
		}

	}
}
