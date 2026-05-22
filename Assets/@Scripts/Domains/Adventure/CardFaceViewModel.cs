using Gameplay.GAS;
using UnityEngine;

namespace Game.Data
{
    public enum ECardFaceType
    {
        Portrait,
        Choice,
        Locked,
    }

    public abstract class CardFaceViewModel : ScriptableObject
    {
        [SerializeField]
        private GameplayTagReference _frameTag;

        public abstract ECardFaceType FaceType { get; }
        public GameplayTag FrameTag => _frameTag.Tag;
    }
}
