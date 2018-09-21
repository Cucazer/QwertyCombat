using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Barbar.HexGrid;
using qwerty.Objects;
using Point = System.Drawing.Point;

namespace qwerty
{
    class GameLogic
    {
        private Ship activeShip
        {
            get { return this.objectManager.ActiveShip; }
            set { this.objectManager.ActiveShip = value; }
        }

        public string ActiveShipDescription => this.activeShip?.Description ?? "";
        public string ActivePlayerDescription
        {
            get
            {
                switch (this.activePlayer)
                {
                    case Player.FirstPlayer:
                        return "First player";
                    case Player.SecondPlayer:
                        return "Second player";
                    default:
                        return this.activePlayer.ToString();
                }
            }
        }

        public readonly ObjectManager objectManager;
        private Player activePlayer = Player.FirstPlayer;

        public int FirstPlayerShipCount => this.objectManager.FirstPlayerShipCount;
        public int SecondPlayerShipCount=> this.objectManager.SecondPlayerShipCount;
        public int BitmapWidth => this.objectManager.BitmapWidth;
        public int BitmapHeight => this.objectManager.BitmapHeight;

        public GameLogic(int fieldWidth, int fieldHeight)
        {
            this.objectManager = new ObjectManager(fieldWidth, fieldHeight);
        }

        public void HandleFieldClick(Point clickLocation)
        {
            OffsetCoordinates clickedHexagon;
            SpaceObject clickedObject;
            try
            {
                clickedHexagon = this.objectManager.PixelToOffsetCoordinates(clickLocation);
                clickedObject = this.objectManager.PixelToSpaceObject(clickLocation);
            }
            catch (ArgumentOutOfRangeException)
            {
                // clicked pixel outside game field
                return;
            }

            if (this.activeShip == null)
            {
                // Nothing active and nothing to be activated
                if (clickedObject == null) return;

                if (this.activePlayer == clickedObject.Owner)
                {
                    this.activeShip = (Ship) clickedObject;
                }
                return;
            }
            
            // если выбранная клетка пуста - определяем возможность перемещения 
            if (clickedObject == null)
            {
                this.MoveActiveShip(clickedHexagon);
                return;
            }
            
            if (clickedObject.Owner == this.activePlayer)
            {
                this.activeShip = (Ship) clickedObject;
            }
            else
            {
                this.ActiveShipAttack(clickedObject);
            }
        }

        private void ActiveShipAttack(SpaceObject enemyObject)
        {
            if (this.activeShip.EquippedWeapon.AttackRange < this.objectManager.GetDistance(this.activeShip, enemyObject) || this.activeShip.ActionsLeft < this.activeShip.EquippedWeapon.EnergyСonsumption)
            {
                // another object is out of range or requires more energy than is left
                return;
            }
            
            var rotateAngle = this.objectManager.GetRelativeHexagonAngle(this.activeShip, enemyObject.ObjectCoordinates);
            this.objectManager.RotateObject(this.activeShip, rotateAngle);

            this.objectManager.AttackObject(this.activeShip, enemyObject);

            this.objectManager.RotateObject(this.activeShip, -rotateAngle);

            if (this.activeShip.ActionsLeft == 0)
            {
                this.activeShip = null;
            }
        }
        
        private void MoveActiveShip(OffsetCoordinates clickedHexagon)
        {
            if (this.activeShip.ActionsLeft <= 0 || !this.objectManager.CanMoveObjectTo(this.activeShip, clickedHexagon)) return;

            var rotateAngle = this.objectManager.GetRelativeHexagonAngle(this.activeShip, clickedHexagon);
            this.objectManager.RotateObject(this.activeShip, rotateAngle);

            this.objectManager.MoveObjectTo(this.activeShip, clickedHexagon);
            this.activeShip.ActionsLeft--;

            this.objectManager.RotateObject(this.activeShip, -rotateAngle);

            if (this.activeShip.ActionsLeft == 0)
            {
                this.activeShip = null;
            }
        }
        
        private void MoveMeteors()
        {
            foreach (var meteor in this.objectManager.Meteors)
            {
                var meteorNextStepCoordinates = this.objectManager.GetMeteorNextStepCoordinates(meteor);
                var objectOnTheWay = this.objectManager.GetObjectByOffsetCoordinates(meteorNextStepCoordinates.Column, meteorNextStepCoordinates.Row);

                if (objectOnTheWay == null)
                {
                    this.objectManager.MoveObjectTo(meteor, meteorNextStepCoordinates);
                    continue;
                }

                this.objectManager.MoveObjectTo(meteor, meteorNextStepCoordinates, true);
                this.objectManager.DealDamage(objectOnTheWay, meteor.CollisionDamage);
                this.objectManager.DeleteObject(meteor);
            }

            if (new Random().Next(0, 100) <= ObjectManager.MeteorAppearanceChance)
            {
                this.objectManager.CreateMeteor();
            }
        }

        public void EndTurn()
        {
            this.activePlayer = this.activePlayer == Player.FirstPlayer ? Player.SecondPlayer : Player.FirstPlayer;
            this.activeShip = null;
            this.MoveMeteors();
            this.objectManager.EndTurn();
        }
    }
}