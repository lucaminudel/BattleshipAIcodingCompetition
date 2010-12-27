using System;
using System.Drawing;

namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
	public class Griglia
	{
		#region Dichiarazioni
		
		private Cella[] _celle;
		private Size _size;

		#endregion Dichiarazioni

		#region Costruttori

		public Griglia(Size size, Double defaultValue)
		{
			_size = size;
			int len = (size.Width * size.Height);
			_celle = new Cella[len];
			for (int i = 0; i < len; i++)
				if (Double.IsNaN(defaultValue))
					_celle[i] = new Cella(Utility.DaIndiceAPosizione(i, size));
				else
					_celle[i] = new Cella(Utility.DaIndiceAPosizione(i, size), defaultValue);

		}
		public Griglia(Size size):this(size, Double.NaN)
		{
		}

		#endregion Costruttori

		#region Proprietà
		public Cella[] Celle{ get { return _celle; } }
		public Size Dimensioni { get { return _size; } }
		#endregion Proprietà

		#region Metodi Pubblici
		#endregion Metodi Pubblici

		#region Metodi Privati
		#endregion Metodi Privati
	}
}