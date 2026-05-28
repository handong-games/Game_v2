namespace Domains.Player
{
    public sealed class CoinFlipDto
    {
        public CoinFlipDto(ECoinFace[] faces)
        {
            Faces = faces;
        }

        public ECoinFace[] Faces { get; }
    }
}
