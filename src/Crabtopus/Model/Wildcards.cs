namespace Crabtopus.App.Model
{
    public class Wildcards
    {
        public int Common { get; set; }

        public int Uncommon { get; set; }

        public int Rare { get; set; }

        public int MythicRare { get; set; }

        public void Add(int count, Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:
                    Common += count;
                    break;

                case Rarity.Uncommon:
                    Uncommon += count;
                    break;

                case Rarity.Rare:
                    Rare += count;
                    break;

                case Rarity.MythicRare:
                    MythicRare += count;
                    break;
            }
        }

        public override string ToString()
        {
            return $"{MythicRare}{Emotes.MythicRareWildcard} {Rare}{Emotes.RareWildcard} {Uncommon}{Emotes.UncommonWildcard} {Common}{Emotes.CommonWildcard}";
        }
    }
}
