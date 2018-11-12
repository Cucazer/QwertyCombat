
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

        public GameState(int cellCount)
        {
            this.SpaceObjects = new SpaceObject[cellCount];
        }

        private GameState(SpaceObject[] spaceObjects, Ship activeShip)
        {
            this.SpaceObjects = spaceObjects;
            this.ActiveShip = activeShip;
        }

        public object Clone()
        {
            return new GameState(this.SpaceObjects.Select(x => x == null ? null : (SpaceObject)x.Clone()).ToArray(), ActiveShip == null ? null : (Ship)ActiveShip.Clone());
        }
    }
}