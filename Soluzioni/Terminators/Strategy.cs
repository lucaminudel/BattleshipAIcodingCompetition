using System.Drawing;

namespace Battleship.Opponents.Terminators
{
    abstract class Strategy
    {
        public GameInfo GameInfo { get; set; }
        public MatchInfo MatchInfo { get; set; }

        protected Strategy(MatchInfo matchInfo, GameInfo gameInfo)
        {
            GameInfo = gameInfo;
            MatchInfo = matchInfo;
        }

        public abstract bool Completed { get; set; }

        public abstract Point GetNextShot();

        public virtual void ShotHit(Point shot) {}
        public virtual void ShotHitAndSink(Point shot, Ship sunkShip) {}        
        
        public virtual void ShotMiss(Point shot) {}
    };
}
