namespace Battleship.Opponents.Nebuchadnezzar
{
	public static class Navy
	{
		private static readonly Ship PatrolBoat = new Ship(2);
		private static readonly Ship Submarine = new Ship(3);
		private static readonly Ship Destroyer = new Ship(3);
		private static readonly Ship Battleship = new Ship(4);
		private static readonly Ship Aircraftcarrier = new Ship(5);

		public static readonly int NavyTotalLengh = PatrolBoat.Length + Submarine.Length + Destroyer.Length +
		                                            Battleship.Length + Aircraftcarrier.Length;

		public static Ship NewPatrolBoat()
		{
			return PatrolBoat.Clone();
		}

		public static Ship NewSubmarine()
		{
			return Submarine.Clone();
		}

		public static Ship NewDestroyer()
		{
			return Destroyer.Clone();
		}

		public static Ship NewBattleship()
		{
			return Battleship.Clone();
		}

		public static Ship NewAircraftcarrier()
		{
			return Aircraftcarrier.Clone();
		}

	}
}