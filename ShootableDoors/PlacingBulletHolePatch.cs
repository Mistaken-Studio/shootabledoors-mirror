// -----------------------------------------------------------------------
// <copyright file="PlacingBulletHolePatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace Mistaken.ShootableDoors
{
    [HarmonyPatch(typeof(InventorySystem.Items.Firearms.Modules.StandardHitregBase), nameof(InventorySystem.Items.Firearms.Modules.StandardHitregBase.PlaceBullethole), typeof(Ray), typeof(RaycastHit))]
    internal static class PlacingBulletHolePatch
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        public static bool Prefix(InventorySystem.Items.Firearms.Modules.StandardHitregBase __instance, Ray ray, RaycastHit hit)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            var player = Player.Get(__instance.Hub);
            if (Exiled.CustomItems.API.Features.CustomItem.TryGet(player.CurrentItem, out _))
                return true;

            if (!DoorHandler.Doors.TryGetValue(hit.collider.gameObject, out var door) || door == null)
                return true;

            if (!DoorHandler.DoorsPenetration.TryGetValue(door, out var penetration))
                return true;

            float damage = (__instance.BaseStats.DamageAtDistance(__instance.Firearm, hit.distance) / (__instance is BuckshotHitreg buckshot ? buckshot._buckshotSettings.MaxHits : 1f)) * 0.1f;
            damage = InventorySystem.Items.Armor.BodyArmorUtils.ProcessDamage(penetration, damage, Mathf.RoundToInt(__instance.Firearm.ArmorPenetration * 100f));
            door.DamageDoor(damage, DoorDamageType.Weapon);
            __instance.Conn.Send(new InventorySystem.Items.Firearms.BasicMessages.RequestMessage(0, InventorySystem.Items.Firearms.BasicMessages.RequestType.Hitmarker), 0);
            return true;
        }
    }
}
