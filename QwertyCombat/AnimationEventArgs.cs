using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QwertyCombat.Objects;

namespace QwertyCombat
{
    public enum AnimationType
    {
        Movement,
        Rotation,
        Sprites
    }
    
    class AnimationEventArgs: EventArgs
    {
        public GameState CurrentGameState { get; }
        public SpaceObject SpaceObject { get; }
        public Point MovementStart { get; }
        public List<Bitmap> OverlaySprites { get; }
        public double RotationAngle { get; }
        public Point MovementDestination { get; }
        public AnimationType AnimationType { get; }

        public AnimationEventArgs(GameState currentGameState, SpaceObject spaceObject, Point movementStart, Point movementDestination)
        {
            this.AnimationType = AnimationType.Movement;
            this.CurrentGameState = currentGameState;
            this.SpaceObject = spaceObject;
            this.MovementStart = movementStart;
            this.MovementDestination = movementDestination;
        }

        public AnimationEventArgs(GameState currentGameState, SpaceObject spaceObject, double rotationAngle)
        {
            this.AnimationType = AnimationType.Rotation;
            this.CurrentGameState = currentGameState;
            this.SpaceObject = spaceObject;
            this.RotationAngle = rotationAngle;
        }

        public AnimationEventArgs(GameState currentGameState, SpaceObject spaceObject, List<Bitmap> overlaySprites)
        {
            this.AnimationType = AnimationType.Sprites;
            this.CurrentGameState = currentGameState;
            this.SpaceObject = spaceObject;
            this.OverlaySprites = overlaySprites;
        }
    }
}
