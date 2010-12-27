using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
	public class Nave : ProprietaNave
	{
		#region Dichiarazioni
		Dictionary<Point, bool> _celleColpite = new Dictionary<Point, bool>();
		private ShipOrientation _orientamento = ShipOrientation.Horizontal;
		private bool _affondata = false;
		#endregion Dichiarazioni

		#region Costruttori
		public Nave(Point posizione, int lunghezza, ShipOrientation orientamento)
			: base(lunghezza)
		{
			_orientamento = orientamento;

			_celleColpite.Add(posizione, false);

			//Calcola le celle occupate dalla nave
			for (int i = 1; i < lunghezza; i++)
			{
				if (orientamento == ShipOrientation.Horizontal)
					posizione.X++;
				else
					posizione.Y++;

				_celleColpite.Add(posizione, false);
			}
		}
		#endregion Costruttori

		#region Proprietà
		public ShipOrientation Orientamento { get { return _orientamento; } }
		public List<Point> Posizioni { get { return _celleColpite.Keys.ToList(); } }
		public Dictionary<Point, bool> CelleNave { get { return _celleColpite; } }
		public bool Affondata { get { return _affondata; } set { _affondata = value; } }
		#endregion Proprietà

		#region Metodi Pubblici
		public void CellaColpita(Point colpo)
		{
			_celleColpite[colpo] = true;

			if (!_celleColpite.Values.Contains(false))
				_affondata = true;
		}
		#endregion Metodi Pubblici

		#region Metodi Privati
		#endregion Metodi Privati
	}
}