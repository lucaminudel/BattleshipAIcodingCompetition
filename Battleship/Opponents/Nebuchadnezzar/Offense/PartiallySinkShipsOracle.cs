using System;
using System.Collections.Generic;
using System.Drawing;

namespace Battleship.Opponents.Nebuchadnezzar.Offense
{
	public class PartiallySinkShipsOracle : IPartiallySinkShipsOracle
	{
		private readonly IOpponentBattlefield _opponentBattlefield;
		private readonly Point[] _searchDirections = new[] { new Point(1, 0), new Point(0, 1) };

		public PartiallySinkShipsOracle(IOpponentBattlefield opponentBattlefield)
		{
			_opponentBattlefield = opponentBattlefield;
		}


		public Point GuessTheBestShotOnAPartiallySinkShip(double[,] weights)
		{
			return GuessTheBestShotOnlyOnAPartiallySinkShip(weights);
		}

		private Point GuessTheBestShotOnlyOnAPartiallySinkShip(double[,] weights)
		{
			var cellsHitAndNotSinkToEvaluate = new List<Point>(_opponentBattlefield.CellsHitAndNotSink());
			var directionsToSearch = new List<Point>(_searchDirections);

			foreach (var hit in _opponentBattlefield.CellsHitAndNotSink())
			{
				foreach (var searchDirection in _searchDirections)
				{
					int count = _opponentBattlefield.CountAdjacentCellsHit(hit, searchDirection.X, searchDirection.Y)
								+ _opponentBattlefield.CountAdjacentCellsHit(hit, -searchDirection.X, -searchDirection.Y);

					bool firstHitOnTheUnsinkShip = (count == 0);
					if (firstHitOnTheUnsinkShip == false)
					{
						cellsHitAndNotSinkToEvaluate.Clear();
						cellsHitAndNotSinkToEvaluate.Add(hit);

						directionsToSearch.Clear();
						directionsToSearch.Add(searchDirection);

						Point shot;
						try
						{
							shot = FindTheShotWithHigherProbability(cellsHitAndNotSinkToEvaluate, directionsToSearch, weights);
						}
						catch (InvalidOperationException)
						{
							return GuessTheBestShotOnAPartiallySinkShipAndOnTouchingShips(weights);
						}

						return shot;
					}
				}
			}

			return GuessTheBestShotOnAPartiallySinkShipAndOnTouchingShips(weights);
		}

		private Point GuessTheBestShotOnAPartiallySinkShipAndOnTouchingShips(double[,] weights)
		{

			return FindTheShotWithHigherProbability(_opponentBattlefield.CellsHitAndNotSink(), _searchDirections, weights);

		}

		private Point FindTheShotWithHigherProbability(IEnumerable<Point> cellsHitAndNotSinkToEvaluate, IEnumerable<Point> directionsToSearch, double[,] weights)
		{
			double maxProbability = 0;
			Point maxProbabilityHit = Point.Empty;

			foreach (var startPoint in cellsHitAndNotSinkToEvaluate)
			{
				var hitProbability = new Dictionary<Point, double>();

				foreach (var searchDirection in directionsToSearch)
				{
					SearchCandidateHitTargetsAndCalculateTheirProbability(startPoint, searchDirection.X, searchDirection.Y, hitProbability, weights);
				}

				foreach (var hitProbabilityPair in hitProbability)
				{
					if (hitProbabilityPair.Value > maxProbability)
					{
						maxProbability = hitProbabilityPair.Value;
						maxProbabilityHit = hitProbabilityPair.Key;
					}
				}

			}

			if (maxProbability == 0)
			{
				throw new InvalidOperationException("Possible hit not found");
			}

			return maxProbabilityHit;
		}

		private void SearchCandidateHitTargetsAndCalculateTheirProbability(Point startPoint, int serachDirectionX, int serachDirectionY, IDictionary<Point, double> hitProbability, double[,] weights)
		{
			int forwardLength = _opponentBattlefield.CountAdjacentCellsEmptyOrHit(startPoint, serachDirectionX, serachDirectionY);
			int backwardLength = _opponentBattlefield.CountAdjacentCellsEmptyOrHit(startPoint, -serachDirectionX, -serachDirectionY);
			int totalLength = forwardLength + backwardLength + 1;
			foreach (int unsunkShipLength in _opponentBattlefield.UnsinkShipsLengthShorterThan(totalLength))
			{
				CalculateProbabilityForCandidateHitTargets(startPoint, serachDirectionX, serachDirectionY, Math.Min(forwardLength, unsunkShipLength - 1), hitProbability, weights);
				CalculateProbabilityForCandidateHitTargets(startPoint, -serachDirectionX, -serachDirectionY, Math.Min(backwardLength, unsunkShipLength - 1), hitProbability, weights);
			}
		}

		private void CalculateProbabilityForCandidateHitTargets(Point startPoint, int deltaX, int deltaY, int maxDistance, IDictionary<Point, double> hitProbability, double[,] weights)
		{
			foreach (var distanceCellPair in _opponentBattlefield.EmptyCellsAlongTheDirection(startPoint, deltaX, deltaY, maxDistance))
			{
				var candidateHitPoint = distanceCellPair.Value;
				var distance = distanceCellPair.Key;

				int probability = maxDistance - distance + 1;

				double currentValue = 0;
				hitProbability.TryGetValue(candidateHitPoint, out currentValue);
				hitProbability[candidateHitPoint] = currentValue + probability * (1 + weights[candidateHitPoint.X, candidateHitPoint.Y]);

			}
		}

	}
}
