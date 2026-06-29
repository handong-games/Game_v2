using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Domains.Adventure
{
    // Role:
    // Carries localized and sprite presentation data for the first Adventure screen.
    // It does not load assets or decide animation timing.
    public sealed class AdventureEntryPresentationViewModel
    {
        public AdventureEntryPresentationViewModel(
            LocalizedString regionName,
            LocalizedString title,
            LocalizedString subtitle,
            Sprite background,
            Sprite emblem)
        {
            RegionName = RequireLocalizedString(regionName, nameof(regionName));
            Title = RequireLocalizedString(title, nameof(title));
            Subtitle = RequireLocalizedString(subtitle, nameof(subtitle));
            Background = background;
            Emblem = emblem;
        }

        public LocalizedString RegionName { get; }
        public LocalizedString Title { get; }
        public LocalizedString Subtitle { get; }
        public Sprite Background { get; }
        public Sprite Emblem { get; }

        private static LocalizedString RequireLocalizedString(
            LocalizedString value,
            string parameterName)
        {
            if (value == null || value.IsEmpty)
                throw new ArgumentException("Adventure entry localized string is required.", parameterName);

            return value;
        }
    }

    // Role:
    // Carries persistent HUD data for the Adventure screen entry state.
    // It keeps ResourceStatusBar data separate from intro banner copy.
    public sealed class AdventureResourceStatusViewModel
    {
        public AdventureResourceStatusViewModel(LocalizedString regionName)
        {
            if (regionName == null || regionName.IsEmpty)
                throw new ArgumentException("Adventure resource status region name is required.", nameof(regionName));

            RegionName = regionName;
        }

        public LocalizedString RegionName { get; }
    }

    // Role:
    // Carries reward presentation data emitted after combat victory.
    // Empty rewards are valid so the screen can preserve the same flow contract.
    public sealed class AdventureRewardViewModel
    {
        public AdventureRewardViewModel()
            : this(Array.Empty<uint>())
        {
        }

        public AdventureRewardViewModel(IReadOnlyList<uint> rewardIds)
        {
            RewardIds = rewardIds ?? Array.Empty<uint>();
        }

        public IReadOnlyList<uint> RewardIds { get; }
    }

    // Role:
    // Reports the reward choices completed by the UI.
    // It lets reward presentation finish before gameplay advances to the next board.
    public sealed class AdventureRewardUIResult
    {
        private AdventureRewardUIResult(
            bool completed,
            IReadOnlyList<uint> claimedRewardIds)
        {
            Completed = completed;
            ClaimedRewardIds = claimedRewardIds ?? Array.Empty<uint>();
        }

        public bool Completed { get; }
        public IReadOnlyList<uint> ClaimedRewardIds { get; }

        public static AdventureRewardUIResult Complete(IReadOnlyList<uint> claimedRewardIds)
        {
            return new AdventureRewardUIResult(true, claimedRewardIds);
        }

        public static AdventureRewardUIResult Empty()
        {
            return Complete(Array.Empty<uint>());
        }

        public static AdventureRewardUIResult Canceled()
        {
            return new AdventureRewardUIResult(false, Array.Empty<uint>());
        }
    }

    // Role:
    // Carries the board presentation used when the next choice set is shown after reward completion.
    // It separates "next choice presentation" from generic board refresh events.
    public sealed class AdventureChoiceRefreshViewModel
    {
        public AdventureChoiceRefreshViewModel(AdventureBoardPresentationViewModel board)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public AdventureBoardPresentationViewModel Board { get; }
    }
}
