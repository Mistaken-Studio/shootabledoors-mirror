// -----------------------------------------------------------------------
// <copyright file="DoorHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Mistaken.API.Diagnostics;
using UnityEngine;

namespace Mistaken.ShootableDoors
{
    /// <inheritdoc/>
    internal class DoorHandler : Module
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
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Server_WaitingForPlayers;
        }

        private void Server_WaitingForPlayers()
        {
            this.Log.Debug("[DOOR] Starting", PluginHandler.Instance.Config.VerbouseOutput);
            if (Map.Doors == null)
            {
                this.Log.Warn("[DOOR] Doors not found");
                return;
            }

            HashSet<DoorVariant> toIgnore = new HashSet<DoorVariant>();
            if (!Map.Doors.Any(d => d.Type == DoorType.CheckpointEntrance))
                this.Log.Warn("[DOOR] CheckpointEZ door not found");
            else
            {
                var checkpointEZ = Map.Doors.First(d => d.Type == DoorType.CheckpointEntrance).Base as CheckpointDoor;
                foreach (var door in checkpointEZ._subDoors)
                    toIgnore.Add(door);
            }

            this.Log.Debug("[DOOR] CheckpointEZ Done", PluginHandler.Instance.Config.VerbouseOutput);
            if (!Map.Doors.Any(d => d.Type == DoorType.CheckpointLczA))
                this.Log.Warn("[DOOR] CheckpointA door not found");
            else
            {
                var checkpointLCZ_A = Map.Doors.First(d => d.Type == DoorType.CheckpointLczA).Base as CheckpointDoor;
                foreach (var door in checkpointLCZ_A._subDoors)
                    toIgnore.Add(door);
            }

            this.Log.Debug("[DOOR] CheckpointA Done", PluginHandler.Instance.Config.VerbouseOutput);
            if (!Map.Doors.Any(d => d.Type == DoorType.CheckpointLczB))
                this.Log.Warn("[DOOR] CheckpointB door not found");
            else
            {
                var checkpointLCZ_B = Map.Doors.First(d => d.Type == DoorType.CheckpointLczB).Base as CheckpointDoor;

                foreach (var door in checkpointLCZ_B._subDoors)
                    toIgnore.Add(door);
            }

            this.Log.Debug("[DOOR] CheckpointB Done", PluginHandler.Instance.Config.VerbouseOutput);

            foreach (var door in Map.Doors.Where(d => d.Type == DoorType.Scp106Primary || d.Type == DoorType.Scp106Secondary || d.Type == DoorType.Scp106Bottom).Select(d => (d.Base as CheckpointDoor)._subDoors[0]))
                toIgnore.Add(door);

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

                    DoorType GetDoorType()
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
                    }

                    int armorResistance;

                    switch (GetDoorType())
                    {
                        case DoorType.HeavyContainmentDoor:
                            armorResistance = 150;
                            break;
                        case DoorType.EntranceDoor:
                        case DoorType.LightContainmentDoor:
                            armorResistance = 120;
                            break;
                        case DoorType.PrisonDoor:
                            armorResistance = 100;
                            break;
                        default:
                            door.IgnoredDamageTypes |= DoorDamageType.Weapon;
                            this.Log.Debug($"[DOOR] Skipped {door.Type}|{door.Nametag}|{door.Base.name}", PluginHandler.Instance.Config.VerbouseOutput);
                            continue;
                    }

                    HashSet<GameObject> obj = new HashSet<GameObject>();
                    foreach (var item in door.Base.gameObject.GetComponentsInChildren<Collider>())
                        obj.Add(item.gameObject);

                    foreach (var item in obj)
                    {
                        try
                        {
                            var doorScript = item.AddComponent<DoorTargetScript>();
                            doorScript.ArmorResistance = armorResistance;
                            doorScript.Door = door.Base as BreakableDoor;
                        }
                        catch (System.Exception ex)
                        {
                            this.Log.Error(ex);
                        }
                    }

                    door.IgnoredDamageTypes = DoorDamageType.None;
                }

                this.Log.Debug("[DOOR] Doors done", PluginHandler.Instance.Config.VerbouseOutput);
            }
            catch (System.Exception ex)
            {
                this.Log.Error("[DOOR] Failed to set door health");
                this.Log.Error(ex.Message);
                this.Log.Error(ex.StackTrace);
            }
        }
    }
}
