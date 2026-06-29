using System;
using Game.Data;

namespace Domains.Adventure
{
    public static class CardFaceViewModelFactory
    {
        public static CardFaceViewModel Create(CardFaceModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return CreateOptional(model);
        }

        public static CardFaceViewModel CreateOptional(CardFaceModel model)
        {
            if (model == null)
                return null;

            return model switch
            {
                PortraitCardFaceModel portrait => PortraitCardFaceViewModel.CreateRuntime(
                    portrait.LocalizedName,
                    portrait.Portrait),
                LockedCardFaceModel => LockedCardFaceViewModel.CreateRuntime(),
                _ => throw new ArgumentOutOfRangeException(nameof(model)),
            };
        }
    }
}
