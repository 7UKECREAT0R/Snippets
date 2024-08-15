using System.Diagnostics;
using System.Runtime.InteropServices;
using UIAutomationClient;

namespace Snippets;

internal static class CaretPosition
{
    private static readonly CUIAutomation8 automationClient = new();

    /// <summary>
    ///     Attempts to get the coordinate of the text caret on the screen. Only works with programs that implement
    ///     UIAutomation8.<br />
    ///     <br />
    ///     NOTE: <b>Requires a COM Reference to UIAutomationClient.dll, and "Embed Interop Types" on the DLL set to false.</b>
    /// </summary>
    /// <param name="actualSuccess"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">If the queried application does not support UIAutomation8.</exception>
    public static Point TryGetCaretPosition(out bool actualSuccess)
    {
        // try and get the active element
        IUIAutomationElement element = automationClient.GetFocusedElement();

        Guid targetGUID = typeof(IUIAutomationTextPattern2).GUID;
        IntPtr patternPtr = element.GetCurrentPatternAs(UIA_PatternIds.UIA_TextPattern2Id, ref targetGUID);

        if (patternPtr == IntPtr.Zero)
        {
            Debug.WriteLine("Defaulting to 0, 0 because element " + element.CurrentName +
                            " did not have the target pattern.");
            actualSuccess = false;
            return Point.Empty;
        }

        if (Marshal.GetObjectForIUnknown(patternPtr) is not IUIAutomationTextPattern2 pattern)
        {
            Debug.WriteLine("Defaulting to 0, 0 because element " + element.CurrentName + " returned a bad pointer.");
            actualSuccess = false;
            return Point.Empty;
        }

        IUIAutomationTextRangeArray ranges = pattern.GetSelection();

        if (ranges.Length < 1)
        {
            // attempt to get closest point on element.
            int clickablePointResult = element.GetClickablePoint(out tagPOINT point);
            if (clickablePointResult != 0)
            {
                Debug.WriteLine("Defaulting to 0, 0 because element " + element.CurrentName +
                                "'s had no caret or clickable surface.");
                actualSuccess = false;
                return Point.Empty;
            }

            actualSuccess = true; // kinda...
            return new Point(point.x, point.y);
        }

        IUIAutomationTextRange range = ranges.GetElement(0);
        IUIAutomationTextRange fullRange = pattern.DocumentRange;
        int caretOffsetFromStart = range.CompareEndpoints(TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start,
            fullRange, TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);
        int caretOffsetFromEnd = range.CompareEndpoints(TextPatternRangeEndpoint.TextPatternRangeEndpoint_End,
            fullRange, TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start);

        Debug.WriteLine(caretOffsetFromEnd);

        if (caretOffsetFromStart == 0)
            range.Move(TextUnit.TextUnit_Character, 1);

        int attemptsLeft = 1;
        tryAgainWorkaround:

        range.ExpandToEnclosingUnit(TextUnit.TextUnit_Character);
        double[] rectangles = (double[]) range.GetBoundingRectangles();

        if (rectangles.Length < 4)
        {
            // might be at the last character in Discord (awful automation support). try again but further left.
            if (attemptsLeft > 0)
            {
                attemptsLeft--;
                range.Move(TextUnit.TextUnit_Character, -1);
                goto tryAgainWorkaround;
            }

            // attempt to get closest point on element.
            int clickablePointResult = element.GetClickablePoint(out tagPOINT point);
            if (clickablePointResult != 0)
            {
                Debug.WriteLine("Defaulting to 0, 0 because element " + element.CurrentName +
                                "'s delivered an incomplete rectangle.");
                actualSuccess = false;
                return Point.Empty;
            }

            actualSuccess = true;
            return new Point(point.x, point.y);
        }

        int x = (int) rectangles[0];
        int y = (int) rectangles[1];
        x += (int) rectangles[2];
        y += (int) rectangles[3];

        actualSuccess = true;
        return new Point(x, y);
    }
}