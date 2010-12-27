#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public class FillHolesShotProvider : IShotProvider {
		private readonly Grid _grid;

		public FillHolesShotProvider( Grid grid, int maxShipSize ) {
			MaxShipSize = maxShipSize;
			_grid = grid;
		}

		public int MaxShipSize { get; private set; }

		#region IShotProvider Members
		public IEnumerable<Shot> Shots() {
			var shotAndDistances = _grid
				.Where( shot => shot.IsAvailable )
				.Select( shot => new {
				                     	Shot = shot,
				                     	Distance = shot.GetDistance( _grid )
				                     } );

			var maxLikeness = shotAndDistances.Max( sd => sd.Distance.GetQuality( MaxShipSize ) );

			return
				shotAndDistances
					.Where( sd => ( sd.Distance.GetQuality( MaxShipSize ) == maxLikeness ) &&
					              ( sd.Distance.Vertical >= MaxShipSize || sd.Distance.Horizontal >= MaxShipSize ) )
					.Select( sd => sd.Shot );
		}
		#endregion
	}
}