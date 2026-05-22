using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Data.Editor
{
    public static class CardFaceAssetGenerator
    {
        private const string RootPath = "Assets/@Resources/CardFaces";
        private const string CommonPath = RootPath + "/Common";
        private const string CharacterPath = RootPath + "/Characters";
        private const string MonsterPath = RootPath + "/Monsters";
        private const string FrontFieldName = "_front";
        private const string BackFieldName = "_back";
        private const string LocalizedNameFieldName = "_localizedName";
        private const string PortraitFieldName = "_portrait";

        [MenuItem("Game/Tools/Card Faces/Generate Missing Card Faces")]
        public static void GenerateMissingCardFaces()
        {
            EnsureFolders();

            LockedCardFaceModel lockedFace = EnsureAsset<LockedCardFaceModel>(
                CommonPath,
                "Character_LockedFace");

            ChoiceCardFaceModel monsterFace = EnsureChoiceFace("Choice_MonsterFace", EChoiceCardType.Monster);
            ChoiceCardFaceModel eliteFace = EnsureChoiceFace("Choice_EliteFace", EChoiceCardType.Elite);
            ChoiceCardFaceModel bossFace = EnsureChoiceFace("Choice_BossFace", EChoiceCardType.Boss);
            ChoiceCardFaceModel eventFace = EnsureChoiceFace("Choice_EventFace", EChoiceCardType.Event);
            ChoiceCardFaceModel shopFace = EnsureChoiceFace("Choice_ShopFace", EChoiceCardType.Shop);

            GenerateCharacterFaces(lockedFace);
            GenerateMonsterFaces(monsterFace, eliteFace, bossFace);
            GenerateEventFaces(eventFace);
            GenerateShopFaces(shopFace);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated missing card face assets.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "@Resources");
            EnsureFolder("Assets/@Resources", "CardFaces");
            EnsureFolder(RootPath, "Common");
            EnsureFolder(RootPath, "Characters");
            EnsureFolder(RootPath, "Monsters");
        }

        private static void EnsureFolder(string parentPath, string folderName)
        {
            string path = $"{parentPath}/{folderName}";
            if (AssetDatabase.IsValidFolder(path))
                return;

            AssetDatabase.CreateFolder(parentPath, folderName);
        }

        private static ChoiceCardFaceModel EnsureChoiceFace(string assetName, EChoiceCardType styleType)
        {
            ChoiceCardFaceModel face = EnsureAsset<ChoiceCardFaceModel>(CommonPath, assetName);
            AssignChoiceFaceData(face, styleType);
            return face;
        }

        private static void GenerateCharacterFaces(LockedCardFaceModel lockedFace)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(CharacterModel)}");
            for (int i = 0; i < guids.Length; i++)
            {
                CharacterModel model = LoadAsset<CharacterModel>(guids[i]);
                if (model == null)
                    continue;

                PortraitCardFaceModel frontFace = EnsurePortraitFace(
                    CharacterPath,
                    $"{model.Name}_FrontFace",
                    model);

                AssignFaceIfMissing(model, FrontFieldName, frontFace);
                AssignFaceIfMissing(model, BackFieldName, lockedFace);
            }
        }

        private static void GenerateMonsterFaces(
            ChoiceCardFaceModel monsterFace,
            ChoiceCardFaceModel eliteFace,
            ChoiceCardFaceModel bossFace)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(MonsterModel)}");
            for (int i = 0; i < guids.Length; i++)
            {
                MonsterModel model = LoadAsset<MonsterModel>(guids[i]);
                if (model == null)
                    continue;

                PortraitCardFaceModel frontFace = EnsurePortraitFace(
                    MonsterPath,
                    $"{model.Name}_FrontFace",
                    model);

                AssignFaceIfMissing(model, FrontFieldName, frontFace);
                AssignFaceIfMissing(model, BackFieldName, GetMonsterBackFace(
                    model,
                    monsterFace,
                    eliteFace,
                    bossFace));
            }
        }

        private static void GenerateEventFaces(ChoiceCardFaceModel eventFace)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(EventModel)}");
            for (int i = 0; i < guids.Length; i++)
            {
                EventModel model = LoadAsset<EventModel>(guids[i]);
                if (model == null)
                    continue;

                AssignFaceIfMissing(model, BackFieldName, eventFace);
            }
        }

        private static void GenerateShopFaces(ChoiceCardFaceModel shopFace)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ShopModel)}");
            for (int i = 0; i < guids.Length; i++)
            {
                ShopModel model = LoadAsset<ShopModel>(guids[i]);
                if (model == null)
                    continue;

                AssignFaceIfMissing(model, BackFieldName, shopFace);
            }
        }

        private static ChoiceCardFaceModel GetMonsterBackFace(
            MonsterModel model,
            ChoiceCardFaceModel monsterFace,
            ChoiceCardFaceModel eliteFace,
            ChoiceCardFaceModel bossFace)
        {
            return model.Rank switch
            {
                EMonsterRank.Boss => bossFace,
                EMonsterRank.Elite => eliteFace,
                _ => monsterFace,
            };
        }

        private static PortraitCardFaceModel EnsurePortraitFace(
            string folderPath,
            string assetName,
            CharacterModel model)
        {
            PortraitCardFaceModel face = EnsureAsset<PortraitCardFaceModel>(
                folderPath,
                assetName);

            AssignPortraitFaceData(face, model.LocalizedName, model.Portrait);
            return face;
        }

        private static PortraitCardFaceModel EnsurePortraitFace(
            string folderPath,
            string assetName,
            MonsterModel model)
        {
            PortraitCardFaceModel face = EnsureAsset<PortraitCardFaceModel>(
                folderPath,
                assetName);

            AssignPortraitFaceData(face, model.LocalizedName, model.Portrait);
            return face;
        }

        private static void AssignPortraitFaceData(
            PortraitCardFaceModel face,
            UnityEngine.Localization.LocalizedString localizedName,
            Sprite portrait)
        {
            SerializedObject serializedObject = new(face);
            serializedObject.FindProperty(LocalizedNameFieldName).boxedValue = localizedName;
            serializedObject.FindProperty(PortraitFieldName).objectReferenceValue = portrait;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(face);
        }

        private static void AssignChoiceFaceData(
            ChoiceCardFaceModel face,
            EChoiceCardType styleType)
        {
            SerializedObject serializedObject = new(face);
            serializedObject.FindProperty("_styleType").enumValueIndex = (int)styleType;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(face);
        }

        private static void AssignFaceIfMissing(
            Object model,
            string fieldName,
            CardFaceModel face)
        {
            SerializedObject serializedObject = new(model);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property.objectReferenceValue != null)
                return;

            property.objectReferenceValue = face;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(model);
        }

        private static T EnsureAsset<T>(string folderPath, string assetName)
            where T : ScriptableObject
        {
            string path = $"{folderPath}/{assetName}.asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static T LoadAsset<T>(string guid)
            where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
