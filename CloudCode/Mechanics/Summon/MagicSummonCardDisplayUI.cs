using Cloud.CloudCode.Cards.Ancient;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Cloud.CloudCode.Mechanics.Summon;

public static class MagicSummonCardDisplayUI
{
    private const string NodeName = "MagicSummon_UI";
    private const string ScenePath = "res://Cloud/scenes/MagicCardDisplay.tscn";

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
            (model is IMagicCard || model is ISummonCard)
            && model is not Odin
            && model is not Bahamut;
        
        node.Position = new Vector2(75f, -205f);
    }
}