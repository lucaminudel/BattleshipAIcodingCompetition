#region

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public class Grid :IEnumerable<Shot> {
		private List<Shot> _shots;

		public Grid( int width, int heigth )
			: this( new Size( width, heigth ) ) {}

		internal Grid( Size size ) {
			Size = size;
			Init();
		}

		public Size Size { get; set; }

		public IList<Ship> SunkShips { get; private set; }

		public Shot this[ Point point ] {
			get { return _shots.SingleOrDefault( s => s.Position.Equals( point ) ); }
		}

		private void Init() {
			SunkShips = new List<Ship>();

			_shots = ( from rowIndex in Enumerable.Range( 0, Size.Height )
			           from columnIndex in Enumerable.Range( 0, Size.Width )
			           select new Shot( columnIndex, rowIndex ) ).ToList();
		}

		public Shot At( int column, int row ) {
			return this[ new Point( column, row ) ];
		}

		public void Fired( Point point ) {
			var shot = this[ point ];
			if ( shot != null ) {
				shot.Fired();
			}
		}

		public IEnumerator<Shot> GetEnumerator() {
			return _shots.OrderBy( shot => shot.Position.Y ).ThenBy( shot => shot.Position.X ).GetEnumerator();
		}

		public override string ToString() {
			var result = new StringBuilder();
			result.Append( "   " );
			//var ordered = _shots.OrderBy( shot => shot.Position.Y * Size.Width + shot.Position.X );
			Enumerable.Range( 0, Size.Width ).Do(
				index => result.Append( index )
				);
			result.AppendLine();

			_shots.GroupBy( s => s.Position.Y )
				.Do( g => {
				          	result.Append( g.Key + " |" );
				          	g.OrderBy( shot => shot.Position.X )
				          		.Do( shot => {
				          		             	var sunkShip = SunkShips.SingleOrDefault( ship => ship.GetAllLocations().Contains( shot.Position ) );

				          		             	var charToPrint = " ";
				          		             	if ( sunkShip != null ) {
				          		             		charToPrint = sunkShip.Length.ToString();
				          		             	}
				          		             	else if ( !shot.IsAvailable ) {
				          		             		charToPrint = "X";
				          		             	}

				          		             	result.Append( charToPrint );
				          		}
				          		);

				          	result.AppendLine( "|" );
				} );

			return result.ToString();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Sunk( Ship sunkShip ) {
			SunkShips.Add( sunkShip );
		}
	}
}