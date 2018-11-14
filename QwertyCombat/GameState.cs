
using System;
using System.Collections.Generic;
using System.Linq;
using QwertyCombat.Objects;

namespace QwertyCombat
{
    class GameState : ICloneable
    {
        public SpaceObject[] SpaceObjects;

        public List<Meteor> Meteors => this.SpaceObjects.OfType<Meteor>().ToList();
        public List<Ship> Ships => this.SpaceObjects.OfType<Ship>().ToList();

        public Ship ActiveShip { get; set; }
        public SpaceObject ActiveSpaceObject { get; set; }

        public GameState(int cellCount)
        {
            this.SpaceObjects = new SpaceObject[cellCount];
        }

        private GameState(SpaceObject[] spaceObjects)
        {
            this.SpaceObjects = spaceObjects;
        }

        public object Clone()
        {
            var clonedGameState = new GameState(this.SpaceObjects.Select(x => x == null ? null : (SpaceObject)x.Clone()).ToArray());
            if (this.ActiveShip != null)
            {
                clonedGameState.ActiveShip = clonedGameState.Ships.First(sh => sh.ObjectCoordinates.Equals(this.ActiveShip.ObjectCoordinates));
            }
            if (this.ActiveSpaceObject != null)
            {
                clonedGameState.ActiveSpaceObject = clonedGameState.SpaceObjects.First(so => so.ObjectCoordinates.Equals(this.ActiveSpaceObject.ObjectCoordinates));
            }
            return clonedGameState;
        }
    }
}