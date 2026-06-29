using Domains.Adventure;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Plays Adventure reward presentation after victory.
    // V1 uses a temporary click-to-close panel while preserving the reward result contract.
    public sealed class AdventureRewardUIFlow
    {
        private const string RewardOverlayClass = "adventure-reward-temp";
        private const string RewardPanelClass = "adventure-reward-temp__panel";
        private const string RewardTitleClass = "adventure-reward-temp__title";
        private const string RewardTextClass = "adventure-reward-temp__text";

        public async Awaitable<AdventureRewardUIResult> Play(
            VisualElement adventureRoot,
            AdventureRewardViewModel viewModel,
            Func<bool> canContinue)
        {
            if (adventureRoot == null)
                throw new ArgumentNullException(nameof(adventureRoot));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (!CanContinue(canContinue))
                return AdventureRewardUIResult.Canceled();

            VisualElement overlay = CreateOverlay();
            VisualElement panel = CreatePanel();
            overlay.Add(panel);
            adventureRoot.Add(overlay);

            AwaitableCompletionSource completionSource = new();
            bool completed = false;
            bool canceled = false;
            EventCallback<ClickEvent> onPanelClicked = null;
            EventCallback<DetachFromPanelEvent> onDetached = null;

            void Complete(bool isCanceled)
            {
                if (completed)
                    return;

                completed = true;
                canceled = isCanceled;
                panel.UnregisterCallback(onPanelClicked);
                overlay.UnregisterCallback(onDetached);
                completionSource.SetResult();
            }

            onPanelClicked = _ => Complete(isCanceled: false);
            onDetached = _ => Complete(isCanceled: true);

            panel.RegisterCallback(onPanelClicked);
            overlay.RegisterCallback(onDetached);

            try
            {
                await completionSource.Awaitable;
                if (canceled || !CanContinue(canContinue))
                    return AdventureRewardUIResult.Canceled();

                return AdventureRewardUIResult.Empty();
            }
            finally
            {
                panel.UnregisterCallback(onPanelClicked);
                overlay.UnregisterCallback(onDetached);
                overlay.RemoveFromHierarchy();
            }
        }

        private static VisualElement CreateOverlay()
        {
            VisualElement overlay = new()
            {
                name = "adventure-reward-temp",
                pickingMode = PickingMode.Position,
            };

            overlay.AddToClassList(RewardOverlayClass);
            return overlay;
        }

        private static VisualElement CreatePanel()
        {
            VisualElement panel = new()
            {
                name = "adventure-reward-temp-panel",
                pickingMode = PickingMode.Position,
            };

            Label title = new("Reward")
            {
                name = "adventure-reward-temp-title",
            };
            Label text = new("Click to continue")
            {
                name = "adventure-reward-temp-text",
            };

            panel.AddToClassList(RewardPanelClass);
            title.AddToClassList(RewardTitleClass);
            text.AddToClassList(RewardTextClass);

            panel.Add(title);
            panel.Add(text);
            return panel;
        }

        private static bool CanContinue(Func<bool> canContinue)
        {
            return canContinue == null || canContinue.Invoke();
        }
    }
}
