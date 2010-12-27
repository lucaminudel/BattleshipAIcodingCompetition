using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar
{
	public class Battlefield
	{
		public const int Size = 10;

		public IEnumerable<Point> Cells()
		{
			for (int x = 0; x < Size; ++x)
			{
				for (int y = 0; y < Size; ++y)
				{
					yield return new Point(x, y);
				}
			}
		}
	}
}