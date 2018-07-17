using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public enum GameMode {
    preGame,    // Перед началом игры
    loading,    // Лист слов загружается
    makeLevel,  // Уровень создаётся
    levelPrep,  // Визуальное окружение создаётся
    inLevel // Уровень проходится
}

public class WordGame : MonoBehaviour {
    static public WordGame S;   // Синглтон

    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = Color.white;
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);

    public List<float> scoreFontSizes = new List<float> { 24, 36, 36, 1 };
    public Vector3 scoreMidPoint = new Vector3(1, 1, 0);
    public float scoreComboDelay = 0.5f;

    public Color[] wyrdPalette;

    public bool __________________;

    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    void Awake() {
        S = this;
    }

    void Start() {
        mode = GameMode.loading;
        // Говорим WordList.S обрабатывать все слова
        WordList.S.Init();
    }

    // Вызывается SendMessage() из WordList
    public void WordListParseComplete() {
        mode = GameMode.makeLevel;
        // Создаём уровень и приравниваем его к текущему уровню
        currLevel = MakeWordLevel();
    }

    // С базовым значением -1, метод создаст уровень из случайного слова
    public WordLevel MakeWordLevel(int levelNum = -1) {
        WordLevel level = new WordLevel();
        if(levelNum == -1) {
            // Выбираем рандомный уровень
            level.longWordIndex = Random.Range(0, WordList.S.longWordCount);
        } else {
            // Добавим позже
        }
        level.levelNum = levelNum;
        level.word = WordList.S.GetLongWord(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        // Создаём сопроцесс чтобы найти все подходящие слова
        StartCoroutine(FindSubWordsCoroutine(level));

        // Возвращаем уровень перед тем как сопроцесс окончится
        // так что SubWordSearchComplete() когда сопроцесс закончится
        return (level);
    }

    public IEnumerator FindSubWordsCoroutine(WordLevel level) {
        level.subWords = new List<string>();
        string str;

        List<string> words = WordList.S.GetWords();
        // ^ работает быстро т.к. возвращает ссылку

        // Повторяем среди всех слов в списке
        for (int i = 0; i < WordList.S.wordCount; i++) {
            str = words[i];
            // Проверяем может ли слово сделанно из букв
            if(WordLevel.CheckWordInLevel(str, level)) {
                level.subWords.Add(str);
            }
            // Уступаем если мы передали много слов в этот кадр
            if(i%WordList.S.numToParseBeforeYield == 0) {
                // Уступаем до следующего кадра
                yield return null;
            }
        }

        // List<string>.Sort() сортирует по алфавиту
        level.subWords.Sort();
        // Теперь сортируем по длинне
        level.subWords = SortWordsByLength(level.subWords).ToList();

        // Сопроцесс окончен, вызываем SubWordSearchComplete()
        SubWordSearchComplete();
    }

    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> sList) {
        // Используем LINQ
        var sorted = from s in sList
                     orderby s.Length ascending
                     select s;
        return sorted;
    }

    public void SubWordSearchComplete() {
        mode = GameMode.levelPrep;
        Layout();
    }

    void Layout() {
        // Распологаем буквы каждого подходящего слова на экран
        wyrds = new List<Wyrd>();

        // Объявляем много переменных которые будут использованны в этом методе
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        // Считаем как много строчек из букв вместится на экране
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        // Создаём Wyrd для каждого подходящего слова в level.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++) {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];

            // Если слово длиннее чем columnWidth, расширяем его
            columnWidth = Mathf.Max(columnWidth, word.Length);

            // Создаёт префаб буквы для каждой буквы в этом слове
            for (int j = 0; j < word.Length; j++) {
                c = word[j];    // Выбираем букву в слове
                go = Instantiate(prefabLetter) as GameObject;
                lett = go.GetComponent<Letter>();
                lett.c = c; // Задаём отображаемую букву
                // Ставим букву
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);
                // % Выстраивает несколько столбцов в линию
                pos.y -= (i % numRows) * letterSize;

                // Двигаем букву сразу в позицию над экраном
                lett.position = pos + Vector3.up * (20 + i % numRows);
                // Задаём позицию для перехода
                lett.pos = pos;
                // Увеличиваем  lett.timeStart чтобы двигать слова в разное время
                lett.timeStart = Time.time + i * 0.05f;

                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett); // Добавляем сразу и в слово и в список Wyrd
            }

            if (showAllWyrds) wyrd.visible = true;      // Эта линия для тестирования

            // Раскрашиваем слово в зависимости от длинны
            wyrd.color = wyrdPalette[word.Length - WordList.S.wordLengthMin];

            wyrds.Add(wyrd);

            // Если мы уже достигли нужного кол-ва строчек, начинаем новый столбец
            if (i%numRows == numRows - 1) {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        // Распологаем большие буквы
        // Создаём для них списки
        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        // Создаём большую букву для каждой буквы в целевом слове
        for (int i = 0; i < currLevel.word.Length; i++) {
            // Похоже на процесс с обычными буквами
            c = currLevel.word[i];
            go = Instantiate(prefabLetter) as GameObject;
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            // Начальная позиция под экраном
            lett.position = new Vector3(0, -100, 0);
            pos = new Vector3(0, -100, 0);
            lett.pos = pos;

            // Повышаем lett.timeStart чтобы большие буквы пришли последними
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCurve = Easing.Sin + "-0.18";    // Качающее смягчение

            col = bigColorDim;
            lett.color = col;
            lett.visible = true;    // Всегда правда для больших букв
            lett.big = true;
            bigLetters.Add(lett);
        }
        // Мешаем большие буквы
        bigLetters = ShuffleLetters(bigLetters);
        // Распологаем их на экране
        ArrangeBigLetters();

        // Задаём статус как "в игре"
        mode = GameMode.inLevel;
    }

    List<Letter> ShuffleLetters(List<Letter> letts) {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while(letts.Count> 0) {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return (newL);
    }

    void ArrangeBigLetters() {
        // halfWidth позволяет большим буквам быть отцентрованными
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for (int i = 0; i < bigLetters.Count; i++) {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }
        // bigLettersActive
        halfWidth = ((float)bigLettersActive.Count) / 2f - 0.5f;
        for (int i = 0; i < bigLettersActive.Count; i++) {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLettersActive[i].pos = pos;
        }
    }
    
    void Update() {
        // Объявляем несколько полезных локальных переменных
        Letter lett;
        char c;

        switch (mode) {
            case GameMode.inLevel:
                // Повторяем для каждого символа что игрок ввёл в этот кадр
                foreach (char cIt in Input.inputString) {
                    // Делаем букву заглавной
                    c = System.Char.ToUpperInvariant(cIt);

                    // Проверяем что это заглавная буква
                    if (upperCase.Contains(c)) { // Какая то из заглавных букв
                        // Находим доступную букву в bigLetters с этим символом
                        lett = FindNextLetterByChar(c);
                        // Если буква была возвращенна
                        if (lett != null) {
                            // Добавляем букву в testWOrd
                            testWord += c.ToString();
                            // Перемещаем в список активных
                            bigLettersActive.Add(lett);
                            bigLetters.Remove(lett);
                            lett.color = bigColorSelected;  // Окрашиваем в активный цвет
                            ArrangeBigLetters();
                        }
                    }
                    if(c == '\b') { // Типо стирает что-то 
                        // Убираем последнюю букву в bigLettersActive
                        if (bigLettersActive.Count == 0) return;
                        if (testWord.Length > 1) {
                            // Убираем последнюю букву в testWord
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        } else {
                            testWord = "";
                        }
                        // Перемещаем букву из списка активных
                        lett = bigLettersActive[bigLettersActive.Count - 1];
                        bigLettersActive.Remove(lett);
                        bigLetters.Add(lett);
                        lett.color = bigColorDim;   // Делаем неактивного цвета
                        ArrangeBigLetters();    // Заново распологаем большие буквы
                    }
                    if(c == '\n' || c == '\r') { // Return/Enter
                        // Сверяем testWord со всеми словами в WordLevel
                        //CheckWord();
                        StartCoroutine(CheckWord());
                    }
                    if(c == ' ') { // Space
                        // Мешаем bigLetters
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

    // Находит доступную букву в bigLetters
    // Если нет доступных, возвращает null
    Letter FindNextLetterByChar(char c) {
        // Ищем во всех буквах bigLetters
        foreach (Letter lett in bigLetters) {
            // Если есть такая же буква, возвращаем её
            if(lett.c == c) {
                return (lett);
            }
        }
        // В других случаях вовзвращаем null
        return (null);
    }

    public IEnumerator CheckWord() {
        // Сверяем слово с другими level.subWords
        string subWord;
        bool foundTestWord = false;

        // Создаём список индексов других subWords, которые состоят в testWord
        List<int> containedWords = new List<int>();

        // Повторяем для каждого слова в currLevel.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++) {
            // Если какое то слово уже было найденно
            if (wyrds[i].found) {
                // Просто продолжаем
                continue;
                // Это работает потому что Wyrds на экране и слова в списке
                // subWords в одном и том же порядке
            }

            subWord = currLevel.subWords[i];
            // Если subWord это testWord
            if (string.Equals(testWord, subWord)) {
                // тогда подсвечиваем subWord
                HighlightWyrd(i);
                Score(wyrds[i], 1); // Записываем testWord
                foundTestWord = true;
            } else if (testWord.Contains(subWord)) {
                // Если testWord содержит слово, тогда добавляем его в список
                containedWords.Add(i);
            }
        }

        // Если слово для теста было найденно в subWords
        if (foundTestWord) {
            // ... тогда подсвечиваем остальные слова содержащиеся в testWord
            int numContained = containedWords.Count;
            int ndx;
            // Подсвечиваем каждое слово в обратном порядке
            for (int i = 0; i < numContained; i++) {

                // Прерываем перед каждый подсвеченным словом
                yield return (new WaitForSeconds(scoreComboDelay));
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                Score(wyrds[containedWords[ndx]], i + 2);   // Подсчитываем другие слова
                // Второй параметр, это номер текущего слово в комбо
            }
        }

        // Чистим активные большие буквы независимо от проверки testWord
        ClearBigLettersActive();
    }

    // Подсвечиваем Wyrd
    void HighlightWyrd(int ndx) {
        // Активируем subWord
        wyrds[ndx].found = true;    // Говорим ему что он найден
        // Подсвечиваем цвет
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f;
        wyrds[ndx].visible = true;  // Делаем 3d текст видимым
    }

    // Убираем все активные буквы из bigLettersActive
    void ClearBigLettersActive() {
        testWord = "";  // Обнуляем слово для теста
        foreach (Letter lett in bigLettersActive) {
            bigLetters.Add(lett);   // Добавляем буквы в обычный список
            lett.color = bigColorDim;   // Задаём неактивный цвет
        }
        bigLettersActive.Clear();   // Очищаем список
        ArrangeBigLetters(); // Заново распологаем буквы
    }

    // Добавляем счёт за это слово
    // число combo это номер слова по счёту
    void Score(Wyrd wyrd, int combo) {
        // Узнаём позицию первой буквы в wyrd
        Vector3 pt = wyrd.letters[0].transform.position;
        // Создаём список бизер точек для плавающего счёта
        List<Vector3> pts = new List<Vector3>();

        // Сонвертируем pt в Видовую точку в ренже 0- 1
        // используется для GUI координат
        pt = Camera.main.WorldToViewportPoint(pt);
        pt.z = 0;
        // Делаем pt первой точкой ( как раз позиция буквы )
        pts.Add(pt);

        // Добавляем вторую точку ( для дуги )
        pts.Add(scoreMidPoint);

        // Добавляем последнюю точку ( типо происходит слияние счетов
        // и они складываются )
        pts.Add(Scoreboard.S.transform.position);

        // Задаём значение для floating score
        int value = wyrd.letters.Count * combo;
        FloatingScore fs = Scoreboard.S.CreateFloatingScore(value, pts);

        fs.timeDuration = 2f;
        fs.fontSizes = scoreFontSizes;

        // Удваиваем смягчающий эффект
        fs.easingCurve = Easing.InOut + Easing.InOut;

        // Делаем текст типо "3 х 2"
        string txt = wyrd.letters.Count.ToString();
        if (combo > 1) {
            txt += " x " + combo;
        }
        fs.GetComponent<Text>().text = txt;
    }
}
