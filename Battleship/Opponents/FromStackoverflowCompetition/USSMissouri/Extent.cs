using System;

namespace Battleship.Opponents.FromStackoverflowCompetition.USSMissouri
{
	public struct Extent
	{
		public Extent(Int32 min, Int32 max)
		{
			Min = min;
			Max = max;
		}
		public Int32 Min;
		public Int32 Max;
	}
}