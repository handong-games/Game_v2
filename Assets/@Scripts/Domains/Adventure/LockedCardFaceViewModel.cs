using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Card Face/Locked")]
    public sealed class LockedCardFaceViewModel : CardFaceViewModel
    {
        public override ECardFaceType FaceType => ECardFaceType.Locked;

        public static LockedCardFaceViewModel CreateRuntime()
        {
            return CreateInstance<LockedCardFaceViewModel>();
        }
    }
}
