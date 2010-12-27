using System;
using System.Collections.Generic;

namespace Battleship.Opponents.Nebuchadnezzar.Defense.Tests
{
	class StubRandom : Random
	{
		private int nextIntValue;
		private double nextDoubleValue;
		private Queue<double> nextDoubleValues;

		public void SetNextValue(int value)
		{
			nextIntValue = value;
		}

		public void SetNextValue(double value)
		{
			nextDoubleValue = value;
		}

		public void SetNextValue(double[] values)
		{
			nextDoubleValues = new Queue<double>(values);
		}

		public override int Next(int maxValue)
		{
			return nextIntValue;
		}

		public override double NextDouble()
		{
			if (nextDoubleValues != null)
			{
				return nextDoubleValues.Dequeue();
			}

			return nextDoubleValue;
		}

		protected override double Sample() { throw new NotImplementedException(); }

		public override int Next() { throw new NotImplementedException(); }

		public override int Next(int minValue, int maxValue) { throw new NotImplementedException(); }

		public override void NextBytes(byte[] buffer) { throw new NotImplementedException(); }
	}

}
