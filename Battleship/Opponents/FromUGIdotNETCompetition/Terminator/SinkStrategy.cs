using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Terminator
{
    class SinkStrategy : Strategy
    {
        private enum SubStrategy
        {
            DirectionFinding,
            DirectionalAttack,
            OppositeDirectionalAttack
        }

        private readonly ProbabilisticStrategy probabilisticStrategy;
        private SubStrategy currentSubStrategy;

        private readonly Point firstHit;
        private Point lastShot;

        private Point candidateAttackDirection;
        private Point currentAttackDirection;
        private readonly Stack<Point> attackDirections;

        private readonly List<Point> hitPoints;

        private bool completed;

        private readonly Stack<Strategy> strategyStack;

        public override bool Completed
        {
            get
            {
                if (currentSubStrategy != SubStrategy.DirectionFinding)
                {
                    return completed;
                }

                return attackDirections.All(direction => !IsValidShot(Util.Sum(firstHit, direction)));
            }

            set { completed = value; }
        }

        public SinkStrategy(MatchInfo matchInfo, GameInfo gameInfo, Point firstHit, Stack<Strategy> strategyStack, ProbabilisticStrategy probabilisticStrategy = null) : base(matchInfo, gameInfo)
        {
            this.probabilisticStrategy = probabilisticStrategy;
            this.strategyStack = strategyStack;
            this.firstHit = firstHit;

            var directions = new List<Point> {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, +1),
                new Point(0, -1)
            };

            attackDirections = new Stack<Point>(directions.Shuffle());
            hitPoints = new List<Point> { firstHit };

            currentSubStrategy = SubStrategy.DirectionFinding;
        }

        public override Point GetNextShot()
        {
            Point nextShot;

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
            return Board.Contains(shot) && GameInfo.OpponentBoard[shot] == ShotInfo.Unknown;
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
                    DirectionFindingShot(shotInfo, sunk, ship);
                    break;

                case SubStrategy.DirectionalAttack:
                    DirectionalAttackShot(shotInfo, sunk, ship);
                    break;

                case SubStrategy.OppositeDirectionalAttack:
                    OppositeDirectionalAttackShot(shotInfo, sunk, ship);
                    break;
            }

            if (completed)
            {
                // Create new strategy to sink new ships found during the process
                foreach (Point p in hitPoints)
                {
                    strategyStack.Push(new SinkStrategy(MatchInfo, GameInfo, p, strategyStack, probabilisticStrategy));
                }
            }
        }

        private void DirectionFindingShot(ShotInfo shotInfo, bool sunk, Ship ship)
        {
            if (shotInfo == ShotInfo.Missed)
                return;

            if (sunk)
            {
                if (IsTargetShip(ship))
                {
                    completed = true;
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

        private void DirectionalAttackShot(ShotInfo shotInfo, bool sunk, Ship ship)
        {
            if (shotInfo == ShotInfo.Hit)
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

        private void OppositeDirectionalAttackShot(ShotInfo shotInfo, bool sunk, Ship ship)
        {
            if (shotInfo == ShotInfo.Hit)
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

        public override void ShotHit(Point shot)
        {
            hitPoints.Add(shot);

            if (probabilisticStrategy != null)
            {
                probabilisticStrategy.ShotHit(shot);
            }

            Shot(shot, ShotInfo.Hit);
        }

        public override void ShotHitAndSink(Point shot, Ship sunkShip)
        {
            hitPoints.Add(shot);

            if (probabilisticStrategy != null)
            {
                probabilisticStrategy.ShotHitAndSink(shot, sunkShip);
            }

            foreach (Point p in sunkShip.GetAllLocations())
            {
                hitPoints.Remove(p);                
            }

            Shot(shot, ShotInfo.Hit, true, sunkShip);
        }

        public override void ShotMiss(Point shot)
        {
            if (probabilisticStrategy != null)
            {
                probabilisticStrategy.ShotMiss(shot);
            }

            Shot(shot, ShotInfo.Missed);
        }
    }
}
