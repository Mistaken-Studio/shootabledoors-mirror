// -----------------------------------------------------------------------
// <copyright file="PlacingBulletHolePatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace Mistaken.ShootableDoors
{
    [HarmonyPatch(typeof(SingleBulletHitreg), nameof(SingleBulletHitreg.ServerPerformShot))]
    internal static class PlacingBulletHolePatch
    {
        public static bool Prefix(SingleBulletHitreg __instance, Ray ray)
        {
            FirearmBaseStats baseStats = __instance.Firearm.BaseStats;
            Vector3 a = (new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) - Vector3.one / 2f).normalized * UnityEngine.Random.value;
            float num = baseStats.GetInaccuracy(__instance.Firearm, __instance.Firearm.AdsModule.ServerAds, __instance.Hub.playerMovementSync.PlayerVelocity.magnitude, __instance.Hub.playerMovementSync.Grounded);
            if (__instance._usesRecoilPattern)
            {
                __instance._recoilPattern.ApplyShot(1f / __instance.Firearm.ActionModule.CyclicRate);
                num += __instance._recoilPattern.GetInaccuracy();
            }

            ray.direction = Quaternion.Euler(num * a) * ray.direction;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, baseStats.MaxDistance(), StandardHitregBase.HitregMask))
            {
                global::IDestructible destructible;
                if (hit.collider.TryGetComponent<global::IDestructible>(out destructible))
                {
                    __instance.RestorePlayerPosition();
                    float damage = baseStats.DamageAtDistance(__instance.Firearm, hit.distance);
                    if (destructible.Damage(damage, __instance.Firearm, __instance.Firearm.Footprint, hit.point))
                    {
                        global::Hitmarker.SendHitmarker(__instance.Conn, 1f);
                        __instance.ShowHitIndicator(destructible.NetworkId, damage, ray.origin);
                    }
                }
                else
                {
                    __instance.PlaceBullethole(ray, hit);
                }
            }

            return false;
        }
    }
}
