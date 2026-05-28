using Domains.Player;

namespace Domains.Adventure
{
    public sealed class CoinFlipCueData
    {
        public CoinFlipCueData(ECoinFace[] faces)
        {
            Faces = faces;
        }

        public ECoinFace[] Faces { get; }
        public int Count => Faces.Length;
    }
}
