﻿using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QwertyCombat.Objects;
using QwertyCombat.Objects.Weapons;
using Hex = Barbar.HexGrid;

namespace QwertyCombat
{
    class ObjectManager
    {
        public int MapWidth { get; }
        public int MapHeight { get; }
        public List<Meteor> Meteors => this.SpaceObjects.OfType<Meteor>().ToList();
        public List<Ship> Ships => this.SpaceObjects.OfType<Ship>().ToList();
        public const int MeteorAppearanceChance = 20;

        public CombatMap CombatMap;
        public SpaceObject[] SpaceObjects;

        public static event EventHandler<AnimationEventArgs> ObjectAnimated;
        public static event EventHandler<SoundEventArgs> SoundPlayed;
        
        public int FirstPlayerShipCount => this.Ships.Count(sh => sh.Owner == Player.FirstPlayer);
        public int SecondPlayerShipCount => this.Ships.Count(sh => sh.Owner == Player.SecondPlayer);
        public int BitmapWidth => this.CombatMap.BitmapWidth;
        public int BitmapHeight => this.CombatMap.BitmapHeight;
        public Ship ActiveShip { get; set; }

        public ObjectManager(int mapWidth, int mapHeight)
        {
            this.MapWidth = mapWidth;
            this.MapHeight = mapHeight;
            this.CombatMap = new CombatMap(mapWidth, mapHeight);
            this.SpaceObjects = new SpaceObject[mapWidth * mapHeight];

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
            return this.CombatMap.AreNeighbors(spaceObject.ObjectCoordinates, destination);
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
            return this.SpaceObjects[this.GetObjectIndexByOffsetCoordinates(column, row)];
        }
        
        public void SetObjectAtOffsetCoordinates(SpaceObject spaceObject)
        {
            // TODO: rename before using - no coordinates in parameters
            this.SetObjectAtOffsetCoordinates(spaceObject, spaceObject.ObjectCoordinates.Column, spaceObject.ObjectCoordinates.Row);
        }

        public void SetObjectAtOffsetCoordinates(SpaceObject spaceObject, int column, int row)
        {
            this.SpaceObjects[this.GetObjectIndexByOffsetCoordinates(column, row)] = spaceObject;
        }

        private int GetObjectIndexByOffsetCoordinates(int column, int row)
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

            var newMeteor = new Meteor(meteorCoordinates, meteorHealth, meteorDmg, movementDirection);
            this.SetObjectAtOffsetCoordinates(newMeteor, meteorCoordinates.Column, meteorCoordinates.Row);
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
            ObjectAnimated?.Invoke(this, new AnimationEventArgs((SpaceObject)spaceObject.Clone(), spaceObject, this.CombatMap.HexToPixel(spaceObject.ObjectCoordinates), this.CombatMap.HexToPixel(destination)));
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

            this.SpaceObjects[this.GetObjectIndexByOffsetCoordinates(spaceObject.ObjectCoordinates.Column, spaceObject.ObjectCoordinates.Row)] = null;
            this.SpaceObjects[this.GetObjectIndexByOffsetCoordinates(destination.Column, destination.Row)] = spaceObject;
            spaceObject.ObjectCoordinates = destination;
        }

        public void AttackObject(SpaceObject attacker, SpaceObject victim)
        {
            var attackerShip = attacker as Ship;
            if (attackerShip != null)
            {
                var attackSprites = attackerShip.EquippedWeapon.GetAttackSprites(
                    this.CombatMap.HexToPixel(attackerShip.ObjectCoordinates),
                    this.CombatMap.HexToPixel(victim.ObjectCoordinates));
                SoundPlayed?.Invoke(this, new SoundEventArgs(attackerShip.EquippedWeapon.AttackSound));
                ObjectAnimated?.Invoke(this, new AnimationEventArgs((SpaceObject)attacker.Clone(), attacker, attackSprites));
                this.DealDamage(victim, attackerShip.AttackDamage);
                attackerShip.ActionsLeft -= attackerShip.EquippedWeapon.EnergyСonsumption;
            }
        }

        public void DealDamage(SpaceObject victim, int damageAmount)
        {
            victim.CurrentHealth -= damageAmount;
            if (victim.CurrentHealth <= 0)
            {
                this.DeleteObject(victim);
            }
        }

        public void RotateObject(SpaceObject spaceObject, double angle)
        {
            ObjectAnimated?.Invoke(this, new AnimationEventArgs((SpaceObject)spaceObject.Clone(), spaceObject, angle));
            spaceObject.Rotate(angle);
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
            this.SetObjectAtOffsetCoordinates(newShip, newShip.ObjectCoordinates.Column, newShip.ObjectCoordinates.Row);
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
