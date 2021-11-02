// -----------------------------------------------------------------------
// <copyright file="ButtonTargetScript.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using Footprinting;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using UnityEngine;

namespace Mistaken.ShootableDoors
{
    internal class ButtonTargetScript : MonoBehaviour, IDestructible
    {
        public uint NetworkId => this.Door.netId;

        public Vector3 CenterOfMass => Vector3.zero;

        public bool Damage(float damage, IDamageDealer src, Footprint attackerFootprint, Vector3 exactHitPos)
        {
            if (!this.enabled)
                return false;

            if (this.Door.RequiredPermissions.RequiredPermissions != KeycardPermissions.None)
                return false;

            /*if (UnityEngine.Random.Range(0, 100) >= PluginHandler.Instance.Config.Chance)
                return false;

            if (this.Door.ActiveLocks != 0)
                return false;*/

            this.OnDamaged(damage / 10);
            Log.Debug(this.health);
            return true;
        }

        internal DoorVariant Door { get; set; }

        internal float RegenPerSecond { get; set; } = 25f;

        internal float DamagePercent { get; set; } = 0.75f;

        internal float BaseRegenCooldown { get; set; } = 15f;

        internal float MaxHealth
        {
            get => this.maxHealth;
            set
            {
                this.maxHealth = value;
                this.health = value;
            }
        }

        private float maxHealth = 500f;
        private float health = 500f;

        private float regenCooldown;
        private bool partialyDestroyed;

        private void OnDamaged(float damage)
        {
            this.regenCooldown = this.BaseRegenCooldown;

            this.health -= damage;

            if (this.health <= 0)
                this.OnDestroyed();
            else if (!this.partialyDestroyed && this.health < this.maxHealth * this.DamagePercent)
                this.OnPartialyDestroyed();
        }

        private void OnPartialyDestroyed()
        {
            if (this.Door.ActiveLocks != 0)
                return;
            this.partialyDestroyed = true;
            this.Door.ServerChangeLock(DoorLockReason.NoPower, true);
            this.Door.NetworkTargetState = true;
        }

        private void OnDestroyed()
        {
            this.Door.ServerChangeLock(DoorLockReason.NoPower | DoorLockReason.SpecialDoorFeature, true);
            this.Door.NetworkTargetState = true;
            this.enabled = false;
        }

        private void FixedUpdate()
        {
            if (this.regenCooldown == 0)
            {
                if (this.health == this.maxHealth)
                {
                    if (this.partialyDestroyed)
                    {
                        this.partialyDestroyed = false;
                        this.Door.ServerChangeLock(DoorLockReason.NoPower, false);
                    }

                    return;
                }

                this.health += this.RegenPerSecond * Time.fixedDeltaTime;

                if (this.health > this.maxHealth)
                    this.health = this.maxHealth;

                return;
            }

            this.regenCooldown -= Time.fixedDeltaTime;

            if (this.regenCooldown < 0)
                this.regenCooldown = 0;
        }
    }
}
