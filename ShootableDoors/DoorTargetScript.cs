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
using UnityEngine;

namespace Mistaken.ShootableDoors
{
    internal class DoorTargetScript : MonoBehaviour, IDestructible
    {
        public uint NetworkId => this.door.netId;

        public Vector3 CenterOfMass => Vector3.zero;

        public bool Damage(float damage, IDamageDealer src, Footprint attackerFootprint, Vector3 exactHitPos)
        {
            /*            var player = Player.Get(attackerFootprint.Hub);
                        if (Exiled.CustomItems.API.Features.CustomItem.TryGet(player.CurrentItem, out _))
                            return false;
            */
            damage = BodyArmorUtils.ProcessDamage(this.ArmorResistance, damage, Mathf.RoundToInt(src.ArmorPenetration * 100f));

            this.Door.ServerDamage(damage, DoorDamageType.Weapon);
            Log.Debug($"[DOOR] {attackerFootprint.LoggedHubName} done {damage} damage to doors, {this.door._remainingHealth} left", PluginHandler.Instance.Config.VerbouseOutput);
            return true;
        }

        internal int ArmorResistance { get; set; } = 100;

        internal BreakableDoor Door
        {
            get => this.door;
            set
            {
                this.door = value;
                this.door._maxHealth = 1000;
                this.door._remainingHealth = this.door._maxHealth;
            }
        }

        private BreakableDoor door;
    }
}
