using Gameplay.GAS;
using UIToolkit.Timeline;
using UnityEngine;

namespace Game.AbilitySystem
{
    [CreateAssetMenu(
        menuName = "Game/AbilitySystem/Timeline/Gameplay Tag Timeline Notify")]
    public sealed class GameplayTagTimelineNotifyDefinition : TimelineNotifyDefinition
    {
        [SerializeField]
        private GameplayTag _tag;

        public GameplayTag Tag => _tag;
    }
}
