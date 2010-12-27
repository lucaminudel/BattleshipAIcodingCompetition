using NUnit.Framework;

namespace Battleship.Opponents.Nebuchadnezzar.Defense.Tests
{

	[TestFixture]
	public class BattlefieldDNATest
	{
	
		[Test]
		public void Calculate_DNA_Crossover()
		{
			StubRandom stubRandom = new StubRandom();
			stubRandom.SetNextValue(3);

			BattlefieldDNA mother = new BattlefieldDNA(new int[] { 1, 1, 1, 1, 1 }, stubRandom);
			BattlefieldDNA offspring = mother.GenerateOffspring(new BattlefieldDNA(new int[] {2, 2, 2, 2, 2}, stubRandom));

			offspring.VisitDNA(delegate(int[] geneticSequence)
			                   	{
									CollectionAssert.AreEqual(new int[] { 1, 1, 1, 2, 2 }, geneticSequence, "offspring genetic sequence");
			                   	});
		}

		[Test]
		public void Zero_DNA_mutation_probabillity_does_not_cause_any_mutation()
		{
			StubRandom stubRandom = new StubRandom();
			int mutatedValue = 3;
			stubRandom.SetNextValue(mutatedValue);
			double mutationEvent = 0.5;
			stubRandom.SetNextValue(mutationEvent);

			BattlefieldDNA target = new BattlefieldDNA(new int[] { 1, 1, 1, 1, 1 }, stubRandom);
			int mutationProbability = 0;
			target.Mutate(mutationProbability);

			target.VisitDNA(delegate(int[] geneticSequence)
			{
				CollectionAssert.AreEqual(new int[] { 1, 1, 1, 1, 1 }, geneticSequence, "mutated  genetic sequence");
			});
		}

		[Test]
		public void One_DNA_mutation_probabillity_does_cause_all_mutations()
		{
			StubRandom stubRandom = new StubRandom();
			int mutatedValue = 3 -1;
			stubRandom.SetNextValue(mutatedValue);
			double mutationEvent = 0.5;
			stubRandom.SetNextValue(mutationEvent);

			BattlefieldDNA target = new BattlefieldDNA(new int[] { 1, 1, 1, 1, 1 }, stubRandom);
			int mutationProbability = 1;
			target.Mutate(mutationProbability);

			target.VisitDNA(delegate(int[] geneticSequence)
			{
				CollectionAssert.AreEqual(new int[] { 3, 3, 3, 3, 3 }, geneticSequence, "mutated genetic sequence");
			});
		}

		[Test]
		public void DNA_mutation()
		{
			StubRandom stubRandom = new StubRandom();
			int mutatedValue = 3 - 1;
			stubRandom.SetNextValue(mutatedValue);
			double[] mutationEvent = {1, 0, 1, 0, 1};
			stubRandom.SetNextValue(mutationEvent);

			BattlefieldDNA target = new BattlefieldDNA(new int[] { 1, 1, 1, 1, 1 }, stubRandom);
			double mutationProbability = 0.5;
			target.Mutate(mutationProbability);

			target.VisitDNA(delegate(int[] geneticSequence)
			{
				CollectionAssert.AreEqual(new int[] { 1, 3, 1, 3, 1 }, geneticSequence, "mutated genetic sequence");
			});
		}	

	}
}
