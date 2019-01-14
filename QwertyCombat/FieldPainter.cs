using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using System.Linq;
using Barbar.HexGrid;
using QwertyCombat.Objects;
using Point = Eto.Drawing.Point;

namespace QwertyCombat
{
    class FieldPainter
    {
        public Bitmap CurrentBitmap;

        private ObjectManager objectManager;
        private readonly GameState defaultGameState;
        private GameState gameStateToDraw;

        private CombatMap combatMap => this.objectManager.CombatMap;

        //private UITimer animationTimer = new UITimer();
        private Queue<AnimationEventArgs> AnimationQueue = new Queue<AnimationEventArgs>();

        public event EventHandler BitmapUpdated;

        private double defaultTimerInterval => Eto.Platform.Detect.IsGtk ? 0.07 : 0.01;

        private Dictionary<Player, Color> teamColors = new Dictionary<Player, Color>
        {
            { Player.FirstPlayer, Colors.Blue },
            { Player.SecondPlayer, Colors.Red },
            { Player.None, Colors.Gray }
        };

        public FieldPainter(int fieldWidth, int fieldHeight, ObjectManager objectManager)
        {
            this.objectManager = objectManager;
            this.defaultGameState = objectManager.GameState;
            this.gameStateToDraw = objectManager.GameState;
            this.CurrentBitmap = new Bitmap(fieldWidth, fieldHeight, PixelFormat.Format32bppRgba);
        }

        public void UpdateBitmap(AnimationEventArgs animationToPerform = null)  
        {
            if (this.performingAnimation)
            {
                //don't disturb animation
                return;
            }
            
            if (animationToPerform == null)
            {
                this.DrawField();
                return;
            }

            switch (animationToPerform.AnimationType)
            {
                case AnimationType.Sprites:
                    this.AnimateAttack(animationToPerform.SpaceObject, animationToPerform.OverlaySprites);
                    break;
                case AnimationType.Rotation:
                    // maybe save game state
                    this.AnimateRotation(animationToPerform.SpaceObject, animationToPerform.RotationAngle);
                    break;
                case AnimationType.Movement:
                    this.AnimateMovingObjects(animationToPerform.SpaceObject, animationToPerform.MovementStart,
                        animationToPerform.MovementDestination);
                    break;
                case AnimationType.Explosion:
                    this.AnimateExplosion(animationToPerform.ExplosionCenter, animationToPerform.ExplosionRadius);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool performingAnimation = false;
        public void HandleAnimationQueue(AnimationEventArgs animationToPerform = null)
        {
            if (animationToPerform != null)
            {
                AnimationQueue.Enqueue(animationToPerform);
            }

            if (performingAnimation)
            {
                return;
            }

            if (!AnimationQueue.Any())
            {
                this.DrawField();
                this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
                return;
            }

            var nextAnimation = AnimationQueue.Dequeue();
            this.gameStateToDraw = nextAnimation.CurrentGameState;
            switch (nextAnimation.AnimationType)
            {
                case AnimationType.Sprites:
                    this.AnimateAttack(nextAnimation.SpaceObject, nextAnimation.OverlaySprites);
                    break;
                case AnimationType.Rotation:
                    this.AnimateRotation(nextAnimation.SpaceObject, nextAnimation.RotationAngle);
                    break;
                case AnimationType.Movement:
                    this.AnimateMovingObjects(nextAnimation.SpaceObject, nextAnimation.MovementStart,
                        nextAnimation.MovementDestination);
                    break;
                case AnimationType.Explosion:
                    this.AnimateExplosion(nextAnimation.ExplosionCenter, nextAnimation.ExplosionRadius);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void DrawField()
        {
            // should always ensure .Dispose() is called when you are done with a Graphics object
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.Clear(Colors.Black);

                foreach (var hexagonCorners in this.combatMap.AllHexagonCorners)
                {
                    g.DrawPolygon(Pens.Purple, hexagonCorners);
                }

                if (this.gameStateToDraw.ActiveShip != null)
                {
                    // highlight active ship attack range
                    Pen redPen = new Pen(Colors.Red, 1);
                    foreach (var hexagonCorners in this.combatMap.GetAllHexagonCornersInRange(this.gameStateToDraw.ActiveShip.ObjectCoordinates, this.gameStateToDraw.ActiveShip.EquippedWeapon.AttackRange))
                    {
                        g.DrawPolygon(redPen, hexagonCorners);
                    }

                    // highlight active ship attack targets
                    var highlightColor = new Color(Colors.Red, 0.35F);
                    foreach (var targetCell in this.objectManager.GetAttackTargets(this.gameStateToDraw.ActiveShip))
                    {
                        g.FillPolygon(highlightColor, this.combatMap.GetHexagonCorners(targetCell));
                    }


                    // highlight active ship movement range
                    highlightColor = new Color(Colors.Lime, 0.35F);
                    foreach (var availableCell in this.objectManager.GetMovementRange(this.gameStateToDraw.ActiveShip))
                    {
                        g.FillPolygon(highlightColor, this.combatMap.GetHexagonCorners(availableCell));
                    }

                    // highlight active ship
                    Pen activeShipAriaPen = new Pen(Colors.Purple, 5);
                    g.DrawPolygon(activeShipAriaPen, this.combatMap.GetHexagonCorners(this.gameStateToDraw.ActiveShip.ObjectCoordinates.Column,
                                                              this.gameStateToDraw.ActiveShip.ObjectCoordinates.Row));
                }
            }

            foreach (var ship in this.gameStateToDraw.Ships)
            {
                if (!ship.IsMoving)
                {
                    this.DrawShip(ship);
                }
            }

            foreach (var meteor in this.gameStateToDraw.Meteors)
            {
                if (!meteor.IsMoving)
                {
                    this.DrawMeteor(meteor);
                }
            }

#if DEBUG
            using (var g = new Graphics(this.CurrentBitmap))
            {
                // draw hexagon coordinates
                for (int x = 0; x < this.combatMap.FieldWidth; x++)
                {
                    for (int y = 0; y < this.combatMap.FieldHeight; y++)
                    {
                        g.DrawText(Fonts.Monospace(7), Brushes.DeepSkyBlue, this.combatMap.GetHexagonCorners(x, y)[2], $"C{x}R{y}");

                        var cubeCoordinates = this.combatMap.HexGrid.ToCubeCoordinates(new OffsetCoordinates(x, y));
                        g.DrawText(Fonts.Monospace(7), Brushes.DeepSkyBlue, this.combatMap.GetHexagonCorners(x, y)[4] + new Size(0, -12), $"Q{cubeCoordinates.Q}R{cubeCoordinates.R}S{cubeCoordinates.S}");
                    }
                }
            }
#endif
        }

        bool tooltipShown = false;
        public void ActivateTooltip(Point location, string text)
        {
            if (this.performingAnimation)
            { 
                return;
            }
            this.tooltipShown = true;
            this.DrawField();
            // extract to DrawTooltip?
            var textBoxSize = new Size(120, 70);
            var textBoxLocation = location + new Size(10, 5);
            if (!this.CurrentBitmap.Size.Contains(location + textBoxSize))
            {
                textBoxLocation -= textBoxSize + new Size(10, 5); // fix for bottom-left and top-right corners!
            }
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillRectangle(Colors.Black, new Rectangle(textBoxLocation, textBoxSize));
                g.DrawRectangle(Colors.Red, new Rectangle(textBoxLocation, textBoxSize));
                g.DrawText(Fonts.Sans(7), Colors.Lime, textBoxLocation + new Size(5, 5), text);
            }
            this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void DeactivateTooltip()
        {
            if (this.tooltipShown)
            {
                this.DrawField();
                this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
                this.tooltipShown = false;
            }
        }

        private void DrawSpaceObject(SpaceObject spaceObject)
        {
            this.DrawSpaceObject(spaceObject, this.combatMap.HexToPixel(spaceObject.ObjectCoordinates));
        }

        private void DrawSpaceObject(SpaceObject spaceObject, Point spaceObjectCoordinates)
        {
            if (spaceObject is Meteor)
            {
                this.DrawMeteor((Meteor) spaceObject, spaceObjectCoordinates);
                return;
            }
            if (spaceObject is Ship)
            {
                this.DrawShip((Ship) spaceObject, spaceObjectCoordinates);
                return;
            }

            throw new ArgumentException($"Drawing of {spaceObject.GetType()} not supported");
        }

        private void DrawMeteor(Meteor meteor)
        {
            this.DrawMeteor(meteor, this.combatMap.HexToPixel(meteor.ObjectCoordinates));
        }

        private void DrawMeteor(Meteor meteor, Point meteorCoordinates)
        {
            var meteorRadius = 15;
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillEllipse(Colors.DarkGray,
                    new Rectangle(meteorCoordinates - meteorRadius, new Size(2 * meteorRadius, 2 * meteorRadius)));
                // TODO: add dark spots
                int spotRadius = (int)(meteorRadius / 3.0);
                g.FillEllipse(Colors.DimGray, new Rectangle(meteorCoordinates - spotRadius, new Size(spotRadius, spotRadius)));

                g.DrawText(Fonts.Sans(8), Brushes.Red, meteorCoordinates + new Size(5, -25), meteor.CurrentHealth.ToString());
                // TODO: better indicate meteor's way
                var directionAngle = 60 * (int)meteor.MovementDirection - 30;
                var directionAngleRadians = (float)directionAngle / 180 * Math.PI;
                var beamStartAngles = new List<double>
                {
                    directionAngleRadians + Math.PI / 2,
                    directionAngleRadians + 5 * Math.PI / 6,
                    //directionAngleRadians + 7 * Math.PI / 8,
                    directionAngleRadians + Math.PI,
                    //directionAngleRadians + 9 * Math.PI / 8,
                    directionAngleRadians + 7 * Math.PI / 6,
                    directionAngleRadians + 3 * Math.PI / 2
                };
                foreach (var beamStartAngle in beamStartAngles)
                {
                    var beamStartPoint = new Point((int)(meteorRadius * Math.Cos(beamStartAngle)),
                        (int)(-meteorRadius * Math.Sin(beamStartAngle)));
                    var beamEndPoint = beamStartPoint + new Size((int)(-20 * Math.Cos(directionAngleRadians)), (int)(20 * Math.Sin(directionAngleRadians)));
                    g.DrawLine(new Pen(Colors.Yellow, 2), meteorCoordinates + beamStartPoint, meteorCoordinates + beamEndPoint);
                }
            }
        }

        private void DrawShip(Ship ship)
        {
            this.DrawShip(ship, this.combatMap.HexToPixel(ship.ObjectCoordinates));
        }

        private void DrawShip(Ship ship, Point shipCoordinates)
        {
            foreach (var shape in ship.ObjectAppearance)
            {
                this.DrawShape(shape, teamColors[ship.Owner], shipCoordinates);
            }

            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.DrawText(Fonts.Sans(8), Brushes.Blue, shipCoordinates + new Size(0, 15), ship.ActionsLeft.ToString());
                g.DrawText(Fonts.Sans(8), Brushes.Red, shipCoordinates + new Size(0, -25), ship.CurrentHealth.ToString());
            }
        }

        private void DrawShape(DrawableShape shape, Color teamColor, Point offset)
        {
            switch (shape)
            {
                case Ellipsis e:
                    this.DrawEllipsis(e, teamColor, offset);
                    break;
                case Polygon p:
                    this.DrawPolygon(p, teamColor, offset);
                    break;
                default:
                    throw new ArgumentException($"Drawing of {shape.GetType()} not supported");
            }
        }

        private void DrawEllipsis(Ellipsis ellipsis, Color teamColor, Point offset)
        {
            throw new NotImplementedException();
        }

        private void DrawPolygon(Polygon polygon, Color teamColor, Point offset)
        {
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillPolygon(polygon.IsTeamColor ? teamColor : polygon.Color,
                    polygon.Points.Select(p => p + offset).ToArray());
            }
        }

        public void OnAnimationPending(object sender, AnimationEventArgs eventArgs)
        {
            this.HandleAnimationQueue(eventArgs);
        }

        private void AnimateMovingObjects(SpaceObject spaceObject,PointF movementStartPoint, PointF movementDestinationPoint)
        {
            // disabling ability to move multiple objects at once, because it kinda hurts turn-based principle, where each object moves on it's own turn
            if (spaceObject == null)
            {
                return;
            }
            spaceObject.IsMoving = true;
            performingAnimation = true;
            var stepDifference = new SizeF((movementDestinationPoint.X - movementStartPoint.X) / 10, (movementDestinationPoint.Y - movementStartPoint.Y) / 10);
            var currentCoordinates = movementStartPoint;
            var animationTimer = new UITimer { Interval = defaultTimerInterval };
            var steps = 0;
            animationTimer.Elapsed += (sender, eventArgs) =>
            {
                currentCoordinates += stepDifference;
                this.DrawField();
                this.DrawSpaceObject(spaceObject, Point.Round(currentCoordinates));
                this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
                if (++steps >= 10)
                {
                    animationTimer.Stop();
                    this.gameStateToDraw = this.defaultGameState;
                    performingAnimation = false;
                    spaceObject.IsMoving = false;
                    this.HandleAnimationQueue();
                }
            };
            animationTimer.Start();
        }
        
        private void AnimateAttack(SpaceObject pendingAnimationSpaceObject, List<Bitmap> pendingAnimationOverlaySprites)
        {
            performingAnimation = true;
            var animationTimer = new UITimer { Interval = defaultTimerInterval };
            var overlaySpriteIndex = 0;
            animationTimer.Elapsed += delegate {
                this.DrawField();
                using (var g = new Graphics(this.CurrentBitmap))
                {
                    g.DrawImage(pendingAnimationOverlaySprites[overlaySpriteIndex], 0, 0);
                }
                this.BitmapUpdated?.Invoke(this, EventArgs.Empty);

                if (++overlaySpriteIndex >= pendingAnimationOverlaySprites.Count)
                {
                    animationTimer.Stop();
                    this.gameStateToDraw = this.defaultGameState;
                    performingAnimation = false;
                    HandleAnimationQueue();
                }
            };
            animationTimer.Start();
        }

        private void AnimateRotation(SpaceObject spaceObject, double angle)
        {
            var totalStepCount = 7;
            spaceObject.IsMoving = true;
            performingAnimation = true;
            var dAngle = angle / totalStepCount;

            var spaceObjectToAnimate = spaceObject;

            var animationTimer = new UITimer { Interval = defaultTimerInterval };
            var steps = 0;
            animationTimer.Elapsed += (sender, eventArgs) =>
            {
                spaceObjectToAnimate.Rotate(dAngle);
                this.DrawField();
                this.DrawSpaceObject(spaceObjectToAnimate);
                this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
                if (++steps >= totalStepCount)
                {
                    animationTimer.Stop();
                    this.gameStateToDraw = this.defaultGameState;
                    performingAnimation = false;
                    spaceObject.IsMoving = false;
                    this.HandleAnimationQueue();
                }
            };
            animationTimer.Start();
        }

        private void AnimateExplosion(Point explosionCenter, int explosionRadius)
        {
            var totalStepCount = 6;
            var starColors = new List<Color> { Colors.Red, Colors.Orange, Colors.Yellow };
            var outerStarRatios = new List<float> { 1, 0.75F, 0.5F };
            var innerStarRatios = new List<float> { 0.9F, 0.65F, 0.4F };
            var starVertices = new PointF[8];
            var vertexAngleRotation = 3 * Math.PI / 4;
            performingAnimation = true;

            var animationTimer = new UITimer { Interval = defaultTimerInterval };
            var steps = 0;

            animationTimer.Elapsed += delegate {
                this.DrawField();
                
                var currentExplosionRadius = explosionRadius * (steps + 1) / (float)totalStepCount;
                using (var g = new Graphics(this.CurrentBitmap))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        double angleOffset = 0;
                        for (int j = 0; j < 8; j++)
                        {
                            starVertices[j] = explosionCenter + new PointF(outerStarRatios[i] * currentExplosionRadius * (float)Math.Cos(angleOffset),
                                                                           outerStarRatios[i] * currentExplosionRadius * (float)Math.Sin(angleOffset));
                            angleOffset += vertexAngleRotation;
                        }
                        g.FillPolygon(starColors[i], starVertices);

                        angleOffset = Math.PI / 8;
                        for (int j = 0; j < 8; j++)
                        {
                            starVertices[j] = explosionCenter + new PointF(innerStarRatios[i] * currentExplosionRadius * (float)Math.Cos(angleOffset),
                                                                           innerStarRatios[i] * currentExplosionRadius * (float)Math.Sin(angleOffset));
                            angleOffset += vertexAngleRotation;
                        }
                        g.FillPolygon(starColors[i], starVertices);
                    }
                }

                this.BitmapUpdated?.Invoke(this, EventArgs.Empty);

                if (++steps >= totalStepCount)
                {
                    animationTimer.Stop();
                    this.gameStateToDraw = this.defaultGameState;
                    performingAnimation = false;
                    HandleAnimationQueue();
                }
            };

            animationTimer.Start();
        }
    }
}