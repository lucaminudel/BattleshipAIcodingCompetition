using System.Drawing;
using NUnit.Framework;

namespace Battleship.Opponents.Nebuchadnezzar.Defense.Tests
{
    [TestFixture]
    public class BattlefieldDNA_DecodeTest
    {
        Point _shipPosition;
        ShipOrientation _shipOrientation;

        [SetUp]
        public void SetUp()
        {
            _shipPosition = Point.Empty;
            _shipOrientation = (ShipOrientation)(-1);
        }

        [Test]
        public void Encoded_position_1_should_be_decoded_into_Point_0_0_Horizontal()
        {
            BattlefieldDNA.DecodeShipPlace(1, out _shipPosition, out _shipOrientation);

            Assert.AreEqual(new Point(0, 0), _shipPosition, "decoded shipPosition");
            Assert.AreEqual(ShipOrientation.Horizontal, _shipOrientation, "decoded shipOrientation");
        }

        [Test]
        public void Encoded_position_100_should_be_decoded_into_Point_9_9_Horizontal()
        {
            BattlefieldDNA.DecodeShipPlace(100, out _shipPosition, out _shipOrientation);

            Assert.AreEqual(new Point(9, 9), _shipPosition, "decoded shipPosition");
            Assert.AreEqual(ShipOrientation.Horizontal, _shipOrientation, "decoded shipOrientation");
        }

        [Test]
        public void Encoded_position_101_should_be_decoded_into_Point_0_0_Vertical()
        {
            BattlefieldDNA.DecodeShipPlace(101, out _shipPosition, out _shipOrientation);

            Assert.AreEqual(new Point(0, 0), _shipPosition, "decoded shipPosition");
            Assert.AreEqual(ShipOrientation.Vertical, _shipOrientation, "decoded shipOrientation");
        }

        [Test]
        public void Encoded_position_200_should_be_decoded_into_Point_9_9_Vertical()
        {
            BattlefieldDNA.DecodeShipPlace(200, out _shipPosition, out _shipOrientation);

            Assert.AreEqual(new Point(9, 9), _shipPosition, "decoded shipPosition");
            Assert.AreEqual(ShipOrientation.Vertical, _shipOrientation, "decoded shipOrientation");
        }
    }
}
