using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace Magnethands.Menus.Main
{
    /// <summary>
    /// Create a ScriptableObject that acts as a TMPro_InputValidator 
    /// with any valid C# regular expression. 
    /// </summary>
    [CreateAssetMenu(fileName = "Input Field Validator")]
    public class TMPRegexValidator : TMPro.TMP_InputValidator
    {
        public string charPattern;
        public bool DEBUG = false;
        [Tooltip("Leave as 0 for no limit.")]
        public uint maxLength = 0;
        private int _maxLength;
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (DEBUG)
                Debug.Log($"text={text}, pos={pos}, char={ch}");
            if (maxLength == 0)
                _maxLength = int.MaxValue;
            else
                _maxLength = (int)maxLength;

            // If the typed character is a number, insert it into the text argument at the text insertion position (pos argument)
            if (Regex.IsMatch(ch.ToString(), charPattern) && text.Length < _maxLength)
            {
                // Insert the character at the given position if we're working in the Unity Editor
#if UNITY_EDITOR
                text = text.Insert(pos, ch.ToString());
#endif

                // Increment the insertion point by 1
                pos++;
                return ch;
            }
            // If the character is not valid, return null
            else
            {
                return '\0';
            }
        }
    }
}