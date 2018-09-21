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
        public SpaceObject SpaceObject { get; }
        public Point MovementStart { get; }
        public List<Bitmap> OverlaySprites { get; }
        public double RotationAngle { get; }
        public Point MovementDestination { get; }
        public AnimationType AnimationType { get; }

        public AnimationEventArgs(SpaceObject spaceObject, Point movementStart, Point movementDestination)
        {
            this.AnimationType = AnimationType.Movement;
            this.SpaceObject = spaceObject;
            this.MovementStart = movementStart;
            this.MovementDestination = movementDestination;
        }

        public AnimationEventArgs(SpaceObject spaceObject, double rotationAngle)
        {
            this.AnimationType = AnimationType.Rotation;
            this.SpaceObject = spaceObject;
            this.RotationAngle = rotationAngle;
        }

        public AnimationEventArgs(SpaceObject spaceObject, List<Bitmap> overlaySprites)
        {
            this.AnimationType = AnimationType.Sprites;
            this.SpaceObject = spaceObject;
            this.OverlaySprites = overlaySprites;
        }
    }
}
