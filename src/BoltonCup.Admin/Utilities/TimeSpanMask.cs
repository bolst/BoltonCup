using System.Text.RegularExpressions;
using MudBlazor;

namespace BoltonCup.Admin.Utilities;

public partial class TimeSpanMask : PatternMask
{
    private char _hourChar;
    private char _minuteChar;
    private char _secondChar;

    /// <summary>
    /// Creates a new TimeSpan mask.
    /// </summary>
    /// <param name="mask">The mask to use (e.g. <c>mm:ss</c> or <c>hh:mm:ss</c>).</param>
    /// <param name="hour">Defaults to <c>h</c>. The character which represents hours.</param>
    /// <param name="minute">Defaults to <c>m</c>. The character which represents minutes.</param>
    /// <param name="second">Defaults to <c>s</c>. The character which represents seconds.</param>
    public TimeSpanMask(string mask, char hour = 'h', char minute = 'm', char second = 's') : base(mask)
    {
        _hourChar = hour;
        _minuteChar = minute;
        _secondChar = second;
        
        // Register the specific mask characters to only accept digits
        MaskChars = MaskChars.Concat([
            MaskChar.Digit(hour), 
            MaskChar.Digit(minute), 
            MaskChar.Digit(second)
        ]).ToArray();
    }

    /// <inheritdoc />
    protected override void ModifyPartiallyAlignedMask(string mask, string text, int maskOffset, ref int textIndex, ref int maskIndex, ref string alignedText)
    {
        if (string.IsNullOrEmpty(alignedText))
        {
            return;
        }

        MinuteLogic(mask, maskOffset, ref maskIndex, ref alignedText);
        SecondLogic(mask, maskOffset, ref maskIndex, ref alignedText);
    }

    private void MinuteLogic(string mask, int maskOffset, ref int maskIndex, ref string alignedText)
    {
        var minutePattern = new string(_minuteChar, 2);
        var (minuteString, index) = Extract(minutePattern, mask, maskOffset, alignedText);
        
        if (minuteString == null || !int.TryParse(minuteString, out var minute))
            return;

        if (minuteString.Length == 1)
        {
            // First digit of mm. If > 5 (e.g. 6, 7, 8, 9), it cannot be a valid tens digit for a 0-59 range.
            // We automatically prepend a 0 (so typing '6' becomes '06').
            if (minute > 5)
            {
                alignedText = alignedText.Insert(index, "0");
                maskIndex++;
            }
        }
        else if (minuteString.Length == 2)
        {
            // Cap to 59 if the user pastes or somehow injects a larger number
            if (minute > 59)
            {
                alignedText = alignedText.Remove(index, 2).Insert(index, "59");
            }
        }
    }

    private void SecondLogic(string mask, int maskOffset, ref int maskIndex, ref string alignedText)
    {
        var secondPattern = new string(_secondChar, 2);
        var (secondString, index) = Extract(secondPattern, mask, maskOffset, alignedText);
        
        if (secondString == null || !int.TryParse(secondString, out var second))
            return;

        if (secondString.Length == 1)
        {
            // First digit of ss. If > 5, prepend 0.
            if (second > 5)
            {
                alignedText = alignedText.Insert(index, "0");
                maskIndex++;
            }
        }
        else if (secondString.Length == 2)
        {
            // Cap to 59
            if (second > 59)
            {
                alignedText = alignedText.Remove(index, 2).Insert(index, "59");
            }
        }
    }

    /// <summary>
    /// Adjusts the input to be valid for special situations. Ensures minutes and seconds do not exceed 59.
    /// </summary>
    protected override string ModifyFinalText(string text)
    {
        if (Mask is null)
        {
            return text;
        }

        try
        {
            var mm = new string(_minuteChar, 2);
            var ss = new string(_secondChar, 2);
            
            var maskHasMinute = Mask.Contains(mm);
            var maskHasSecond = Mask.Contains(ss);

            if (maskHasMinute)
            {
                var (minuteString, minuteIndex) = Extract(mm, Mask, 0, text);
                if (minuteIndex >= 0 && minuteString?.Length == 2 && int.TryParse(minuteString, out var m) && m > 59)
                {
                    text = text.Remove(minuteIndex, 2).Insert(minuteIndex, "59");
                }
            }

            if (maskHasSecond)
            {
                var (secondString, secondIndex) = Extract(ss, Mask, 0, text);
                if (secondIndex >= 0 && secondString?.Length == 2 && int.TryParse(secondString, out var s) && s > 59)
                {
                    text = text.Remove(secondIndex, 2).Insert(secondIndex, "59");
                }
            }
        }
        catch (Exception)
        {
            return text;
        }
        
        return text;
    }

    private static (string?, int) Extract(string maskPart, string mask, int maskOffset, string alignedText)
    {
        var maskIndex = mask.IndexOf(maskPart, StringComparison.Ordinal);
        if (maskIndex < 0)
            return (null, -1);

        var index = maskIndex - maskOffset;
        if (index < 0 || index >= alignedText.Length)
            return (null, -1);
            
        var subString = alignedText.Substring(index, Math.Min(maskPart.Length, alignedText.Length - index));
        if (!ValidDigitRegularExpression().IsMatch(subString))
            return (null, -1);
            
        return (subString, index);
    }

    /// <inheritdoc />
    public override void UpdateFrom(IMask? mask)
    {
        base.UpdateFrom(mask);
        if (mask is TimeSpanMask timeSpanMask)
        {
            _hourChar = timeSpanMask._hourChar;
            _minuteChar = timeSpanMask._minuteChar;
            _secondChar = timeSpanMask._secondChar;
        }
    }

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex ValidDigitRegularExpression();
}