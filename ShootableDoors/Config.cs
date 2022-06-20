// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Mistaken.Updater.Config;

namespace Mistaken.ShootableDoors
{
    /// <inheritdoc/>
    internal class Config : IAutoUpdatableConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug should be displayed.
        /// </summary>
        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        /// <summary>
        /// Gets or sets door damage multiplayer for all weapons except shotgun.
        /// </summary>
        [Description("Door damage multiplayer for all weapons (except shotgun)")]
        public float WeaponDoorDamageMultiplayer { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets door damage multiplayer for shotgun.
        /// </summary>
        [Description("Door damage multiplayer for shotgun")]
        public float ShotgunDoorDamageMultiplayer { get; set; } = 0.2f;

        /// <inheritdoc/>
        [Description("Auto Update Settings")]
        public System.Collections.Generic.Dictionary<string, string> AutoUpdateConfig { get; set; }
    }
}
