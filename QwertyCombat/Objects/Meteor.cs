using Hex = Barbar.HexGrid;

namespace qwerty.Objects
{
    // TODO: rename to meteoroid? meteor is already in atmosphere!
    class Meteor : SpaceObject
    {
        private const string ObjectDescription = "Moving meteor";
        public readonly int CollisionDamage;
        public readonly HexagonNeighborDirection MovementDirection;

        public Meteor(Hex.OffsetCoordinates meteorCoordinates, int health, int damage, HexagonNeighborDirection movementDirection): base(Player.None, health, ObjectType.Meteor, 1)
        {
            this.MovementDirection = movementDirection;
            this.ObjectCoordinates = meteorCoordinates;
            this.CollisionDamage = damage;
        }

        public override string Description => ObjectDescription + "\nCollision damage: " + this.CollisionDamage
                                              + "\nHP - " + this.CurrentHealth
                                              + "\nMovement direction: \n" + this.MovementDirection;

        public override void Rotate(double angle)
        {
            throw new System.NotImplementedException();
        }
    }
}
