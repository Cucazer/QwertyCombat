using System;
using System.Collections.Generic;
using Eto.Drawing;
using Barbar.HexGrid;

namespace QwertyCombat.Objects
{
    public enum ObjectType
    {
        Ship,
        Meteor
    }

    public abstract class SpaceObject: ICloneable
    {
        //TODO: implement overridable name, use as tooltip caption
        //public abstract static string Name;

        public readonly int MaxActions;

        public readonly int MaxHealth;

        // TODO: is this field really needed?
        public readonly ObjectType objectType;

        public readonly Player Owner;

        public List<DrawableShape> ObjectAppearance;

        protected SpaceObject(Player owner, int maxHealth, ObjectType objectType, int maxActions)
        {
            this.Owner = owner;
            this.MaxHealth = maxHealth;
            this.CurrentHealth = maxHealth;
            this.objectType = objectType;
            this.MaxActions = maxActions;
            this.ActionsLeft = maxActions;
        }

        public int ActionsLeft { get; set; }
        public int CurrentHealth { get; set; }
        public OffsetCoordinates ObjectCoordinates { get; set; }
        public abstract Dictionary<string, string> Properties { get; }
        public bool IsMoving { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public abstract void Rotate(double angle);
    }
}