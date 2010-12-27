namespace Battleship
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public sealed class Ship
    {
        private bool isPlaced = false;
        private Point location;
        private ShipOrientation orientation;
        private int length;

        public Ship(int length)
        {
            if (length <= 1)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            this.length = length;
        }

        public bool IsPlaced
        {
            get
            {
                return this.isPlaced;
            }
        }

        public Point Location
        {
            get
            {
                if (!this.isPlaced)
                {
                    throw new InvalidOperationException();
                }

                return this.location;
            }
        }

        public ShipOrientation Orientation
        {
            get
            {
                if (!this.isPlaced)
                {
                    throw new InvalidOperationException();
                }

                return this.orientation;
            }
        }

        public int Length
        {
            get
            {
                return this.length;
            }
        }

        public void Place(Point location, ShipOrientation orientation)
        {
            this.location = location;
            this.orientation = orientation;
            this.isPlaced = true;
        }

        public bool IsValid(Size boardSize)
        {
            if (!this.isPlaced)
            {
                return false;
            }

            if (this.location.X < 0 || this.location.Y < 0)
            {
                return false;
            }

            if (this.orientation == ShipOrientation.Horizontal)
            {
                if (this.location.Y >= boardSize.Height || this.location.X + this.length > boardSize.Width)
                {
                    return false;
                }
            }
            else
            {
                if (this.location.X >= boardSize.Width || this.location.Y + this.length > boardSize.Height)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsAt(Point location)
        {
            if (this.Orientation == ShipOrientation.Horizontal)
            {
                return (this.location.Y == location.Y) && (this.location.X <= location.X) && (this.location.X + this.length > location.X);
            }
            else
            {
                return (this.location.X == location.X) && (this.location.Y <= location.Y) && (this.location.Y + this.length > location.Y);
            }
        }

        public IEnumerable<Point> GetAllLocations()
        {
            if (this.Orientation == ShipOrientation.Horizontal)
            {
                for (int i = 0; i < this.length; i++)
                {
                    yield return new Point(this.location.X + i, this.location.Y);
                }
            }
            else
            {
                for (int i = 0; i < this.length; i++)
                {
                    yield return new Point(this.location.X, this.location.Y + i);
                }
            }
        }

        public bool ConflictsWith(Ship otherShip)
        {
            foreach (var otherShipLocation in otherShip.GetAllLocations())
            {
                if (this.IsAt(otherShipLocation))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSunk(IEnumerable<Point> shots)
        {
            foreach (Point location in this.GetAllLocations())
            {
                if (!shots.Where(s => s.X == location.X && s.Y == location.Y).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
