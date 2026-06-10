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
}
