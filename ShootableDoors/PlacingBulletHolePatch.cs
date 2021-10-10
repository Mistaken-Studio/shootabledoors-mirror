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
#pragma warning disable IDE0060 // Usuń nieużywany parametr
        public static bool Prefix(InventorySystem.Items.Firearms.Modules.StandardHitregBase __instance, Ray ray, RaycastHit hit)
#pragma warning restore IDE0060 // Usuń nieużywany parametr
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            var player = Player.Get(__instance.Hub);
            if (Exiled.CustomItems.API.Features.CustomItem.TryGet(player.CurrentItem, out _))
                return true;

            if (!DoorHandler.Doors.TryGetValue(hit.collider.gameObject, out var door) || door == null)
                return true;

            if (!DoorHandler.DoorsPenetration.TryGetValue(door, out var penetration))
                return true;

            float damage = (__instance.Firearm.BaseStats.DamageAtDistance(__instance.Firearm, hit.distance) / (__instance is BuckshotHitreg buckshot ? buckshot._buckshotSettings.MaxHits : 1f)) * 0.1f;
            damage = InventorySystem.Items.Armor.BodyArmorUtils.ProcessDamage(penetration, damage, Mathf.RoundToInt(__instance.Firearm.ArmorPenetration * 100f));
            door.DamageDoor(damage, DoorDamageType.Weapon);
            Hitmarker.SendHitmarker(__instance.Conn, 1f);
            return true;
        }
    }
}
