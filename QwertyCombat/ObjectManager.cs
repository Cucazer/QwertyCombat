﻿using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using QwertyCombat.Objects;
using QwertyCombat.Objects.Weapons;
using Hex = Barbar.HexGrid;

namespace QwertyCombat
{
    class ObjectManager
    {
        public int MapWidth { get; }
        public int MapHeight { get; }
        public const int MeteorAppearanceChance = 20;

        public CombatMap CombatMap;
        public SpaceObject[] SpaceObjects => this.GameState.SpaceObjects;
        public List<Meteor> Meteors => this.GameState.Meteors;
        public List<Ship> Ships => this.GameState.Ships;

        public static event EventHandler<AnimationEventArgs> ObjectAnimated;
        public static event EventHandler<SoundEventArgs> SoundPlayed;
        
        public int BitmapWidth => this.CombatMap.BitmapWidth;
        public int BitmapHeight => this.CombatMap.BitmapHeight;

        public readonly GameState GameState;

        public ObjectManager(int mapWidth, int mapHeight)
        {
            this.MapWidth = mapWidth;
            this.MapHeight = mapHeight;
            this.CombatMap = new CombatMap(mapWidth, mapHeight);
            this.GameState = new GameState(mapWidth * mapHeight);

            this.CreateShip(ShipType.Scout, WeaponType.LightIon, Player.FirstPlayer);
            this.CreateShip(ShipType.Scout, WeaponType.LightIon, Player.FirstPlayer);
            this.CreateShip(ShipType.Assaulter, WeaponType.HeavyLaser, Player.FirstPlayer);

            this.CreateShip(ShipType.Scout, WeaponType.LightLaser, Player.SecondPlayer);
            this.CreateShip(ShipType.Scout, WeaponType.LightLaser, Player.SecondPlayer);
            this.CreateShip(ShipType.Assaulter, WeaponType.HeavyLaser, Player.SecondPlayer);

            this.CreateMeteor();
        }

        public SpaceObject PixelToSpaceObject(Point pixelCoordinates)
        {
            var hexOffsetCoordinates = this.PixelToOffsetCoordinates(pixelCoordinates);
            return this.GetObjectByOffsetCoordinates(hexOffsetCoordinates.Column, hexOffsetCoordinates.Row);
        }
        
        public Hex.OffsetCoordinates PixelToOffsetCoordinates(Point pixelCoordinates)
        {
            return this.CombatMap.PixelToOffsetCoordinates(pixelCoordinates);
        }

        public bool CanMoveObjectTo(SpaceObject spaceObject, Hex.OffsetCoordinates destination)
        {
            return this.PerformBFS(spaceObject.ObjectCoordinates, spaceObject.ActionsLeft).Keys.Contains(destination);
        }

        public List<Hex.OffsetCoordinates> GetMovementRange(Ship ship)
        {
            return this.PerformBFS(ship.ObjectCoordinates, ship.ActionsLeft).Keys.Skip(1).ToList();
        }


        public List<Hex.OffsetCoordinates> GetHexagonPath(Hex.OffsetCoordinates startHexagon, Hex.OffsetCoordinates finishHexagon)
        {
            var stepOriginHexagons = this.PerformBFS(startHexagon);
            var currentHexagon = finishHexagon;
            var path = new List<Hex.OffsetCoordinates>();

            while (!currentHexagon.Equals(startHexagon))
            {
                path.Add(currentHexagon);
                currentHexagon = stepOriginHexagons[currentHexagon];
            }

            path.Reverse();
            return path;
        }

        private Dictionary<Hex.OffsetCoordinates, Hex.OffsetCoordinates> PerformBFS(Hex.OffsetCoordinates startHexagon, int maxDistance = -1)
        {
            var bfsQueue = new Queue<Hex.OffsetCoordinates>();
            bfsQueue.Enqueue(startHexagon);
            var cameFromHexagons = new Dictionary<Hex.OffsetCoordinates, Hex.OffsetCoordinates>() { { startHexagon, new Hex.OffsetCoordinates(-1, -1) } };
            var stepsToHexagon = new Dictionary<Hex.OffsetCoordinates, int>() { { startHexagon, 0 } };

            while (bfsQueue.Count > 0)
            {
                var currentHexagon = bfsQueue.Dequeue();

                if (maxDistance >= 0 && stepsToHexagon[currentHexagon] >= maxDistance)
                {
                    continue;
                }

                foreach (var neighborHexagon in this.CombatMap.GetAllNeighbors(currentHexagon).Where(c => this.SpaceObjects[this.OffsetCoordinatesToIndex(c)] == null))
                {
                    if (!cameFromHexagons.ContainsKey(neighborHexagon))
                    {
                        bfsQueue.Enqueue(neighborHexagon);
                        cameFromHexagons.Add(neighborHexagon, currentHexagon);
                        stepsToHexagon.Add(neighborHexagon, stepsToHexagon[currentHexagon] + 1);
                    }
                }
            }
            return cameFromHexagons;
        }

        public List<Hex.OffsetCoordinates> GetAttackTargets(Ship ship)
        {
            var hexagonsInAttackRange = this.CombatMap.GetAllHexagonsInRange(ship.ObjectCoordinates, ship.EquippedWeapon.AttackRange);
            return hexagonsInAttackRange.Where(c => (this.SpaceObjects[this.OffsetCoordinatesToIndex(c)]?.Owner ?? ship.Owner) != ship.Owner).ToList();
        }

        public void AddObject(SpaceObject spaceObject)
        {
            this.SpaceObjects[this.OffsetCoordinatesToIndex(spaceObject.ObjectCoordinates)] = spaceObject;
        }

        public void DeleteObject(SpaceObject spaceObject)
        {
            this.SpaceObjects[Array.IndexOf(this.SpaceObjects, spaceObject)] = null;
        }

        public SpaceObject GetObjectByOffsetCoordinates(int column, int row)
        {
            if (column < 0 || column >= this.MapWidth || row < 0 || row >= this.MapHeight)
            {
                return null;
            }
            return this.SpaceObjects[this.OffsetCoordinatesToIndex(column, row)];
        }

        private int OffsetCoordinatesToIndex(Hex.OffsetCoordinates offsetCoordinates)
        {
            return this.OffsetCoordinatesToIndex(offsetCoordinates.Column, offsetCoordinates.Row);
        }

        private int OffsetCoordinatesToIndex(int column, int row)
        {
            return row * this.MapWidth + column;
        }

        public int GetDistance(SpaceObject firstObject, SpaceObject secondObject)
        {
            return this.CombatMap.GetDistance(firstObject.ObjectCoordinates, secondObject.ObjectCoordinates);
        }

        public Hex.OffsetCoordinates GetMeteorNextStepCoordinates(Meteor meteor)
        {
            return this.CombatMap.GetNeighborCoordinates(meteor.ObjectCoordinates, (int)meteor.MovementDirection);
        }
        
        public void CreateMeteor()
        {
            var meteorCoordinates = new Hex.OffsetCoordinates();
            HexagonNeighborDirection movementDirection = 0;
            
            var rand = new Random();
            var randomMapSide = rand.Next(4);
            switch(randomMapSide)
            {
                case 0:  // left
                    meteorCoordinates = this.GetRandomVacantHexagon(0, 0, 0, this.MapHeight - 1);
                    movementDirection = rand.Next(2) == 0
                        ? HexagonNeighborDirection.NorthEast
                        : HexagonNeighborDirection.SouthEast;
                    break;
                case 1: // top
                    meteorCoordinates = this.GetRandomVacantHexagon(0, this.MapWidth - 1, 0, 0);
                    movementDirection = rand.Next(2) == 0
                        ? HexagonNeighborDirection.SouthEast
                        : HexagonNeighborDirection.SouthWest;
                    break;
                case 2: // right
                    meteorCoordinates = this.GetRandomVacantHexagon(this.MapWidth - 1, this.MapWidth - 1, 0, this.MapHeight - 1);
                    movementDirection = rand.Next(2) == 0
                        ? HexagonNeighborDirection.NorthWest
                        : HexagonNeighborDirection.SouthWest;
                    break;
                case 3: // bottom
                    meteorCoordinates = this.GetRandomVacantHexagon(0, this.MapWidth - 1, this.MapHeight - 1, this.MapHeight - 1);
                    movementDirection = rand.Next(2) == 0
                        ? HexagonNeighborDirection.NorthEast
                        : HexagonNeighborDirection.NorthWest;
                    break;
            }

            var meteorHealth = rand.Next(1, 150);
            var meteorDmg = meteorHealth / 4;

            var meteor = new Meteor(meteorCoordinates, meteorHealth, meteorDmg, movementDirection);
            var headingAngle = this.CombatMap.GetAngle(meteorCoordinates,
                this.CombatMap.GetNeighborCoordinates(meteorCoordinates, (int) movementDirection));
            meteor.Rotate(headingAngle);
            this.AddObject(meteor);
        }

        public double GetRelativeHexagonAngle(SpaceObject sourceSpaceObject, Hex.OffsetCoordinates targetOffsetCoordinates)
        {
            var angle = this.CombatMap.GetAngle(sourceSpaceObject.ObjectCoordinates, targetOffsetCoordinates);
            if (sourceSpaceObject.Owner == Player.SecondPlayer)
            {
                angle -= 180;
            }

            while (angle > 180)
            {
                angle -= 360;
            }
            
            while (angle < -180)
            {
                angle += 360;
            }

            return angle;
        }
        
        public double GetTargetHexagonAngle(Hex.OffsetCoordinates sourceOffsetCoordinates, Hex.OffsetCoordinates targetOffsetCoordinates)
        {
            return this.CombatMap.GetAngle(sourceOffsetCoordinates, targetOffsetCoordinates);
        }

        public void MoveObjectTo(SpaceObject spaceObject, Hex.OffsetCoordinates destination, bool onlyAnimate = false)
        {
            if (spaceObject is Ship)
            {
                SoundPlayed?.Invoke(this, new SoundEventArgs(Properties.Resources.spaceShipFly));
            }
            ObjectAnimated?.Invoke(this, new AnimationEventArgs(this.CaptureGameState(spaceObject), this.CombatMap.HexToPixel(spaceObject.ObjectCoordinates), this.CombatMap.HexToPixel(destination)));
            if (destination.Column < 0 || destination.Column >= this.MapWidth ||
                destination.Row < 0 || destination.Row >= this.MapHeight)
            {
                // moving object outside bounds = deleting object
                this.DeleteObject(spaceObject);
                return;
            }

            if (onlyAnimate)
            {
                return;
            }

            this.SpaceObjects[this.OffsetCoordinatesToIndex(spaceObject.ObjectCoordinates)] = null;
            this.SpaceObjects[this.OffsetCoordinatesToIndex(destination)] = spaceObject;
            spaceObject.ObjectCoordinates = destination;
        }

        public void AttackObject(SpaceObject attacker, SpaceObject victim)
        {
            var attackerShip = attacker as Ship;
            if (attackerShip != null)
            {
                var attackSprites = attackerShip.EquippedWeapon.GetAttackSprites(
                    this.CombatMap.HexToPixel(attackerShip.ObjectCoordinates) + attackerShip.WeaponPoint,
                    this.CombatMap.HexToPixel(victim.ObjectCoordinates));
                SoundPlayed?.Invoke(this, new SoundEventArgs(attackerShip.EquippedWeapon.AttackSound));
                ObjectAnimated?.Invoke(this, new AnimationEventArgs(this.CaptureGameState(attacker), attackSprites));
                this.DealDamage(victim, attackerShip.AttackDamage);
                attackerShip.ActionsLeft -= attackerShip.EquippedWeapon.EnergyСonsumption;
            }
        }

        public void DealDamage(SpaceObject victim, int damageAmount)
        {
            ObjectAnimated?.Invoke(this, new AnimationEventArgs(this.CaptureGameState(),
                this.CombatMap.HexToPixel(victim.ObjectCoordinates), (int)(Math.Min(1, 1 - ((victim.CurrentHealth - damageAmount) / (double)victim.MaxHealth)) * CombatMap.HexagonSideLength)));
            victim.CurrentHealth -= damageAmount;
            if (victim.CurrentHealth <= 0)
            {
                this.DeleteObject(victim);
            }
        }

        public void RotateObject(SpaceObject spaceObject, double angle)
        {
            ObjectAnimated?.Invoke(this, new AnimationEventArgs(this.CaptureGameState(spaceObject), angle));
            spaceObject.Rotate(angle);
        }

        private GameState CaptureGameState(SpaceObject activeSpaceObject = null)
        {
            var capturedGameState = (GameState)this.GameState.Clone();
            if (activeSpaceObject != null)
            {
                capturedGameState.ActiveSpaceObject = capturedGameState.SpaceObjects[this.OffsetCoordinatesToIndex(activeSpaceObject.ObjectCoordinates)];
            }
            return capturedGameState;
        }

        private void CreateShip(ShipType shipType, WeaponType weaponType, Player owner)
        {
            Ship newShip;
            switch (shipType)
            {
                case ShipType.Scout:
                    newShip = new ShipScout(owner, weaponType);
                    break;
                case ShipType.Assaulter:
                    newShip = new ShipAssaulter(owner, weaponType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shipType), shipType, null);
            }

            int minColumnIndex = owner == Player.FirstPlayer ? 0 : this.MapWidth - 2;
            int maxColumnIndex = owner == Player.FirstPlayer ? 1 : this.MapWidth - 1;
            newShip.ObjectCoordinates = this.GetRandomVacantHexagon(minColumnIndex, maxColumnIndex, 0, this.MapHeight - 1);
            this.AddObject(newShip);
        }

        private Hex.OffsetCoordinates GetRandomVacantHexagon(int minColumnIndex, int maxColumnIndex, int minRowIndex, int maxRowIndex)
        {
            var rand = new Random();
            int randomColumn;
            int randomRow;
            do
            {
                randomColumn = rand.Next(minColumnIndex, maxColumnIndex+ 1);
                randomRow = rand.Next(minRowIndex, maxRowIndex + 1);
            } while (this.GetObjectByOffsetCoordinates(randomColumn, randomRow) != null);
            return new Hex.OffsetCoordinates(randomColumn, randomRow);
        }

        public void EndTurn()
        {
            foreach (var ship in this.Ships)
            {
                ship.RefillEnergy();
            }
        }
    }
}
