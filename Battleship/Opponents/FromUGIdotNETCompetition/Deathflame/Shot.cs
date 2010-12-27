#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

#endregion

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public class Shot {
		public Shot( int columnIndex, int rowIndex ) :
			this( new Point( columnIndex, rowIndex ) ) {}

		internal Shot( Point point ) {
			Position = point;
			IsAvailable = true;
		}

		public bool IsAvailable { get; set; }

		public Point Position { get; private set; }

		public void Fired() {
			IsAvailable = false;
		}

		public bool Equals( Shot other ) {
			if ( ReferenceEquals( null, other ) ) {
				return false;
			}
			if ( ReferenceEquals( this, other ) ) {
				return true;
			}
			return other.Position.Equals( Position );
		}

		public override bool Equals( object obj ) {
			if ( ReferenceEquals( null, obj ) ) {
				return false;
			}
			if ( ReferenceEquals( this, obj ) ) {
				return true;
			}
			if ( obj.GetType() != typeof ( Shot ) ) {
				return false;
			}
			return Equals( (Shot) obj );
		}

		public override int GetHashCode() {
			return Position.GetHashCode();
		}

		public override string ToString() {
			return string.Format( "{{C:{0}, R:{1}}} - {2}", Position.X, Position.Y, IsAvailable ? "O" : "X" );
		}

		public IEnumerable<Shot> GetNeighbours( Grid grid ) {
			return GetHorizontalNeighbours( grid )
				.Concat( GetVerticalNeighbours( grid ) );
		}

		internal IEnumerable<Shot> GetHorizontalNeighbours( Grid grid ) {
			return new[] {
			             	Left( grid ), Right( grid )
			             }.Where( shot => shot != null );
		}

		internal IEnumerable<Shot> GetVerticalNeighbours( Grid grid ) {
			return new[] {
			             	Top( grid ), Bottom( grid )
			             }.Where( shot => shot != null );
		}

		internal Shot Left( Grid grid ) {
			return grid[ Point.Add( Position, new Size( -1, 0 ) ) ];
		}

		internal Shot Right( Grid grid ) {
			return grid[ Point.Add( Position, new Size( 1, 0 ) ) ];
		}

		internal Shot Top( Grid grid ) {
			return grid[ Point.Add( Position, new Size( 0, -1 ) ) ];
		}

		internal Shot Bottom( Grid grid ) {
			return grid[ Point.Add( Position, new Size( 0, +1 ) ) ];
		}

		public Distance GetDistance( Grid grid ) {
			return new Distance( this, grid );
		}

		#region Nested type: Distance
		public class Distance {
			private readonly Grid _grid;
			private readonly Shot _shot;

			public Distance( int left, int right, int top, int bottom ) {
				Left = left;
				Right = right;
				Top = top;
				Bottom = bottom;
			}

			public Distance( Shot shot, Grid grid ) {
				_shot = shot;
				_grid = grid;

				Left = LeftDistance();
				Right = RightDistance();
				Top = TopDistance();
				Bottom = BottomDistance();
			}

			public int Left { get; private set; }
			public int Right { get; private set; }
			public int Top { get; private set; }
			public int Bottom { get; private set; }

			public int Vertical {
				get { return 1 + Top + Bottom; }
			}

			public int Horizontal {
				get { return 1 + Right + Left; }
			}

			private int RightDistance() {
				return GetDistance( ( s, g ) => s.Right( g ) );
			}

			private int LeftDistance() {
				return GetDistance( ( s, g ) => s.Left( g ) );
			}

			private int TopDistance() {
				return GetDistance( ( s, g ) => s.Top( g ) );
			}

			private int BottomDistance() {
				return GetDistance( ( s, g ) => s.Bottom( g ) );
			}

			private int GetDistance( Func<Shot, Grid, Shot> next ) {
				var shot = _grid[ _shot.Position ];
				var result = 0;
				while ( next( shot, _grid ) != null && next( shot, _grid ).IsAvailable ) {
					result++;
					shot = next( shot, _grid );
				}
				return result;
			}

			public int GetQuality( int maxHoleSize ) {
				var horizontal = ( ( Left + 1 ) * ( Right + 1 ) ) *
				                 ( ( maxHoleSize - ( Left + 1 ) % maxHoleSize ) + ( maxHoleSize - ( Right + 1 ) % maxHoleSize ) );
				var vertical = ( ( Top + 1 ) * ( Bottom + 1 ) ) *
				               ( ( maxHoleSize - ( Top + 1 ) % maxHoleSize ) + ( maxHoleSize - ( Bottom + 1 ) % maxHoleSize ) );
				return horizontal + vertical;
			}

			public override string ToString() {
				return string.Format( "L: {0}, R: {1}, T:{2}, B:{3}", Left, Right, Top, Bottom );
			}
		}
		#endregion
	}
}