﻿using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Threading;
using Barbar.HexGrid;
using QwertyCombat.Objects;
using Point = Eto.Drawing.Point;

namespace QwertyCombat
{
    class GameLogic
    {
        private GameState GameState => this.objectManager.GameState;

        private Ship activeShip
        {
            get => this.GameState.ActiveShip;
            set => this.GameState.ActiveShip = value;
        }

        public readonly ObjectManager objectManager;

        private Player activePlayer
        {
            get => this.objectManager.GameState.ActivePlayer;
            set => this.objectManager.GameState.ActivePlayer = value;
        }

        public int BitmapWidth => this.objectManager.BitmapWidth;
        public int BitmapHeight => this.objectManager.BitmapHeight;

        public GameLogic(int fieldWidth, int fieldHeight)
        {
            this.objectManager = new ObjectManager(fieldWidth, fieldHeight);
            this.activePlayer = Player.FirstPlayer;
        }

        public void HandleFieldClick(Point clickLocation)
        {
            if (this.GameState.GameOver)
            {
                return;
            }

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
            
            // cell is free to move to
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

        public Dictionary<string, string> HandleFieldHover(Point moveLocation)
        {
            SpaceObject hoveredObject;
            try
            {
                hoveredObject = this.objectManager.PixelToSpaceObject(moveLocation);
            }
            catch (ArgumentOutOfRangeException)
            {
                // clicked pixel outside game field
                return null;
            }

            return hoveredObject?.Properties ?? null;
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

            double currentAngle = 0;
            foreach (var nextHexagon in this.objectManager.GetHexagonPath(this.activeShip.ObjectCoordinates, clickedHexagon))
            {
                var nextStepAngle = Math.Round(this.objectManager.GetRelativeHexagonAngle(this.activeShip, nextHexagon), 2);
                var rotateAngle = nextStepAngle - currentAngle;
                currentAngle = nextStepAngle;
                if (rotateAngle != 0)
                {
                    this.objectManager.RotateObject(this.activeShip, rotateAngle);
                }

                this.objectManager.MoveObjectTo(this.activeShip, nextHexagon);
                this.activeShip.ActionsLeft--;
            }
            // restore initial facing direction
            this.objectManager.RotateObject(this.activeShip, -currentAngle);

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
                this.objectManager.DeleteObject(meteor);
                this.objectManager.DealDamage(objectOnTheWay, meteor.CollisionDamage);
            }

            if (new Random().Next(0, 100) <= ObjectManager.MeteorAppearanceChance)
            {
                this.objectManager.CreateMeteor();
            }
        }

        public void EndTurn()
        {
            if (this.GameState.GameOver)
            {
                return;
            }
            this.activePlayer = this.activePlayer == Player.FirstPlayer ? Player.SecondPlayer : Player.FirstPlayer;
            this.activeShip = null;
            this.MoveMeteors();
            this.objectManager.EndTurn();
        }
    }
}