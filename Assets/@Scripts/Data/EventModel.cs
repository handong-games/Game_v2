using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Event")]
    public sealed class EventModel : AbstractModel<EEvent>, ICardModel
    {
    }
}
