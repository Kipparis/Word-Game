using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour {

    private char _c;    // Символ показываемый на этой Букве
    public TextMesh tMesh;  // TextMesh чтобы показать букву
    public Renderer tRend;  // Будет определять видимость буквы
    public bool big = false;    // Большие буквы действуют по другому

    // Поля для линейных переходов
    public List<Vector3> pts = null;
    public float timeDuration = 0.5f;
    public float timeStart = -1f;
    public string easingCurve = Easing.InOut;   // Смягчение из Utils.cs

    void Awake() {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        visible = false;
    }

    // Используется чтобы задавать _c и показываемую букву
    public char c {
        get { return (_c); }
        set {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }

    // Задаёт и узнаёт видно или нет текст
    public bool visible {
        get { return (tRend.enabled); }
        set { tRend.enabled = value; }
    }

    // Получает или задаёт цвет закруглённого квадрата
    public Color color {
        get { return (GetComponent<Renderer>().material.color); }
        set { GetComponent<Renderer>().material.color = value; }
    }

    // Задаёт позицию ио
    public Vector3 pos {
        set {
            //transform.position = value;

            // Находим точку по центру между текущей позиций и переданной
            Vector3 mid = (transform.position + value) / 2f;
            // Рандомная дистанция будет с 1\4 длинны линнии от середины
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;
            // Создаём список точек
            pts = new List<Vector3>() { transform.position, mid, value };
            // Если timeStart имеет начальное значение задаём его
            if (timeStart == -1) timeStart = Time.time;
        }
    }

    // Двигает сразу же в новую позицию
    public Vector3 position {
        set { transform.position = value; }
    }

    // Код для перехода
    void Update() {
        if (timeStart == -1) { return; }

        // Стандартный код для переходов
        float u = (Time.time - timeStart) / timeDuration;
        u = Mathf.Clamp01(u);   // Ограничение между 0 и 1
        float u1 = Easing.Ease(u, easingCurve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        // Если интерполяция законченна, задаём время старта -1
        if (u == 1) timeStart = -1;
    }
}
