using System;
using System.Collections.Generic;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	internal static class EumerableEx {
		public static void Do<T>( this IEnumerable<T> source, Action<T> action ) {
			foreach ( var item in source ) {
				action( item );
			}
		}

		public static ICollection<T> Shuffle<T>( this ICollection<T> source ) {
			var random = new Random();
			var a = new T[source.Count];
			source.CopyTo( a, 0 );
			var b = new byte[a.Length];
			random.NextBytes( b );
			Array.Sort( b, a );
			return new List<T>( a );
		}
	}
}