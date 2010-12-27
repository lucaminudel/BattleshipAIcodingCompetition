using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Defense
{
    public class CalculateFitnessWithMinimumWeightStrategy : ICalculateFitnessStrartegy
    {
        private readonly int[,] _weights;

        public CalculateFitnessWithMinimumWeightStrategy(int[,] weights)
        {
            _weights = weights;
            if (weights.Length != Battlefield.Size * Battlefield.Size)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public float CalculateFitness(BattlefieldDNA chromosome)
        {
            float totalWeight = 0;
            chromosome.VisitDNA(delegate(int[] dna)
                                    {

                                        List<Ship> battlefield = BattlefieldDNA.GetNormalizedBattlefieldFromDNA(dna);

                                        totalWeight = CalculatedFitness(battlefield);
                                    });

            return totalWeight;
        }

    	private float CalculatedFitness(IList<Ship> battlefield)
    	{
			float totalWeight = 0;
    		var  evaluatedShipsLocations = new List<Point>();
    		foreach (Ship ship in battlefield)
    		{
    			if (ship.AreOverlapping(evaluatedShipsLocations))
    			{
    				return float.MaxValue;
    			}

    			var shipLocations = new List<Point>(ship.GetAllLocations());
    			foreach (Point location in shipLocations)
    			{
					totalWeight += _weights[location.X, location.Y];
    				evaluatedShipsLocations.Add(location);
    			}
    		}

    		return totalWeight;
    	}

    	public bool TargetReached(float fitnessValue)
        {
            return false;
        }

        public void SortPopulation(float[] populationFitness, BattlefieldDNA[] population)
        {
            Array.Sort(populationFitness, population);
            Array.Reverse(populationFitness);
            Array.Reverse(population);
        }
    }
}
