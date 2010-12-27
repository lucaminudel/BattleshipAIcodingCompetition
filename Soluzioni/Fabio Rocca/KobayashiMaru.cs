using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Battleship.Opponents.KobayashiMaru
{
	public enum FasePartita
	{
		RicercaNavi = 0,
		NaveColpita = 1,
		AffondamentoNave = 2
	}

	public enum Direzione
	{
		Su = 0,
		Giu = 1,
		Destra = 2,
		Sinistra = 3
	}

	public class KobayashiMaru : IBattleshipOpponent
	{
		#region Dichiarazioni

		private List<ProprietaNave> _listaNavi = new List<ProprietaNave>(); // Ship Sizes { 5, 4, 3, 3, 2 }

		//Analizza la sequenza e preferenza dei colpi sparati dall'avversario
		private Griglia _statisticaColpiAvversario;
		//Analizza la preferenza nella disposizione delle navi da parte dall'avversario
		private Griglia _statisticaPosizioneNaviAvversario;
		//Analizza la preferenza dall'avversario ad orientare le navi disposte
		private Dictionary<ProprietaNave, Double> _statisticaOrientamentoNaviAvversario;
		//Analizza la preferenza dall'avversario a seguire un percorso dopo aver colpito una nave
		private Double _statisticaRicercaAvversarioNaveColpita;

		//Posizionamento proprie navi per la partita corrente
		private List<Nave> _navi;

		//Contiene l'elenco di possibili zone in cui piazzare le proprie navi,
		// calcolato in base ai colpi sparati dall'avversario nelle precedenti partite.
		private List<AreeNavi> _areeNavi;

		//Memorizza la sequenza dei colpi sparati dall'avversario ad ogni partita
		private int[] _colpiAvversario;
		//Memorizza le celle occupate dalle navi dell'avversario
		private int[] _naviAvversario;
		//Memorizza il primo colpo a segno dell'Avversario
		private Point _primoColpoAvversario;
		//Numero colpo avversario quando è andato a segno per la prima volta
		private int _numPrimoColpoAvversario = 0;

		//Riporta lo stato corrente dell'Avversario
		private FasePartita _statoAvversario = FasePartita.RicercaNavi;
		//Contatore colpi avversario
		private int _numColpiAvversario = 0;


		//Contatore per riportare la sequenza dei colpi dell'avversario utilizzata nelle statistiche.
		private int _PesoColpoAvversario;

		//Riporta la sequenza dei propri colpi sparati ad ogni partita
		private Dictionary<Point, bool> _colpiSparati;
		//Contiene la lista principale dei possibili colpi da sparare basato su un disegno a scacchiera
		private List<Point> _patternPrimario;
		//Contiene la lista secondaria dei possibili colpi da sparare basato su un disegno a scacchiera
		private List<Point> _patternSecondario;
		//Nell'eventualità che tutti i colpi della scacchiera siano stati sparati, riporta gli altri colpi della griglia non ancora colpiti
		private List<Point> _colpiRimasti;


		//Quando si è in modalità "NaveColpita", contiene la sequenza dei colpi da sparare per completare l'affondamento della/e navi.
		private List<Dictionary<Point, Direzione>> _direzioniPossibili;
		//Quando si è in modalità "NaveColpita", riporta il prossimo colpo da sparare in GetShot
		private Point _prossimoColpoDaSparare;
		//Quando si è in modalità "NaveColpita", riporta la direzione che si sta seguendo nella strategia di affondamento
		private Direzione _direzioneCorrente;
		//Quando si è in modalità "NaveColpita", conta i colpi andati a segno fino all'affondamento della nave.
		//Tale contatore viene utilizzato per verificare se, nella strategia di "affondamento" sia stata colpita anche un'altra nave
		private int _colpiASegnoSuccessivi = 0;
		//Mantiene traccia se, durante l'affondamento di una nave, è cambiata la direzione dei colpi andati a segno,
		// questo per ottimizzare la sequenza dei colpi da sparare nel caso di navi affiancate nella stessa direzione.
		// (per il momento non viene usato)
		private bool _orientamentoCambiato = false;

		//Riporta lo stato corrente di esecuzione della partita
		private FasePartita _statoCorrente = FasePartita.RicercaNavi;

		//Indica il numero di partite da cui iniziare ad adottare le statistiche analizzate
		// per sparare i colpi in base alle attidudini dell'avversario.
		private const int numPartiteAllenamento = 150;
		//Numero partite giocate.
		int _numPartite = 0;
		/********************************************/

		/**** DEBUG *****/
		//Variabili utilizzate per il debug
		bool _mostraPartita = false;
		bool _tracciaPartita = false;
		bool _statistichePartita = false;
		Double _mediaColpiSparati = 0.0;
		/**** DEBUG *****/

		/********************************************/
		Random _rand = new Random();
		Version _version = new Version(1, 0);
		Size _gameSize = new Size(10, 10);

		#endregion Dichiarazioni

		#region Proprietà

		public string Name { get { return "Kobayashi Maru"; } }
		public Version Version { get { return this._version; } }

		#endregion Proprietà

		#region Metodi Pubblici
		//01 - Inizia la sfida
		public void NewMatch(string opponent) 
		{
			//  Viene passato il nome dell'avversario + Versione
			//  ... potrebbe essere utile per uno storico delle partite x eventuali sfide future.    
    
            
            
			//Per me la dimensione della griglia e il tempo a disposizione per le mosse 
			// andrebbero impostati qui e non ad ogni partita!!! :-(

			//Inizializza tabelle per lo storico delle sfide.
			InizializzaSfida();
		}

		//02 - Inizia una nuova partita, però nella stessa sfida
		public void NewGame(Size size, TimeSpan timeSpan)
		{
			//Se la dimensione è diversa da quella prevista (10x10),
			// allora reinizializzo le tabelle dello storico.
			if (size.Width != _gameSize.Width &&
			    size.Height != _gameSize.Height)
			{
				this._gameSize = size;

				//Inizializza tabelle per lo storico delle sfide.
				InizializzaSfida();
			}

			//timeSpan tempo a disposizione per ogni mossa, se superato si perde la partita.

			_numPartite++;

			//Inizializza tabelle per una nuova partita.
			InizializzaPartita();
		}

		//03 - Piazza le navi sulla scacchiera
		public void PlaceShips(ReadOnlyCollection<Ship> ships)
		{
			#region Verifica se il numero e le dimensioni delle navi sono cambiate
			bool naviCambiate = false;

			if (ships.Count != _listaNavi.Count)
			{
				naviCambiate = true;
			}
			else
			{
				List<int> tmpNavi = (from entry in _listaNavi select entry.Lunghezza).ToList<int>() ;

				foreach (Ship s in ships)
				{
					if (tmpNavi.Contains(s.Length))
						tmpNavi.Remove(s.Length);
					else
					{
						naviCambiate = true;
						break;
					}
				}
				if (tmpNavi.Count >0)
					naviCambiate = true;
			}

			if (naviCambiate)
			{
				List<int> tmpNavi = new List<int>();
				foreach (Ship s in ships)
					tmpNavi.Add(s.Length);
                
				// Mette prima le navi più grandi
				tmpNavi.Sort();
				tmpNavi.Reverse();

				_listaNavi = new List<ProprietaNave>();
				foreach(int len in tmpNavi)
					_listaNavi.Add(new ProprietaNave(len));

				//Reimposta Statistiche
				_statisticaOrientamentoNaviAvversario = new Dictionary<ProprietaNave, Double>();
				foreach (ProprietaNave pn in _listaNavi)
					_statisticaOrientamentoNaviAvversario.Add(pn, 0.50);

				ImpostaAreeNavi();
			}
			#endregion Verifica se il numero e le dimensioni delle navi sono cambiate

			//foreach (Ship s in ships)
			//{
			//    s.Place(
			//        new Point(
			//            _rand.Next(this._gameSize.Width),
			//            _rand.Next(this._gameSize.Height)),
			//        (ShipOrientation)_rand.Next(2));
			//}


			// Dispone le navi nelle zone di solito non colpite ai primi colpi dall'avversario 
			// usando la statistica della sequenza di colpi subiti
			foreach (Ship s in ships)
			{
				// Orientare le navi in modo contrario alla sua tattica di "affondamento" 
				// (statistica dell'orientamento seguito (V/O) per l'affondamento)

				//ShipOrientation orientamento = (ShipOrientation)_rand.Next(2);
				//TODO
				ShipOrientation orientamento = ShipOrientation.Vertical;
				int val = _rand.Next(100);
				int peso = Convert.ToInt32(100.0 * _statisticaRicercaAvversarioNaveColpita);

				//Inverte preferenza avversario ( con val < peso si ha l'orientamento corretto)
				if (val >= peso)
					orientamento = ShipOrientation.Horizontal;

				//if (_numPartite > numPartiteAllenamento)
				//    Debug.WriteLine("_statisticaRicercaAvversarioNaveColpita = " + _statisticaRicercaAvversarioNaveColpita.ToString());


				AreeNavi areaNavecorrente = null;
				foreach (AreeNavi an in _areeNavi)
				{
					if (an.Lunghezza == s.Length &&
					    an.Orientamento == orientamento &&
					    !an.Usata)
					{
						areaNavecorrente = an;
						an.Usata = true;
						break;
					}
				}

				Point[] posizioniNave = areaNavecorrente.PrimePosizioni(1).ToArray();
				Point posizioneNave = posizioniNave[_rand.Next(posizioniNave.Length)];

				s.Place(posizioneNave, areaNavecorrente.Orientamento);

				//Rimuove nelle _areeNavi non ancora usate, le posizioni che si sovrappongono con quella usata
				foreach (AreeNavi an in _areeNavi)
					if (!an.Usata)
						an.RimuoviPosizioni(posizioneNave, s.Length, orientamento);
			}

			foreach (Ship s in ships)
				_navi.Add(new Nave(s.Location, s.Length, s.Orientation));

			//StampaNavi();
		}

		//04 - Spara un colpo all'avversario
		public Point GetShot()
		{
			//Prima di sparare occorre verificare in qual stato si è:
			// 1 - Se si è in 'ShotHit', bisogna adottare la strategia di affondare la nave
			// 2 - Se si è in 'ShotMiss', bisogna adottare la strategia di ricerca sul pattern definito

			Point colpo;

			if (_statoCorrente == FasePartita.RicercaNavi)
			{
				// *** STRATEGIA 2 'ShotMiss' - Cerca in base al pattern e alla tendenza dell'avversario ***
				//  Calcolare il colpo da sparare seguendo i pattern diagonali ed intervallati, 
				//   prediligendo i punti dove solitamente l'avversario piazza le navi (statistica delle celle occupate dalla navi dell'avversario)
                
				bool colpoValido = true;

				do
				{
					//TODO: Va sviluppato meglio perchè si rileva un'aram a doppio taglio.
					//      Per ora prende i colpi dal pattern senza tenere conto delle disposizioni delle navi dell'avversario.

					//Si è ancora in fase di studio, i colpi nella scacchiera sono scelti casualmente
					//if (_numPartite < numPartiteAllenamento)
					//{
					int sceltaPattern = _rand.Next(100);

					if ((_patternPrimario.Count > 0 && sceltaPattern < 90) ||
					    (_patternPrimario.Count > 0 && _patternSecondario.Count == 0))
					{
						colpo = _patternPrimario[_rand.Next(_patternPrimario.Count)];
						_patternPrimario.Remove(colpo);
						if (_patternPrimario.Count == 0 && _patternSecondario.Count == 0)
							ImpostaColpiRimasti(colpo);
					}
					else if (_patternSecondario.Count > 0)
					{
						colpo = _patternSecondario[_rand.Next(_patternSecondario.Count)];
						_patternSecondario.Remove(colpo);
						if (_patternPrimario.Count == 0 && _patternSecondario.Count == 0)
							ImpostaColpiRimasti(colpo);
					}
					else
					{
						//Se i colpi sul pattern sono terminati,
						// allora sceglie casualmente tra i colpi rimasti sulla scacchiera.
						// In  teoria questo caso non dovrebbe mai capitare se la strategia di affondamento
						// è corretta.
						colpo = _colpiRimasti[_rand.Next(_colpiRimasti.Count)];
						_colpiRimasti.Remove(colpo);

						//colpo = new Point(_rand.Next(this._gameSize.Width), _rand.Next(this._gameSize.Height));
					}
					//}
					////Ora, avendo un po' d'informazioni, si può dedurre la tendenza dell'avversario nel disporre le navi.
					//else
					//{
					//    //Pur basandosi su un pattern a scacchiera, la sequenza dei colpi viene 
					//    // stabilita in base all'attitudine dell'avversario di disporre le navi.
					//    // (Prima i colpi dove la presenza di navi "statisticamente" è maggiore).
					//    if (_patternPrimario.Count > 0)
					//    {
					//        colpo = _patternPrimario.First();
					//        _patternPrimario.Remove(colpo);

					//        if (_patternPrimario.Count == 0)
					//            ImpostaColpiRimasti(colpo);
					//    }
					//    else
					//    {
					//        colpo = _colpiRimasti.First();
					//        _colpiRimasti.Remove(colpo);
					//    }
					//}

					//  Verificare che vi sia almeno una cella adiacente non utilizzata, 
					//   altrimenti rimuovere il colpo dal pattern e cercare un altro colpo.
					//   Se ho sparato nelle 4 celle intorno (L/T/R/B), non può esserci una nave di dimensione 1.
					colpoValido = VerificaColpiAdiacentiLiberi(colpo);

				} while (!colpoValido);
			}
			else
			{
				// Applica la Strategia - Affonda la nave ***
				colpo = _prossimoColpoDaSparare;

				if (_patternPrimario.Contains(colpo))
					_patternPrimario.Remove(colpo);

				if (_patternSecondario.Contains(colpo))
					_patternSecondario.Remove(colpo);

				if (_colpiRimasti.Contains(colpo))
					_colpiRimasti.Remove(colpo);

				EliminaDirezionePossibile(colpo);
			}

			Traccia("Spara a : ", colpo);

			if (!_colpiSparati.Keys.Contains(colpo))
				_colpiSparati.Add(colpo, false);
			else
			{
				//ERORRE!!!
				_mostraPartita = true;
				MostraPartita("Colpo già sparato !!! X=" + colpo.X.ToString() + ", Y=" + colpo.Y.ToString());
				_mostraPartita = false;
			}
			return colpo;
		}

		//05 a - Notifica che il colpo sparato all'avversario è andato a vuoto
		public void ShotMiss(Point shot) 
		{
			//Se stavo in fase NaveColpita,
			// allora cambio direzione.
			if (_statoCorrente == FasePartita.NaveColpita)
			{
				CambiaDirezione();
				ImpostaProssimoColpo();
			}
		}
		//05 b - Notifica che il colpo sparato all'avversario ha colpito una nave ma ancora non non l'ha affondata
		public void ShotHit(Point shot)
		{
			ShotHit(shot, false);

			// *** STRATEGIA 1 'ShotHit' - Affonda la nave ***

			// Se è il primo colpo a segno, cercare l'adiacente O/V in base alle statistiche di orientamento preferito dall'avversario.
			if (_statoCorrente == FasePartita.RicercaNavi)
			{
				Traccia("PRIMO colpo a SEGNO!", shot);

				_statoCorrente = FasePartita.NaveColpita;
				_colpiASegnoSuccessivi = 1;
				_orientamentoCambiato = false;

				GeneraColpiPossibili(shot);

				ImpostaProssimoColpo();
			}
			else // _statoCorrente = FasePartita.NaveColpita
			{
				//Se sono andato a segno ed ancora non ho affondato la nave
				// memorizzo l'intorno dei colpi possibili nel caso di navi affiancate.
				AccodaAltriColpiPossibili(shot);

				Traccia("Colpo a segno", shot);

				//Continuo per la direzione corrente                
				Point posizionePossibile = PosizioneAdiacente(shot, _direzioneCorrente);
				if (PosizioneValida(posizionePossibile))
				{
					_prossimoColpoDaSparare = posizionePossibile;
				}
				else
				{
					CambiaDirezione();
					ImpostaProssimoColpo();
				}
			}
		}

		//05 c - Notifica che il colpo sparato all'avversario ha colpito e affondato la nave passata come parametro.
		public void ShotHitAndSink(Point shot, Ship sunkShip)
		{
			ShotHit(shot, true);

			Traccia("Nave Affondata - (" + sunkShip.Length.ToString() +")", shot);

			// Segna la posizione della nave affondata
			// per aggiornare la statistica delle Posizioni Navi 
			foreach (Point cella in sunkShip.GetAllLocations())
				_naviAvversario[Utility.DaPosizioneAIndice(cella, _gameSize)] ++;

			// Aggiorna Statistica orientamento Nave
			foreach (KeyValuePair<ProprietaNave, Double> kvp in _statisticaOrientamentoNaviAvversario)
			{
				if (!kvp.Key.Usata && kvp.Key.Lunghezza == sunkShip.Length )
				{
					Double percentuale = 0.0;
					if (sunkShip.Orientation == ShipOrientation.Horizontal)
						percentuale = 1.0;

					_statisticaOrientamentoNaviAvversario[kvp.Key] = (kvp.Value + percentuale) / 2.0;                    
					kvp.Key.Usata = true;
					break;
				}
			}
           
			// Verifica il numero dei colpi messi a segno con la lunghezza della nave affondata.
			// Se coincidono, allora la nave è affondata e si ritorna in modalità di ricerca
			if (sunkShip.Length == _colpiASegnoSuccessivi)
			{
				// altrimenti tornare in stato di ricerca
				_statoCorrente = FasePartita.RicercaNavi;

				_direzioniPossibili.Clear();
				_direzioniPossibili = new List<Dictionary<Point, Direzione>>();
				_prossimoColpoDaSparare = new Point(-1, -1);
			}
				//Altrimenti, se Lunghezza nave è < dei colpi a segno successivi, 
				// allora vuol dire che si è colpito anche un'altra nave adiacente e quindi bisonga
				// continuare con la strategia di affondamento e cercare in tutti i punti adiacenti a quelli andati a segno.
			else
			{
				Traccia("Navi Affiancate.", shot);

				_colpiASegnoSuccessivi = _colpiASegnoSuccessivi - sunkShip.Length;

				//TODO: Caso per ottimizzazione affondamento Navi Affiancate.
				// Se durante la fase di affondamento il verso non cambia(O/V) e ci sono navi affiancate,
				// Allora cercare all'estremità opposta dell'ultimo punto colpito.

				//if (!_orientamentoCambiato)
				//{

				//}


				//Deve prendere gli altri colpi possibili !!!
				Direzione precDirezione = _direzioneCorrente;
				_direzioneCorrente = _direzioniPossibili.First().First().Value;
				VerificaOrientamentoCambiato(precDirezione);

				ImpostaProssimoColpo();
			}

		}
		private void ShotHit(Point shot, bool sunk) 
		{
			_colpiASegnoSuccessivi++;

			_colpiSparati[shot] = true;
		}

		//06 - L'avversario ha sparato un colpo
		public void OpponentShot(Point shot)
		{
			_numColpiAvversario++;

			//Se l'avversario sta seguendo una ricerca della Nave dopo averla colpita,
			// il _PesoColpoAvversario non andrebbe diminuito e non andrebbero memorizzati i colpi 
			// per non alterare la strategia di ricerca utilizzata dall'avversario (da sfruttare nel posizionamento delle proprie navi).
			// Tale accorgimento però, avrebbe senso solo con avversari "intelligenti", nel caso di colpi sparati casualmente, 
			// la sospensione del contatore si rileverebbe errata.
			// Per potersi accorgere delle due strategie (Intelligente o casuale) andrebbe calcolata la distanza in punti tra il primo colpo
			// a segno dell'avversario ed il successivo. Se la distanza è 1, allora l'avversario sta seguendo una ricerca intelligente, 
			// altrimenti sono "colpi a caso" e non va interrotto il decremento del contatore.
			//
			//Per il momento decremento sempre.

			_colpiAvversario[Utility.DaPosizioneAIndice(shot, _gameSize)] = _PesoColpoAvversario;

			_PesoColpoAvversario--;

			//Verifica se l'avversario ha già colpito una nostra nave o se è ancora in ricerca
			if (_statoAvversario == FasePartita.RicercaNavi)
			{

				//Verifica se l'avversario ha appena colpito una nostra nave.
				var nave = (from s in _navi where s.Posizioni.Contains(shot)
				            select s).SingleOrDefault();

				//Verifica se sono stato colpito
				if (nave != null)
				{
					// Se è il primo colpo a segno andrebbe analizzata la strategia preferita per la ricerca della nave colpita 
					// (se predilige cercare in orizzontale o verticale, le proprie navi andrebbero messe nel verso opposto).        

					_statoAvversario = FasePartita.NaveColpita;
                    
					_primoColpoAvversario = shot;
					_numPrimoColpoAvversario = _numColpiAvversario;

					nave.CellaColpita(shot);
				}

			}
			else // _statoAvversario = FasePartita.NaveColpita
			{
				//Verifica se è il colpo immediatamente successivo al primo a segno
				if (_numColpiAvversario == _numPrimoColpoAvversario + 1)
				{
					//Verfica la strategia adottata dall'avversario (Intelligente o casuale)
					// Se la distanza è 1, allora l'avversario sta seguendo una ricerca intelligente
					if ((Math.Abs(_primoColpoAvversario.X - shot.X) <= 1) &&
					    (Math.Abs(_primoColpoAvversario.Y - shot.Y) <= 1))
					{
						if (_primoColpoAvversario.X != shot.X)
							//Preferenza per l'orizzontale
							_statisticaRicercaAvversarioNaveColpita = (_statisticaRicercaAvversarioNaveColpita + 1.0) / 2.0;
						else
							//Preferenza per il verticale
							_statisticaRicercaAvversarioNaveColpita = _statisticaRicercaAvversarioNaveColpita / 2.0;
					}
					else
					{
						//I colpi sono sparati senza seguire una strategia di affondamento diretta
						_statoAvversario = FasePartita.RicercaNavi;
					}
				}

				//Verifica se l'avversario ha affondato la nave.
				var nave = (from s in _navi
				            where s.Posizioni.Contains(shot)
				            select s).SingleOrDefault();

				//Verifica se sono stato colpito e la nave è affondata
				if (nave != null)
				{
					nave.CellaColpita(shot);

					//Se la nave è affondata, ripristina la modalità Ricerca
					if (nave.Affondata)
					{
						_statoAvversario = FasePartita.RicercaNavi;
					}           
				}
			}



		}

		//07 a - Abbiamo vinto la partita
		public void GameWon() 
		{
			_statoCorrente = FasePartita.RicercaNavi;

			AggiornaStatistiche();
		}
		//07 b - Abbiamo perso la partita
		public void GameLost()
		{
			_statoCorrente = FasePartita.RicercaNavi;

			AggiornaStatistiche();
		}

		//08 - La sfida è finita.
		public void MatchOver() 
		{
			//Debug.WriteLine(" ******************* ");
			//Debug.WriteLine(Name);

			//_statisticaColpiAvversario.Stampa("Statistica Colpi Avversario");

			//_statisticaPosizioneNaviAvversario.Stampa("Statistica Posizione Navi Avversario");

			////Stampa Statistica Orientamenti
			//Debug.WriteLine("Percentuale Orientamento Navi Avversario");
			//foreach (KeyValuePair<ProprietaNave, Double> kvp in _statisticaOrientamentoNaviAvversario)
			//    Debug.WriteLine("Nave: Lun. " + kvp.Key.Lunghezza.ToString() + ", Percentuale Orientamento = " + kvp.Value.ToString());

			//Debug.WriteLine(" ******************* ");
		}

		#endregion Metodi Pubblici

		#region Metodi Privati
		private void InizializzaSfida()
		{
			_statisticaColpiAvversario = new Griglia(_gameSize, 50.0);
			_statisticaPosizioneNaviAvversario = new Griglia(_gameSize);

			_statisticaRicercaAvversarioNaveColpita = 0.50;

			_listaNavi = new List<ProprietaNave>();
			_listaNavi.Add(new ProprietaNave(5));
			_listaNavi.Add(new ProprietaNave(4));
			_listaNavi.Add(new ProprietaNave(3));
			_listaNavi.Add(new ProprietaNave(3));
			_listaNavi.Add(new ProprietaNave(2));

			_statisticaOrientamentoNaviAvversario = new Dictionary<ProprietaNave, Double>();
			foreach (ProprietaNave pn in _listaNavi)
				_statisticaOrientamentoNaviAvversario.Add(pn, 0.50);

			ImpostaAreeNavi();
		}
		private void InizializzaPartita()
		{
			_colpiAvversario = new int[(_gameSize.Width * _gameSize.Height)];
			_naviAvversario = new int[(_gameSize.Width * _gameSize.Height)];
			_PesoColpoAvversario = (_gameSize.Width * _gameSize.Height);
			_colpiSparati = new Dictionary<Point, bool>();
			_colpiRimasti = new List<Point>();
			_navi = new List<Nave>();
			_numColpiAvversario = 0;

			//Sceglie casualmente (prediligendo il primo) tra due pattern di scacchiera da seguire
			int sceltaPattern = _rand.Next(10);
			if (sceltaPattern < 7)
				InizializzaPattern1();
			else
				InizializzaPattern2();

			////Se la fase di studio è finita, allora imposta la sequenza di colpi da sparare
			//// in base alla tendenza dell'avversario di disporre le navi.
			//if (_numPartite >= numPartiteAllenamento)
			//{
			//    //Unisce i due pattern
			//    _patternPrimario.AddRange(_patternSecondario);

			//    //Oridna i colpi da sparere
			//    _patternPrimario = _statisticaPosizioneNaviAvversario.OrdinaPosizioni(_patternPrimario, false);
			//}

			foreach(ProprietaNave pn in _statisticaOrientamentoNaviAvversario.Keys)
				pn.Usata=false;

			_direzioniPossibili = new List<Dictionary<Point, Direzione>>();
		}
        
		#region *** Genera Pattern di ricerca diagonali a scacchiera ***
		private void InizializzaPattern1()
		{
			//Primario
			_patternPrimario = new List<Point>();
			AggiungiPuntiInDiagonale(new Point(0, 0), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(4, 0), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(8, 0), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(0, 4), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(0, 8), ref _patternPrimario);

			//Secondario
			_patternSecondario = new List<Point>();
			AggiungiPuntiInDiagonale(new Point(2, 0), ref _patternSecondario);
			AggiungiPuntiInDiagonale(new Point(6, 0), ref _patternSecondario);
			AggiungiPuntiInDiagonale(new Point(0, 2), ref _patternSecondario);
			AggiungiPuntiInDiagonale(new Point(0, 6), ref _patternSecondario);
		}
		private void InizializzaPattern2()
		{
			//Primario
			_patternPrimario = new List<Point>();
			AggiungiPuntiInDiagonale(new Point(2, 0), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(6, 0), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(0, 2), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(0, 6), ref _patternPrimario);
			_patternPrimario.Add(new Point(0, 0));
			_patternPrimario.Add(new Point(9, 9));
			AggiungiPuntiInDiagonale(new Point(8, 0), ref _patternPrimario);
			AggiungiPuntiInDiagonale(new Point(0, 8), ref _patternPrimario);

			//Secondario
			_patternSecondario = new List<Point>();
			AggiungiPuntiInDiagonale(new Point(4, 0), ref _patternSecondario);
			AggiungiPuntiInDiagonale(new Point(0, 4), ref _patternSecondario);
			AggiungiPuntiInDiagonale(new Point(1, 1), ref _patternSecondario);
			_patternSecondario.RemoveAt(_patternSecondario.Count - 1);
		}
		private void AggiungiPuntiInDiagonale(Point inizio, ref List<Point> pattern)
		{
			int x = inizio.X;
			int y = inizio.Y;
			do
			{
				pattern.Add(new Point(x, y));
				x++;
				y++;
			} while (x < _gameSize.Width && y < _gameSize.Height);
		}
		#endregion *** Genera Pattern di ricerca diagonali a scacchiera ***

		private void ImpostaColpiRimasti(Point colpoCorrente)
		{
			for (int r = 0; r < _gameSize.Height; r++)
				for (int c = 0; c < _gameSize.Width; c++)
				{
					Point colpo = new Point(c, r);
					if (!_colpiSparati.Keys.Contains(colpo))
						_colpiRimasti.Add(colpo);
				}

			_colpiRimasti.Remove(colpoCorrente);

			//Oridna Colpi Rimasti
			_colpiRimasti = _statisticaPosizioneNaviAvversario.OrdinaPosizioni(_colpiRimasti, false);
		}

		private void AggiornaStatistiche()
		{
			MostraPartita();
			Traccia(" ************* Fine Partita.");

			_statisticaColpiAvversario.AggiornaStatistica(_colpiAvversario);
			_statisticaPosizioneNaviAvversario.Incrementa(_naviAvversario);

			if (_statistichePartita)
			{
				bool tmpTraccia = _tracciaPartita;
				_tracciaPartita = true;

				_mediaColpiSparati += _colpiSparati.Count;

				Traccia("Num. Colpi sparati : " + _colpiSparati.Count.ToString() + ", Media = " + (_mediaColpiSparati / _numPartite).ToString());

				_tracciaPartita = tmpTraccia;
			}

			ImpostaAreeNavi();
		}
		private void ImpostaAreeNavi()
		{
			_areeNavi = new List<AreeNavi>();

			foreach(ProprietaNave pn in _listaNavi)
			{
				_areeNavi.Add(new AreeNavi(pn.Lunghezza, ShipOrientation.Horizontal, _statisticaColpiAvversario));
				_areeNavi.Add(new AreeNavi(pn.Lunghezza, ShipOrientation.Vertical, _statisticaColpiAvversario));
			}
		}
		private void Traccia(string messaggio)
		{
			if (!_tracciaPartita)
				return;

			Debug.WriteLine(messaggio);
		}
		private void Traccia(string messaggio, Point colpo)
		{
			if (!_tracciaPartita)
				return;

			string testoColpo = "Colpo: X = " + colpo.X + " - Y = " + colpo.Y;
			Debug.WriteLine(messaggio + " - " + testoColpo);
		}
		private void MostraPartita(string messaggio)
		{
			if (!_mostraPartita)
				return;

			Debug.WriteLine(messaggio);
			MostraPartita();
		}
		private void MostraPartita()
		{
			if (!_mostraPartita)
				return;

			string[,] griglia = new string[_gameSize.Width, _gameSize.Height];
			Point[] colpi = _colpiSparati.Keys.ToArray();
			string testoColpo = string.Empty;

			for (int i = 0; i < colpi.Length; i++)
			{
				Point colpo = new Point();
				for (int j = 0; j <= i; j++)
				{
					colpo = colpi[j];
					if (_colpiSparati[colpo])
						griglia[colpo.X, colpo.Y] = "X";
					else
						griglia[colpo.X, colpo.Y] = "O";
				}

				testoColpo = "Colpo: " + i.ToString() + ", X = " + colpo.X + " - Y = " + colpo.Y;

				StampaGriglia(griglia, testoColpo);
			}
            
		}
		private void StampaGriglia(string[,] griglia, string testo)
		{
			Debug.WriteLine(testo);
			StringBuilder colonne = new StringBuilder(" ");

			for (int i = 0; i < _gameSize.Width; i++)
				colonne.Append(i.ToString());

			Debug.WriteLine(colonne.ToString());

			for(int r=0;r<_gameSize.Height;r++)
			{
				StringBuilder riga = new StringBuilder();
				riga.Append(r.ToString());
				for (int c = 0; c < _gameSize.Width; c++)
				{
					if (string.IsNullOrEmpty(griglia[c, r]))
						riga.Append(" ");
					else
						riga.Append(griglia[c, r]);
				}

				Debug.WriteLine(riga.ToString());
			}
            
		}

		//Popola la lista dei prossimi colpi possibili
		private void GeneraColpiPossibili(Point shot)
		{
			//ShipOrientation dispozioneNaveSupposta = ShipOrientation.Horizontal;

			//Genera colpi di affondamento in base alla tendenza delle navi avversario

			Double peso = 0.0;
			int numValidi = 0;

			//Verifica la tendenza dell'avversario nel disporre le navi (O/V)
			// eventualmente, in base alle navi rimaste, 
			// calcolare la possibilità dell'orientamento specifico per ogni nave rimasta
			foreach (KeyValuePair<ProprietaNave, Double> kvp in _statisticaOrientamentoNaviAvversario)
			{
				if (!kvp.Key.Usata)
				{
					peso += kvp.Value;                    
					numValidi++;
				}
			}

			peso = (peso / numValidi);

			//if (_numPartite > numPartiteAllenamento)
			//    Debug.WriteLine("peso = " + peso.ToString());

			ShipOrientation dispozioneNaveSupposta = ShipOrientation.Vertical;
			int val = _rand.Next(100);
			if (val < Convert.ToInt32(100.0 *peso))
				dispozioneNaveSupposta = ShipOrientation.Horizontal;

			//Svuota lista
			_direzioniPossibili.Clear();
			_direzioniPossibili = new List<Dictionary<Point, Direzione>>();

			_direzioniPossibili.Add(new Dictionary<Point, Direzione>());

			CreaColpiPossibili(shot, dispozioneNaveSupposta);
		}

		//Accoda altri colpi possibili nel caso di navi affiancate
		private void AccodaAltriColpiPossibili(Point shot)
		{
			_direzioniPossibili.Add(new Dictionary<Point, Direzione>());

			//Se attualmente sto colpendo in orizzontale, 
			// do la precedenza ai colpi in verticale, e viceversa.
			if (_direzioneCorrente == Direzione.Sinistra ||
			    _direzioneCorrente == Direzione.Destra)
			{
				AggiungiPosizionePossibile(Direzione.Giu, shot);
				AggiungiPosizionePossibile(Direzione.Su, shot);
			}
			else
			{
				AggiungiPosizionePossibile(Direzione.Sinistra, shot);
				AggiungiPosizionePossibile(Direzione.Destra, shot);
			}
		}
		//Popola la lista dei prossimi colpi possibili
		private void CreaColpiPossibili(Point shot, ShipOrientation verso)
		{
			if (verso == ShipOrientation.Horizontal)
			{
				bool valido = false;
				_direzioneCorrente = Direzione.Sinistra;

				if (!AggiungiPosizionePossibile(Direzione.Sinistra, shot))
					_direzioneCorrente = Direzione.Destra;
				else
					valido = true;

				if (!AggiungiPosizionePossibile(Direzione.Destra, shot) && !valido)
					_direzioneCorrente = Direzione.Su;
				else
					valido = true;

				if (!AggiungiPosizionePossibile(Direzione.Su, shot) && !valido)
					_direzioneCorrente = Direzione.Giu;

				AggiungiPosizionePossibile(Direzione.Giu, shot);
			}
			else
			{
				bool valido = false;
				_direzioneCorrente = Direzione.Su;

				if (!AggiungiPosizionePossibile(Direzione.Su, shot))
					_direzioneCorrente = Direzione.Giu;
				else
					valido = true;

				if (!AggiungiPosizionePossibile(Direzione.Giu, shot) && !valido)
					_direzioneCorrente = Direzione.Sinistra;
				else
					valido = true;

				if (!AggiungiPosizionePossibile(Direzione.Sinistra, shot) && !valido)
					_direzioneCorrente = Direzione.Destra;
				else
					valido = true;

				AggiungiPosizionePossibile(Direzione.Destra, shot);
			}
		}
		private bool AggiungiPosizionePossibile(Direzione direzione, Point shot)
		{
			bool valido = true;

			Dictionary<Point, Direzione> ultimaDirezionePossibile = _direzioniPossibili.Last();

			Point posizionePossibile = PosizioneAdiacente(shot, direzione);
			if (PosizioneValida(posizionePossibile))
			{
				if (!ultimaDirezionePossibile.Keys.Contains(posizionePossibile)) 
					ultimaDirezionePossibile.Add(posizionePossibile, direzione);
			}
			else
				valido = false;
            
			return valido;
		}
		private bool PosizioneValida(Point posizione)
		{
			return !((posizione.X < 0 || posizione.X > _gameSize.Width-1) ||
			         (posizione.Y < 0 || posizione.Y > _gameSize.Height-1) ||
			         _colpiSparati.Keys.Contains(posizione));
		}
		private Point PosizioneAdiacente(Point posizione, Direzione direzione)
		{
			Point posAdiacente = posizione;
			switch (direzione)
			{
				case Direzione.Su:
					posAdiacente.Y--;
					break;
				case Direzione.Giu:
					posAdiacente.Y++;
					break;
				case Direzione.Destra:
					posAdiacente.X++;
					break;
				case Direzione.Sinistra:
					posAdiacente.X--;
					break;
			}
			return posAdiacente;
		}
		private bool VerificaColpiAdiacentiLiberi(Point colpo)
		{
			return  PosizioneValida(PosizioneAdiacente(colpo, Direzione.Sinistra)) ||
			        PosizioneValida(PosizioneAdiacente(colpo, Direzione.Destra)) ||
			        PosizioneValida(PosizioneAdiacente(colpo, Direzione.Su)) ||
			        PosizioneValida(PosizioneAdiacente(colpo, Direzione.Giu));
		}

		private void ImpostaProssimoColpo()
		{
			_prossimoColpoDaSparare = new Point(-1, -1);

			Dictionary<Point, Direzione> prossimaDirezionePossibile = _direzioniPossibili.First();

			if (prossimaDirezionePossibile != null && prossimaDirezionePossibile.Count > 0)
			{
				foreach (KeyValuePair<Point, Direzione> kvp in prossimaDirezionePossibile)
				{
					if (kvp.Value == _direzioneCorrente)
					{
						_prossimoColpoDaSparare = kvp.Key;
						break;
					}
				}
			}
			else
			{
				_statoCorrente = FasePartita.RicercaNavi;

				//ERORRE!!!
				_mostraPartita = true;
				MostraPartita("_direzioniPossibili vuoto !!!");
				_mostraPartita = false;
			}

			//Se non ci sono più punti possibili per la direzione corrente,
			// allora cerco i punti nelle altre direzioni prendendo il primo punto nella lista.
			if (_prossimoColpoDaSparare.X < 0 && _direzioniPossibili.Count > 0)
			{
				Direzione precDirezione = _direzioneCorrente;
				_direzioneCorrente = _direzioniPossibili.First().First().Value;
				VerificaOrientamentoCambiato(precDirezione);

				_prossimoColpoDaSparare = _direzioniPossibili.First().First().Key;
			}
			if (_prossimoColpoDaSparare.X < 0)
			{
				//ERORRE!!!
				_mostraPartita = true;
				MostraPartita("Nessun Colpo assegnato !!!");
				_mostraPartita = false;
			}
		}
		private void EliminaDirezionePossibile(Point colpo)
		{
			for (int i = _direzioniPossibili.Count - 1; i >= 0; i--)
			{
				Dictionary<Point, Direzione> prossimaDirezionePossibile = _direzioniPossibili[i];

				prossimaDirezionePossibile.Remove(_prossimoColpoDaSparare);

				if (prossimaDirezionePossibile.Count == 0)
					_direzioniPossibili.Remove(prossimaDirezionePossibile);
			}
		}
		private void CambiaDirezione()
		{
			Direzione precDirezione = _direzioneCorrente;

			Dictionary<Point, Direzione> prossimaDirezionePossibile = _direzioniPossibili.First();

			if (_direzioneCorrente == Direzione.Sinistra)
			{
				if (prossimaDirezionePossibile.ContainsValue(Direzione.Destra))
					_direzioneCorrente = Direzione.Destra;
				else if (prossimaDirezionePossibile.ContainsValue(Direzione.Su))
					_direzioneCorrente = Direzione.Su;
				else
					_direzioneCorrente = Direzione.Giu;
			}
			else if (_direzioneCorrente == Direzione.Destra)
			{
				if (prossimaDirezionePossibile.ContainsValue(Direzione.Sinistra))
					_direzioneCorrente = Direzione.Sinistra;
				else if (prossimaDirezionePossibile.ContainsValue(Direzione.Su))
					_direzioneCorrente = Direzione.Su;
				else
					_direzioneCorrente = Direzione.Giu;
			}
			else if (_direzioneCorrente == Direzione.Su)
			{
				if (prossimaDirezionePossibile.ContainsValue(Direzione.Giu))
					_direzioneCorrente = Direzione.Giu;
				else if (prossimaDirezionePossibile.ContainsValue(Direzione.Sinistra))
					_direzioneCorrente = Direzione.Sinistra;
				else
					_direzioneCorrente = Direzione.Destra;
			}
			else if (_direzioneCorrente == Direzione.Giu)
			{
				if (prossimaDirezionePossibile.ContainsValue(Direzione.Su))
					_direzioneCorrente = Direzione.Su;
				else if (prossimaDirezionePossibile.ContainsValue(Direzione.Sinistra))
					_direzioneCorrente = Direzione.Sinistra;
				else
					_direzioneCorrente = Direzione.Destra;
			}

			VerificaOrientamentoCambiato(precDirezione);

			Traccia("Cambia direzione - Da " + precDirezione.ToString() + " a " + _direzioneCorrente.ToString() );

		}
		private Direzione CambiaDirezione(Direzione direzioneCorrente, Point posizione)
		{
			Direzione altraDirezione = Direzione.Destra;

			switch (direzioneCorrente)
			{
				case Direzione.Su:
					if (posizione.Y < _gameSize.Height-1)
						altraDirezione = Direzione.Giu;
					else
					{
						if (posizione.X > 0)
							altraDirezione = Direzione.Sinistra;
						else
							altraDirezione = Direzione.Destra;
					}
					break;
				case Direzione.Giu:
					if (posizione.Y > 0)
						altraDirezione = Direzione.Su;
					else
					{
						if (posizione.X > 0)
							altraDirezione = Direzione.Sinistra;
						else
							altraDirezione = Direzione.Destra;
					}
					break;
				case Direzione.Destra:
					if (posizione.X > 0)
						altraDirezione = Direzione.Sinistra;
					else
					{
						if (posizione.Y > 0)
							altraDirezione = Direzione.Su;
						else
							altraDirezione = Direzione.Giu;
					}
					break;
				case Direzione.Sinistra:
					if (posizione.X < _gameSize.Width-1)
						altraDirezione = Direzione.Destra;
					else
					{
						if (posizione.Y > 0)
							altraDirezione = Direzione.Su;
						else
							altraDirezione = Direzione.Giu;
					}
					break;
			}

			return altraDirezione;
		}

		private void VerificaOrientamentoCambiato(Direzione precDirezione)
		{
			if (((precDirezione == Direzione.Sinistra ||
			      precDirezione == Direzione.Destra) &&
			     (_direzioneCorrente == Direzione.Su ||
			      _direzioneCorrente == Direzione.Giu))
			    ||
			    ((precDirezione == Direzione.Su ||
			      precDirezione == Direzione.Giu) &&
			     (_direzioneCorrente == Direzione.Sinistra ||
			      _direzioneCorrente == Direzione.Destra)))
				_orientamentoCambiato = true;
		}

		//DEBUG 
		private void StampaNavi()
		{
			string[,] griglia = new string[_gameSize.Width, _gameSize.Height];
			foreach (Nave n in _navi)
			{
				Debug.WriteLine("Nave di :" + n.Lunghezza.ToString() + ", " + n.Orientamento.ToString());
				foreach (Point cella in n.Posizioni)
				{
					griglia[cella.X, cella.Y] = n.Lunghezza.ToString();
					Debug.WriteLine("\t Posizioni X=" + cella.X.ToString() + ", Y=" + cella.Y.ToString());
				}
			}
            
			StampaGriglia(griglia, "Posizione Navi");
		}
		#endregion Metodi Privati
	}

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

	public class ProprietaNave
	{
		#region Dichiarazioni
		private int _lunghezza = 0;
		private bool _usata = false;
		#endregion Dichiarazioni

		#region Costruttori
		public ProprietaNave(int lunghezza)
		{
			_lunghezza = lunghezza;
		}
		#endregion Costruttori

		#region Proprietà
		public int Lunghezza { get { return _lunghezza; } }
		public bool Usata { get { return _usata; } set { _usata = value; } }
		#endregion Proprietà

		#region Metodi Pubblici
		#endregion Metodi Pubblici

		#region Metodi Privati
		#endregion Metodi Privati
	}

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

	public class AreeNavi : ProprietaNave
	{
		#region Dichiarazioni
		Dictionary<Point, Double> _aree = new Dictionary<Point, Double>();
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

		public void RimuoviPosizioni(Point inizio, int lunghezza, ShipOrientation orientamento)
		{
			for (int i = 0; i < lunghezza; i++)
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
						if ((pos.X == inizio.X) && (pos.Y <= inizio.Y) && (pos.Y + Lunghezza > inizio.Y))
							_posizioni.Remove(pos);
					}
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
		#endregion Metodi Privati

	}

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

			if (crescente)
				ordinato = (from entry in griglia.Celle orderby entry.Valore ascending select entry);
			else
				ordinato = (from entry in griglia.Celle orderby entry.Valore descending select entry);

			List<Point> retPosizioni = new List<Point>();
			foreach (Cella cella in ordinato)
				if (posizioni.Contains(cella.Posizione))
					retPosizioni.Add(cella.Posizione);

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
			StringBuilder colonne = new StringBuilder(" ");            

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