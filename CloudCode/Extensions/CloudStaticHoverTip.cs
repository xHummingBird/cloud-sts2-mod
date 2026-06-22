using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Cloud.CloudCode.Extensions;

public static class CloudStaticHoverTip
{
    public static readonly IHoverTip ATB = new HoverTip(
        new LocString("static_hover_tips", "CLOUD_ATB.title"),
        new LocString("static_hover_tips", "CLOUD_ATB.description")
    );

    public static readonly IHoverTip Limit = new HoverTip(
        new LocString("static_hover_tips", "CLOUD_LIMIT.title"),
        new LocString("static_hover_tips", "CLOUD_LIMIT.description")
    );

    public static readonly IHoverTip Summon = new HoverTip(
        new LocString("static_hover_tips", "CLOUD_SUMMON.title"),
        new LocString("static_hover_tips", "CLOUD_SUMMON.description")
    );

    public static readonly IHoverTip Magic = new HoverTip(
        new LocString("static_hover_tips", "CLOUD_MAGIC.title"),
        new LocString("static_hover_tips", "CLOUD_MAGIC.description")
    );

    public static readonly IHoverTip Stance = new HoverTip(
        new LocString("static_hover_tips", "CLOUD_STANCE.title"),
        new LocString("static_hover_tips", "CLOUD_STANCE.description")
    );

    public static readonly IHoverTip Punisher = new HoverTip(
        new LocString("static_hover_tips", "CLOUD_PUNISHER.title"),
        new LocString("static_hover_tips", "CLOUD_PUNISHER.description")
    );

    public static readonly IHoverTip Operator = new HoverTip(
        new LocString("static_hover_tips", "CLOUD_OPERATOR.title"),
        new LocString("static_hover_tips", "CLOUD_OPERATOR.description")
    );
}
