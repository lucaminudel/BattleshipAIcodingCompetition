using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
	public static class Extensions
	{
		public static Point[] ToArray(this Griglia griglia)
		{
			var array = from posizione
			            	in griglia.Celle
			            select posizione.Posizione;

			return array.ToArray();
		}

		//public static Double Min(this Griglia griglia)
		//{
		//    var tmpValori = (from entry in  griglia.Celle select entry.Valore);
		//    return tmpValori.Min();
		//}
		//public static Double Max(this Griglia griglia)
		//{
		//    var tmpValori = (from entry in griglia.Celle select entry.Valore);
		//    return tmpValori.Max();
		//}

		public static List<Point> OrdinaPosizioni(this Griglia griglia, List<Point> posizioni, bool crescente)
		{           
			IEnumerable<Cella> ordinato;
			
			//bool _statistichePartita = true;

			if (crescente)
				ordinato = (from entry in griglia.Celle orderby entry.Valore ascending select entry);
			else
				ordinato = (from entry in griglia.Celle orderby entry.Valore descending select entry);

			List<Point> retPosizioni = new List<Point>();
			foreach (Cella cella in ordinato)
			{
				//if (_statistichePartita)
				//    Debug.WriteLine("OrdinaPosizioni: X = " + cella.Posizione.X + " - Y = " + cella.Posizione.Y + " Valore =" + cella.Valore.ToString());

				//Prende solo le celle del Pattern passato.
				if (posizioni.Contains(cella.Posizione))
					retPosizioni.Add(cella.Posizione);
			}
			return retPosizioni;
		}
		public static void AggiornaStatistica(this Griglia griglia, int[] valori)
		{
			for (int i = 0; i < griglia.Celle.Length; i++)
				griglia.Celle[i].Valore = (griglia.Celle[i].Valore + valori[i]) / 2;
		}

		public static void Incrementa(this Griglia griglia, int[] valori)
		{
			for (int i = 0; i < griglia.Celle.Length; i++)
				griglia.Celle[i].Valore += valori[i];
		}

		public static void Stampa(this Griglia griglia, string testo)
		{
			Debug.WriteLine(testo);
			string tab = "\t";
			StringBuilder colonne = new StringBuilder(tab);            

			for (int i = 0; i < griglia.Dimensioni.Width; i++)
				colonne.Append(i.ToString() + tab);

			Debug.WriteLine(colonne.ToString());

			for (int r = 0; r < griglia.Dimensioni.Height; r++)
			{
				StringBuilder riga = new StringBuilder();
				riga.Append(r.ToString() + tab);
				for (int c = 0; c < griglia.Dimensioni.Width; c++)
				{
					Point cella = new Point(c, r);
					riga.Append((griglia.Celle[Utility.DaPosizioneAIndice(cella, griglia.Dimensioni)].Valore).ToString() + tab);
				}

				Debug.WriteLine(riga.ToString());
			}
		}
	}
}