using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
	public class SubArea
	{
		#region Dichiarazioni
		private Dictionary<Point, double> _pesoCelle = new Dictionary<Point,double>();
		private double _valoreArea = 0.0;
		#endregion Dichiarazioni

		#region Costruttori
		public SubArea()
		{
			//_pesoCelle = new Dictionary<Point,double>(numCelle);
		}
		#endregion Costruttori

		#region Proprietà
		public double ValoreSubArea{ get {return _valoreArea;} }
		public Point[] Celle { get { return _pesoCelle.Keys.ToArray(); } }
		#endregion Proprietà

		#region Metodi Pubblici
		public void AggiungiCella(Point cella, double valore)
		{
			if (!_pesoCelle.ContainsKey(cella))
			{
				_pesoCelle.Add(cella, valore);
				AggiornaValore();
			}
		}
		public void RimuoviCella(Point cella)
		{
			if (_pesoCelle.ContainsKey(cella))
			{
				_pesoCelle.Remove(cella);
				AggiornaValore();
			}
		}
		public double ValoreCella(Point cella)
		{
			if (_pesoCelle.ContainsKey(cella))
				return _pesoCelle[cella];
			else
				return 0.0;
		}
		#endregion Metodi Pubblici

		#region Metodi Privati
		private void AggiornaValore()
		{
			_valoreArea = 0.0;
			foreach (KeyValuePair<Point, double> kvp in _pesoCelle)
				_valoreArea += kvp.Value;
		}
		#endregion Metodi Privati
	}
}