namespace Domains.Player
{
    public sealed class CoinFlipDto
    {
        public CoinFlipDto(ECoinFace[] faces)
        {
            Faces = faces;

            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i] == ECoinFace.Heads)
                    HeadsCount++;
                else
                    TailsCount++;
            }
        }

        public ECoinFace[] Faces { get; }
        public int Count => Faces.Length;
        public int HeadsCount { get; }
        public int TailsCount { get; }
    }
}
