using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Defense
{
	public class BattlefieldDNA
	{
		readonly int[] _geneticSequence;
		readonly Random _laDeaFortuna;

		public BattlefieldDNA()
			: this(NewRandomNumberGenerator())
		{
		}

		public BattlefieldDNA(int[] geneticSequence)
			: this(geneticSequence, NewRandomNumberGenerator())
		{
		}

		public BattlefieldDNA(int[] geneticSequence, Random laDeaFortuna)
		{
			if (geneticSequence == null || laDeaFortuna == null)
			{
				throw new ArgumentNullException();
			}

			if (geneticSequence.Length != 5)
			{
				throw new ArgumentOutOfRangeException("geneticSequence", "Length must be 5"); 
			}

			_laDeaFortuna = laDeaFortuna;
			_geneticSequence = geneticSequence;
		}

		private BattlefieldDNA(Random laDeaFortuna)
			: this(NewGeneticSequence(laDeaFortuna), laDeaFortuna)
		{
		}

		private static Random NewRandomNumberGenerator()
		{
			return new Random(DateTime.Now.GetHashCode());
		}

		private static int[] NewGeneticSequence(Random laDeaFortuna)
		{
			int[] geneticSequence = new int[5];

			for (int i = 0; i < 5; i++)
			{
				geneticSequence[i] = 1 + laDeaFortuna.Next(200);
			}

			return geneticSequence;
		}

		public BattlefieldDNA GenerateOffspring(BattlefieldDNA partner)
		{
			//Crossover
			//    * One point - part of the first parent is copied and the rest is taken in the same order as in the second parent
			//    * Two point - two parts of the first parent are copied and the rest between is taken in the same order as in the second parent
			//    * None - no crossover, offspring is exact copy of parents 

			// Crossover probability says how often will be crossover performed. If there is no crossover, offspring is exact copy of parents. 
			// If there is a crossover, offspring is made from parts of parents' chromosome. If crossover probability is 100%, then all offspring 
			// is made by crossover. If it is 0%, whole new generation is made from exact copies of chromosomes from old population (but this does 
			// not mean that the new generation is the same!).
			// Crossover is made in hope that new chromosomes will have good parts of old chromosomes and maybe the new chromosomes will be better. 
			// However it is good to leave some part of population survive to next generation.

			// Crossover probability = 100;
			
			int[] offspringGeneticSequence = new int[_geneticSequence.Length];
			int treshhold = _laDeaFortuna.Next(_geneticSequence.Length -2);
			
			for (int i = 0; i < treshhold; i++)
			{
				offspringGeneticSequence[i] = _geneticSequence[i];			
			}
			
			for (int i = treshhold; i < _geneticSequence.Length; i++)
			{
				offspringGeneticSequence[i] = partner._geneticSequence[i];			
			}

			return new BattlefieldDNA(offspringGeneticSequence);
		}

		public void VisitDNA(Action<int[]> visitor)
		{
			if (visitor != null)
			{
				visitor(_geneticSequence);
			}
		}

		public void Mutate(double mutationProbability)
		{
			//Mutation
			//    * Normal random - a few cities are chosen and exchanged
			//    * Random, only improving - a few cities are randomly chosen and exchanged only if they improve solution (increase fitness)
			//    * Systematic, only improving - cities are systematically chosen and exchanged only if they improve solution (increase fitness)
			//    * Random improving - the same as "random, only improving", but before this is "normal random" mutation performed
			//    * Systematic improving - the same as "systematic, only improving", but before this is "normal random" mutation performed
			//    * None - no mutation 

			// Mutation probability says how often will be parts of chromosome mutated. If there is no mutation, offspring is taken after crossover 
			// (or copy) without any change. If mutation is performed, part of chromosome is changed. If mutation probability is 100%, whole chromosome
            // is changed, if it is 0%, nothing is changed.
			// Mutation is made to prevent falling GA into local extreme, but it should not occur very often, because then GA will in fact change to 
            // random search. 



			for (int i = 0; i < _geneticSequence.Length; i++)
			{
				if(_laDeaFortuna.NextDouble() < mutationProbability)
				{
					_geneticSequence[i] = 1 + _laDeaFortuna.Next(200);
				}
			}			
		}

		public static void DecodeShipPlace(int place, out Point position, out ShipOrientation orientation)
		{
			int encodedOrientation = (place-1)/100;
			orientation = (ShipOrientation) encodedOrientation;


			int encodedPosition = (place - 1)%(Battlefield.Size * Battlefield.Size);
            int x = encodedPosition % Battlefield.Size;
            int y = encodedPosition / Battlefield.Size;

			position = new Point(x,y);
		}


        public static List<Ship> GetNormalizedBattlefieldFromDNA(int[] dna)
        {
            List<Ship> battlefield = new List<Ship>();
            battlefield.Add(Navy.NewAircraftcarrier());
			battlefield.Add(Navy.NewBattleship());
			battlefield.Add(Navy.NewDestroyer());
			battlefield.Add(Navy.NewSubmarine());
			battlefield.Add(Navy.NewPatrolBoat());

            for (int i = 0; i < dna.Length; i++)
            {
                Point position;
                ShipOrientation orientation;

                DecodeShipPlace(dna[i], out position, out orientation);
                Ships.NormalizeShipPlace(battlefield[i].Length, ref position, ref orientation);
                battlefield[i] = new Ship(battlefield[i].Length);
                battlefield[i].Place(position, orientation);
            }
            return battlefield;
        }
	}
}
