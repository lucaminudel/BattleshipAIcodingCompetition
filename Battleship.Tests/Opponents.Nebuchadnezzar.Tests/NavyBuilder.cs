using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Tests
{
	public class NavyBuilder
	{
		private int _length;
		private int _locationX;
		private int _locationY;
		private ShipOrientation _orientation;

		public static NavyBuilder AShip()
		{
			return new NavyBuilder();
		}

		public NavyBuilder ThatIsLikeA(Ship template)
		{
			_length = template.Length;
			return this;
		}

		public NavyBuilder AtLocation(int x, int y)
		{
			_locationX = x;
			_locationY = y;
			return this;
		}

		public NavyBuilder WithOrientation(ShipOrientation orientation)
		{
			_orientation = orientation;
			return this;
		}

		public Ship Build()
		{
			var ship = new Ship(_length);
			ship.Place(new Point(_locationX, _locationY), _orientation);

			return ship;
		}

		public NavyBuilder But()
		{
			var clone = new NavyBuilder
			            	{
			            		_length = _length,
			            		_locationX = _locationX,
			            		_locationY = _locationY,
			            		_orientation = _orientation
			            	};


			return clone;
		}

	}
}
