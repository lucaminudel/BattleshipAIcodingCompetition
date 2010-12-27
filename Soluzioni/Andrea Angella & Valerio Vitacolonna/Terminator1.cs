using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Battleship.FinalTerminator
{
    public class Terminator1 : IBattleshipOpponent    
    {
        public static readonly Size gameSize = new Size(10, 10);

        private Board myBoard;
        private Board opponentBoard;

        private HashSet<Point> shooted;

        private Stack<Strategy> strategyStack;
        private Strategy currentStrategy;

        private ReadOnlyCollection<Ship> myShips;

        private int myNumberOfShots;
        private int opponentNumberOfShots;

        public Terminator1() 
        {
        }
        
        public void NewMatch(string opponent) 
        {
        }
        
        public void MatchOver() 
        {
        }

        public void NewGame(Size size, TimeSpan timeSpan)
        {
            if (size.Width != 10 || size.Height != 10)
                throw new NotImplementedException();

            myBoard = new Board();
            opponentBoard = new Board();

            shooted = new HashSet<Point>();

            strategyStack = new Stack<Strategy>();
            strategyStack.Push(new RandomStrategy());
            strategyStack.Push(new ProbabilisticStrategy(opponentBoard));
            currentStrategy = strategyStack.Pop();

            myNumberOfShots = 0;
            opponentNumberOfShots = 0;
        }

        public void PlaceShips(ReadOnlyCollection<Ship> ships)
        {
            int max = Board.Size * Board.Size;
            int initializedShip = 0;

            foreach (Ship s in ships)
            {
                int count = Util.GetRandomIntFromInterval(0, max - 1);
                ShipOrientation orientation = Util.GetRandomShipOrientation();

                bool conflict;

                do
                {
                    int x = count / 10;
                    int y = count % 10;

                    s.Place(new Point(x, y), orientation);
                    
                    conflict = false;

                    for (int i = 0; i < initializedShip; ++i)
                    {
                        if (ships[i].ConflictsWith(s))
                        {
                            conflict = true;
                            break;
                        }
                    }

                    count = (count + 1) % (max);
                } 
                while (conflict || !s.IsValid(gameSize));

                initializedShip++;
            }

            this.myShips = ships;
        }

        public Point GetShot()
        {
            Point point = Point.Empty;

            bool completed = false;

            while(true)
            {
                do
                {
                    completed = currentStrategy.Completed;
                    if (completed) break;

                    point = currentStrategy.GetNextShot();
                } 
                while (shooted.Contains(point));

                if (!completed)
                {
                    shooted.Add(point);
                    myNumberOfShots++;

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
            ++opponentNumberOfShots;

            ShotInfo shotInfo = ShotInfo.MISSED;

            foreach(Ship s in myShips)
            {
                if (s.IsAt(shot)) 
                {
                    shotInfo = ShotInfo.HIT;
                    break;
                }
            }

            myBoard[shot] = shotInfo;
        }

        public void ShotHit(Point shot, bool sunk, Ship ship)
        {
            opponentBoard[shot] = ShotInfo.HIT;

            currentStrategy.ShotHit(shot, sunk, ship);

            if (!(currentStrategy is SinkStrategy))
            {
                strategyStack.Push(currentStrategy);

                if (currentStrategy is ProbabilisticStrategy)
                {
                    currentStrategy = new SinkStrategy(shot, opponentBoard, strategyStack, currentStrategy as ProbabilisticStrategy);
                }
                else
                {
                    currentStrategy = new SinkStrategy(shot, opponentBoard, strategyStack);
                }
            }
        }

        public void ShotMiss(Point shot)
        {
            opponentBoard[shot] = ShotInfo.MISSED;
            currentStrategy.ShotMiss(shot);
        }

        public void GameWon()
        {
        }

        public void GameLost()
        {
        }

        public string Name
        {
            get { return "Terminator"; }
        }

        public Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public void ShotHit(Point shot)
        {
            ShotHit(shot, false, null);
        }

        public void ShotHitAndSink(Point shot, Ship sunkShip)
        {
            ShotHit(shot, true, sunkShip);
        }
    }

    static class Util
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);

        public static int GetRandomBoardIndex()
        {
            return random.Next(0, Board.Size - 1);
        }

        public static ShipOrientation GetRandomShipOrientation()
        {
            return (ShipOrientation)Util.GetRandomIntFromInterval(0, 1);
        }

        public static int GetRandomIntFromInterval(int minInclusiveValue, int maxInclusiveValue)
        {
            return random.Next(minInclusiveValue, maxInclusiveValue + 1);
        }

        public static Point GetRandomPoint()
        {
            return new Point(GetRandomBoardIndex(), GetRandomBoardIndex());
        }

        public static Point Sum(Point p1, Point p2)
        {
            Point p = p1;
            p.Offset(p2);

            return p;
        }

        public static Point Opposite(Point p)
        {
            return new Point(-p.X, -p.Y);
        }
    }

    static class ListPointExtensions
    {
        public static List<Point> Shuffle(this List<Point> points)
        {
            int len = points.Count();

            for (int i = 0; i < len; ++i)
            {
                int j = Util.GetRandomIntFromInterval(i, len-1);

                Point temp = points[i];
                points[i] = points[j];
                points[j] = temp;
            }

            return points;
        }
    }

    abstract class Strategy
    {
        public abstract bool Completed { get; set; }
        public abstract Point GetNextShot();

        public virtual void ShotHit(Point shot, bool sunk, Ship ship) { }
        public virtual void ShotMiss(Point shot) { }
    };

    class RandomStrategy : Strategy
    {
        public override bool Completed
        {
            get { return false; }
            set { }
        }

        public override Point GetNextShot()
        {
            return Util.GetRandomPoint();
        }
    };

    class ProbabilisticStrategy : Strategy
    {
        private static int N = Board.Size;

        private Board opponentBoard;

        private double[,] p2 = new double[N, N];
        private double[,] p3 = new double[N, N];
        private double[,] p4 = new double[N, N];
        private double[,] p5 = new double[N, N];
        private double[,] pmax = new double[N, N];

        private List<Ship> ships2;
        private List<Ship> ships3;
        private List<Ship> ships4;
        private List<Ship> ships5;

        bool lastThreeShip = false;

        public ProbabilisticStrategy(Board opponentBoard)
        {
            this.opponentBoard = opponentBoard;

            this.ships2 = GenerateAllPossibleShips(2);
            this.ships3 = GenerateAllPossibleShips(3);
            this.ships4 = GenerateAllPossibleShips(4);
            this.ships5 = GenerateAllPossibleShips(5);
        }

        private List<Ship> GenerateAllPossibleShips(int length)
        {
            var ships = new List<Ship>();

            for(int x = 0; x <= N - length; ++x)
                for (int y = 0; y < N; ++y)
                {
                    var hship = new Ship(length);
                    hship.Place(new Point(x, y), ShipOrientation.Horizontal);

                    var vship = new Ship(length);
                    vship.Place(new Point(y, x), ShipOrientation.Vertical);

                    ships.Add(hship);
                    ships.Add(vship);
                }

            return ships;
        }

        public override bool Completed
        {
            get { return false; }
            set { }
        }

        private void Clear(double[,] matrix)
        {
            for (int i = 0; i < N; ++i)
                for (int j = 0; j < N; ++j)
                    matrix[i, j] = 0;
        }

        private double Max(params double[] v)
        {
            double max = double.MinValue;

            for (int i = 0; i < v.Length; ++i)
                if (v[i] > max) max = v[i];

            return max;                
        }

        public override Point GetNextShot()
        {
            Point nextShot = Point.Empty;

            Clear(p2);
            Clear(p3);
            Clear(p4);
            Clear(p5);
            Clear(pmax);

            // Perform the calculus of probabilities for each possible next shot
            // and keep a reference to the point with maximum probability

            for (int x = 0; x < N; ++x)
                for (int y = 0; y < N; ++y)
                    if (opponentBoard[x, y] == ShotInfo.UNKNOWN)
                    {
                        var point = new Point(x, y);

                        if (ships2.Count > 0)
                        {
                            p2[x, y] = ships2.Count<Ship>(ship => ship.IsAt(point)) / (double) ships2.Count;
                        }

                        if (ships3.Count > 0)
                        {
                            p3[x, y] = ships3.Count<Ship>(ship => ship.IsAt(point)) / (double) ships3.Count;
                        }

                        if (ships4.Count > 0)
                        {
                            p4[x, y] = ships4.Count<Ship>(ship => ship.IsAt(point)) / (double) ships4.Count;
                        }

                        if (ships5.Count > 0)
                        {
                            p5[x, y] = ships5.Count<Ship>(ship => ship.IsAt(point)) / (double) ships5.Count;
                        }

                        pmax[x, y] = Max(p2[x, y], p3[x, y], p4[x, y], p5[x, y]);

                        if (pmax[nextShot.X, nextShot.Y] < pmax[x, y])
                        {
                            nextShot.X = x;
                            nextShot.Y = y;
                        }
                    }

            double max = pmax[nextShot.X, nextShot.Y];

            var shotCandidates = new List<Point>();

            for (int x = 0; x < N; ++x)
                for (int y = 0; y < N; ++y)
                    if (pmax[x, y] >= max)
                    {
                        shotCandidates.Add(new Point(x, y));
                    }

            return shotCandidates[Util.GetRandomIntFromInterval(0, shotCandidates.Count - 1)];
        }

        private void RemoveShips(Point point)
        {
            ships2.RemoveAll(ship => ship.IsAt(point));
            ships3.RemoveAll(ship => ship.IsAt(point));
            ships4.RemoveAll(ship => ship.IsAt(point));
            ships5.RemoveAll(ship => ship.IsAt(point));
        }

        public override void ShotHit(Point shot, bool sunk, Ship ship)
        {
            RemoveShips(shot);

            if (sunk)
            {
                if (ship.Length == 2) ships2.Clear();
                else if (ship.Length == 4) ships4.Clear();
                else if (ship.Length == 5) ships5.Clear();
                else 
                {
                    if(lastThreeShip) ships3.Clear();
                    lastThreeShip = true;
                }
            }
        }

        public override void ShotMiss(Point shot)
        {
            RemoveShips(shot);
        }
    };

    class SinkStrategy : Strategy
    {
        private enum SubStrategy
        {
            DirectionFinding,
            DirectionalAttack,
            OppositeDirectionalAttack
        }

        private ProbabilisticStrategy probabilisticStrategy;
        private SubStrategy currentSubStrategy;

        Point firstHit;
        Point lastShot;

        Point candidateAttackDirection;
        Point currentAttackDirection;
        Stack<Point> attackDirections;

        List<Point> hitPoints;

        Board opponentBoard;

        bool completed;

        Stack<Strategy> strategyStack;

        public override bool Completed
        {
            get 
            {
                if (currentSubStrategy == SubStrategy.DirectionFinding)
                {
                    foreach (Point direction in attackDirections)
                        if (IsValidShot(Util.Sum(firstHit, direction)))
                            return false;

                    // If all the directions are invalid the ship has been already sunk
                    return true;
                }

                return completed; 
            }
            
            set { completed = value; }
        }

        public SinkStrategy(Point firstHit, Board opponentBoard, Stack<Strategy> strategyStack, ProbabilisticStrategy probabilisticStrategy = null)
        {
            this.probabilisticStrategy = probabilisticStrategy;
            this.strategyStack = strategyStack;
            this.opponentBoard = opponentBoard;
            this.firstHit = firstHit;

            var directions = new List<Point>()
            {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, +1),
                new Point(0, -1)
            };

            this.attackDirections = new Stack<Point>(directions.Shuffle());
            this.hitPoints = new List<Point>() { firstHit };

            this.currentSubStrategy = SubStrategy.DirectionFinding;
        }

        public override Point GetNextShot()
        {
            Point nextShot = Point.Empty;

            switch (currentSubStrategy)
            {
                case SubStrategy.DirectionFinding:
                    
                    do
                    {
                        candidateAttackDirection = attackDirections.Pop();
                        nextShot = Util.Sum(firstHit, candidateAttackDirection);
                    }
                    while (!IsValidShot(nextShot));

                    return nextShot;
                        
                case SubStrategy.DirectionalAttack:
                case SubStrategy.OppositeDirectionalAttack:
                default:

                    nextShot = Util.Sum(lastShot, currentAttackDirection);

                    if (!IsValidShot(nextShot))
                    {
                        Debugger.Break();
                        Debug.Fail("");
                    }

                    return nextShot;
            }
        }

        private bool IsValidShot(Point shot)
        {
            return Board.Contains(shot) && opponentBoard[shot] == ShotInfo.UNKNOWN;
        }

        private bool IsTargetShip(Ship ship)
        {
            return ship.IsAt(firstHit);
        }

        private void Shot(Point shot, ShotInfo shotInfo, bool sunk = false, Ship ship = null)
        {
            lastShot = shot;

            switch (currentSubStrategy)
            {
                case SubStrategy.DirectionFinding:
                    DirectionFinding_Shot(shot, shotInfo, sunk, ship);
                    break;

                case SubStrategy.DirectionalAttack:
                    DirectionalAttack_Shot(shot, shotInfo, sunk, ship);
                    break;

                case SubStrategy.OppositeDirectionalAttack:
                    OppositeDirectionalAttack_Shot(shot, shotInfo, sunk, ship);
                    break;
            }

            if (completed)
            {
                // Create new strategy to sink new ships found during the process
                foreach (Point p in hitPoints)
                {
                    strategyStack.Push(new SinkStrategy(p, opponentBoard, strategyStack, probabilisticStrategy));
                }
            }
        }

        private void DirectionFinding_Shot(Point shot, ShotInfo shotInfo, bool sunk, Ship ship)
        {
            if (shotInfo == ShotInfo.MISSED)
                return;

            if (sunk)
            {
                if (IsTargetShip(ship))
                {
                    completed = true;
                }
                else
                {
                    // In this case the direction was wrong.
                    // The target ship is not sunk, a new direction have to be tried
                }
            }
            else
            {
                currentAttackDirection = candidateAttackDirection;
                currentSubStrategy = SubStrategy.DirectionalAttack;

                Point nextShot = Util.Sum(lastShot, currentAttackDirection);

                if (!IsValidShot(nextShot))
                {
                    currentAttackDirection = Util.Opposite(currentAttackDirection);
                    currentSubStrategy = SubStrategy.OppositeDirectionalAttack;
                    lastShot = firstHit;

                    nextShot = Util.Sum(lastShot, currentAttackDirection);

                    if (!IsValidShot(nextShot))
                    {
                        // In this case the direction was wrong.
                        // The target ship is not sink, but it will be sink by a next generated SinkStrategy
                        completed = true;
                    }
                }
            }
        }

        private void DirectionalAttack_Shot(Point shot, ShotInfo shotInfo, bool sunk, Ship ship)
        {
            if (shotInfo == ShotInfo.HIT)
            {
                if (sunk)
                {
                    if (IsTargetShip(ship))
                    {
                        completed = true;
                    }
                    else
                    {
                        currentAttackDirection = Util.Opposite(currentAttackDirection);
                        currentSubStrategy = SubStrategy.OppositeDirectionalAttack;
                        lastShot = firstHit;

                        Point nextShot = Util.Sum(lastShot, currentAttackDirection);

                        if (!IsValidShot(nextShot))
                        {
                            // In this case the direction was wrong.
                            // The target ship is not sink, but it will be sink by a next generated SinkStrategy
                            completed = true;
                        }
                    }
                }
                else
                {
                    Point nextShot = Util.Sum(lastShot, currentAttackDirection);

                    if (!IsValidShot(nextShot))
                    {
                        currentAttackDirection = Util.Opposite(currentAttackDirection);
                        currentSubStrategy = SubStrategy.OppositeDirectionalAttack;
                        lastShot = firstHit;

                        nextShot = Util.Sum(lastShot, currentAttackDirection);

                        if (!IsValidShot(nextShot))
                        {
                            completed = true;
                        }
                    }
                }
            }
            else
            {
                currentAttackDirection = Util.Opposite(currentAttackDirection);
                currentSubStrategy = SubStrategy.OppositeDirectionalAttack;
                lastShot = firstHit;

                Point nextShot = Util.Sum(lastShot, currentAttackDirection);

                if (!IsValidShot(nextShot))
                {
                    // In this case the direction was wrong.
                    // The target ship is not sink, but it will be sink by a next generated SinkStrategy
                    completed = true;
                }
            }
        }

        private void OppositeDirectionalAttack_Shot(Point shot, ShotInfo shotInfo, bool sunk, Ship ship)
        {
            if (shotInfo == ShotInfo.HIT)
            {
                if (sunk)
                {
                    if (!IsTargetShip(ship))
                    {
                        // In this case the direction was wrong.
                        // The target ship is not sunk, but it will be sink by a next generated SinkStrategy
                    }

                    completed = true;
                }
                else
                {
                    Point nextShot = Util.Sum(lastShot, currentAttackDirection);

                    if (!IsValidShot(nextShot))
                    {
                        // In this case the direction was wrong.
                        // The target ship is not sink, but it will be sink by a next generated SinkStrategy
                        completed = true;
                    }
                }
            }
            else
            {
                // In this case the direction was wrong.
                // The target ship is not sink, but it will be sink by a next generated SinkStrategy
                completed = true;
            }
        }

        public override void ShotHit(Point shot, bool sunk, Ship ship)
        {
            hitPoints.Add(shot);

            if (probabilisticStrategy != null)
            {
                probabilisticStrategy.ShotHit(shot, sunk, ship);
            }

            if (sunk)
            {
                foreach (Point p in ship.GetAllLocations())
                    hitPoints.Remove(p);
            }

            Shot(shot, ShotInfo.HIT, sunk, ship);
        }

        public override void ShotMiss(Point shot)
        {
            if (probabilisticStrategy != null)
            {
                probabilisticStrategy.ShotMiss(shot);
            }

            Shot(shot, ShotInfo.MISSED);
        }
    }

    enum ShotInfo : int
    {
        UNKNOWN,
        MISSED,
        HIT
    }

    class Board
    {
        public const int Size = 10;

        private ShotInfo[,] shotInfo { get; set; }

        public Board()
        {
            shotInfo = new ShotInfo[Size, Size];
        }

        public ShotInfo this[Point p]
        {
            get { return shotInfo[p.X, p.Y]; }
            set { this.shotInfo[p.X, p.Y] = value; }
        }

        public ShotInfo this[int x, int y]
        {
            get { return shotInfo[x, y]; }
            set { this.shotInfo[x, y] = value; }
        }

        public static bool Contains(Point p)
        {
            return (0 <= p.X && p.X < Board.Size && 0 <= p.Y && p.Y < Board.Size);
        }
    }
}
