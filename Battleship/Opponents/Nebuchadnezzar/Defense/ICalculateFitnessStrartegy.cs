namespace Battleship.Opponents.Nebuchadnezzar.Defense
{
    public interface  ICalculateFitnessStrartegy
    {
        float CalculateFitness(BattlefieldDNA chromosome);


        bool TargetReached(float fitnessValue);

        void SortPopulation(float[] populationFitness, BattlefieldDNA[] population);
    }
}
