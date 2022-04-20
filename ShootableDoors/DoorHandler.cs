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
            Instance = this;
        }

        /// <inheritdoc/>
        public override string Name => "Door";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Events.Handlers.CustomEvents.GeneratedCache += this.CustomEvents_GeneratedCache;

            foreach (var item in GameObject.FindObjectsOfType<ButtonTargetScript>())
                item.enabled = true;

            foreach (var item in GameObject.FindObjectsOfType<DoorTargetScript>())
                item.enabled = true;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Events.Handlers.CustomEvents.GeneratedCache -= this.CustomEvents_GeneratedCache;

            foreach (var item in GameObject.FindObjectsOfType<ButtonTargetScript>())
                item.enabled = false;

            foreach (var item in GameObject.FindObjectsOfType<DoorTargetScript>())
                item.enabled = false;
        }

        internal static DoorHandler Instance { get; private set; }

        private void CustomEvents_GeneratedCache()
        {
            this.Log.Debug("[DOOR] Starting", PluginHandler.Instance.Config.VerbouseOutput);
            if (Door.List == null)
            {
                this.Log.Warn("[DOOR] Doors not found");
                return;
            }

            HashSet<DoorVariant> toIgnore = new HashSet<DoorVariant>();
            if (!Door.List.Any(d => d.Type == DoorType.CheckpointEntrance))
                this.Log.Warn("[DOOR] CheckpointEZ door not found");
            else
            {
                var checkpointEZ = Door.List.First(d => d.Type == DoorType.CheckpointEntrance).Base as CheckpointDoor;
                foreach (var door in checkpointEZ._subDoors)
                    toIgnore.Add(door);
            }

            this.Log.Debug("[DOOR] CheckpointEZ Done", PluginHandler.Instance.Config.VerbouseOutput);
            if (!Door.List.Any(d => d.Type == DoorType.CheckpointLczA))
                this.Log.Warn("[DOOR] CheckpointA door not found");
            else
            {
                var checkpointLCZ_A = Door.List.First(d => d.Type == DoorType.CheckpointLczA).Base as CheckpointDoor;
                foreach (var door in checkpointLCZ_A._subDoors)
                    toIgnore.Add(door);
            }

            this.Log.Debug("[DOOR] CheckpointA Done", PluginHandler.Instance.Config.VerbouseOutput);
            if (!Door.List.Any(d => d.Type == DoorType.CheckpointLczB))
                this.Log.Warn("[DOOR] CheckpointB door not found");
            else
            {
                var checkpointLCZ_B = Door.List.First(d => d.Type == DoorType.CheckpointLczB).Base as CheckpointDoor;

                foreach (var door in checkpointLCZ_B._subDoors)
                    toIgnore.Add(door);
            }

            this.Log.Debug("[DOOR] CheckpointB Done", PluginHandler.Instance.Config.VerbouseOutput);

            foreach (var door in Door.List.Where(d => d.Type == DoorType.Scp106Primary || d.Type == DoorType.Scp106Secondary || d.Type == DoorType.Scp106Bottom).Select(d => (d.Base as CheckpointDoor)._subDoors[0]))
                toIgnore.Add(door);

            this.Log.Debug("[DOOR] 106 Done", PluginHandler.Instance.Config.VerbouseOutput);
            try
            {
                this.Log.Debug("[DOOR] Starting doors", PluginHandler.Instance.Config.VerbouseOutput);
                foreach (var door in Door.List)
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

                    foreach (var doorButton in door.Base.gameObject.GetComponentsInChildren<RegularDoorButton>())
                    {
                        var buttonCollider = doorButton.GetComponent<BoxCollider>();
                        if (buttonCollider == null)
                            buttonCollider = doorButton.gameObject.AddComponent<BoxCollider>();

                        var tmp = new GameObject("button_trigger", typeof(ButtonTargetScript), typeof(BoxCollider));
                        tmp.transform.parent = doorButton.gameObject.transform;
                        tmp.layer = LayerMask.NameToLayer("Hitbox");
                        if (door.Type == Exiled.API.Enums.DoorType.HeavyContainmentDoor)
                            tmp.transform.localPosition = Vector3.zero;
                        else
                            tmp.transform.localPosition = Vector3.forward * 0.15f;
                        tmp.transform.localRotation = Quaternion.identity;
                        tmp.transform.localScale = Vector3.one * 0.9f;
                        var collider = tmp.GetComponent<BoxCollider>();
                        collider.size = buttonCollider.size;
                        var script = tmp.GetComponent<ButtonTargetScript>();
                        script.Door = door.Base;
                    }
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
