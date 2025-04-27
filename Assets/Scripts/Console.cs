using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PSInterpreter;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Linq;

public class Console : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField textField;

    private string lastText;

    void Start()
    {
        SelectInputField();
        Prompt();
        MoveCaretToEnd();
        Interpreter.DisplayToConsole += DisplayToConsole;
    }

    /// <summary>
    /// Selects the input field for user input.
    /// </summary>
    private void SelectInputField()
    {
        textField.OnSelect(new BaseEventData(EventSystem.current));
    }

    /// <summary>
    /// Prompts user to input a value.
    /// </summary>
    private void Prompt()
    {
        // display prompt with current number of elements in stack
        textField.text += $"REPL({Interpreter.StackCount()})> ";
        UpdateLastText();
        MoveCaretToEnd();
    }

    /// <summary>
    /// Moves caret and selection to the end of the text field.
    /// </summary>
    private void MoveCaretToEnd()
    {
        int len = textField.text.Length;
        textField.caretPosition = len;
        textField.selectionAnchorPosition = len;
        textField.selectionFocusPosition = len;
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateLastText()
    {
        lastText = textField.text;
    }

    /// <summary>
    /// Triggered when text changes.Processes new input and prompts again.
    /// Triggered on enter/return.
    /// </summary>
    public void OnValueChanged()
    {
        string input = GetInput();

        if (!string.IsNullOrWhiteSpace(input))
        {
            Interpreter.ProcessInput(input);
        }

        StartCoroutine(WaitAndPrompt());
    }

    /// <summary>
    /// Causes new prompt to be printed on new line. Deselecting textField by clicking triggers OnValueChanged().
    /// Called when clicking in program because clicking selects the transparent overlay preventing the selection
    /// of previous input. This deselects the textField and calls OnDeselected(). Deselecting also changes the
    /// value, thus OnValueChanged() is called.
    /// </summary>
    public void OnDeselected()
    {
        if (string.IsNullOrWhiteSpace(GetInput()))
        {
            textField.text += "\n";
            UpdateLastText();
        }
    }

    /// <summary>
    /// Gets difference between lastText and current updated textField. Parses user input.
    /// </summary>
    /// <returns></returns>
    private string GetInput()
    {
        if (textField.text.Length >= lastText.Length)
        {
            string input = textField.text.Substring(lastText.Length);
            UpdateLastText();

            Debug.Log($"Input was: '{input}'");

            return input;
        }

        return string.Empty;
    }

    private void DisplayToConsole(string output)
    {
        textField.text += output + "\n";
    }

    /// <summary>
    /// Waits for UI to update before displaying the next prompt.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAndPrompt()
    {
        yield return new WaitForSeconds(0.01f);
        SelectInputField();
        Prompt();
    }
}
