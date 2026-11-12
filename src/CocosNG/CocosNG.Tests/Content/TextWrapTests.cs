using CocosNG.Core.Content;

namespace CocosNG.Tests.Content;

/// <summary>
/// How label text is split into lines (CCTexture2D.WrapLines). Each character is 10 wide here.
/// </summary>
[TestClass]
public class TextWrapTests
{
    private static List<string> Wrap(string text, float width) => CCTexture2D.WrapLines(text, width, s => s.Length * 10f);

    [TestMethod]
    public void OneParagraph_HasNoBlankLineAfterIt()
    {
        // A trailing blank line made centred text half a line too high, cutting off its top line.
        CollectionAssert.AreEqual(new[] { "aaa bbb", "ccc" }, Wrap("aaa bbb ccc", 75f));
        CollectionAssert.AreEqual(new[] { "short" }, Wrap("short", 200f));
    }

    [TestMethod]
    public void Paragraphs_AreSeparatedByOneBlankLine()
    {
        CollectionAssert.AreEqual(new[] { "aa", "", "bb" }, Wrap("aa\nbb", 200f));
        CollectionAssert.AreEqual(new[] { "aa", "", "", "bb" }, Wrap("aa\n\nbb", 200f));
    }

    [TestMethod]
    public void AWordWiderThanTheLine_GetsALineOfItsOwn()
    {
        CollectionAssert.AreEqual(new[] { "abcdefghij" }, Wrap("abcdefghij", 50f));
        CollectionAssert.AreEqual(new[] { "ab", "abcdefghij" }, Wrap("ab abcdefghij", 50f));
    }
}
