using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
	public class AreeNavi : ProprietaNave
	{
		#region Dichiarazioni
		Dictionary<Point, double> _aree = new Dictionary<Point, double>();
		List<Point> _posizioni = new List<Point>();
		private ShipOrientation _orientamento = ShipOrientation.Horizontal;
		#endregion Dichiarazioni

		#region Costruttori
		public AreeNavi(int lunghezza, ShipOrientation orientamento, Griglia griglia):base(lunghezza)
		{
			_orientamento = orientamento;

			//Calcola Aree
			CalcolaAree(griglia);
			OrdinaAree();
		}
		#endregion Costruttori

		#region Proprietà
		public ShipOrientation Orientamento { get { return _orientamento; } }
		public List<Point> Posizioni { get { return _posizioni; } }
		#endregion Proprietà

		#region Metodi Pubblici
		public List<Point> PrimePosizioni(int numeroDiRaggruppamenti)
		{
			List<Point> tmpPos = new List<Point>();
			Double valorePrecedente = _aree[_posizioni.First()];
			foreach(Point pos in _posizioni)
			{
				if (_aree[pos] == valorePrecedente)
				{
					tmpPos.Add(pos);
				}
				else
				{
					valorePrecedente = _aree[pos];
					numeroDiRaggruppamenti--;
				}

				if (numeroDiRaggruppamenti == 0)
					break;
			}

			return tmpPos;
		}

		public void RimuoviPosizioniOLD(Point inizio, int lunghezza, ShipOrientation orientamento)
		{
			for (int i = -1; i < (lunghezza + 1); i++)
			{
				for (int n = _posizioni.Count - 1; n >= 0; n--)
				{
					Point pos = _posizioni[n];
					if (Orientamento == ShipOrientation.Horizontal)
					{
						if ((pos.Y == inizio.Y) && (pos.X <= inizio.X) && (pos.X + Lunghezza > inizio.X))
							_posizioni.Remove(pos);
					}
					else
					{
						if ((pos.X == inizio.X ) && (pos.Y <= inizio.Y) && (pos.Y + Lunghezza > inizio.Y))
							_posizioni.Remove(pos);
					}
				}

				if (orientamento == ShipOrientation.Horizontal)
					inizio.X++;
				else
					inizio.Y++;
			}
		}

		public void RimuoviPosizioni(Point inizio, int lunghezza, ShipOrientation orientamento)
		{

//            if (orientamento == ShipOrientation.Horizontal)
//                inizio.X--;
//            else
//                inizio.Y--;

//            for (int i = -1; i < (lunghezza + 1); i++)
//            {
//                for (int n = _posizioni.Count - 1; n >= 0; n--)
//                {
//                    Point pos = _posizioni[n];
//                    if (Orientamento == ShipOrientation.Horizontal)
//                    {
////						if ((pos.Y == (inizio.Y - 1)) && (pos.X <= inizio.X) && ((pos.X + Lunghezza + 1)> inizio.X))
////                      if ((pos.Y == inizio.Y) && (pos.X <= inizio.X) && (pos.X + Lunghezza > inizio.X))
////                      if ((Math.Abs(pos.Y - inizio.Y) <= 1) && (pos.X <= inizio.X) && (pos.X + Lunghezza > inizio.X))
//                        if ((Math.Abs(pos.Y - inizio.Y) <= 1) && ((pos.X - 1) <= inizio.X) && (pos.X + Lunghezza > inizio.X))
//                                _posizioni.Remove(pos);
//                    }
//                    else
//                    {
////						if ((pos.X == (inizio.X - 1)) && (pos.Y <= inizio.Y) && ((pos.Y + Lunghezza + 1) > inizio.Y))
////						if ((pos.X == inizio.X ) && (pos.Y <= inizio.Y) && (pos.Y + Lunghezza > inizio.Y))
//                        if ((Math.Abs(pos.X - inizio.X) <= 1) && ((pos.Y - 1) <= inizio.Y) && (pos.Y + Lunghezza > inizio.Y))
//                            _posizioni.Remove(pos);
//                    }
//                }

//                if (orientamento == ShipOrientation.Horizontal)
//                    inizio.X++;
//                else
//                    inizio.Y++;
//            }       

			if (orientamento == ShipOrientation.Horizontal)
				inizio.X--;
			else
				inizio.Y--;

			for (int i = -1; i < (lunghezza + 1); i++)
			{
				for (int n = _posizioni.Count - 1; n >= 0; n--)
				{
					Point pos = _posizioni[n];

					bool rimuovi = false;
					if (i < 0)
					{
						if (AreaContienePunto(inizio, pos))
							rimuovi = true;
					}
					else if (i > lunghezza)
					{
						if (AreaContienePunto(inizio, pos))
							rimuovi = true;
					}
					else
					{
						if (orientamento == ShipOrientation.Horizontal)
						{
							if (AreaContienePunto(inizio, pos))
								rimuovi = true;
							else if (AreaContienePunto(new Point(inizio.X, inizio.Y - 1), pos))
								rimuovi = true;
							else if (AreaContienePunto(new Point(inizio.X, inizio.Y + 1), pos))
								rimuovi = true;
						}
						else
						{
							if (AreaContienePunto(inizio, pos))
								rimuovi = true;
							else if (AreaContienePunto(new Point(inizio.X - 1, inizio.Y), pos))
								rimuovi = true;
							else if (AreaContienePunto(new Point(inizio.X + 1, inizio.Y), pos))
								rimuovi = true;
						}
					}
					if (rimuovi)
						_posizioni.Remove(pos);

				}

				if (orientamento == ShipOrientation.Horizontal)
					inizio.X++;
				else
					inizio.Y++;
			}       

		}
		#endregion Metodi Pubblici

		#region Metodi Privati
		private void CalcolaAree(Griglia griglia)
		{
			if (Orientamento == ShipOrientation.Horizontal)
			{
				//Calcola aree orizzontali
				for (int r = 0; r < griglia.Dimensioni.Height; r++)
				{
					for (int c = 0; c < griglia.Dimensioni.Width - Lunghezza; c++)
					{
						Double val = 0;
						Point cella = new Point(c, r);

						for (int i = 0; i < Lunghezza; i++)
						{
							Point pos = cella;
							pos.X += i;
							val += griglia.Celle[Utility.DaPosizioneAIndice(pos, griglia.Dimensioni)].Valore;
						}
						_aree.Add(cella, val);
					}
				}
			}
			else
			{
				//Calcola aree verticali
				for (int c = 0; c < griglia.Dimensioni.Width; c++)
				{
					for (int r = 0; r < griglia.Dimensioni.Height - Lunghezza; r++)
					{
						Double val = 0;
						Point cella = new Point(c, r);

						for (int i = 0; i < Lunghezza; i++)
						{
							Point pos = cella;
							pos.Y += i;
							val += griglia.Celle[Utility.DaPosizioneAIndice(pos, griglia.Dimensioni)].Valore;
						}
						_aree.Add(cella, val);
					}
				}
			}             
		}
		private void OrdinaAree()
		{
			var sortedDict = (from entry in _aree orderby entry.Value ascending select entry);

			foreach (KeyValuePair<Point, Double> kvp in sortedDict)
				_posizioni.Add(kvp.Key);


			//myList.Sort((firstPair,nextPair) => 
			//    { 
			//        return firstPair.Value.CompareTo(nextPair.Value); 
			//    } 
			//);
		}

		public bool AreaContienePunto(Point punto, Point posizione)
		{
			if (Orientamento == ShipOrientation.Horizontal)
				return (posizione.Y == punto.Y) && (posizione.X <= punto.X) && (posizione.X + Lunghezza > punto.X);
			else
				return (posizione.X == punto.X) && (posizione.Y <= punto.Y) && (posizione.Y + Lunghezza > punto.Y);
		}

		#endregion Metodi Privati

	}
}