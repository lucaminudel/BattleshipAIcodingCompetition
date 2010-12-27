namespace Battleship.Opponents.FromUGIdotNETCompetition.Terminator
{
    class MatchInfo
    {
        public int NumberOfPlayedGames { get; set; }

        public CountMap MyShipsPositionFrequency { get; set; }
        public CountMap OpponentShipsPositionFrequency { get; set; }

        // Count all the opponent shots
        public CountMap OpponentTotalShotsFrequency { get; set; }

        // Count only the shots when the opponent is in search mode
        public CountMap OpponentSearchShotsFrequency { get; set; }

        public MatchInfo()
        {
            NumberOfPlayedGames = 0;

            MyShipsPositionFrequency = new CountMap();
            OpponentShipsPositionFrequency = new CountMap();

            OpponentTotalShotsFrequency = new CountMap();
            OpponentSearchShotsFrequency = new CountMap(); 
        }
    }
}
