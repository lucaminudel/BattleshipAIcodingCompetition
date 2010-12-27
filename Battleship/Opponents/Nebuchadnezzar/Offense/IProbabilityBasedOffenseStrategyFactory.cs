namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public interface IProbabilityBasedOffenseStrategyFactory
	{
		void CreateDependencies(out IOpponentBattlefield opponentBattlefield, out IPartiallySinkShipsOracle partiallySinkShipsOracle, out IEmptyCellsOracle emptyCellsOracle);
	}
}