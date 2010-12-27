using System.Drawing;

namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
	public static class Utility
	{
		public static Point DaIndiceAPosizione(int indice, Size size)
		{
			return new Point(indice % size.Width, indice / size.Width);
		}
		public static int DaPosizioneAIndice(Point posizione, Size size)
		{
			if (posizione == null)
				return 0;

			return (posizione.Y * size.Width + posizione.X);
		}
	}
}