using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour {
    public static WordList S;

    public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;

    public bool ________________;

    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;
}
