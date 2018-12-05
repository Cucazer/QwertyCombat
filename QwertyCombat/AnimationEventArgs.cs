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
        Sprites,
        Explosion
    }
    
    class AnimationEventArgs: EventArgs
    {
        public AnimationType AnimationType { get; }
        public GameState CurrentGameState { get; }
        public SpaceObject SpaceObject { get; }
        public Point MovementStart { get; }
        public double RotationAngle { get; }
        public Point MovementDestination { get; }
        public List<Bitmap> OverlaySprites { get; }
        public Point ExplosionCenter { get; }
        public int ExplosionRadius { get; }

        public AnimationEventArgs(GameState currentGameState, Point movementStart, Point movementDestination)
        {
            this.AnimationType = AnimationType.Movement;
            this.CurrentGameState = currentGameState;
            this.SpaceObject = currentGameState.ActiveSpaceObject;
            this.MovementStart = movementStart;
            this.MovementDestination = movementDestination;
        }

        public AnimationEventArgs(GameState currentGameState, double rotationAngle)
        {
            this.AnimationType = AnimationType.Rotation;
            this.CurrentGameState = currentGameState;
            this.SpaceObject = currentGameState.ActiveSpaceObject;
            this.RotationAngle = rotationAngle;
        }

        public AnimationEventArgs(GameState currentGameState, List<Bitmap> overlaySprites)
        {
            this.AnimationType = AnimationType.Sprites;
            this.CurrentGameState = currentGameState;
            this.SpaceObject = currentGameState.ActiveSpaceObject;
            this.OverlaySprites = overlaySprites;
        }

        public AnimationEventArgs(GameState currentGameState, Point explosionCenter, int explosionRadius)
        {
            this.AnimationType = AnimationType.Explosion;
            this.CurrentGameState = currentGameState;
            this.ExplosionCenter = explosionCenter;
            this.ExplosionRadius = explosionRadius;
        }
    }
}
