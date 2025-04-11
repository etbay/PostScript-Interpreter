using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PSInterpreter;
using static System.Net.Mime.MediaTypeNames;

public class Console : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField textField;
    private string lastText;

    // Start is called before the first frame update
    void Start()
    {
        SelectField();
        Prompt();

        int len = textField.text.Length;

        textField.caretPosition = len;
        textField.selectionAnchorPosition = len;
        textField.selectionFocusPosition = len;
    }

    public void OnValueChanged()
    {
        string input = GetInput();

        Interpreter.ProcessInput(input);
        StartCoroutine(WaitAndPrompt());
    }

    public void OnDeselected()
    {
        string input = GetInput();

        if (input == string.Empty)
        {
            textField.text += "\n";
        }
    }

    private string GetInput()
    {
        string input = textField.text.Substring(lastText.Length);
        lastText = textField.text;
        return input;
    }

    // Waits for textField to add newline before displaying
    private IEnumerator WaitAndPrompt()
    {
        yield return new WaitForSeconds(0.01f);
        SelectField();
        Prompt();
    }

    private void SelectField()
    {
        BaseEventData newEvent = new BaseEventData(EventSystem.current);
        textField.OnSelect(newEvent);
    }

    private void Prompt()
    {
        textField.text += "REPL(" + Interpreter.StackCount() + ")> ";
        lastText = textField.text;
        textField.stringPosition = textField.text.Length;
    }
}
