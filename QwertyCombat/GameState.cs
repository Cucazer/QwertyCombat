
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
        
        public GameState(int cellCount)
        {
            this.SpaceObjects = new SpaceObject[cellCount];
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}