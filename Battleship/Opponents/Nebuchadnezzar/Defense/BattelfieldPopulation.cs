using System;
using System.Collections.Generic;

namespace Battleship.Opponents.Nebuchadnezzar.Defense
{
	public class BattelfieldPopulation
	{
	    public delegate void BattelfieldPopulationVisitor(int generation, float bestFitness, float avarageFitness, float totalFitness, IList<Ship> bestResult);

		private const int PopulationMaxSize = 150;

        private readonly ICalculateFitnessStrartegy _calculateFitnessStrategy;
        private readonly float _mutationProbability;
	    private readonly BattlefieldDNA[] _sortedPopulation;
		private readonly float[] _sortedPopulationFitness;              
		private int _generationsCount;
		private readonly Random _laDeaFortuna;

		public BattelfieldPopulation(float mutationProbability, ICalculateFitnessStrartegy calculateFitnessStrategy)
		{
			_sortedPopulation = new BattlefieldDNA[PopulationMaxSize];
			_sortedPopulationFitness = new float[PopulationMaxSize];
            _calculateFitnessStrategy = calculateFitnessStrategy;
            _laDeaFortuna = new Random(DateTime.Now.GetHashCode());
            _mutationProbability = mutationProbability;

		    GenerateARandomPopulation();
		    SortPopulation();
			_generationsCount = 1;
		}

        public void ProcreateTheNewGeneration(out bool targetReached)
        {
            NaturalSelection();
            GenerateOffsprings();
            Mutation();

            SortPopulation();

            _generationsCount += 1;

            targetReached = TargetReached();
        }

        public void VisitPopulation(BattelfieldPopulationVisitor visitor)
        {
            float totalFitness = 0;
            for (int i = 0; i < PopulationMaxSize; i++)
            {
                totalFitness += _sortedPopulationFitness[i];
            }

            List<Ship> battlefield = null;
            _sortedPopulation[PopulationMaxSize - 1].VisitDNA(
                delegate(int[] dna)
                {
                    battlefield = BattlefieldDNA.GetNormalizedBattlefieldFromDNA(dna);
                });

            visitor(_generationsCount, _sortedPopulationFitness[PopulationMaxSize - 1], totalFitness / PopulationMaxSize, totalFitness, battlefield);
        }


	    private void GenerateARandomPopulation()
	    {
	        for (int i = 0; i < PopulationMaxSize; i++)
	        {
	            BattlefieldDNA chromosome = new BattlefieldDNA();
	            AddToPopulation(i, chromosome);
	        }
	    }

		private void NaturalSelection()
		{
			// http://www.obitko.com/tutorials/genetic-algorithms/selection.php

            for (int i = 0; i < PopulationMaxSize / 3; i++)
            {
				// Bye bye baby, it has been fun, but it has come to an end ;-)
				RemoveFromPopulation(i);
			}
			
		}

		private void GenerateOffsprings()
		{
			for (int i = 0; i < PopulationMaxSize / 3; i++)
			{
				// Reproduction time! Recreational time! Have fun, fuck hard!
				int motherId = _laDeaFortuna.Next(PopulationMaxSize / 3, PopulationMaxSize);
				int fatherId = _laDeaFortuna.Next(PopulationMaxSize / 3, PopulationMaxSize);

				BattlefieldDNA mother = _sortedPopulation[motherId];
				BattlefieldDNA father = _sortedPopulation[fatherId];
				BattlefieldDNA offspring  = mother.GenerateOffspring(father);

				AddToPopulation(i, offspring);
			}

		}

		private void Mutation()
		{
			// Preserv champion, merge propery to maximize speed
			// http://www.obitko.com/tutorials/genetic-algorithms/crossover-mutation.php

            for (int i = 0; i < PopulationMaxSize -10; i++)
            {
				// Let deep space radiations do their job 
				MutatePopulation(i);
			}

		}

		private float CalculateFitness(BattlefieldDNA chromosome)
		{
		    float fitness = _calculateFitnessStrategy.CalculateFitness(chromosome);
		    return fitness;
		}

		private void AddToPopulation(int position, BattlefieldDNA Chromosome)
		{
			_sortedPopulation[position] = Chromosome;
			_sortedPopulationFitness[position] = CalculateFitness(Chromosome);
		}

		private void MutatePopulation(int i)
		{
			_sortedPopulation[i].Mutate(_mutationProbability);
			_sortedPopulationFitness[i] = CalculateFitness(_sortedPopulation[i]);
		}

		private void RemoveFromPopulation(int i)
		{
			_sortedPopulation[i] = null;
			_sortedPopulationFitness[0] = 0;
		}
		
		private void SortPopulation()
		{
			_calculateFitnessStrategy.SortPopulation(_sortedPopulationFitness, _sortedPopulation);
		}

        private bool TargetReached()
        {
            return  _calculateFitnessStrategy.TargetReached(_sortedPopulationFitness[PopulationMaxSize - 1]);
        }
	}
}
