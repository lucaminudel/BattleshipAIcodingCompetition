using System.Collections.Generic;

namespace Battleship.Opponents.FromUGIdotNETCompetition.Deathflame
{
	public interface IShotProvider {
		IEnumerable<Shot> Shots();
	}
}