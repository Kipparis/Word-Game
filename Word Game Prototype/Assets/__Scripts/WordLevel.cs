using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordLevel {    // Не наследует MonoBehaviour
    public int levelNum;
    public int longWordIndex;
    public string word;
    // Словарь всех букв в слове
    public Dictionary<char, int> charDict;
    // Все слова что могут быть сделанны из букв в charDict
    public List<string> subWords;

    // Статичная функция которая считает буквы в строке и возвращает словарь
    static public Dictionary<char,int> MakeCharDict(string w) {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        foreach (char c in w) {
            if (dict.ContainsKey(c)) {
                dict[c]++;
            } else {
                dict.Add(c, 1);
            }
        }
        return (dict);
    }

    // Статичный метод даёт знать можно ли взять слово с текущим level.charDict
    public static bool CheckWordInLevel(string str, WordLevel level) {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        foreach (char c in str) {
            // Если charDict содержит такую букву
            if (level.charDict.ContainsKey(c)) {
                // Если наш счётчик ещё не содержит такой буквы как ключа
                if (!counts.ContainsKey(c)) {
                    // Добавляем новый ключ
                    counts.Add(c, 1);
                } else { // counts содержит такой ключ
                    counts[c]++;
                }
                // Если в переданном слове больше каких то букв чем 
                // в charDict, значит слово недоступно
                if (counts[c] > level.charDict[c]) {
                    return (false);
                }
            } else {
                // charDict не содержит такой буквы
                return (false);
            }
        }
        return (true);
    }
}
