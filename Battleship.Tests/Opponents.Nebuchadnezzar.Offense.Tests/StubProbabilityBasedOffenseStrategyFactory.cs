namespace Battleship.Opponents.Nebuchadnezzar.Offense.Tests
{
	class StubProbabilityBasedOffenseStrategyFactory : IProbabilityBasedOffenseStrategyFactory
	{
		private IOpponentBattlefield _opponentBattlefield;
		private IPartiallySinkShipsOracle _partiallySinkShipsOracle;
		private IEmptyCellsOracle _emptyCellsOracle;
	
		public void SetCreateDependenciesReturnValues(IOpponentBattlefield opponentBattlefield, IPartiallySinkShipsOracle partiallySinkShipsOracle, IEmptyCellsOracle emptyCellsOracle)
		{
			_opponentBattlefield = opponentBattlefield;
			_partiallySinkShipsOracle = partiallySinkShipsOracle;
			_emptyCellsOracle = emptyCellsOracle;
		}

		public void CreateDependencies(out IOpponentBattlefield opponentBattlefield, out IPartiallySinkShipsOracle partiallySinkShipsOracle, out IEmptyCellsOracle emptyCellsOracle)
		{
			opponentBattlefield = _opponentBattlefield;
			partiallySinkShipsOracle = _partiallySinkShipsOracle;
			emptyCellsOracle = _emptyCellsOracle;
		}
	}
}