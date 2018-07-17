using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wyrd{
    public string str;  // Строчноя презентация слова
    public List<Letter> letters = new List<Letter>();
    public bool found = false;  // Истина если игрок нашёл это слово

    // Задаём или получаем видимость каждой буквы
    public bool visible {
        get {
            if (letters.Count == 0) return (false);
            return (letters[0].visible);
        }
        set {
            foreach (Letter lett in letters) {
                lett.visible = value;
            }
        }
    }
	
    // Свойство чтобы задавать цвет каждого сглаженного квадратика
    public Color color {
        get {
            if (letters.Count == 0) return Color.black;
            return letters[0].color;
        }
        set {
            foreach (Letter lett in letters) {
                lett.color = value;
            }
        }
    }

    // Добавляет букву к буквам
    public void Add(Letter lett) {
        str += lett.c.ToString();
        letters.Add(lett);
    }
}
