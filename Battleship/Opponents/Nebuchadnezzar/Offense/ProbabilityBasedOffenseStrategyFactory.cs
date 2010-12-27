namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public class ProbabilityBasedOffenseStrategyFactory : IProbabilityBasedOffenseStrategyFactory
	{
		public void CreateDependencies(out IOpponentBattlefield opponentBattlefield, out IPartiallySinkShipsOracle partiallySinkShipsOracle, out IEmptyCellsOracle emptyCellsOracle)
		{
			opponentBattlefield  = new OpponentBattlefield();
			partiallySinkShipsOracle = new PartiallySinkShipsOracle(opponentBattlefield);
			emptyCellsOracle = new EmptyCellsOracle(opponentBattlefield);
		}
	}
}
