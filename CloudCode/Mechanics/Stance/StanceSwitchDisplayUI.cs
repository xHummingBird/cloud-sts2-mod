using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Cloud.CloudCode.Mechanics.Stance;

public static class StanceSwitchDisplayUI
{
    private const string NodeName = "StanceSwitch_UI";
    private const string ScenePath = "res://Cloud/scenes/StanceDisplay.tscn";

    public static void Ensure(NCard card)
    {
        var model = card.Model;
        var body = card.Body;

        if (model == null || body == null)
            return;

        var node = body.GetNodeOrNull<Control>(NodeName);

        if (node == null)
        {
            var scene = GD.Load<PackedScene>(ScenePath);
            if (scene == null)
                return;

            node = scene.Instantiate<Control>();
            node.Name = NodeName;
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
        }

        node.Visible =
            (model is IStanceCard);

        node.Position = new Vector2(75f, -205f);
    }
}