using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : Singleton<CharacterManager>
{
    public BuffStackStorage buffStackStorage;
    private List<Character> activityCharacters = new List<Character>(900);

    public void ActivityCharacter(Character _character)
    {
        activityCharacters.Add(_character);
    }
    public void InactiveCharacter(Character _character)
    {
        if (activityCharacters.Contains(_character))
            activityCharacters.Remove(_character);
    }
    public List<Character> GetActivityCharacters()
    {
        return activityCharacters;
    }
    public BuffStackData GetCharacterBuffStackData(string _key)
    {
        if (!buffStackStorage.ContainsKey(_key))
        {
            Debug.LogError(buffStackStorage.name + " does not have a\"" + _key + "\" key.");
            return null;
        }
        return buffStackStorage.GetValue(_key);
    }
    public void StopAllCharacterUpdates()
    {
        foreach (Character character in activityCharacters)
            character.SetCharacterUpdate(false);
    }
}
