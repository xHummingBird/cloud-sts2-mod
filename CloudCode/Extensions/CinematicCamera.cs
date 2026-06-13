using Godot;

namespace Cloud.CloudCode.Extensions;


public partial class CinematicCamera : Camera2D
{
    public static CinematicCamera Instance { get; private set; }

    private Vector2 _defaultGlobalPosition;
    private Node2D _target;
    private bool _isActive = false;
    private bool _initialized = false;

    // Internal smoothing only; card does not need to pass anything.
    private const float MoveSpeed = 10f;
    private const float ReturnSpeed = 8f;

    public override void _Ready()
    {
        Instance = this;
        _defaultGlobalPosition = GlobalPosition;
        _initialized = true;
    }

    public override void _Process(double delta)
    {
        if (!_initialized)
            return;

        float dt = (float)delta;

        if (_isActive && _target != null && GodotObject.IsInstanceValid(_target))
        {
            Vector2 desired = _target.GlobalPosition;
            GlobalPosition = GlobalPosition.Lerp(desired, 1.0f - Mathf.Exp(-MoveSpeed * dt));
        }
        else
        {
            GlobalPosition = GlobalPosition.Lerp(_defaultGlobalPosition, 1.0f - Mathf.Exp(-ReturnSpeed * dt));
        }
    }

    public static void Start(Node2D target)
    {
        if (Instance == null || target == null || !GodotObject.IsInstanceValid(target))
            return;

        Instance._target = target;
        Instance._isActive = true;
        Instance.Enabled = true;
    }

    public static void End()
    {
        if (Instance == null)
            return;

        Instance._target = null;
        Instance._isActive = false;
    }
}

