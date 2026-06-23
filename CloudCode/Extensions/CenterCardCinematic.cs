using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Cloud.CloudCode.Extensions;

public static class CenterCardCinematic
{
    private static bool _isHidden = false;

    public static void Start(ulong netId)
    {
        if (!IsLocalPlayer(netId)) return;
        if (_isHidden) return;

        SetHidden(true);
        _isHidden = true;
    }

    public static void End(ulong netId)
    {
        if (!IsLocalPlayer(netId)) return;
        if (!_isHidden) return;

        SetHidden(false);
        _isHidden = false;
    }

    private static bool IsLocalPlayer(ulong netId)
    {
        return RunManager.Instance != null
               && RunManager.Instance.NetService != null
               && netId == RunManager.Instance.NetService.NetId;
    }

    private static void SetHidden(bool hidden)
    {
        var playContainer = NCombatRoom.Instance?.Ui?.PlayContainer;
        if (playContainer == null)
            return;

        // ✅ Smooth fade (optional but nice)
        var tween = playContainer.CreateTween();
        tween.TweenProperty(
            playContainer,
            "modulate",
            hidden ? Colors.Transparent : Colors.White,
            0.1f
        );
    }
}