using System.Drawing;

namespace Battleship.Opponents.FromStackoverflowCompetition.BSKiller4
{
	public class shotPossibilities
	{
		public shotPossibilities(int r, Point s, bool h)
		{
			this.run = r;
			this.shot = s;
			this.isHorizontal = h;
		}
		public int run { get; set; }
		public Point shot { get; set; }
		public bool isHorizontal { get; set; }
	}
}