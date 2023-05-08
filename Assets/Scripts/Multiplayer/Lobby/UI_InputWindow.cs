using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InputWindow : MonoBehaviour
{
    private static UI_InputWindow instance;

    private Button okBtn;
    private Button cancelBtn;
    private TMP_InputField inputField;

    private void Awake()
    {
        instance = this;

        okBtn = transform.Find("okBtn").GetComponent<Button>();
        cancelBtn = transform.Find("cancelBtn").GetComponent<Button>();
        inputField = transform.Find("inputField").GetComponent<TMP_InputField>();

        Hide();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            okBtn.onClick.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cancelBtn.onClick.Invoke();
        }
    }

    private void Show(string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onOk)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        inputField.characterLimit = characterLimit;
        inputField.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateChar(validCharacters, addedChar);
        };

        inputField.text = inputString;
        inputField.Select();

        okBtn.onClick.AddListener(() =>
        {
            Hide();
            onOk(inputField.text);
        });

        cancelBtn.onClick.AddListener(() =>
        {
            Hide();
            onCancel();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private char ValidateChar(string validCharacters, char addedChar)
    {
        if (validCharacters.IndexOf(addedChar) != -1)
        {
            // Valid
            return addedChar;
        }
        else
        {
            // Invalid
            return '\0';
        }
    }

    public static void Show_Static(string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onOk)
    {
        instance.Show(inputString, validCharacters, characterLimit, onCancel, onOk);
    }

    public static void Show_Static(int defaultInt, Action onCancel, Action<int> onOk)
    {
        instance.Show(defaultInt.ToString(), "0123456789-", 20, onCancel,
            (string inputText) =>
            {
                // Try to Parse input string
                if (int.TryParse(inputText, out int _i))
                {
                    onOk(_i);
                }
                else
                {
                    onOk(defaultInt);
                }
            }
        );
    }
}