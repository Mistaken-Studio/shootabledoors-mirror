﻿// -----------------------------------------------------------------------
// <copyright file="DoorHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mistaken.API.Diagnostics;
using UnityEngine;

namespace Mistaken.ShootableDoors
{
    /// <inheritdoc/>
    public class DoorHandler : Module
    {
        /// <inheritdoc cref="Module.Module(Exiled.API.Interfaces.IPlugin{Exiled.API.Interfaces.IConfig})"/>
        public DoorHandler(PluginHandler p)
            : base(p)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Door";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => this.Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => this.Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => this.Server_RoundStarted(), "RoundStart");
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => this.Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => this.Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => this.Server_RoundStarted(), "RoundStart");
        }

        internal static readonly Dictionary<GameObject, Door> Doors = new Dictionary<GameObject, Door>();
        internal static readonly Dictionary<Door, int> DoorsPenetration = new Dictionary<Door, int>();

        private Door gateA;
        private bool ignoreGateA;
        private Door gateB;
        private bool ignoreGateB;

        private void Server_RoundStarted()
        {
            this.gateA = Map.Doors.First(d => d.Type == DoorType.GateA);
            this.gateB = Map.Doors.First(d => d.Type == DoorType.GateB);
            this.RunCoroutine(this.DoRoundLoop(), "RoundLoop");
        }

        private IEnumerator<float> DoRoundLoop()
        {
            yield return Timing.WaitForSeconds(1);
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(1);
                if (!this.ignoreGateA && this.gateA.IsOpen && !this.gateA.IsLocked)
                    this.gateA.IsOpen = false;
                if (!this.ignoreGateB && this.gateB.IsOpen && !this.gateB.IsLocked)
                    this.gateB.IsOpen = false;
            }
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            var type = ev.Door.Type;
            if (type == DoorType.EscapePrimary)
                Map.Doors.First(d => d.Type == DoorType.EscapeSecondary).IsOpen = ev.Door.IsOpen;
            else if (type == DoorType.EscapeSecondary)
                Map.Doors.First(d => d.Type == DoorType.EscapePrimary).IsOpen = ev.Door.IsOpen;
            else if ((type == DoorType.GateA || type == DoorType.GateB) && !ev.Door.IsOpen)
            {
                if (type == DoorType.GateA)
                    this.ignoreGateA = true;
                else
                    this.ignoreGateB = true;
                ev.Door.ChangeLock(ev.Door.DoorLockType | DoorLockType.SpecialDoorFeature);
                this.CallDelayed(
                    15f,
                    () =>
                    {
                        ev.Door.ChangeLock(ev.Door.DoorLockType & ~DoorLockType.SpecialDoorFeature);
                        if (type == DoorType.GateA)
                            this.ignoreGateA = false;
                        else
                            this.ignoreGateB = false;
                    },
                    "Closing Gate");
            }
        }

        private void Server_WaitingForPlayers()
        {
            this.Log.Debug("[DOOR] Starting", PluginHandler.Instance.Config.VerbouseOutput);
            if (Map.Doors == null)
            {
                this.Log.Warn("[DOOR] Doors not found");
                return;
            }

            if (!Map.Doors.Any(d => d.Type == DoorType.HID))
                this.Log.Warn("[DOOR] HID door not found");
            else
                Map.Doors.First(d => d.Type == DoorType.HID).IgnoredDamageTypes |= DoorDamageType.Grenade;
            this.Log.Debug("[DOOR] HID Done", PluginHandler.Instance.Config.VerbouseOutput);
            HashSet<DoorVariant> toIgnore = new HashSet<DoorVariant>();
            if (!Map.Doors.Any(d => d.Type == DoorType.CheckpointEntrance))
                this.Log.Warn("[DOOR] CheckpointEZ door not found");
            else
            {
                var checkpointEZ = Map.Doors.First(d => d.Type == DoorType.CheckpointEntrance).Base as CheckpointDoor;
                foreach (var door in checkpointEZ._subDoors)
                {
                    toIgnore.Add(door);
                    if (!(door is BreakableDoor damageableDoor))
                        continue;
                    damageableDoor._maxHealth = 2000;
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    damageableDoor._ignoredDamageSources = DoorDamageType.Weapon | DoorDamageType.Grenade;
                }
            }

            this.Log.Debug("[DOOR] CheckpointEZ Done", PluginHandler.Instance.Config.VerbouseOutput);
            if (!Map.Doors.Any(d => d.Type == DoorType.CheckpointLczA))
                this.Log.Warn("[DOOR] CheckpointA door not found");
            else
            {
                var checkpointLCZ_A = Map.Doors.First(d => d.Type == DoorType.CheckpointLczA).Base as CheckpointDoor;
                foreach (var door in checkpointLCZ_A._subDoors)
                {
                    toIgnore.Add(door);
                    if (!(door is BreakableDoor damageableDoor))
                        continue;
                    damageableDoor._maxHealth = 1000;
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    damageableDoor._ignoredDamageSources = DoorDamageType.Weapon | DoorDamageType.Grenade;
                }
            }

            this.Log.Debug("[DOOR] CheckpointA Done", PluginHandler.Instance.Config.VerbouseOutput);
            if (!Map.Doors.Any(d => d.Type == DoorType.CheckpointLczB))
                this.Log.Warn("[DOOR] CheckpointB door not found");
            else
            {
                var checkpointLCZ_B = Map.Doors.First(d => d.Type == DoorType.CheckpointLczB).Base as CheckpointDoor;

                foreach (var door in checkpointLCZ_B._subDoors)
                {
                    toIgnore.Add(door);
                    if (!(door is BreakableDoor damageableDoor))
                        continue;
                    damageableDoor._maxHealth = 1000;
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    damageableDoor._ignoredDamageSources = DoorDamageType.Weapon | DoorDamageType.Grenade;
                }
            }

            this.Log.Debug("[DOOR] CheckpointB Done", PluginHandler.Instance.Config.VerbouseOutput);

            foreach (var door in Map.Doors.Where(d => d.Type == DoorType.Scp106Primary || d.Type == DoorType.Scp106Secondary || d.Type == DoorType.Scp106Bottom).Select(d => (d.Base as CheckpointDoor)._subDoors[0]))
            {
                toIgnore.Add(door);
                if (door is BreakableDoor d)
                    d._ignoredDamageSources |= DoorDamageType.Weapon | DoorDamageType.Grenade;
            }

            this.Log.Debug("[DOOR] 106 Done", PluginHandler.Instance.Config.VerbouseOutput);
            try
            {
                this.Log.Debug("[DOOR] Starting doors", PluginHandler.Instance.Config.VerbouseOutput);
                foreach (var door in Map.Doors)
                {
                    if (!door.IsBreakable)
                        continue;
                    if (toIgnore.Contains(door.Base))
                    {
                        this.Log.Debug("[DOOR] Skipped " + door.Type, PluginHandler.Instance.Config.VerbouseOutput);
                        continue;
                    }

                    // Log.Debug("Checking " + door.name);
                    Func<DoorType> type = () =>
                    {
                        switch (door.Base.name.RemoveBracketsOnEndOfName())
                        {
                            case "Prison BreakableDoor":
                                return DoorType.PrisonDoor;
                            case "ESCAPE_PRIMARY":
                                return DoorType.EntranceDoor;
                            case "ESCAPE_SECONDARY":
                                return DoorType.EntranceDoor;
                            case "INTERCOM":
                                return DoorType.EntranceDoor;
                            case "NUKE_ARMORY":
                                return DoorType.HeavyContainmentDoor;
                            case "LCZ_ARMORY":
                                return DoorType.LightContainmentDoor;
                            case "012":
                                return DoorType.HeavyContainmentDoor;
                            case "HCZ_ARMORY":
                                return DoorType.HeavyContainmentDoor;
                            case "096":
                                return DoorType.HeavyContainmentDoor;
                            case "049_ARMORY":
                                return DoorType.HeavyContainmentDoor;
                            case "012_LOCKER":
                                return DoorType.LightContainmentDoor;
                            case "SERVERS_BOTTOM":
                                return DoorType.HeavyContainmentDoor;
                            case "173_CONNECTOR":
                                return DoorType.LightContainmentDoor;
                            case "LCZ_WC":
                                return DoorType.LightContainmentDoor;
                            case "HID_RIGHT":
                                return DoorType.HeavyContainmentDoor;
                            case "012_BOTTOM":
                                return DoorType.LightContainmentDoor;
                            case "HID_LEFT":
                                return DoorType.HeavyContainmentDoor;
                            case "173_ARMORY":
                                return DoorType.LightContainmentDoor;
                            case "LCZ_CAFE":
                                return DoorType.LightContainmentDoor;
                            case "173_BOTTOM":
                                return DoorType.LightContainmentDoor;
                            case "LightContainmentDoor":
                                return DoorType.LightContainmentDoor;
                            case "EntrDoor":
                                return DoorType.EntranceDoor;
                            default:
                                switch (door.Base.name.GetBefore(' '))
                                {
                                    case "LCZ":
                                        return DoorType.LightContainmentDoor;
                                    case "HCZ":
                                        return DoorType.HeavyContainmentDoor;
                                    case "EZ":
                                        return DoorType.EntranceDoor;
                                    default:
                                        {
                                            switch (door.Type) 
                                            {
                                                case DoorType.EscapeSecondary:
                                                case DoorType.EscapePrimary:
                                                    return DoorType.EntranceDoor;
                                                case DoorType.Intercom:
                                                    return DoorType.EntranceDoor;
                                                case DoorType.NukeArmory:
                                                    return DoorType.HeavyContainmentDoor;
                                                case DoorType.LczArmory:
                                                    return DoorType.LightContainmentDoor;
                                                case DoorType.Scp012:
                                                    return DoorType.HeavyContainmentDoor;
                                                case DoorType.HczArmory:
                                                    return DoorType.HeavyContainmentDoor;
                                                case DoorType.Scp096:
                                                    return DoorType.HeavyContainmentDoor;
                                                case DoorType.Scp049Armory:
                                                    return DoorType.HeavyContainmentDoor;
                                                case DoorType.Scp012Locker:
                                                    return DoorType.LightContainmentDoor;
                                                case DoorType.ServersBottom:
                                                    return DoorType.HeavyContainmentDoor;
                                                case DoorType.Scp173Connector:
                                                    return DoorType.LightContainmentDoor;
                                                case DoorType.LczWc:
                                                    return DoorType.LightContainmentDoor;
                                                case DoorType.HIDLeft:
                                                case DoorType.HIDRight:
                                                    return DoorType.HeavyContainmentDoor;
                                                case DoorType.Scp012Bottom:
                                                    return DoorType.LightContainmentDoor;
                                                case DoorType.Scp173Armory:
                                                    return DoorType.LightContainmentDoor;
                                                case DoorType.LczCafe:
                                                    return DoorType.LightContainmentDoor;
                                                case DoorType.Scp173Bottom:
                                                    return DoorType.LightContainmentDoor;
                                                default:
                                                    return DoorType.UnknownDoor;
                                            }
                                        }
                                }
                        }
                    };

                    switch (type())
                    {
                        case DoorType.HeavyContainmentDoor:
                            door.MaxHealth = 100;
                            DoorsPenetration[door] = 150;
                            break;
                        case DoorType.EntranceDoor:
                        case DoorType.LightContainmentDoor:
                            door.MaxHealth = 100;
                            DoorsPenetration[door] = 120;
                            break;
                        case DoorType.PrisonDoor:
                            door.MaxHealth = 100;
                            DoorsPenetration[door] = 100;
                            break;
                        default:
                            door.IgnoredDamageTypes |= DoorDamageType.Weapon;
                            this.Log.Debug($"[DOOR] Skipped {door.Type}|{door.Nametag}|{door.Base.name}", PluginHandler.Instance.Config.VerbouseOutput);
                            continue;
                    }

                    // Log.Debug("Updating " + door.name);
                    (door.Base as BreakableDoor)._remainingHealth = door.MaxHealth;
                    door.IgnoredDamageTypes = DoorDamageType.None;
                }

                this.Log.Debug("[DOOR] Doors done", PluginHandler.Instance.Config.VerbouseOutput);

                this.Log.Debug("[DOOR] Registering Doors", PluginHandler.Instance.Config.VerbouseOutput);
                foreach (var door in Map.Doors)
                {
                    if (!door.IsBreakable)
                        continue;
                    if ((door.IgnoredDamageTypes & DoorDamageType.Weapon) == (DoorDamageType)0)
                        this.RegisterDoors(door, door.Base.transform);
                }

                this.Log.Debug("[DOOR] Registered Doors", PluginHandler.Instance.Config.VerbouseOutput);
            }
            catch (System.Exception ex)
            {
                this.Log.Error("[DOOR] Failed to set door health");
                this.Log.Error(ex.Message);
                this.Log.Error(ex.StackTrace);
            }
        }

        private void RegisterDoors(Door door, Transform transform)
        {
            Log.Debug($"[REG DOOR] {door.Type}", PluginHandler.Instance.Config.VerbouseOutput);
            Doors[transform.gameObject] = door;
            for (int i = 0; i < transform.childCount; i++)
                this.RegisterDoors(door, transform.GetChild(i));
        }
    }
}
