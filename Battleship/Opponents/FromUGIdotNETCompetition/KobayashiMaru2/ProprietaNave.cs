namespace Battleship.Opponents.FromUGIdotNETCompetition.KobayashiMaru2
{
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
}