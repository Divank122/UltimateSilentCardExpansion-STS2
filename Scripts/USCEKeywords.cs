using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace USCE.Scripts;

public class USCEKeywords
{
    [CustomEnum("Thirsty")] [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Thirsty;

    [CustomEnum("Sluggish")] [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Sluggish;

    [CustomEnum("GreatBlade")] [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword GreatBlade;

    [CustomEnum("Extinct")] [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Extinct;

    [CustomEnum("Drifting")] [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Drifting;

    [CustomEnum("Kinetic")] [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Kinetic;
}
