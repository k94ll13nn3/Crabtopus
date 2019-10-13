namespace Crabtopus.App.Model
{
    public static class Emotes
    {
        public static string Black => "<:mtg_black:632851289268682762>";

        public static string Red => "<:mtg_red:632851310554644481>";

        public static string White => "<:mtg_white:632851299079028748>";

        public static string Green => "<:mtg_green:632851317559263232>";

        public static string Blue => "<:mtg_blue:632851326203592725>";

        public static string MythicRareWildcard => "<:wc_mythic:632854745866043392>";

        public static string RareWildcard => "<:wc_rare:632854745832226816>";

        public static string UncommonWildcard => "<:wc_uncommon:632854745983483904>";

        public static string CommonWildcard => "<:wc_common:632854745828294656>";

        public static string Convert(CardColor color)
        {
            return color switch
            {
                CardColor.W => White,
                CardColor.U => Blue,
                CardColor.R => Red,
                CardColor.B => Black,
                CardColor.G => Green,
                _ => null,
            };
        }

        public static string Convert(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.MythicRare => MythicRareWildcard,
                Rarity.Rare => RareWildcard,
                Rarity.Uncommon => UncommonWildcard,
                Rarity.Common => CommonWildcard,
                _ => null,
            };
        }
    }
}
