namespace Battleship
{
	using System;
	using System.Drawing;
	using System.Linq;

	class Program
	{
		static void Main(string[] args)
		{
			//  1) Nebuchadnezzar 0.8

			//  2) Stackoverflow - Dreadnought 1.2

			//  3) UGIdotNET - Terminator 2 2.0
			//  4) UGIdotNET - Kobayashi Maru 1.1.1
			//  5) UGIdotNET - Deathflame 0.1.0.1
			
			//  6) Stackoverflow - BSKiller4 0.4
			//  7) Stackoverflow - Professor Farnsworth 1.1
			//  8) Stackoverflow - USS Missouri 6.3
			//  9) Stackoverflow - BP7 0.7
			// 10) Stackoverflow - Agent Smith 2.1
			// 11) Stackoverflow - Random 1.1


			var op1 = new Opponents.FromStackoverflowCompetition.USSMissouri.USSMissouri();
			var op2 = new Opponents.FromUGIdotNETCompetition.Deathflame.DFBattleshipOpponent();

			BattleshipCompetition bc = new BattleshipCompetition(
				op1,
				op2,
				new TimeSpan(0, 0, 4),  // Time per game
				501,                    // Wins per match
				true,                   // Play out?
				new Size(10, 10),       // Board Size
				2, 3, 3, 4, 5           // Ship Sizes
			);

			var scores = bc.RunCompetition();

			foreach (var key in scores.Keys.OrderByDescending(k => scores[k]))
			{
				Console.WriteLine("{0} {1}:\t{2}", key.Name, key.Version, scores[key]);
			}

			Console.ReadKey(true);
		}
	}
}
