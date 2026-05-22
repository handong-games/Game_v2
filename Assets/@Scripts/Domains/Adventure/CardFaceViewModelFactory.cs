using System;
using Game.Data;

namespace Domains.Adventure
{
    public static class CardFaceViewModelFactory
    {
        public static CardFaceViewModel Create(CardFaceModel model)
        {
            if (model == null)
                return null;

            return model switch
            {
                PortraitCardFaceModel portrait => PortraitCardFaceViewModel.CreateRuntime(
                    portrait.LocalizedName,
                    portrait.Portrait),
                ChoiceCardFaceModel choice => ChoiceCardFaceViewModel.CreateRuntime(
                    choice.StyleType,
                    choice.Icon,
                    choice.Label),
                LockedCardFaceModel => LockedCardFaceViewModel.CreateRuntime(),
                _ => throw new ArgumentOutOfRangeException(nameof(model)),
            };
        }
    }
}
