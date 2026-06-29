using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Cloud.CloudCode.Extensions;

public static class CloudAssets
{
    private static PackedScene? _cloudScene;
    private static PackedScene? _iceScene;

    private const string CloudScenePath = "res://Cloud/scenes/cloud.tscn";
    private const string IceVfxPath = "res://Cloud/scenes/ice_vfx.tscn";

    public static PackedScene? CloudScene
    {
        get
        {
            _cloudScene = LoadOrReload(_cloudScene, CloudScenePath, "Cloud scene");
            return _cloudScene;
        }
    }

    public static PackedScene? IceScene
    {
        get
        {
            _iceScene = LoadOrReload(_iceScene, IceVfxPath, "Ice VFX");
            return _iceScene;
        }
    }

    private static PackedScene? LoadOrReload(PackedScene? cachedScene, string path, string label)
    {
        if (cachedScene != null && GodotObject.IsInstanceValid(cachedScene))
            return cachedScene;

        GD.Print($"CloudAssets: Loading {label} from {path}");

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"CloudAssets: FAILED to load {label}: {path}");
            return null;
        }

        GD.Print($"CloudAssets: Loaded {label}");
        return scene;
    }

    public static void EnsurePreloaded()
    {
        _ = CloudScene;
        _ = IceScene;

        GD.Print("CloudAssets: EnsurePreloaded finished");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class CloudAfterActEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState)
    {
        var player = runState?.Players?.FirstOrDefault();

        if (player?.Character is not Character.Cloud)
            return;

        GD.Print("AfterActEntered: Cloud detected → preloading");

        CloudAssets.EnsurePreloaded();
    }
}


[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class CloudAfterRoomEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        var player = runState?.Players?.FirstOrDefault();

        if (player?.Character is not Character.Cloud)
            return;

        GD.Print($"AfterRoomEntered: Cloud detected → preloading. Room = {room.GetType().Name}");

        CloudAssets.EnsurePreloaded();
    }
}
