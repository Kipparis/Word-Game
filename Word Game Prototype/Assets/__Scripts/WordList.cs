using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour {
    static public WordList S;   // Синглтон

    public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;

    public bool ________________;

    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;

    // Некоторые переменные сделанны приватными чтобы скрыть их от появления в проспекторе
    // Появление таких больших переменных, может значительно снизить скорость
    // Сделанны так чтобы были видны только в классе WordList
    private string[] lines;
    private List<string> longWords;
    private List<string> words;

    void Awake() {
        S = this;
    }

    public void Init() {
        // Разрезаем текст по линиям, что создаёт огромный моссив
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;

        // Создаёт соработу ParseLines(). Соработы могут быть приостановленны в середине
        // чтобы позволить другому коду выполниться
        StartCoroutine(ParseLines());
    }

    // Все соработы имеют IEnumerator как возвращаемый тип
    public IEnumerator ParseLines() {
        string word;
        // Создаём лист чтобы обрабатывать все длинные слова и подходящие слова
        longWords = new List<string>();
        words = new List<string>();

        for (currLine = 0; currLine < totalLines; currLine++) {
            word = lines[currLine];

            // Если слово такое же по длинне как wordLengthMax
            if(word.Length == wordLengthMax) {
                // Записываем
                longWords.Add(word);
            }
            // Если оно между максимальным и минимальным значением...
            if (word.Length >= wordLengthMin && word.Length <= wordLengthMax) {
                // Добавляем к словарю подходящих слов
                words.Add(word);
            }

            // Определяем когда соработа должна уступить
            // используем % модуль чтобы уступать каждую 10000-ую запись
            if (currLine % numToParseBeforeYield == 0) {
                // Подсчитываем слова в каждом списке чтобы показать что
                // передача в прогрессе
                longWordCount = longWords.Count;
                wordCount = words.Count;
                // Уступает выполнение до следующего кадра
                yield return null;

                // yield говорит что мы ждём здесь до тех пор пока другой код не выполнится
            }
        }
        this.gameObject.SendMessage("WordListParseComplete");
    }

    // Эти методы позволяют другим класса получить приватные списки
    public List<string> GetWords() {
        return (words);
    }

    public string GetWord(int ndx) {
        return (words[ndx]);
    }

    public List<string> GetLongWords() {
        return (longWords);
    }

    public string GetLongWord(int ndx) {
        return (longWords[ndx]);
    }
}
