using System;
using System.Drawing;

namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
	public class Cella
	{
		private Point _posizione;
		private Double _valore;

		public Cella(Point posizione)
		{
			_posizione = posizione;
		}
		public Cella(Point posizione, Double value)
			: this(posizione)
		{
			_valore = value;
		}
		public Point Posizione { get { return _posizione; } set { _posizione = value; } }
		public Double Valore { get { return _valore; } set { _valore = value; } }

		public int Indice(Size size)
		{
			if (_posizione == null)
				return 0;

			return (_posizione.Y * size.Width + _posizione.X);
		}

	}
}