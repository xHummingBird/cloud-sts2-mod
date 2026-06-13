using Godot;

namespace Cloud.Cloud.images.charui;

public partial class CharacterBackground : Control
{
	public override void _Ready()
	{
		// Get nodes
		var video = GetNode<VideoStreamPlayer>("VideoStreamPlayer");

		// Play video (looping)
		video.Loop = true;
		video.Play();
	}
}
