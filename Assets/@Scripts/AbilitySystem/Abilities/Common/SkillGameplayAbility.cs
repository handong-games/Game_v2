using System;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.AbilitySystem.Abilities
{
    public abstract class SkillGameplayAbility : GameplayAbility
    {
        private bool _canActivate;

        [SerializeField]
        private LocalizedString _name;

        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        private ESkillTargetType _targetType;

        public LocalizedString Name => _name;
        public Sprite Icon => _icon;
        public ESkillTargetType TargetType => _targetType;
        public event Action<bool> ActivationStateChanged;

        public override bool CanActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo)
        {
            return base.CanActivateAbility(handle, actorInfo) &&
                   CheckCost(handle, actorInfo);
        }

        public override void OnGiveAbility(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpec spec)
        {
            base.OnGiveAbility(actorInfo, spec);

            CostAttributeSet costSet = actorInfo?.AbilitySystem.GetSet<CostAttributeSet>();
            if (costSet == null)
                return;

            costSet.CoinHeads.CurrentValueChanged += OnCostChanged;
            costSet.CoinTails.CurrentValueChanged += OnCostChanged;
        }

        public override void OnRemoveAbility(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpec spec)
        {
            CostAttributeSet costSet = actorInfo?.AbilitySystem.GetSet<CostAttributeSet>();
            if (costSet != null)
            {
                costSet.CoinHeads.CurrentValueChanged -= OnCostChanged;
                costSet.CoinTails.CurrentValueChanged -= OnCostChanged;
            }

            ActivationStateChanged = null;
            base.OnRemoveAbility(actorInfo, spec);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
        }

        private void OnCostChanged(float oldValue, float newValue)
        {
            bool canActivate = CheckCost(CurrentSpec.Handle, CurrentActorInfo);
            if (_canActivate == canActivate)
                return;

            _canActivate = canActivate;
            ActivationStateChanged?.Invoke(canActivate);
        }
    }
}
