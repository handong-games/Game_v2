using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Data;
using Game.Scenes.Adventure;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class CodexAdventureChoiceCardUIAssetGenerator
{
    private const string AssetFolder = "Assets/@Resources/Model/Adventure/ChoiceCards";
    private const string IconFolder = "Assets/@Resources/UI/ChoiceCards";
    private const string TablePath = "Assets/@Resources/Model/Tables/AdventureChoiceCardUITable.asset";
    private const string AdventureViewUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventureView.uxml";
    private const string ChoiceWidgetUxmlPath = "Assets/@Scripts/Domains/View/Card/AdventureChoiceCardWidget.uxml";
    private const string PlayerWidgetUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventurePlayerCardWidget.uxml";
    private const string MonsterWidgetUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventureMonsterCardWidget.uxml";
    private const string DisplayWidgetUxmlPath = "Assets/@Scripts/Domains/View/Adventure/AdventureDisplayCardWidget.uxml";
    private const string PortraitFaceWidgetUxmlPath = "Assets/@Scripts/Domains/View/Card/PortraitCardFaceWidget/PortraitCardFaceWidget.uxml";
    private const string LockedFaceWidgetUxmlPath = "Assets/@Scripts/Domains/View/Card/LockedCardFaceWidget/LockedCardFaceWidget.uxml";
    private const string SkillSlotWidgetUxmlPath = "Assets/@Scripts/Domains/View/Common/SkillSlotWidget.uxml";
    private const string LocalizationCollection = "AdventureView";
    private const string DefaultGroupName = "Default Local Group";
    private const string UIGroupName = "UI";

    private static readonly FieldInfo NameField =
        typeof(AbstractModel).GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo DisplayNameField =
        typeof(AdventureChoiceCardUIModel).GetField("_displayName", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo IconField =
        typeof(AdventureChoiceCardUIModel).GetField("_icon", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo UssClassNameField =
        typeof(AdventureChoiceCardUIModel).GetField("_ussClassName", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo RowsField =
        typeof(AbstractTable<AdventureChoiceCardUIModel, EChoiceCardType>)
            .GetField("_rows", BindingFlags.Instance | BindingFlags.NonPublic);

    private sealed class Definition
    {
        public Definition(
            EChoiceCardType type,
            string assetName,
            string localizationKey,
            string iconPath,
            string korean,
            string english,
            string japanese,
            string ussClassName)
        {
            Type = type;
            AssetName = assetName;
            LocalizationKey = localizationKey;
            IconPath = iconPath;
            Korean = korean;
            English = english;
            Japanese = japanese;
            UssClassName = ussClassName;
        }

        public EChoiceCardType Type { get; }
        public string AssetName { get; }
        public string LocalizationKey { get; }
        public string IconPath { get; }
        public string Korean { get; }
        public string English { get; }
        public string Japanese { get; }
        public string UssClassName { get; }
    }

    private static readonly Definition[] Definitions =
    {
        new(
            EChoiceCardType.Monster,
            "ChoiceCardUI_Monster",
            "choice_card_monster",
            $"{IconFolder}/choice-card-icon-monster.png",
            "몬스터",
            "Monster",
            "モンスター",
            "choice-card--monster"),
        new(
            EChoiceCardType.Elite,
            "ChoiceCardUI_Elite",
            "choice_card_elite",
            $"{IconFolder}/choice-card-icon-elite.png",
            "엘리트",
            "Elite",
            "エリート",
            "choice-card--elite"),
        new(
            EChoiceCardType.Boss,
            "ChoiceCardUI_Boss",
            "choice_card_boss",
            $"{IconFolder}/choice-card-icon-boss.png",
            "보스",
            "Boss",
            "ボス",
            "choice-card--boss"),
        new(
            EChoiceCardType.Event,
            "ChoiceCardUI_Event",
            "choice_card_event",
            $"{IconFolder}/choice-card-icon-event.png",
            "이벤트",
            "Event",
            "イベント",
            "choice-card--event"),
        new(
            EChoiceCardType.Shop,
            "ChoiceCardUI_Shop",
            "choice_card_shop",
            $"{IconFolder}/choice-card-icon-shop.png",
            "상점",
            "Shop",
            "ショップ",
            "choice-card--shop"),
    };

    [MenuItem("Tools/Codex/Generate Adventure Choice Card UI Assets")]
    public static void Generate()
    {
        EnsureFolders();
        EnsureLocalizationEntries();

        Dictionary<EChoiceCardType, AdventureChoiceCardUIModel> models = new();
        foreach (Definition definition in Definitions)
        {
            AdventureChoiceCardUIModel model = CreateOrUpdateModel(definition);
            models[definition.Type] = model;
        }

        AdventureChoiceCardUITable table = CreateOrUpdateTable(models);
        ConfigureAddressables(table, models);
        ConfigureWidgetAddressables();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Adventure choice card UI assets generated.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/@Resources/Model/Adventure");
        EnsureFolder(AssetFolder);
        EnsureFolder(IconFolder);
        EnsureFolder("Assets/@Resources/Model/Tables");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = System.IO.Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static AdventureChoiceCardUIModel CreateOrUpdateModel(Definition definition)
    {
        string path = $"{AssetFolder}/{definition.AssetName}.asset";
        AdventureChoiceCardUIModel model =
            AssetDatabase.LoadAssetAtPath<AdventureChoiceCardUIModel>(path);

        if (model == null)
        {
            model = ScriptableObject.CreateInstance<AdventureChoiceCardUIModel>();
            AssetDatabase.CreateAsset(model, path);
        }

        model.SetId(definition.Type);
        NameField.SetValue(model, definition.AssetName);
        DisplayNameField.SetValue(
            model,
            new LocalizedString(LocalizationCollection, definition.LocalizationKey));
        IconField.SetValue(
            model,
            AssetDatabase.LoadAssetAtPath<Sprite>(definition.IconPath));
        UssClassNameField.SetValue(model, definition.UssClassName);
        EditorUtility.SetDirty(model);
        return model;
    }

    private static AdventureChoiceCardUITable CreateOrUpdateTable(
        IReadOnlyDictionary<EChoiceCardType, AdventureChoiceCardUIModel> models)
    {
        AdventureChoiceCardUITable table =
            AssetDatabase.LoadAssetAtPath<AdventureChoiceCardUITable>(TablePath);

        if (table == null)
        {
            table = ScriptableObject.CreateInstance<AdventureChoiceCardUITable>();
            AssetDatabase.CreateAsset(table, TablePath);
        }

        List<AssetReferenceT<AdventureChoiceCardUIModel>> rows = new();
        foreach (EChoiceCardType type in Enum.GetValues(typeof(EChoiceCardType)))
        {
            AdventureChoiceCardUIModel model = models[type];
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(model));
            rows.Add(new AssetReferenceT<AdventureChoiceCardUIModel>(guid));
        }

        RowsField.SetValue(table, rows);
        EditorUtility.SetDirty(table);
        return table;
    }

    private static void ConfigureAddressables(
        AdventureChoiceCardUITable table,
        IReadOnlyDictionary<EChoiceCardType, AdventureChoiceCardUIModel> models)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            throw new InvalidOperationException("AddressableAssetSettings not found.");

        AddressableAssetGroup group = settings.FindGroup(DefaultGroupName) ?? settings.DefaultGroup;

        foreach (AdventureChoiceCardUIModel model in models.Values)
        {
            string path = AssetDatabase.GetAssetPath(model);
            string guid = AssetDatabase.AssetPathToGUID(path);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = model.name;
        }

        string tableGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
        AddressableAssetEntry tableEntry = settings.CreateOrMoveEntry(tableGuid, group);
        tableEntry.address = AdventureSceneAddressables.AdventureChoiceCardUITable;
        tableEntry.SetLabel(AdventureSceneAddressables.ModelTableLabel, true, true);

        EditorUtility.SetDirty(settings);
    }

    private static void ConfigureWidgetAddressables()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            throw new InvalidOperationException("AddressableAssetSettings not found.");

        AddressableAssetGroup group = settings.FindGroup(UIGroupName) ?? settings.DefaultGroup;
        ConfigureWidgetAddressable(settings, group, AdventureViewUxmlPath, AdventureSceneAddressables.AdventureView);
        ConfigureWidgetAddressable(settings, group, ChoiceWidgetUxmlPath, AdventureSceneAddressables.AdventureChoiceCardWidget);
        ConfigureWidgetAddressable(settings, group, PlayerWidgetUxmlPath, AdventureSceneAddressables.AdventurePlayerCardWidget);
        ConfigureWidgetAddressable(settings, group, MonsterWidgetUxmlPath, AdventureSceneAddressables.AdventureMonsterCardWidget);
        ConfigureWidgetAddressable(settings, group, DisplayWidgetUxmlPath, AdventureSceneAddressables.AdventureDisplayCardWidget);
        ConfigureWidgetAddressable(settings, group, PortraitFaceWidgetUxmlPath, AdventureSceneAddressables.PortraitCardFaceWidget);
        ConfigureWidgetAddressable(settings, group, LockedFaceWidgetUxmlPath, AdventureSceneAddressables.LockedCardFaceWidget);
        ConfigureWidgetAddressable(settings, group, SkillSlotWidgetUxmlPath, AdventureSceneAddressables.SkillSlotWidget);

        EditorUtility.SetDirty(settings);
    }

    private static void ConfigureWidgetAddressable(
        AddressableAssetSettings settings,
        AddressableAssetGroup group,
        string widgetUxmlPath,
        string widgetAddress)
    {
        string guid = AssetDatabase.AssetPathToGUID(widgetUxmlPath);
        if (string.IsNullOrWhiteSpace(guid))
            throw new InvalidOperationException($"Adventure card widget UXML not found: {widgetUxmlPath}");

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = widgetAddress;
        entry.SetLabel(AdventureSceneAddressables.AdventureSceneLabel, true, true);
    }

    private static void EnsureLocalizationEntries()
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationCollection);

        if (collection == null)
            throw new InvalidOperationException($"StringTableCollection not found: {LocalizationCollection}");

        foreach (Definition definition in Definitions)
        {
            foreach (StringTable table in collection.StringTables)
            {
                string value = GetLocalizedValue(table.LocaleIdentifier.Code, definition);
                table.AddEntry(definition.LocalizationKey, value);
                EditorUtility.SetDirty(table);
            }
        }

        EditorUtility.SetDirty(collection.SharedData);
    }

    private static string GetLocalizedValue(string localeCode, Definition definition)
    {
        return localeCode switch
        {
            "ko-KR" => definition.Korean,
            "ja-JP" => definition.Japanese,
            _ => definition.English,
        };
    }
}
