using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar
{
	public static class Ships
	{
		public static Ship Clone(this Ship template)
		{
			var ship = new Ship(template.Length);

			if (template.IsPlaced)
			{
				ship.Place(template.Location, template.Orientation);
			}

			return template;
		}

		public static bool AreOverlapping(this Ship ship, List<Point> shipsLocations)
		{
			foreach (Point point in ship.GetAllLocations())
			{
				if (shipsLocations.Contains(point))
				{
					return true;
				}
			}

			return false;
		}

		public static void NormalizeShipPlace(int length, ref Point position, ref ShipOrientation orientation)
		{
			if (orientation == ShipOrientation.Vertical)
			{
				position.Y = Math.Min(position.Y, Battlefield.Size - length);
			}

			if (orientation == ShipOrientation.Horizontal)
			{
				position.X = Math.Min(position.X, Battlefield.Size - length);
			}
		}
	}
}