using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character Database")]
    public class SO_CharacterDatabase : ScriptableObject
    {
        [SerializeField]
        private List<SO_CharacterData> _characters = new List<SO_CharacterData>();

        private Dictionary<int, SO_CharacterData> _characterMapById;

        private void OnEnable()
        {
            BuildMap();
        }

        public void BuildMap()
        {
            _characterMapById = new Dictionary<int, SO_CharacterData>();
            if (_characters == null)
                return;

            for (int i = 0; i < _characters.Count; i++)
            {
                SO_CharacterData character = _characters[i];
                if (character == null || character.Id <= 0)
                    continue;

                if (_characterMapById.ContainsKey(character.Id))
                {
                    Debug.LogWarning($"[SO_CharacterDatabase] Duplicate character id: {character.Id} ({character.name})");
                    continue;
                }

                _characterMapById.Add(character.Id, character);
            }
        }

        public SO_CharacterData GetCharacter(int id)
        {
            if (_characterMapById == null)
            {
                BuildMap();
            }

            _characterMapById.TryGetValue(id, out SO_CharacterData result);
            return result;
        }

        public List<SO_CharacterData> GetAllCharacters()
        {
            return _characters;
        }

        public void SetCharacters(List<SO_CharacterData> characters)
        {
            _characters = characters ?? new List<SO_CharacterData>();
            BuildMap();
        }
    }
}
