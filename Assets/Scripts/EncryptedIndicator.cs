using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class EncryptedIndicator : MonoBehaviour {
    public TextMesh NumberText;
    string visibleText = "";
    string actualLabel = "XXX";
    public MeshRenderer lightMesh;
    public Material lightOnMat;
    public SpriteRenderer onSprite;
    bool lit = false;

    string[] labels = new string[] { "CLR", "IND", "TRN", "FRK", "CAR", "FRQ", "NSA", "SIG", "MSA", "SND", "BOB" };

    #region settings defined

    int max = 2;

    #endregion

    static int lastTime = 0;
    static int modules = 0;
    int current = 0;


    Dictionary<char, int[]> answers = new Dictionary<char, int[]>();
    Dictionary<char, char[]> secondary_answers = new Dictionary<char, char[]>();
    List<char> chars;

    void Awake() {
        GetComponent<KMWidget>().OnQueryRequest += GetQueryResponse;
        GetComponent<KMWidget>().OnWidgetActivate += Activate;
        onSprite.enabled = false;
        NumberText.text = "";

        if (lastTime != (int)Time.realtimeSinceStartup) {
            modules = 0;
            lastTime = (int)Time.realtimeSinceStartup;
        }
        modules++;
        current = modules;

        loadSettings();
        initDicts();

        setSolution();

#if UNITY_EDITOR
        Activate(); //The test harness will not call this at the right time
        List<char> chars = new List<char>(answers.Keys);
        string answer = "";
        char[] alphabet = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'Y', 'X', 'Z' };
        for (int i = 0; i < chars.Count; i++) {
			answer += "addSymbol('" + chars[i] + "', new int[]{ " + Random.Range(-2,6) + ", " + Random.Range(-2,6)  + ", " + Random.Range( 2,6)  + "},\tnew char[]{ '" + alphabet[Random.Range(0, 26)] + "', '" + alphabet[Random.Range(0, 26)] + "', '" + alphabet[Random.Range(0, 26)] + "'});\n";
		}
		Debug.Log(answer);
        answer = "<table class=\"repeaters-table\"><tbody><tr><th rowspan = \"2\">Symbol</th><th colspan = \"3\">in Position</th></tr><tr><th>X__</th><th>_X_</th><th>__X</th></tr>";
        for (int i = 0; i < chars.Count; i++)
        {
            answer += "<tr><th><font size=\"6\">" + getSymbol(chars, i) + "</font></th>";
            for (int c = 0; c < 3; c++)
            {
                answer += "<th>" + answers[chars[i]][c] + " / " + secondary_answers[chars[i]][c] + "</th>";
            }

            answer += "</tr>\n";
        }
        answer += "</tbody></table>\n\n<table class=\"repeaters-table\"><tbody><tr><th>Result</th><th>Indicator</th></tr>\n";
        for (int i = 0; i < 11; i++)
        {
            answer += "\t\t\t\t<tr>";
            answer += "<th>" + (i + 1) + "</th><th>" + labels[i] + "</th>";
            answer += "</tr>\n";
        }
        answer += "\t\t\t</tbody></table>";
        Debug.Log(answer);

#endif
    }

    private void setSolution() {
        int solutionIndex = 0;
        List<char> selections = chars;

        // secondary_label is what we will act as if the letters go outside the range
        string secondary_label = "";
        for (int i = 0; i < 3; i++) {
            int index = Random.Range(0, selections.Count);
            char selection = selections[index];
            char result = getSymbol(selections, index);
            visibleText += result;
            selections.Remove(selection);
            solutionIndex += answers[selection][i];
            secondary_label += secondary_answers[result][i];
        }

        actualLabel = getIndicator(solutionIndex, secondary_label);
        lit = (Random.Range(0, 2) == 0);

    }

    //This happens when the bomb turns on, don't turn on any lights or unlit shaders until activate
    public void Activate() {

        string l = (lit) ? "lit" : "unlit";

        if (current > max) {
            Debug.Log("[EncryptedIndicatorWidget] Randomizing: " + l + " " + actualLabel);
            NumberText.text = "" + actualLabel;
        } else {
            Debug.Log("[EncryptedIndicatorWidget] Randomizing: " + l + " " + visibleText + " acting as " + l + " " + actualLabel);
            NumberText.text = "" + visibleText;
        }

        if (lit) {
            lightMesh.material = lightOnMat;
            onSprite.enabled = true;
        }
    }

    public string GetQueryResponse(string queryKey, string queryInfo) {
        if (queryKey == KMBombInfo.QUERYKEY_GET_INDICATOR) {
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("label", actualLabel);
            response.Add("on", (lit) ? "True" : "False");
            response.Add("display", NumberText.text);
            string responseStr = JsonConvert.SerializeObject(response);
            return responseStr;
        }

        return null;
    }



    private string getIndicator(int i, string secondary) {
        i -= 1;
        if (i < 0 || i >= 11) {
            return secondary;
        } else {
            return labels[i];
        }
    }

    void loadSettings() {

        try {
            EncryptedSettings modSettings = JsonConvert.DeserializeObject<EncryptedSettings>(GetComponent<KMModSettings>().Settings);
            if (modSettings != null) {
                max = modSettings.getMax();
            } else {
                Debug.Log("[EncryptedIndicatorWidget] Could not read settings file!");
            }
        } catch (JsonReaderException e) {
            Debug.Log("[EncryptedIndicatorWidget] Malformed settings file! " + e.Message);
        }

    }
    
    private void initDicts() {
        addSymbol('ใ', new int[] { 5, 0, 4 }, new char[] { 'G', 'D', 'G' });
        addSymbol('ɮ', new int[] { 4, 0, 5 }, new char[] { 'Z', 'D', 'R' });
        addSymbol('ʖ', new int[] { 0, -1, 4 }, new char[] { 'C', 'S', 'O' });
        addSymbol('ฬ', new int[] { 0, 2, 5 }, new char[] { 'J', 'X', 'Y' });
        addSymbol('น', new int[] { 2, 1, 2 }, new char[] { 'V', 'B', 'L' });
        addSymbol('Þ', new int[] { -2, 5, 5 }, new char[] { 'T', 'L', 'J' });
        addSymbol('ฏ', new int[] { 4, 1, 2 }, new char[] { 'L', 'A', 'O' });
        addSymbol('Ѩ', new int[] { 3, 5, 4 }, new char[] { 'G', 'A', 'S' });
        addSymbol('Ԉ', new int[] { 4, 4, 2 }, new char[] { 'F', 'S', 'M' });
        addSymbol('Ԓ', new int[] { 3, 2, 3 }, new char[] { 'P', 'O', 'F' });
        addSymbol('ด', new int[] { -1, 3, 4 }, new char[] { 'K', 'Q', 'K' });
        addSymbol('ล', new int[] { -1, -2, 4 }, new char[] { 'D', 'N', 'L' });
        addSymbol('Ж', new int[] { 5, 0, 5 }, new char[] { 'Q', 'O', 'Z' });
        //addSymbol('Ⴟ', new int[] { 5, 5, 3 }, new char[] { 'W', 'M', 'C' });

        chars = new List<char>(answers.Keys);
    }

    private void addSymbol(char c, int[] i, char[] secondary) {
        answers.Add(c, i);
        secondary_answers.Add(c, secondary);
    }
    private char getSymbol(List<char> l, int i) {
        return l[i];
    }

}
