﻿// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;

namespace Mistaken.ShootableDoors
{
    /// <inheritdoc/>
    internal class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "ShootableDoors";

        /// <inheritdoc/>
        public override string Prefix => "MShotableDoors";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Default;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(5, 0, 0);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            var harmony = new Harmony("mistaken.shootabledoors");
            harmony.PatchAll();

            new DoorHandler(this);

            if (this.Config.ShotgunDoorDamageMultiplayer < 0 || this.Config.WeaponDoorDamageMultiplayer < 0)
            {
                Log.Warn("ShotgunDoorDamageMultiplayer or WeaponDoorDamageMultiplayer set to a negative value!");
                this.Config.ShotgunDoorDamageMultiplayer = 0;
                this.Config.WeaponDoorDamageMultiplayer = 0;
            }

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }
    }
}
