
using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

public partial class SmoothFollowCamera : Camera2D
{
	private bool _isActive = false;

	// Correct baseline position captured from the active combat camera
	private Vector2 _restingPos;

	// X-only cinematic offset
	private float _targetOffsetX = 0f;

	// Background references
	private Control _bgContainer;
	private Node2D _backCombatVfxContainer;
	private Vector2 _bgStartPos;
	private Vector2 _vfxStartPos;
	private bool _bgReady = false;

	[Export] private float _followWeight = 0.10f;
	[Export] private float _returnThreshold = 1.0f;
	[Export] private float _bgFollowFactor = 1.0f;
	[Export] private float _vfxFollowFactor = 0.7f;
	[Export] private float _bgLerp = 0.10f;

	public override void _Ready()
	{
		if (Engine.IsEditorHint()) return;

		Enabled = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint()) return;
		if (!Enabled && !_isActive) return;

		Vector2 destination;

		if (_isActive)
		{
			// X-only pan. Y stays fixed.
			destination = new Vector2(
				_restingPos.X + _targetOffsetX,
				_restingPos.Y
			);
		}
		else
		{
			destination = _restingPos;

			if (GlobalPosition.DistanceTo(_restingPos) < _returnThreshold)
			{
				GlobalPosition = _restingPos;
				Enabled = false;

				ResetBackgroundInstant();
				return;
			}
		}

		float distance = GlobalPosition.DistanceTo(destination);

		if (distance > 200f)
		{
			GlobalPosition = GlobalPosition.Lerp(destination, 0.15f);
		}
		else
		{
			float lerpFactor = 1.0f - Mathf.Pow(1.0f - _followWeight, (float)delta * 60.0f);
			GlobalPosition = GlobalPosition.Lerp(destination, lerpFactor);
		}

		UpdateBackgroundHack();
	}

	public void StartCinematic(float offsetX = -300f)
	{
		SyncToActiveCamera();

		_isActive = true;
		_targetOffsetX = offsetX;

		CacheBackgroundNodes();

		Enabled = true;
		MakeCurrent();
	}

	public void EndCinematic()
	{
		_isActive = false;
		_targetOffsetX = 0f;

		// Keep enabled so it can smoothly return
		Enabled = true;
	}

	private void SyncToActiveCamera()
	{
		var currentCam = GetViewport().GetCamera2D();

		if (currentCam != null && currentCam != this)
		{
			GlobalPosition = currentCam.GlobalPosition;
			Zoom = currentCam.Zoom;
			Rotation = currentCam.Rotation;
		}

		_restingPos = GlobalPosition;
	}

	private void CacheBackgroundNodes()
	{
		var room = NCombatRoom.Instance;
		if (room == null) return;

		_bgContainer = room.GetNodeOrNull<Control>("%BgContainer");
		_backCombatVfxContainer = room.GetNodeOrNull<Node2D>("%BackCombatVfxContainer");

		if (_bgContainer != null)
			_bgStartPos = _bgContainer.GlobalPosition;

		if (_backCombatVfxContainer != null)
			_vfxStartPos = _backCombatVfxContainer.GlobalPosition;

		_bgReady = (_bgContainer != null || _backCombatVfxContainer != null);
	}

	private void UpdateBackgroundHack()
	{
		if (!_bgReady) return;

		float offsetX = GlobalPosition.X - _restingPos.X;

		if (_bgContainer != null)
		{
			Vector2 targetBgPos = _bgStartPos - new Vector2(offsetX * _bgFollowFactor, 0);
			_bgContainer.GlobalPosition = _bgContainer.GlobalPosition.Lerp(targetBgPos, _bgLerp);
		}

		if (_backCombatVfxContainer != null)
		{
			Vector2 targetVfxPos = _vfxStartPos - new Vector2(offsetX * _vfxFollowFactor, 0);
			_backCombatVfxContainer.GlobalPosition =
				_backCombatVfxContainer.GlobalPosition.Lerp(targetVfxPos, _bgLerp);
		}
	}

	private void ResetBackgroundInstant()
	{
		if (_bgContainer != null)
			_bgContainer.GlobalPosition = _bgStartPos;

		if (_backCombatVfxContainer != null)
			_backCombatVfxContainer.GlobalPosition = _vfxStartPos;
	}
}
