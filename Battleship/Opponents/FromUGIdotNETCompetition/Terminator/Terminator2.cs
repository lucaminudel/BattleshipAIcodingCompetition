using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Terminator
{
    public class Terminator2 : IBattleshipOpponent    
    {
        private GameInfo gameInfo;
        private MatchInfo matchInfo;

        private Stack<Strategy> strategyStack;
        private Strategy currentStrategy;

        private ReadOnlyCollection<Ship> myShips;
        private List<Ship> myLastShips;

        private bool opponentInSearchStrategy = true;

        void IBattleshipOpponent.NewMatch(string opponent) 
        {
            matchInfo = new MatchInfo();
        }

        void IBattleshipOpponent.MatchOver() 
        {
        }

        void IBattleshipOpponent.NewGame(Size size, TimeSpan timeSpan)
        {
            if (size.Width != 10 || size.Height != 10)
                throw new NotImplementedException();

            gameInfo = new GameInfo();
            opponentInSearchStrategy = true;

            strategyStack = new Stack<Strategy>();
            currentStrategy = new ProbabilisticStrategy(matchInfo, gameInfo);
        }

        public void PlaceShips(ReadOnlyCollection<Ship> ships)
        {
            if (matchInfo.NumberOfPlayedGames < Config.AdaptiveDefenceThreshold)
            {
                PlaceShipsRandomly(ships);
                return;
            }

            // Adaptive defence
            var ships2 = Util.AllShips2.ToList();
            var ships3 = Util.AllShips3.ToList();
            var ships4 = Util.AllShips4.ToList();
            var ships5 = Util.AllShips5.ToList();

            foreach (var ship in ships)
            {
                Ship newShip;
                
                switch (ship.Length)
                {
                    case 2:
                        newShip = ChoiceMinShip(ships2);
                        break;
                    case 3:
                        newShip = ChoiceMinShip(ships3);
                        break;
                    case 4:
                        newShip = ChoiceMinShip(ships4);
                        break;
                    default:
                        newShip = ChoiceMinShip(ships5);
                        break;
                }

                ship.Place(newShip.Location, newShip.Orientation);

                foreach (Point point in newShip.GetAllLocations())
                {
                    Point p = point;

                    ships2.RemoveAll(s => s.IsAt(p));
                    ships3.RemoveAll(s => s.IsAt(p));
                    ships4.RemoveAll(s => s.IsAt(p));
                    ships5.RemoveAll(s => s.IsAt(p));
                }
            }
        }

        private Ship ChoiceMinShip(IEnumerable<Ship> ships)
        {
            int min = int.MaxValue;
            var candidateShips = new List<Ship>();
            
            foreach (var ship in ships)
            {
                int sum;

                if(Config.UseSearchShotsFrequency)
                {
                    sum = matchInfo.OpponentSearchShotsFrequency.Sum(ship.GetAllLocations());
                }
                else
                {
                    sum = matchInfo.OpponentTotalShotsFrequency.Sum(ship.GetAllLocations());                    
                }

                if ( sum == min )
                {
                    candidateShips.Add(ship);
                }

                if (sum < min)
                {
                    min = sum;

                    candidateShips.Clear();
                    candidateShips.Add(ship);
                }
            }

            return candidateShips.ChooseOneAtRandom();
        }

        private void PlaceShipsRandomly(ReadOnlyCollection<Ship> ships)
        {
            int initializedShips = 0;
            var placementBoard = new Board();

            foreach (Ship s in ships)
            {
                List<Point> freeStartPositions = placementBoard.GetAllPositions(ShotInfo.Unknown);

                bool conflict;

                do
                {
                    Point startPoint = freeStartPositions.ChooseOneAtRandom();
                    ShipOrientation orientation = Util.GetRandomShipOrientation();

                    s.Place(startPoint, orientation);

                    conflict = false;

                    for (int i = 0; i < initializedShips; ++i)
                    {
                        if (ships[i].ConflictsWith(s))
                        {
                            conflict = true;
                            break;
                        }
                    }
                }
                while (conflict || !s.IsValid(GameInfo.GameSize));

                ++initializedShips;

                foreach (Point p in s.GetAllLocations())
                {
                    placementBoard[p] = ShotInfo.Hit;
                }
            }

            myShips = ships;
            myLastShips = myShips.ToList();
        }

        public Point GetShot()
        {
            Point point = Point.Empty;

            while(true)
            {
                bool completed;
              
                do
                {
                    completed = currentStrategy.Completed;
                    if (completed) break;

                    point = currentStrategy.GetNextShot();
                } 
                while (gameInfo.MyShots.Contains(point));

                if (!completed)
                {
                    gameInfo.MyShots.Add(point);
                    gameInfo.MyNumberOfShots++;

                    return point;
                }

                // Change in strategy is required              
                if (strategyStack.Count > 0)
                {
                    currentStrategy = strategyStack.Pop();
                }
                else
                {
                    Debug.Fail("");
                }
            }
        }

        public void OpponentShot(Point shot)
        {
            ++gameInfo.OpponentNumberOfShots;
            ++matchInfo.OpponentTotalShotsFrequency[shot];

            if (opponentInSearchStrategy)
            {
                ++matchInfo.OpponentSearchShotsFrequency[shot];
            }
            
            ShotInfo shotInfo = ShotInfo.Missed;

            if (myShips.Any(s => s.IsAt(shot)))
            {
                shotInfo = ShotInfo.Hit;
                opponentInSearchStrategy = false;
            }

            gameInfo.MyBoard[shot] = shotInfo;

            // Check if the opponent is in search strategy or not
            var hitPositions = gameInfo.MyBoard.GetAllPositions(ShotInfo.Hit);

            if (myLastShips.Count(s => s.IsSunk(hitPositions)) > 0)
            {
                // After a sunk the opponent stop the sink strategy
                opponentInSearchStrategy = true;

                myLastShips.RemoveAll(s => s.IsSunk(hitPositions));
            }
        }

        public void ShotHit(Point shot)
        {
            ShotHit(shot, false, null);
        }

        public void ShotHitAndSink(Point shot, Ship sunkShip)
        {
            ShotHit(shot, true, sunkShip);
        }

        public void ShotHit(Point shot, bool sunk, Ship sunkShip)
        {
            gameInfo.OpponentBoard[shot] = ShotInfo.Hit;

            if(sunk)
            {
                currentStrategy.ShotHitAndSink(shot, sunkShip);
            }
            else
            {
                currentStrategy.ShotHit(shot);
            }

            if (currentStrategy is SinkStrategy) 
                return;
            
            strategyStack.Push(currentStrategy);

            if (currentStrategy is ProbabilisticStrategy)
            {
                currentStrategy = new SinkStrategy(matchInfo, gameInfo, shot, strategyStack, currentStrategy as ProbabilisticStrategy);
            }
            else
            {
                currentStrategy = new SinkStrategy(matchInfo, gameInfo, shot, strategyStack);
            }
        }

        public void ShotMiss(Point shot)
        {
            gameInfo.OpponentBoard[shot] = ShotInfo.Missed;
            currentStrategy.ShotMiss(shot);
        }

        public void GameWon()
        {
            UpdateMatchInfo();
        }

        public void GameLost()
        {
            UpdateMatchInfo();
        }

        private void UpdateMatchInfo()
        {
            matchInfo.NumberOfPlayedGames++;

            for (int i = 0; i < Board.Size; i++)
            {
                for (int j = 0; j < Board.Size; j++)
                {
                    if(gameInfo.OpponentBoard[i, j] == ShotInfo.Hit)
                    {
                        matchInfo.OpponentShipsPositionFrequency[i, j]++;
                    }
                }
            }

            foreach(Ship s in myShips)
            {
                foreach (Point p in s.GetAllLocations())
                {
                    matchInfo.MyShipsPositionFrequency[p]++;
                }
            }


            Debug.Print("------------------------------------------");
            Debug.Print("MyShipsPositionFrequency probabilistic view");
            matchInfo.MyShipsPositionFrequency.ToProbabilisticMap().Print();

            Debug.Print("OpponentShipsPositionFrequency");
            matchInfo.OpponentShipsPositionFrequency.Print();

            Debug.Print("OpponentTotalShotsFrequency");
            matchInfo.OpponentTotalShotsFrequency.Print();

            Debug.Print("OpponentSearchShotsFrequency");
            matchInfo.OpponentSearchShotsFrequency.Print();

            //matchInfo.OpponentShipsPositionFrequency.Print();
        }

        public string Name
        {
            get { return "Terminator 2"; }
        }

        public Version Version
        {
            get { return new Version(2, 0); }
        }
    }    
}
