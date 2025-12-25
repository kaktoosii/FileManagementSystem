using Base.Common.ExtentionMethode;

namespace Common.GuardToolkit;

public static class GuardExt
{
    /// <summary>
    /// Checks if the argument is null.
    /// </summary>
    public static void CheckArgumentIsNull(this object o, string name)
    {
        if (o == null)
            throw new ArgumentNullException(name);
    }



    public static bool ContainsNumber(this string inputText)
    {
        return !string.IsNullOrWhiteSpace(inputText) && inputText.ToEnglishNumber().Any(char.IsDigit);
    }

    public static bool HasConsecutiveChars(this string inputText, int sequenceLength = 4)
    {
        var charEnumerator = StringInfo.GetTextElementEnumerator(inputText);
        var currentElement = string.Empty;
        var count = 1;
        while (charEnumerator.MoveNext())
        {
            if (string.Equals(currentElement, charEnumerator.GetTextElement(), StringComparison.Ordinal))
            {
                if (++count >= sequenceLength)
                {
                    return true;
                }
            }
            else
            {
                count = 1;
                currentElement = charEnumerator.GetTextElement();
            }
        }
        return false;
    }

    public static bool IsEmailAddress(this string inputText)
    {
        return !string.IsNullOrWhiteSpace(inputText) && new EmailAddressAttribute().IsValid(inputText);
    }


}
