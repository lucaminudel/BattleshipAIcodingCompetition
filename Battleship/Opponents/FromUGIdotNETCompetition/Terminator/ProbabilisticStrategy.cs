using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Terminator
{
    class ProbabilisticStrategy : Strategy
    {
        private readonly ProbabilisticMap probabilityMap = new ProbabilisticMap();

        private readonly List<Ship> ships2;
        private readonly List<Ship> ships3;
        private readonly List<Ship> ships4;
        private readonly List<Ship> ships5;

        bool lastThreeShip;

        public ProbabilisticStrategy(MatchInfo matchInfo, GameInfo gameInfo) : base(matchInfo, gameInfo)
        {
            ships2 = Util.AllShips2.ToList();
            ships3 = Util.AllShips3.ToList();
            ships4 = Util.AllShips4.ToList();
            ships5 = Util.AllShips5.ToList();
        }

        public override bool Completed
        {
            get { return false; }
            set { }
        }

        public override Point GetNextShot()
        {
            double maxProbability = 0;

            probabilityMap.Clear();

            var opponentShipsProbabilityMap = MatchInfo.OpponentShipsPositionFrequency.ToProbabilisticMap();

            int numberOfRemainedShips = ships2.Count + ships3.Count + ships4.Count + ships5.Count;

            for (int x = 0; x < Board.Size; ++x)
            {
                for (int y = 0; y < Board.Size; ++y)
                {
                    if (GameInfo.OpponentBoard[x, y] != ShotInfo.Unknown) 
                        continue;
                    
                    var point = new Point(x, y);

                    int numberOfShipsOnPoint = ships2.Count(ship => ship.IsAt(point)) +
                                               ships3.Count(ship => ship.IsAt(point)) +
                                               ships4.Count(ship => ship.IsAt(point)) +
                                               ships5.Count(ship => ship.IsAt(point));

                    probabilityMap[x, y] = numberOfShipsOnPoint / (double) numberOfRemainedShips;

                    if (MatchInfo.NumberOfPlayedGames >= Config.AdaptiveAttackThreshold)
                    {
                        probabilityMap[x, y] *= (1 + MatchInfo.OpponentShipsPositionFrequency[x, y] / (1 + MatchInfo.NumberOfPlayedGames));
                    }

                    if (maxProbability < probabilityMap[x, y])
                    {
                        maxProbability = probabilityMap[x, y];
                    }
                }
            }

            probabilityMap.Print();

            var shotCandidates = new List<Point>();

            for (int x = 0; x < Board.Size; ++x)
            {
                for (int y = 0; y < Board.Size; ++y)
                {
                    if (probabilityMap[x, y] >= maxProbability)
                    {
                        shotCandidates.Add(new Point(x, y));
                    }
                }
            }

            return shotCandidates.ChooseOneAtRandom();
        }
        
        private void RemoveShips(Point point)
        {
            ships2.RemoveAll(ship => ship.IsAt(point));
            ships3.RemoveAll(ship => ship.IsAt(point));
            ships4.RemoveAll(ship => ship.IsAt(point));
            ships5.RemoveAll(ship => ship.IsAt(point));
        }

        public override void ShotHit(Point shot)
        {
            RemoveShips(shot);
        }

        public override void ShotHitAndSink(Point shot, Ship sunkShip)
        {
            RemoveShips(shot);

            if (sunkShip.Length == 2) ships2.Clear();
            else if (sunkShip.Length == 4) ships4.Clear();
            else if (sunkShip.Length == 5) ships5.Clear();
            else
            {
                if (lastThreeShip) ships3.Clear();
                lastThreeShip = true;
            }
        }
        
        public override void ShotMiss(Point shot)
        {
            RemoveShips(shot);
        }
    };
}
