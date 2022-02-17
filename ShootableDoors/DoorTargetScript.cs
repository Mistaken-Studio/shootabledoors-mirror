// -----------------------------------------------------------------------
// <copyright file="DoorTargetScript.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using Footprinting;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using PlayerStatsSystem;
using UnityEngine;

namespace Mistaken.ShootableDoors
{
    public class DoorTargetScript : MonoBehaviour, IDestructible
    {
        public uint NetworkId => this.door.netId;

        public Vector3 CenterOfMass => Vector3.zero;

        public bool Damage(float damage, DamageHandlerBase handler, Vector3 exactHitPos)
        {
            if (!(handler is FirearmDamageHandler firearmHandler))
                return false;

            damage = BodyArmorUtils.ProcessDamage(this.ArmorResistance, damage / 10, Mathf.RoundToInt(firearmHandler._penetration * 100f));

            this.Door.ServerDamage(damage, DoorDamageType.Weapon);
            Log.Debug($"[DOOR] {firearmHandler.Attacker.Nickname} done {damage} damage to doors, {this.door._remainingHealth} left", PluginHandler.Instance.Config.VerbouseOutput);
            return true;
        }

        internal int ArmorResistance { get; set; } = 100;

        internal BreakableDoor Door
        {
            get => this.door;
            set
            {
                this.door = value;
                this.door._maxHealth = 100;
                this.door._remainingHealth = this.door._maxHealth;
            }
        }

        private BreakableDoor door;
    }
}
