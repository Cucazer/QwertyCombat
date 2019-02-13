﻿using System;
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
                this.DrawGameScene();
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
                this.AnimationQueue.Enqueue(animationToPerform);
            }

            if (this.performingAnimation)
            {
                return;
            }

            if (!this.AnimationQueue.Any())
            {
                this.DrawGameScene();
                this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
                return;
            }

            var nextAnimation = this.AnimationQueue.Dequeue();
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

        public void DrawGameScene()
        {
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.Clear(Colors.Black);
            }

            this.DrawGameUI();

            // call to draw game field itself, with according offset

            //this.DrawGameField();
            //this.DrawGameObjects();

#if DEBUG
            //this.DisplayCellCoordinates();
#endif
        }

        private void DrawGameUI()
        {
            var shipsAliveBarPoints = new List<PointF>
            {
                new PointF(0, 0),
                new PointF(100, 0),
                new PointF(110, 10),
                new PointF(100, 20),
                new PointF(0, 20)
            };
            var shipsAliveBarOffset = new Point(this.CurrentBitmap.Width / 2, 10);
            var redShipsAliveBarPoints = shipsAliveBarPoints.Select(p => p + shipsAliveBarOffset).ToArray();
            var blueShipsAliveBarPoints = shipsAliveBarPoints.Select(p => new PointF(-p.X, p.Y) + shipsAliveBarOffset).ToArray();

            var endTurnButtonPoints = new List<PointF>
            {
                new PointF(-50, 0),
                new PointF(50, 0),
                new PointF(35, 15),
                new PointF(-35, 15)
            };

            var speakerIconPoints = new List<PointF>
            {
                new PointF(0, 10),
                new PointF(5, 10),
                new PointF(15, 0),
                new PointF(15, 30),
                new PointF(5, 20),
                new PointF(0, 20)
            };
            var speakerNoSoundLinePoints = new List<Tuple<PointF, PointF>>
            {
                new Tuple<PointF, PointF>(new PointF(20, 10), new PointF(30, 20)),
                new Tuple<PointF, PointF>(new PointF(20, 20), new PointF(30, 10))
            };
            var speakerSoundWaveRadiuses = new List<int> {5, 10, 15};
            var speakerSoundWavesPen = new Pen(Colors.LightSlateGray, 3);

            var activeTeamPen = new Pen(Colors.Yellow, 5);
            var inactiveTeamPen = new Pen(Colors.Purple, 2);
            // TODO: make active team accessible
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillPolygon(Colors.Red, redShipsAliveBarPoints);
                g.FillPolygon(Colors.Blue, blueShipsAliveBarPoints);

                g.DrawPolygon(Colors.Purple,
                    endTurnButtonPoints.Select(p => p + new Point(this.CurrentBitmap.Width / 2, 30)).ToArray());
                g.DrawText(Fonts.Monospace(10), Colors.White, new Point(this.CurrentBitmap.Width / 2 - 30, 30), "End turn");
                g.DrawPolygon(inactiveTeamPen,
                    shipsAliveBarPoints.Select(p => p + new Point(this.CurrentBitmap.Width / 2, 10)).ToArray());
                g.DrawPolygon(activeTeamPen,
                    shipsAliveBarPoints.Select(p => new PointF(-p.X, p.Y))
                        .Select(p => p + new Point(this.CurrentBitmap.Width / 2, 10)).ToArray());

                g.DrawText(Fonts.Monospace(12), Colors.White, new Point(this.CurrentBitmap.Width / 2 - 20, 11), "3");
                g.DrawText(Fonts.Monospace(12), Colors.White, new Point(this.CurrentBitmap.Width / 2 + 10, 11), "3");

                var speakerOffset = new Point(this.CurrentBitmap.Width - 50, 10);
                g.FillPolygon(Colors.LightSlateGray, speakerIconPoints.Select(p => p + speakerOffset).ToArray());
                // sound off
                foreach (var linePoints in speakerNoSoundLinePoints)
                {
                    g.DrawLine(speakerSoundWavesPen, linePoints.Item1 + speakerOffset,
                        linePoints.Item2 + speakerOffset);
                }

                // sound on
                foreach (var waveRadius in speakerSoundWaveRadiuses)
                {
                    g.DrawArc(speakerSoundWavesPen,
                        new RectangleF(new PointF(15 - waveRadius, 15 - waveRadius) + speakerOffset,
                            new SizeF(waveRadius * 2, waveRadius * 2)), -45, 90);
                }
            }
        }

        public void DrawGameField()
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
        }

        private void DrawGameObjects()
        {
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
        }

        private void DisplayCellCoordinates()
        {
            using (var g = new Graphics(this.CurrentBitmap))
            {
                // draw hexagon coordinates
                for (int x = 0; x < this.combatMap.FieldWidth; x++)
                {
                    for (int y = 0; y < this.combatMap.FieldHeight; y++)
                    {
                        g.DrawText(Fonts.Monospace(7), Brushes.DeepSkyBlue, this.combatMap.GetHexagonCorners(x, y)[2],
                            $"C{x}R{y}");

                        var cubeCoordinates = this.combatMap.HexGrid.ToCubeCoordinates(new OffsetCoordinates(x, y));
                        g.DrawText(Fonts.Monospace(7), Brushes.DeepSkyBlue,
                            this.combatMap.GetHexagonCorners(x, y)[4] + new Size(0, -12),
                            $"Q{cubeCoordinates.Q}R{cubeCoordinates.R}S{cubeCoordinates.S}");
                    }
                }
            }
        }

        bool tooltipShown = false;
        public void ActivateObjectTooltip(Point location, Dictionary<string, string> objectProperties)
        {
            if (this.performingAnimation)
            { 
                return;
            }
            this.tooltipShown = true;
            this.DrawGameScene();
            //TODO: extract to DrawTooltip?
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
                var lineIndex = 0;
                foreach (var property in objectProperties)
                {
                    g.DrawText(Fonts.Sans(7), Colors.Lime, textBoxLocation + new Size(5, 5 + 12 * lineIndex++), $"{property.Key}...{property.Value}");
                }
            }
            this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void DeactivateTooltip()
        {
            if (this.tooltipShown)
            {
                this.DrawGameScene();
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
            switch (spaceObject)
            {
                case Ship s:
                    this.DrawShip(s, spaceObjectCoordinates);
                    break;
                case Meteor m:
                    this.DrawMeteor(m, spaceObjectCoordinates);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Drawing of {spaceObject.GetType()} not supported");
            }
        }

        private void DrawMeteor(Meteor meteor)
        {
            this.DrawMeteor(meteor, this.combatMap.HexToPixel(meteor.ObjectCoordinates));
        }

        private void DrawMeteor(Meteor meteor, Point meteorCoordinates)
        {
            foreach (var shape in meteor.ObjectAppearance)
            {
                this.DrawShape(shape, new Color(), meteorCoordinates);
            }

            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.DrawText(Fonts.Sans(8), Brushes.Red, meteorCoordinates + new Size(5, -25), meteor.CurrentHealth.ToString());
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
                case Arc a:
                    this.DrawArc(a, teamColor, offset);
                    break;
                case Ellipse e:
                    this.DrawEllipse(e, teamColor, offset);
                    break;
                case Polygon p:
                    this.DrawPolygon(p, teamColor, offset);
                    break;
                case Path p:
                    this.DrawPath(p, teamColor, offset);
                    break;
                default:
                    throw new ArgumentException($"Drawing of {shape.GetType()} not supported");
            }
        }

        private void DrawArc(Arc arc, Color teamColor, Point offset)
        {
            //TODO: fill vs thickness
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillPie(arc.IsTeamColor ? teamColor : arc.Color, new RectangleF(arc.Origin + offset, arc.Size),
                    arc.StartAngle, arc.SweepAngle);
            }
        }

        private void DrawEllipse(Ellipse ellipse, Color teamColor, Point offset)
        {
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillEllipse(ellipse.IsTeamColor ? teamColor : ellipse.Color, new RectangleF(ellipse.Origin + offset, ellipse.Size));
            }
        }

        private void DrawPolygon(Polygon polygon, Color teamColor, Point offset)
        {
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillPolygon(polygon.IsTeamColor ? teamColor : polygon.Color,
                    polygon.Points.Select(p => p + offset).ToArray());
            }
        }

        private void DrawPath(Path path, Color teamColor, Point offset)
        {
            using (var g = new Graphics(this.CurrentBitmap))
            {
                var graphicsPath = new GraphicsPath();
                foreach (var component in path.Components)
                {
                    switch (component)
                    {
                        case Arc a:
                            graphicsPath.AddArc(new RectangleF(a.Origin + offset, a.Size), a.StartAngle, a.SweepAngle);
                            break;
                        default:
                            throw new ArgumentException($"Adding of {component.GetType()} to graphics path not supported");
                    }
                }
                g.FillPath(path.IsTeamColor ? teamColor : path.Color, graphicsPath);
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
                this.DrawGameScene();
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
                this.DrawGameScene();
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
                this.DrawGameScene();
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
                this.DrawGameScene();
                
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