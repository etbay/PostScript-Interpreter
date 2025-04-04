using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PSInterpreter;

public class Console : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField textField;

    // Start is called before the first frame update
    void Start()
    {
        SelectField();
        Prompt();
    }

    public void OnValueChanged()
    {
        if (this.isActiveAndEnabled)
        {
            StartCoroutine(WaitForNewline());
        }
    }

    private IEnumerator WaitForNewline()
    {
        yield return new WaitForSeconds(0.1f);
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
        textField.stringPosition = textField.text.Length;
    }
}
