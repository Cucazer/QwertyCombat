
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

        public bool FirstPlayerWon => this.Ships.Count(s => s.Owner == Player.FirstPlayer) > 0 &&
                                      this.Ships.Count(s => s.Owner == Player.SecondPlayer) == 0;
        public bool SecondPlayerWon => this.Ships.Count(s => s.Owner == Player.FirstPlayer) == 0 &&
                                      this.Ships.Count(s => s.Owner == Player.SecondPlayer) > 0;

        public bool GameOver => this.FirstPlayerWon || this.SecondPlayerWon;

        public Player GameWinner
        {
            get
            {
                if (this.FirstPlayerWon)
                {
                    return Player.FirstPlayer;
                }

                if (this.SecondPlayerWon)
                {
                    return Player.SecondPlayer;
                }

                return Player.None;
            }
        }

        public Player ActivePlayer { get; set; }
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
            clonedGameState.ActivePlayer = this.ActivePlayer;
            return clonedGameState;
        }
    }
}