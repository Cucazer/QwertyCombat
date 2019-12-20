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
        public enum BitmapElement
        {
            GameField,
            EndTurnButton,
            SoundButton,
            None
        }

        public Bitmap CurrentBitmap;

        private const int GameUiHeight = 50;
        private const PixelFormat BitmapPixelFormat = PixelFormat.Format32bppRgba;

        private Bitmap EmptyGameFieldBitmap => new Bitmap(this.gameFieldWidth, this.gameFieldHeight, BitmapPixelFormat);

        public Point GameFieldOffset => new Point(0, GameUiHeight);

        private readonly int gameFieldWidth;
        private readonly int gameFieldHeight;
        private ObjectManager objectManager;
        private readonly GameSettings gameSettings;
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

        public FieldPainter(int fieldWidth, int fieldHeight, ObjectManager objectManager, GameSettings gameSettings)
        {
            this.gameFieldWidth = fieldWidth;
            this.gameFieldHeight = fieldHeight;
            this.objectManager = objectManager;
            this.gameSettings = gameSettings;
            this.defaultGameState = objectManager.GameState;
            this.gameStateToDraw = objectManager.GameState;
            this.CurrentBitmap = new Bitmap(fieldWidth, GameUiHeight + fieldHeight, BitmapPixelFormat);
        }

        public BitmapElement GetUiElementAtLocation(Point location)
        {
            if (location.Y > GameUiHeight)
            {
                return BitmapElement.GameField;
            }

            var endTurnButtonRectangle = new Rectangle(new Point(this.CurrentBitmap.Width / 2 - 50, 30), new Size(100, 15));

            if (endTurnButtonRectangle.Contains(location))
            {
                return BitmapElement.EndTurnButton;
            }

            var soundButtonRectangle = new Rectangle(new Point(this.CurrentBitmap.Width - 50, 10), new Size(30,30));

            if (soundButtonRectangle.Contains(location))
            {
                return BitmapElement.SoundButton;
            }

            return BitmapElement.None;
        }

        public Point GetGameFieldCoordinates(PointF bitmapCoordinates)
        {
            return new Point(bitmapCoordinates - this.GameFieldOffset);
        }

        public void UpdateBitmap()  
        {
            if (this.performingAnimation)
            {
                //don't disturb animation
                return;
            }
            
            this.DrawGameScene();
            this.BitmapUpdated?.Invoke(this, EventArgs.Empty);
        }

        private bool performingAnimation = false;

        private void HandleAnimationQueue(AnimationEventArgs animationToPerform = null)
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

        private void DrawGameScene(Bitmap gameFieldOverlayBitmap = null)
        {
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.Clear(Colors.Black);
                g.DrawImage(this.DrawGameUI(), new Point(0,0));
                g.DrawImage(this.DrawGameField(), this.GameFieldOffset);
                g.DrawImage(this.DrawGameObjects(), this.GameFieldOffset);
                if (gameFieldOverlayBitmap != null)
                {
                    g.DrawImage(gameFieldOverlayBitmap, this.GameFieldOffset);
                }

                if (this.gameStateToDraw.GameOver)
                {
                    g.DrawImage(this.DrawGameOverScreen(), this.GameFieldOffset);
                }
#if DEBUG
                g.DrawImage(this.DisplayCellCoordinates(), this.GameFieldOffset);
#endif
            }
        }

        private Bitmap DrawGameOverScreen()
        {
            var gameOverBitmap = this.EmptyGameFieldBitmap;

            using (var g = new Graphics(gameOverBitmap))
            {
                g.DrawText(Fonts.Sans(40, FontStyle.Bold), Colors.Red, 10, this.gameFieldHeight / 2, "GAME OVER");
            }

            return gameOverBitmap;
        }

        private Bitmap DrawGameUI()
        {
            var uiBitmap = new Bitmap(this.CurrentBitmap.Width, GameUiHeight, BitmapPixelFormat);

            var shipsAliveBarPoints = new List<PointF>
            {
                new PointF(0, 0),
                new PointF(100, 0),
                new PointF(110, 10),
                new PointF(100, 20),
                new PointF(0, 20)
            };
            var shipsAliveBarOffset = new Point(this.CurrentBitmap.Width / 2, 10);
            var redShipCount = this.gameStateToDraw.Ships.Count(sh => sh.Owner == Player.SecondPlayer);
            var blueShipCount = this.gameStateToDraw.Ships.Count(sh => sh.Owner == Player.FirstPlayer);
            //TODO: get rid of this hacky health bar width calculation
            var redShipsAliveBarPoints = shipsAliveBarPoints.Select(p => p + (p.X == 0 ? new Point((3 - redShipCount) * 33, 0) : Point.Empty) + shipsAliveBarOffset).ToArray();
            var blueShipsAliveBarPoints = shipsAliveBarPoints.Select(p => new PointF(-p.X, p.Y) + (p.X == 0 ? new Point((3 - blueShipCount) * -33, 0) : Point.Empty) + shipsAliveBarOffset).ToArray();

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
            using (var g = new Graphics(uiBitmap))
            {
                if (redShipCount > 0)
                {
                    g.FillPolygon(Colors.Red, redShipsAliveBarPoints);
                }

                if (blueShipCount > 0)
                {
                    g.FillPolygon(Colors.Blue, blueShipsAliveBarPoints);
                }

                g.DrawPolygon(Colors.Purple,
                    endTurnButtonPoints.Select(p => p + new Point(this.CurrentBitmap.Width / 2, 30)).ToArray());
                g.DrawText(Fonts.Monospace(10), Colors.White, new Point(this.CurrentBitmap.Width / 2 - 30, 30), "End turn");

                var activeTeamPolygonPoints = shipsAliveBarPoints.Select(p => new PointF(-p.X, p.Y)).ToList();
                var inactiveTeamPolygonPoints = shipsAliveBarPoints;
                if (this.gameStateToDraw.ActivePlayer == Player.SecondPlayer)
                {
                    activeTeamPolygonPoints = shipsAliveBarPoints;
                    inactiveTeamPolygonPoints = shipsAliveBarPoints.Select(p => new PointF(-p.X, p.Y)).ToList();
                }

                g.DrawPolygon(inactiveTeamPen,
                    inactiveTeamPolygonPoints.Select(p => p + new Point(this.CurrentBitmap.Width / 2, 10)).ToArray());
                g.DrawPolygon(activeTeamPen,
                    activeTeamPolygonPoints.Select(p => p + new Point(this.CurrentBitmap.Width / 2, 10)).ToArray());

                g.DrawText(Fonts.Monospace(12), Colors.White, new Point(this.CurrentBitmap.Width / 2 - 20, 11), blueShipCount.ToString());
                g.DrawText(Fonts.Monospace(12), Colors.White, new Point(this.CurrentBitmap.Width / 2 + 10, 11), redShipCount.ToString());

                var speakerOffset = new Point(this.CurrentBitmap.Width - 50, 10);
                g.FillPolygon(Colors.LightSlateGray, speakerIconPoints.Select(p => p + speakerOffset).ToArray());
                if (this.gameSettings.SoundEnabled)
                {
                    foreach (var waveRadius in speakerSoundWaveRadiuses)
                    {
                        g.DrawArc(speakerSoundWavesPen,
                            new RectangleF(new PointF(15 - waveRadius, 15 - waveRadius) + speakerOffset,
                                new SizeF(waveRadius * 2, waveRadius * 2)), -45, 90);
                    }
                }
                else
                {
                    foreach (var linePoints in speakerNoSoundLinePoints)
                    {
                        g.DrawLine(speakerSoundWavesPen, linePoints.Item1 + speakerOffset,
                            linePoints.Item2 + speakerOffset);
                    }
                }
            }

            return uiBitmap;
        }

        private Bitmap DrawGameField()
        {
            var gameFieldBitmap = this.EmptyGameFieldBitmap;

            // should always ensure .Dispose() is called when you are done with a Graphics object
            using (var g = new Graphics(gameFieldBitmap))
            {
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

            return gameFieldBitmap;
        }

        private Bitmap DrawGameObjects()
        {
            var objectsBitmap = this.EmptyGameFieldBitmap;

            foreach (var ship in this.gameStateToDraw.Ships)
            {
                if (!ship.IsMoving)
                {
                    this.DrawShip(ship, objectsBitmap);
                }
            }

            foreach (var meteor in this.gameStateToDraw.Meteors)
            {
                if (!meteor.IsMoving)
                {
                    this.DrawMeteor(meteor, objectsBitmap);
                }
            }

            return objectsBitmap;
        }

        private Bitmap DisplayCellCoordinates()
        {
            var coordinatesBitmap = this.EmptyGameFieldBitmap;

            using (var g = new Graphics(coordinatesBitmap))
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

            return coordinatesBitmap;
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
            var objectName = objectProperties["Name"];
            objectProperties.Remove("Name");
            var maxLineLength = Math.Max(objectName.Length, objectProperties.Max(keyValue => keyValue.Key.Length + keyValue.Value.Length) + 3); // min 3 dots between key and value
            var textBoxSize = new Size(6 * maxLineLength, 15 * (objectProperties.Count + 1));
            var textBoxLocation = location + new Size(0, 20);
            if (!this.CurrentBitmap.Size.Contains(location + textBoxSize))
            {
                textBoxLocation -= textBoxSize + new Size(0, 20);
                if (textBoxLocation.X < 0)
                {
                    textBoxLocation += new Size(textBoxSize.Width, 0);
                }
                if (textBoxLocation.Y < 0)
                {
                    textBoxLocation += new Size(0, textBoxSize.Height);
                }
            }

            //i'm not going to use overlay here because we are working with already bitmap coordinates, tooltip location has no relationship with game field coordinates
            using (var g = new Graphics(this.CurrentBitmap))
            {
                g.FillRectangle(Colors.Black, new Rectangle(textBoxLocation, textBoxSize));
                g.DrawRectangle(Colors.Red, new Rectangle(textBoxLocation, textBoxSize));
                g.DrawLine(Colors.Red, textBoxLocation + new Size(5, 20), textBoxLocation + new Size(textBoxSize.Width - 5, 20));
                //TODO: team color for name?
                g.DrawText(Fonts.Fantasy(12, FontStyle.Bold), Colors.SkyBlue, textBoxLocation + new Size(5 + (textBoxSize.Width - 6 * objectName.Length) / 2, 4), objectName);

                var lineIndex = 1;
                foreach (var property in objectProperties)
                {
                    g.DrawText(Fonts.Monospace(7), Colors.Lime, textBoxLocation + new Size(5, 10 + 12 * lineIndex++), $"{property.Key}{property.Value.PadLeft(maxLineLength - property.Key.Length, '.')}");
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

        private void DrawSpaceObject(SpaceObject spaceObject, Bitmap bitmap)
        {
            this.DrawSpaceObject(spaceObject, this.combatMap.HexToPixel(spaceObject.ObjectCoordinates), bitmap);
        }

        private void DrawSpaceObject(SpaceObject spaceObject, Point spaceObjectCoordinates, Bitmap bitmap)
        {
            switch (spaceObject)
            {
                case Ship s:
                    this.DrawShip(s, spaceObjectCoordinates, bitmap);
                    break;
                case Meteor m:
                    this.DrawMeteor(m, spaceObjectCoordinates, bitmap);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Drawing of {spaceObject.GetType()} not supported");
            }
        }

        private void DrawMeteor(Meteor meteor, Bitmap bitmap)
        {
            this.DrawMeteor(meteor, this.combatMap.HexToPixel(meteor.ObjectCoordinates), bitmap);
        }

        private void DrawMeteor(Meteor meteor, Point meteorCoordinates, Bitmap bitmap)
        {
            foreach (var shape in meteor.ObjectAppearance)
            {
                this.DrawShape(shape, new Color(), meteorCoordinates, bitmap);
            }

            using (var g = new Graphics(bitmap))
            {
                g.FillRectangle(Colors.Red, new RectangleF(meteorCoordinates + new Size(-10, -25), new SizeF(20 * meteor.CurrentHealth / meteor.MaxHealth, 4)));
                g.DrawRectangle(Colors.White, new RectangleF(meteorCoordinates + new Size(-10, -25), new SizeF(20, 4)));
            }
        }

        private void DrawShip(Ship ship, Bitmap bitmap)
        {
            this.DrawShip(ship, this.combatMap.HexToPixel(ship.ObjectCoordinates), bitmap);
        }

        private void DrawShip(Ship ship, Point shipCoordinates, Bitmap bitmap)
        {
            foreach (var shape in ship.ObjectAppearance)
            {
                this.DrawShape(shape, this.teamColors[ship.Owner], shipCoordinates, bitmap);
            }

            if (ship.IsMoving)
            {
                foreach (var flameBounds in ship.FlameBounds)
                {
                    this.DrawPolygon(flameBounds, new Color(), shipCoordinates, bitmap);
                }
            }

            using (var g = new Graphics(bitmap))
            {
                g.FillRectangle(Colors.Blue, new RectangleF(shipCoordinates + new Size(-10, 15), new SizeF(20 * ship.ActionsLeft / ship.MaxActions, 4)));
                g.DrawRectangle(Colors.White, new RectangleF(shipCoordinates + new Size(-10, 15), new SizeF(20, 4)));
                g.FillRectangle(Colors.Red, new RectangleF(shipCoordinates + new Size(-10, -25), new SizeF(20 * ship.CurrentHealth / ship.MaxHealth,4)));
                g.DrawRectangle(Colors.White, new RectangleF(shipCoordinates + new Size(-10, -25), new SizeF(20,4)));
            }
        }

        private void DrawShape(DrawableShape shape, Color teamColor, Point offset, Bitmap bitmap)
        {
            switch (shape)
            {
                case Arc a:
                    this.DrawArc(a, teamColor, offset, bitmap);
                    break;
                case Ellipse e:
                    this.DrawEllipse(e, teamColor, offset, bitmap);
                    break;
                case Polygon p:
                    this.DrawPolygon(p, teamColor, offset, bitmap);
                    break;
                case Path p:
                    this.DrawPath(p, teamColor, offset, bitmap);
                    break;
                default:
                    throw new ArgumentException($"Drawing of {shape.GetType()} not supported");
            }
        }

        private void DrawArc(Arc arc, Color teamColor, Point offset, Bitmap bitmap)
        {
            //TODO: fill vs thickness
            using (var g = new Graphics(bitmap))
            {
                g.FillPie(arc.IsTeamColor ? teamColor : arc.Color, new RectangleF(arc.Origin + offset, arc.Size),
                    arc.StartAngle, arc.SweepAngle);
            }
        }

        private void DrawEllipse(Ellipse ellipse, Color teamColor, Point offset, Bitmap bitmap)
        {
            using (var g = new Graphics(bitmap))
            {
                g.FillEllipse(ellipse.IsTeamColor ? teamColor : ellipse.Color, new RectangleF(ellipse.Origin + offset, ellipse.Size));
            }
        }

        private void DrawPolygon(Polygon polygon, Color teamColor, Point offset, Bitmap bitmap)
        {
            using (var g = new Graphics(bitmap))
            {
                g.FillPolygon(polygon.IsTeamColor ? teamColor : polygon.Color,
                    polygon.Points.Select(p => p + offset).ToArray());
            }
        }

        private void DrawPath(Path path, Color teamColor, Point offset, Bitmap bitmap)
        {
            using (var g = new Graphics(bitmap))
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
                var movingObjectBitmap = this.EmptyGameFieldBitmap;
                this.DrawSpaceObject(spaceObject, Point.Round(currentCoordinates), movingObjectBitmap);
                this.DrawGameScene(movingObjectBitmap);
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
                this.DrawGameScene(pendingAnimationOverlaySprites[overlaySpriteIndex]);
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
                var rotatingObjectBitmap = this.EmptyGameFieldBitmap;
                this.DrawSpaceObject(spaceObjectToAnimate, rotatingObjectBitmap);
                this.DrawGameScene(rotatingObjectBitmap);
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
                var explosionBitmap = this.EmptyGameFieldBitmap;
                var currentExplosionRadius = explosionRadius * (steps + 1) / (float)totalStepCount;
                using (var g = new Graphics(explosionBitmap))
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
                this.DrawGameScene(explosionBitmap);

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