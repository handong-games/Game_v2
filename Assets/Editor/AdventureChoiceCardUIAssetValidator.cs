using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Domains.View.Widgets;
using Game.Data;
using Game.Generated;
using Game.Scenes.Adventure;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

// Role:
// Validates the content wiring required before Adventure UI can be treated as implementation-ready.
public static class AdventureChoiceCardUIAssetValidator
{
    private const string TablePath = "Assets/@Resources/Model/Tables/AdventureChoiceCardUITable.asset";
    private const string AdventureTablePath = "Assets/@Resources/Model/Tables/AdventureTable.asset";
    private const string ChoiceWidgetUxmlPath = "Assets/@Scripts/Domains/View/Card/AdventureChoiceCardWidget.uxml";
    private const string PlayerWidgetUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventurePlayerCardWidget.uxml";
    private const string MonsterWidgetUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventureMonsterCardWidget.uxml";
    private const string DisplayWidgetUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventureDisplayCardWidget.uxml";
    private const string AdventureViewUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventureView.uxml";
    private const string AdventureViewUssPath = "Assets/@Scripts/Domains/View/Adventure/AdventureView.uss";
    private const string AdventureViewAnimationUssPath = "Assets/@Scripts/Domains/View/Adventure/AdventureView.Animation.uss";
    private const string DefaultViewThemePath = "Assets/@Scripts/Domains/View/Common/UnityDefaultRuntimeTheme.tss";
    private const string ViewTransitionUssPath = "Assets/@Scripts/Core/Manager/ViewTransition/ViewTransition.uss";
    private const string CardBoardUxmlPath = "Assets/@Scripts/Domains/View/CardBoard/CardBoard.uxml";
    private const string CardBoardUssPath = "Assets/@Scripts/Domains/View/CardBoard/CardBoard.uss";
    private const string CardDeckUxmlPath = "Assets/@Scripts/Domains/View/CardDeck/CardDeck.uxml";
    private const string CardDeckUssPath = "Assets/@Scripts/Domains/View/CardDeck/CardDeck.uss";
    private const string CardDeckAnimationUssPath = "Assets/@Scripts/Domains/View/CardDeck/CardDeck.Animation.uss";
    private const string SkillSlotWidgetUxmlPath = "Assets/@Scripts/Domains/View/Common/SkillSlotWidget.uxml";
    private const string SkillSlotUssPath = "Assets/@Scripts/Domains/View/Common/SkillSlot.uss";
    private const string SkillSlotWidgetUssPath = "Assets/@Scripts/Domains/View/SkillSlotWidget/SkillSlotWidget.uss";
    private const string CardWidgetUssPath = "Assets/@Scripts/Domains/View/Common/CardWidget.uss";
    private const string HealthWidgetUssPath = "Assets/@Scripts/Domains/View/HealthWidget/HealthWidget.uss";
    private const string ChoiceWidgetUssPath = "Assets/@Scripts/Domains/View/Card/AdventureChoiceCardWidget.uss";
    private const string PlayerWidgetUssPath = "Assets/@Scripts/Domains/View/Adventure/AdventurePlayerCardWidget.uss";
    private const string MonsterWidgetUssPath = "Assets/@Scripts/Domains/View/Adventure/AdventureMonsterCardWidget.uss";
    private const string DisplayWidgetUssPath = "Assets/@Scripts/Domains/View/Adventure/AdventureDisplayCardWidget.uss";
    private const string PortraitFaceWidgetUxmlPath = "Assets/@Scripts/Domains/View/Card/PortraitCardFaceWidget/PortraitCardFaceWidget.uxml";
    private const string LockedFaceWidgetUxmlPath = "Assets/@Scripts/Domains/View/Card/LockedCardFaceWidget/LockedCardFaceWidget.uxml";
    private static readonly FieldInfo RowsField =
        typeof(AbstractTable<AdventureChoiceCardUIModel, EChoiceCardType>)
            .GetField("_rows", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo AdventureRowsField =
        typeof(AbstractTable<AdventureRegionModel, EAdventure>)
            .GetField("_rows", BindingFlags.Instance | BindingFlags.NonPublic);

    [MenuItem("Tools/Codex/Validate Adventure UI Assets")]
    public static void ValidateFromMenu()
    {
        IReadOnlyList<string> errors = Validate();
        if (errors.Count == 0)
        {
            Debug.Log("Adventure UI asset validation passed.");
            return;
        }

        string message = string.Join(Environment.NewLine, errors);
        Debug.LogError(message);
        throw new InvalidOperationException(message);
    }

    public static IReadOnlyList<string> Validate()
    {
        List<string> errors = new();
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            errors.Add("AddressableAssetSettings not found.");
            return errors;
        }

        ValidateWidgetTemplate(settings, errors);
        ValidateViewTransitionTheme(settings, errors);
        ValidateAdventureView(settings, errors);
        ValidateUniqueAddress(settings, errors);
        ValidateTable(settings, errors);
        ValidateAdventureRegions(settings, errors);
        return errors;
    }

    private static void ValidateViewTransitionTheme(
        AddressableAssetSettings settings,
        List<string> errors)
    {
        ThemeStyleSheet themeStyleSheet =
            AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(DefaultViewThemePath);
        if (themeStyleSheet == null)
        {
            errors.Add($"Default view theme asset is missing: {DefaultViewThemePath}");
            return;
        }

        ValidateAddressableEntry(
            settings,
            DefaultViewThemePath,
            AdventureSceneAddressables.DefaultViewTheme,
            requiredLabel: null,
            errors);

        string theme = File.ReadAllText(DefaultViewThemePath);
        if (!theme.Contains(Path.GetFileName(ViewTransitionUssPath), StringComparison.Ordinal))
        {
            errors.Add(
                $"Default view theme must import ViewTransition USS: {ViewTransitionUssPath}");
        }

        StyleSheet viewTransitionStyle =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(ViewTransitionUssPath);
        if (viewTransitionStyle == null)
        {
            errors.Add($"ViewTransition USS asset is missing: {ViewTransitionUssPath}");
            return;
        }

        string uss = File.ReadAllText(ViewTransitionUssPath);
        ValidateRequiredUssClass(uss, "app-overlay-layer", ViewTransitionUssPath, errors);
        ValidateRequiredUssClass(uss, "view-transition--fade-in", ViewTransitionUssPath, errors);
        ValidateRequiredUssClass(uss, "view-transition--fade-out", ViewTransitionUssPath, errors);
        ValidateRequiredUssClass(uss, "ui-transition--hidden", ViewTransitionUssPath, errors);
        ValidateRequiredUssClass(uss, "ui-transition--from-bottom", ViewTransitionUssPath, errors);
        ValidateRequiredUssClass(uss, "ui-transition--enter", ViewTransitionUssPath, errors);
    }

    private static void ValidateWidgetTemplate(
        AddressableAssetSettings settings,
        List<string> errors)
    {
        ValidateWidgetTemplate(
            settings,
            ChoiceWidgetUxmlPath,
            AdventureSceneAddressables.AdventureChoiceCardWidget,
            errors);

        ValidateWidgetTemplate(
            settings,
            PlayerWidgetUxmlPath,
            AdventureSceneAddressables.AdventurePlayerCardWidget,
            errors);

        ValidateWidgetTemplate(
            settings,
            MonsterWidgetUxmlPath,
            AdventureSceneAddressables.AdventureMonsterCardWidget,
            errors);

        ValidateWidgetTemplate(
            settings,
            DisplayWidgetUxmlPath,
            AdventureSceneAddressables.AdventureDisplayCardWidget,
            errors);

        ValidateWidgetTemplate(
            settings,
            PortraitFaceWidgetUxmlPath,
            AdventureSceneAddressables.PortraitCardFaceWidget,
            errors);

        ValidateWidgetTemplate(
            settings,
            LockedFaceWidgetUxmlPath,
            AdventureSceneAddressables.LockedCardFaceWidget,
            errors);

        ValidateWidgetTemplate(
            settings,
            SkillSlotWidgetUxmlPath,
            AdventureSceneAddressables.SkillSlotWidget,
            errors);
    }

    private static void ValidateAdventureView(
        AddressableAssetSettings settings,
        List<string> errors)
    {
        if (!File.Exists(AdventureViewUxmlPath))
        {
            errors.Add($"AdventureView UXML is missing: {AdventureViewUxmlPath}");
            return;
        }

        ValidateAddressableEntry(
            settings,
            AdventureViewUxmlPath,
            AdventureSceneAddressables.AdventureView,
            AdventureSceneAddressables.AdventureSceneLabel,
            errors);

        string uxml = File.ReadAllText(AdventureViewUxmlPath);
        ValidateStyleReference(uxml, CardWidgetUssPath, errors);
        ValidateStyleReference(uxml, ChoiceWidgetUssPath, errors);
        ValidateStyleReference(uxml, PlayerWidgetUssPath, errors);
        ValidateStyleReference(uxml, MonsterWidgetUssPath, errors);
        ValidateStyleReference(uxml, DisplayWidgetUssPath, errors);
        ValidateStyleReference(uxml, HealthWidgetUssPath, errors);
        ValidateStyleReference(uxml, CardBoardUssPath, errors);
        ValidateStyleReference(uxml, CardDeckUssPath, errors);
        ValidateStyleReference(uxml, CardDeckAnimationUssPath, errors);
        ValidateStyleReference(uxml, SkillSlotUssPath, errors);
        ValidateStyleReference(uxml, SkillSlotWidgetUssPath, errors);
        ValidateStyleReference(uxml, AdventureViewAnimationUssPath, errors);
        ValidateAdventureViewTree(errors);
        ValidateCardBoardTree(errors);
        ValidateCardDeckTree(errors);
        ValidateCardBoardStyle(errors);
        ValidateCardDeckStyle(errors);
        ValidateCardDeckAnimationStyle(errors);
        ValidateCardWidgetStyle(errors);
        ValidateChoiceWidgetStyle(errors);
        ValidateAdventureViewStyle(errors);
        ValidatePlayerWidgetStyle(errors);
        ValidateMonsterWidgetStyle(errors);
        ValidateDisplayWidgetStyle(errors);
        ValidateHealthWidgetStyle(errors);
        ValidateAdventureViewAnimationStyle(errors);
        ValidateSkillSlotStyle(errors);
    }

    private static void ValidateAdventureViewTree(List<string> errors)
    {
        VisualTreeAsset template =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AdventureViewUxmlPath);

        if (template == null)
        {
            errors.Add($"AdventureView UXML asset is missing: {AdventureViewUxmlPath}");
            return;
        }

        TemplateContainer container;
        try
        {
            container = template.Instantiate();
        }
        catch (Exception exception)
        {
            errors.Add($"Failed to instantiate UXML: {AdventureViewUxmlPath}. {exception.Message}");
            return;
        }

        ValidateRequired<VisualElement>(container, "adventure-root", AdventureViewUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "adventure-background", AdventureViewUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "adventure-emblem", AdventureViewUxmlPath, errors);
        ValidateRequired<Banner>(container, "banner", AdventureViewUxmlPath, errors);
        ValidateRequired<ResourceStatusBar>(container, "resource-status-bar", AdventureViewUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "progress-bar", AdventureViewUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "progress-bar-track-fill", AdventureViewUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "adventure-effect-layer", AdventureViewUxmlPath, errors);
        ValidateRequired<CoinStatusWidget>(container, "coin-status-widget", AdventureViewUxmlPath, errors);
        ValidateRequired<EndTurnWidget>(container, "end-turn-widget", AdventureViewUxmlPath, errors);
        ValidateRequired<ArrowWidget>(container, "arrow-widget", AdventureViewUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "card-deck", AdventureViewUxmlPath, errors);
        ValidateRequired<AdventureSkillSlotGroup>(container, "skill-slot-group", AdventureViewUxmlPath, errors);
    }

    private static void ValidateCardBoardTree(List<string> errors)
    {
        VisualTreeAsset template =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CardBoardUxmlPath);

        if (template == null)
        {
            errors.Add($"CardBoard UXML asset is missing: {CardBoardUxmlPath}");
            return;
        }

        TemplateContainer container;
        try
        {
            container = template.Instantiate();
        }
        catch (Exception exception)
        {
            errors.Add($"Failed to instantiate UXML: {CardBoardUxmlPath}. {exception.Message}");
            return;
        }

        ValidateRequired<VisualElement>(container, "card-board", CardBoardUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "card-board-left-area", CardBoardUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "card-board-center-area", CardBoardUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "card-board-right-area", CardBoardUxmlPath, errors);
        ValidateRequired<Pouch>(container, "pouch", CardBoardUxmlPath, errors);
    }

    private static void ValidateCardDeckTree(List<string> errors)
    {
        VisualTreeAsset template =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CardDeckUxmlPath);

        if (template == null)
        {
            errors.Add($"CardDeck UXML asset is missing: {CardDeckUxmlPath}");
            return;
        }

        TemplateContainer container;
        try
        {
            container = template.Instantiate();
        }
        catch (Exception exception)
        {
            errors.Add($"Failed to instantiate UXML: {CardDeckUxmlPath}. {exception.Message}");
            return;
        }

        ValidateRequired<VisualElement>(container, "card-deck", CardDeckUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "card-deck-shadow-card", CardDeckUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "card-deck-middle-card", CardDeckUxmlPath, errors);
        ValidateRequired<VisualElement>(container, "card-deck-top-card", CardDeckUxmlPath, errors);
    }

    private static void ValidateCardBoardStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(CardBoardUssPath);
        if (styleSheet == null)
        {
            errors.Add($"CardBoard USS asset is missing: {CardBoardUssPath}");
            return;
        }

        string uss = File.ReadAllText(CardBoardUssPath);
        ValidateRequiredUssClass(uss, "card-board__area--refresh-hidden", CardBoardUssPath, errors);
        ValidateRequiredUssClass(uss, "card-board__card-anchor", CardBoardUssPath, errors);
        ValidateRequiredUssClass(uss, "card-board__card-anchor--enter-pending", CardBoardUssPath, errors);
        ValidateRequiredUssClass(uss, "card-board__card-anchor--enter", CardBoardUssPath, errors);
        ValidateRequiredUssClass(uss, "card-board__card-anchor--exit", CardBoardUssPath, errors);
        ValidateRequiredUssClass(uss, "card-board__card--deal-enter", CardBoardUssPath, errors);
        ValidateRequiredUssClass(uss, "card-board__card--deal-settle", CardBoardUssPath, errors);
        ValidateRequiredUssClass(uss, "card-board__card--deal-settle-release", CardBoardUssPath, errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".card-board__card--deal-enter",
            "transition-property: opacity, scale, translate",
            CardBoardUssPath,
            errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".card-board__card--deal-enter",
            "transition-duration: var(--motion-duration-long-700ms)",
            CardBoardUssPath,
            errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".card-board__card--deal-settle",
            "transition-property: scale, translate",
            CardBoardUssPath,
            errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".card-board__card--deal-settle",
            "transition-duration: var(--motion-duration-instant-80ms)",
            CardBoardUssPath,
            errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".card-board__card--deal-settle-release",
            "transition-property: scale, translate",
            CardBoardUssPath,
            errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".card-board__card--deal-settle-release",
            "transition-duration: var(--motion-duration-instant-80ms)",
            CardBoardUssPath,
            errors);
    }

    private static void ValidateCardDeckStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(CardDeckUssPath);
        if (styleSheet == null)
        {
            errors.Add($"CardDeck USS asset is missing: {CardDeckUssPath}");
            return;
        }

        string uss = File.ReadAllText(CardDeckUssPath);
        ValidateRequiredUssClass(uss, "card-deck", CardDeckUssPath, errors);
        ValidateRequiredUssClass(uss, "card-deck__card", CardDeckUssPath, errors);
        ValidateRequiredUssClass(uss, "card-deck__card--shadow", CardDeckUssPath, errors);
        ValidateRequiredUssClass(uss, "card-deck__card--middle", CardDeckUssPath, errors);
        ValidateRequiredUssClass(uss, "card-deck__card--top", CardDeckUssPath, errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".card-deck__card--top",
            "transition-property: translate",
            CardDeckUssPath,
            errors);
    }

    private static void ValidateCardDeckAnimationStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(CardDeckAnimationUssPath);
        if (styleSheet == null)
        {
            errors.Add($"CardDeck animation USS asset is missing: {CardDeckAnimationUssPath}");
            return;
        }

        string uss = File.ReadAllText(CardDeckAnimationUssPath);
        ValidateRequiredUssClass(uss, "card-deck--hidden", CardDeckAnimationUssPath, errors);
        ValidateRequiredUssClass(uss, "card-deck--enter", CardDeckAnimationUssPath, errors);
    }

    private static void ValidateAdventureViewStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AdventureViewUssPath);
        if (styleSheet == null)
        {
            errors.Add($"AdventureView USS asset is missing: {AdventureViewUssPath}");
            return;
        }

        string uss = File.ReadAllText(AdventureViewUssPath);
        ValidateRequiredUssClass(uss, "adventure-view", AdventureViewUssPath, errors);
    }

    private static void ValidateDisplayWidgetStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(DisplayWidgetUssPath);
        if (styleSheet == null)
        {
            errors.Add($"AdventureDisplayCardWidget USS asset is missing: {DisplayWidgetUssPath}");
            return;
        }

        string uss = File.ReadAllText(DisplayWidgetUssPath);
        ValidateRequiredUssClass(uss, "adventure-display-card", DisplayWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "adventure-display-card__card", DisplayWidgetUssPath, errors);
    }

    private static void ValidateChoiceWidgetStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ChoiceWidgetUssPath);
        if (styleSheet == null)
        {
            errors.Add($"AdventureChoiceCardWidget USS asset is missing: {ChoiceWidgetUssPath}");
            return;
        }

        string uss = File.ReadAllText(ChoiceWidgetUssPath);
        ValidateRequiredUssClass(uss, "card-widget", ChoiceWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-widget__background", ChoiceWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-widget__frame", ChoiceWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-widget__content", ChoiceWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "choice-card-widget__icon", ChoiceWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "choice-card-widget__label", ChoiceWidgetUssPath, errors);
    }

    private static void ValidatePlayerWidgetStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(PlayerWidgetUssPath);
        if (styleSheet == null)
        {
            errors.Add($"AdventurePlayerCardWidget USS asset is missing: {PlayerWidgetUssPath}");
            return;
        }

        string uss = File.ReadAllText(PlayerWidgetUssPath);
        ValidateRequiredUssClass(uss, "adventure-player-card", PlayerWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "adventure-player-card__health-widget", PlayerWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "adventure-player-card__card", PlayerWidgetUssPath, errors);
    }

    private static void ValidateMonsterWidgetStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MonsterWidgetUssPath);
        if (styleSheet == null)
        {
            errors.Add($"AdventureMonsterCardWidget USS asset is missing: {MonsterWidgetUssPath}");
            return;
        }

        string uss = File.ReadAllText(MonsterWidgetUssPath);
        ValidateRequiredUssClass(uss, "adventure-monster-card", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "adventure-monster-card__intent-badge", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "adventure-monster-card__health-widget", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "adventure-monster-card__card", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "intent-badge", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "intent-badge--hidden", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "intent-badge--revealed", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "intent-badge--refresh", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "intent-badge--triggered", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "intent-badge__icon", MonsterWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "intent-badge__number", MonsterWidgetUssPath, errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".intent-badge",
            "transition-property: opacity, scale, translate",
            MonsterWidgetUssPath,
            errors);
        ValidateRequiredUssSelectorFragment(
            uss,
            ".intent-badge",
            "transition-duration: 180ms",
            MonsterWidgetUssPath,
            errors);
    }

    private static void ValidateCardWidgetStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(CardWidgetUssPath);
        if (styleSheet == null)
        {
            errors.Add($"CardWidget USS asset is missing: {CardWidgetUssPath}");
            return;
        }

        string uss = File.ReadAllText(CardWidgetUssPath);
        ValidateRequiredUssClass(uss, "card-v2", CardWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-v2__flip-root", CardWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-v2__face-slot", CardWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-v2__front-slot", CardWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-v2__back-slot", CardWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "card-v2--back", CardWidgetUssPath, errors);
    }

    private static void ValidateHealthWidgetStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(HealthWidgetUssPath);
        if (styleSheet == null)
        {
            errors.Add($"HealthWidget USS asset is missing: {HealthWidgetUssPath}");
            return;
        }

        string uss = File.ReadAllText(HealthWidgetUssPath);
        ValidateRequiredUssClass(uss, "health-widget", HealthWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "health-widget__background", HealthWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "health-widget__fill-clip", HealthWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "health-widget__fill", HealthWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "health-widget__damage-preview", HealthWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "health-widget__text", HealthWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "health-widget__damage-preview-text", HealthWidgetUssPath, errors);
        ValidateRequiredUssClass(uss, "health-widget--damage", HealthWidgetUssPath, errors);
    }

    private static void ValidateAdventureViewAnimationStyle(List<string> errors)
    {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AdventureViewAnimationUssPath);
        if (styleSheet == null)
        {
            errors.Add($"AdventureView animation USS asset is missing: {AdventureViewAnimationUssPath}");
            return;
        }

        string uss = File.ReadAllText(AdventureViewAnimationUssPath);
        ValidateRequiredUssSelector(
            uss,
            ".adventure-view--intro-shown .card-deck",
            AdventureViewAnimationUssPath,
            errors);
    }

    private static void ValidateSkillSlotStyle(List<string> errors)
    {
        StyleSheet skillSlotStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(SkillSlotUssPath);
        if (skillSlotStyle == null)
        {
            errors.Add($"SkillSlot USS asset is missing: {SkillSlotUssPath}");
            return;
        }

        StyleSheet skillSlotWidgetStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(SkillSlotWidgetUssPath);
        if (skillSlotWidgetStyle == null)
        {
            errors.Add($"SkillSlotWidget USS asset is missing: {SkillSlotWidgetUssPath}");
            return;
        }

        string skillSlotUss = File.ReadAllText(SkillSlotUssPath);
        ValidateRequiredUssClass(skillSlotUss, "skill-slot", SkillSlotUssPath, errors);
        ValidateRequiredUssClass(skillSlotUss, "skill-slot__frame", SkillSlotUssPath, errors);
        ValidateRequiredUssClass(skillSlotUss, "skill-slot__icon", SkillSlotUssPath, errors);
        ValidateRequiredUssClass(skillSlotUss, "skill-slot--has-icon", SkillSlotUssPath, errors);

        string skillSlotWidgetUss = File.ReadAllText(SkillSlotWidgetUssPath);
        ValidateRequiredUssClass(skillSlotWidgetUss, "skill-slot-widget", SkillSlotWidgetUssPath, errors);
        ValidateRequiredUssClass(skillSlotWidgetUss, "skill-slot-button", SkillSlotWidgetUssPath, errors);
    }

    private static void ValidateRequiredUssClass(
        string uss,
        string className,
        string stylePath,
        List<string> errors)
    {
        if (uss.Contains($".{className}", StringComparison.Ordinal))
            return;

        errors.Add($"Required USS class is missing in {stylePath}: .{className}");
    }

    private static void ValidateRequiredUssSelector(
        string uss,
        string selector,
        string stylePath,
        List<string> errors)
    {
        if (uss.Contains(selector, StringComparison.Ordinal))
            return;

        errors.Add($"Required USS selector is missing in {stylePath}: {selector}");
    }

    private static void ValidateRequiredUssSelectorFragment(
        string uss,
        string selector,
        string fragment,
        string stylePath,
        List<string> errors)
    {
        int selectorIndex = uss.IndexOf(selector, StringComparison.Ordinal);
        if (selectorIndex < 0)
        {
            errors.Add($"Required USS selector is missing in {stylePath}: {selector}");
            return;
        }

        int blockStart = uss.IndexOf('{', selectorIndex);
        int blockEnd = blockStart >= 0 ? uss.IndexOf('}', blockStart) : -1;
        if (blockStart < 0 || blockEnd < 0)
        {
            errors.Add($"Required USS selector block is malformed in {stylePath}: {selector}");
            return;
        }

        string block = uss.Substring(blockStart, blockEnd - blockStart);
        if (block.Contains(fragment, StringComparison.Ordinal))
            return;

        errors.Add($"Required USS fragment is missing in {stylePath}: {selector} -> {fragment}");
    }

    private static void ValidateStyleReference(
        string uxml,
        string stylePath,
        List<string> errors,
        string ownerPath = AdventureViewUxmlPath)
    {
        string fileName = Path.GetFileName(stylePath);
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
        if (styleSheet == null)
        {
            errors.Add($"Required USS asset is missing: {stylePath}");
            return;
        }

        if (uxml.Contains(fileName, StringComparison.Ordinal))
        {
            ValidateStyleReferenceGuid(
                uxml,
                fileName,
                AssetDatabase.AssetPathToGUID(stylePath),
                stylePath,
                ownerPath,
                errors);
            return;
        }

        errors.Add($"{ownerPath} must include required USS: {stylePath}");
    }

    private static void ValidateStyleReferenceGuid(
        string uxml,
        string fileName,
        string expectedGuid,
        string stylePath,
        string ownerPath,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(expectedGuid))
            return;

        using StringReader reader = new(uxml);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!line.Contains(fileName, StringComparison.Ordinal))
                continue;

            if (!line.Contains("guid=", StringComparison.Ordinal))
                continue;

            if (line.Contains($"guid={expectedGuid}", StringComparison.Ordinal))
                continue;

            errors.Add(
                $"{ownerPath} includes {Path.GetFileName(stylePath)} with a stale GUID. Expected guid={expectedGuid} for {stylePath}.");
        }
    }

    private static void ValidateWidgetTemplate(
        AddressableAssetSettings settings,
        string widgetUxmlPath,
        string widgetAddress,
        List<string> errors)
    {
        VisualTreeAsset template =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(widgetUxmlPath);

        if (template == null)
        {
            errors.Add($"Adventure card widget UXML is missing: {widgetUxmlPath}");
            return;
        }

        ValidateAddressableEntry(
            settings,
            widgetUxmlPath,
            widgetAddress,
            AdventureSceneAddressables.AdventureSceneLabel,
            errors);

        ValidateWidgetStyleReferences(widgetUxmlPath, widgetAddress, errors);

        ValidateWidgetTree(
            template,
            widgetUxmlPath,
            widgetAddress,
            errors);
    }

    private static void ValidateWidgetStyleReferences(
        string widgetUxmlPath,
        string widgetAddress,
        List<string> errors)
    {
        if (!File.Exists(widgetUxmlPath))
            return;

        string uxml = File.ReadAllText(widgetUxmlPath);
        switch (widgetAddress)
        {
            case AdventureSceneAddressables.AdventurePlayerCardWidget:
                ValidateStyleReference(uxml, CardWidgetUssPath, errors, widgetUxmlPath);
                ValidateStyleReference(uxml, PlayerWidgetUssPath, errors, widgetUxmlPath);
                ValidateStyleReference(uxml, HealthWidgetUssPath, errors, widgetUxmlPath);
                break;

            case AdventureSceneAddressables.AdventureChoiceCardWidget:
                ValidateStyleReference(uxml, ChoiceWidgetUssPath, errors, widgetUxmlPath);
                break;

            case AdventureSceneAddressables.AdventureMonsterCardWidget:
                ValidateStyleReference(uxml, CardWidgetUssPath, errors, widgetUxmlPath);
                ValidateStyleReference(uxml, MonsterWidgetUssPath, errors, widgetUxmlPath);
                ValidateStyleReference(uxml, HealthWidgetUssPath, errors, widgetUxmlPath);
                break;

            case AdventureSceneAddressables.AdventureDisplayCardWidget:
                ValidateStyleReference(uxml, CardWidgetUssPath, errors, widgetUxmlPath);
                ValidateStyleReference(uxml, DisplayWidgetUssPath, errors, widgetUxmlPath);
                break;
        }
    }

    private static void ValidateWidgetTree(
        VisualTreeAsset template,
        string widgetUxmlPath,
        string widgetAddress,
        List<string> errors)
    {
        TemplateContainer container;
        try
        {
            container = template.Instantiate();
        }
        catch (Exception exception)
        {
            errors.Add($"Failed to instantiate UXML: {widgetUxmlPath}. {exception.Message}");
            return;
        }

        switch (widgetAddress)
        {
            case AdventureSceneAddressables.AdventureChoiceCardWidget:
                ValidateChoiceWidgetTree(container, widgetUxmlPath, errors);
                break;

            case AdventureSceneAddressables.AdventurePlayerCardWidget:
                ValidatePlayerWidgetTree(container, widgetUxmlPath, errors);
                break;

            case AdventureSceneAddressables.AdventureMonsterCardWidget:
                ValidateMonsterWidgetTree(container, widgetUxmlPath, errors);
                break;

            case AdventureSceneAddressables.AdventureDisplayCardWidget:
                ValidateDisplayWidgetTree(container, widgetUxmlPath, errors);
                break;

            case AdventureSceneAddressables.PortraitCardFaceWidget:
                ValidatePortraitFaceWidgetTree(container, widgetUxmlPath, errors);
                break;

            case AdventureSceneAddressables.LockedCardFaceWidget:
                ValidateRequired<LockedCardFaceWidget>(
                    container,
                    "locked-card",
                    widgetUxmlPath,
                    errors);
                break;

            case AdventureSceneAddressables.SkillSlotWidget:
                ValidateSkillSlotWidgetTree(container, widgetUxmlPath, errors);
                break;
        }
    }

    private static void ValidateChoiceWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequired<VisualElement>(root, "card-root", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "card-background", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "card-frame", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "card-content", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "choice-card-icon", widgetUxmlPath, errors);
        ValidateRequired<Label>(root, "choice-card-label", widgetUxmlPath, errors);
    }

    private static void ValidatePortraitFaceWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequired<PortraitCardFaceWidget>(
            root,
            "portrait-card",
            widgetUxmlPath,
            errors);
        ValidateRequired<VisualElement>(root, "card-portrait", widgetUxmlPath, errors);
        ValidateRequired<Label>(root, "card-front-name", widgetUxmlPath, errors);
    }

    private static void ValidatePlayerWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequired<AdventurePlayerCardWidget>(
            root,
            "adventure-player-card",
            widgetUxmlPath,
            errors);
        ValidateRequired<HealthWidget>(
            root,
            "adventure-player-card-health-widget",
            widgetUxmlPath,
            errors);
        ValidateRequired<CardWidget>(
            root,
            "adventure-player-card-widget",
            widgetUxmlPath,
            errors);
        ValidateHealthWidgetTree(root, widgetUxmlPath, errors);
        ValidateCardWidgetTree(root, widgetUxmlPath, errors);
    }

    private static void ValidateMonsterWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequired<AdventureMonsterCardWidget>(
            root,
            "adventure-monster-card",
            widgetUxmlPath,
            errors);
        ValidateRequired<IntentBadgeWidget>(
            root,
            "adventure-monster-card-intent-badge",
            widgetUxmlPath,
            errors);
        ValidateRequired<HealthWidget>(
            root,
            "adventure-monster-card-health-widget",
            widgetUxmlPath,
            errors);
        ValidateRequired<CardWidget>(
            root,
            "adventure-monster-card-widget",
            widgetUxmlPath,
            errors);
        ValidateIntentBadgeWidgetTree(root, widgetUxmlPath, errors);
        ValidateHealthWidgetTree(root, widgetUxmlPath, errors);
        ValidateCardWidgetTree(root, widgetUxmlPath, errors);
    }

    private static void ValidateDisplayWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequired<AdventureDisplayCardWidget>(
            root,
            "adventure-display-card",
            widgetUxmlPath,
            errors);
        ValidateRequired<CardWidget>(
            root,
            "adventure-display-card-widget",
            widgetUxmlPath,
            errors);
        ValidateCardWidgetTree(root, widgetUxmlPath, errors);
    }

    private static void ValidateCardWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequired<VisualElement>(root, "card-v2-front-slot", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "card-v2-back-slot", widgetUxmlPath, errors);
    }

    private static void ValidateHealthWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequiredClass(root, "health-widget", widgetUxmlPath, errors);
        ValidateRequiredClass(root, "ui-transition--hidden", widgetUxmlPath, errors);
        ValidateRequiredClass(root, "ui-transition--from-bottom", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "health-widget-fill-clip", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "health-widget-damage-preview", widgetUxmlPath, errors);
        ValidateRequired<Label>(root, "health-widget-text", widgetUxmlPath, errors);
        ValidateRequired<Label>(root, "health-widget-damage-preview-text", widgetUxmlPath, errors);
    }

    private static void ValidateIntentBadgeWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequired<VisualElement>(root, "intent-badge-icon", widgetUxmlPath, errors);
        ValidateRequired<Label>(root, "intent-badge-number", widgetUxmlPath, errors);
    }

    private static void ValidateSkillSlotWidgetTree(
        VisualElement root,
        string widgetUxmlPath,
        List<string> errors)
    {
        ValidateRequiredClass(root, "skill-slot", widgetUxmlPath, errors);
        ValidateRequiredClass(root, "skill-slot-button", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "skill-slot-frame", widgetUxmlPath, errors);
        ValidateRequired<VisualElement>(root, "skill-slot-icon", widgetUxmlPath, errors);
    }

    private static void ValidateRequiredClass(
        VisualElement root,
        string className,
        string widgetUxmlPath,
        List<string> errors)
    {
        if (root.Q<VisualElement>(className: className) != null)
            return;

        errors.Add(
            $"Required UXML class is missing in {widgetUxmlPath}: .{className}.");
    }

    private static void ValidateRequired<T>(
        VisualElement root,
        string elementName,
        string widgetUxmlPath,
        List<string> errors)
        where T : VisualElement
    {
        if (root.Q<T>(elementName) != null)
            return;

        errors.Add(
            $"Required UXML element is missing in {widgetUxmlPath}: {typeof(T).Name} '{elementName}'.");
    }

    private static void ValidateUniqueAddress(
        AddressableAssetSettings settings,
        List<string> errors)
    {
        Dictionary<string, List<string>> entriesByAddress = new(StringComparer.Ordinal);
        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
                continue;

            foreach (AddressableAssetEntry entry in group.entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.address))
                    continue;

                if (!entriesByAddress.TryGetValue(entry.address, out List<string> entries))
                {
                    entries = new List<string>();
                    entriesByAddress[entry.address] = entries;
                }

                entries.Add($"{group.Name}:{entry.guid}");
            }
        }

        foreach (KeyValuePair<string, List<string>> pair in entriesByAddress)
        {
            if (pair.Value.Count <= 1)
                continue;

            errors.Add(
                $"Addressables address is duplicated: {pair.Key} ({string.Join(", ", pair.Value)})");
        }
    }

    private static void ValidateTable(
        AddressableAssetSettings settings,
        List<string> errors)
    {
        AdventureChoiceCardUITable table =
            AssetDatabase.LoadAssetAtPath<AdventureChoiceCardUITable>(TablePath);

        if (table == null)
        {
            errors.Add($"AdventureChoiceCardUITable is missing: {TablePath}");
            return;
        }

        ValidateAddressableEntry(
            settings,
            TablePath,
            AdventureSceneAddressables.AdventureChoiceCardUITable,
            AdventureSceneAddressables.ModelTableLabel,
            errors);

        if (RowsField == null)
        {
            errors.Add("AdventureChoiceCardUITable rows field was not found by reflection.");
            return;
        }

        if (RowsField.GetValue(table) is not List<AssetReferenceT<AdventureChoiceCardUIModel>> rows)
        {
            errors.Add("AdventureChoiceCardUITable rows field has an unexpected type.");
            return;
        }

        EChoiceCardType[] values =
            (EChoiceCardType[])Enum.GetValues(typeof(EChoiceCardType));

        if (rows.Count != values.Length)
        {
            errors.Add(
                $"AdventureChoiceCardUITable row count mismatch. Expected {values.Length}, actual {rows.Count}.");
        }

        int count = Math.Min(rows.Count, values.Length);
        for (int i = 0; i < count; i++)
        {
            ValidateRow(settings, values[i], rows[i], errors);
        }
    }

    private static void ValidateAdventureRegions(
        AddressableAssetSettings settings,
        List<string> errors)
    {
        AdventureTable table =
            AssetDatabase.LoadAssetAtPath<AdventureTable>(AdventureTablePath);

        if (table == null)
        {
            errors.Add($"AdventureTable is missing: {AdventureTablePath}");
            return;
        }

        ValidateAddressableEntry(
            settings,
            AdventureTablePath,
            AdventureSceneAddressables.AdventureTable,
            AdventureSceneAddressables.ModelTableLabel,
            errors);

        if (AdventureRowsField == null)
        {
            errors.Add("AdventureTable rows field was not found by reflection.");
            return;
        }

        if (AdventureRowsField.GetValue(table) is not List<AssetReferenceT<AdventureRegionModel>> rows)
        {
            errors.Add("AdventureTable rows field has an unexpected type.");
            return;
        }

        EAdventure[] values = (EAdventure[])Enum.GetValues(typeof(EAdventure));
        if (rows.Count != values.Length)
        {
            errors.Add(
                $"AdventureTable row count mismatch. Expected {values.Length}, actual {rows.Count}.");
        }

        int count = Math.Min(rows.Count, values.Length);
        for (int i = 0; i < count; i++)
        {
            ValidateAdventureRegionRow(settings, values[i], rows[i], errors);
        }
    }

    private static void ValidateAdventureRegionRow(
        AddressableAssetSettings settings,
        EAdventure adventureId,
        AssetReferenceT<AdventureRegionModel> reference,
        List<string> errors)
    {
        string context = $"{nameof(EAdventure)}.{adventureId}";
        if (reference == null)
        {
            errors.Add($"{context}: row reference is null.");
            return;
        }

        if (!reference.RuntimeKeyIsValid())
        {
            errors.Add($"{context}: row reference runtime key is invalid.");
            return;
        }

        string guid = reference.AssetGUID;
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrWhiteSpace(path))
        {
            errors.Add($"{context}: row asset path not found for guid {guid}.");
            return;
        }

        AdventureRegionModel model =
            AssetDatabase.LoadAssetAtPath<AdventureRegionModel>(path);

        if (model == null)
        {
            errors.Add($"{context}: row asset is not an AdventureRegionModel: {path}");
            return;
        }

        ValidateAddressableEntry(
            settings,
            path,
            model.name,
            requiredLabel: null,
            errors);

        ValidateAdventureRegionModel(context, model, errors);
    }

    private static void ValidateAdventureRegionModel(
        string context,
        AdventureRegionModel model,
        List<string> errors)
    {
        ValidateLocalizedString(context, "localized region name", model.LocalizedRegionName, errors);

        AdventureEntryPresentationModel presentation = model.EntryPresentation;
        if (presentation == null)
        {
            errors.Add($"{context}: entry presentation is missing.");
            return;
        }

        ValidateLocalizedString(context, "entry title", presentation.Title, errors);
        ValidateLocalizedString(context, "entry subtitle", presentation.Subtitle, errors);
    }

    private static void ValidateRow(
        AddressableAssetSettings settings,
        EChoiceCardType choiceType,
        AssetReferenceT<AdventureChoiceCardUIModel> reference,
        List<string> errors)
    {
        string context = $"{nameof(EChoiceCardType)}.{choiceType}";
        if (reference == null)
        {
            errors.Add($"{context}: row reference is null.");
            return;
        }

        if (!reference.RuntimeKeyIsValid())
        {
            errors.Add($"{context}: row reference runtime key is invalid.");
            return;
        }

        string guid = reference.AssetGUID;
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrWhiteSpace(path))
        {
            errors.Add($"{context}: row asset path not found for guid {guid}.");
            return;
        }

        AdventureChoiceCardUIModel model =
            AssetDatabase.LoadAssetAtPath<AdventureChoiceCardUIModel>(path);

        if (model == null)
        {
            errors.Add($"{context}: row asset is not an AdventureChoiceCardUIModel: {path}");
            return;
        }

        ValidateAddressableEntry(
            settings,
            path,
            model.name,
            requiredLabel: null,
            errors);

        ValidateDisplayName(context, model.DisplayName, errors);

        if (model.Icon == null)
        {
            errors.Add($"{context}: icon is missing in {path}.");
        }

        if (string.IsNullOrWhiteSpace(model.UssClassName))
        {
            errors.Add($"{context}: USS class name is missing in {path}.");
        }
        else
        {
            ValidateChoiceCardTypeClass(context, model.UssClassName, errors);
        }
    }

    private static void ValidateChoiceCardTypeClass(
        string context,
        string className,
        List<string> errors)
    {
        if (!File.Exists(ChoiceWidgetUssPath))
        {
            errors.Add($"{context}: choice card USS asset is missing: {ChoiceWidgetUssPath}");
            return;
        }

        string uss = File.ReadAllText(ChoiceWidgetUssPath);
        if (uss.Contains($".{className}", StringComparison.Ordinal))
            return;

        errors.Add(
            $"{context}: USS class '{className}' is missing in {ChoiceWidgetUssPath}.");
    }

    private static void ValidateDisplayName(
        string context,
        LocalizedString localizedString,
        List<string> errors)
    {
        ValidateLocalizedString(context, "display name", localizedString, errors);
    }

    private static void ValidateLocalizedString(
        string context,
        string fieldName,
        LocalizedString localizedString,
        List<string> errors)
    {
        if (localizedString == null || localizedString.IsEmpty)
        {
            errors.Add($"{context}: {fieldName} LocalizedString is missing.");
            return;
        }

        string tableName = localizedString.TableReference.TableCollectionName;
        string entryKey = localizedString.TableEntryReference.Key;
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(entryKey))
        {
            errors.Add($"{context}: {fieldName} table or entry key is empty.");
            return;
        }

        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(tableName);

        if (collection == null)
        {
            errors.Add($"{context}: {fieldName} localization table collection is missing: {tableName}");
            return;
        }

        foreach (StringTable table in collection.StringTables)
        {
            if (table.GetEntry(entryKey) == null)
            {
                errors.Add(
                    $"{context}: {fieldName} localization entry '{entryKey}' is missing in {table.LocaleIdentifier.Code}.");
            }
        }
    }

    private static void ValidateAddressableEntry(
        AddressableAssetSettings settings,
        string assetPath,
        string expectedAddress,
        string requiredLabel,
        List<string> errors)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(guid))
        {
            errors.Add($"Addressables validation failed. Asset guid not found: {assetPath}");
            return;
        }

        AddressableAssetEntry entry = settings.FindAssetEntry(guid);
        if (entry == null)
        {
            errors.Add($"Addressables entry is missing: {assetPath}");
            return;
        }

        if (!string.Equals(entry.address, expectedAddress, StringComparison.Ordinal))
        {
            errors.Add(
                $"Addressables address mismatch for {assetPath}. Expected '{expectedAddress}', actual '{entry.address}'.");
        }

        if (!string.IsNullOrWhiteSpace(requiredLabel) && !entry.labels.Contains(requiredLabel))
        {
            errors.Add(
                $"Addressables label '{requiredLabel}' is missing for {assetPath}.");
        }
    }
}
