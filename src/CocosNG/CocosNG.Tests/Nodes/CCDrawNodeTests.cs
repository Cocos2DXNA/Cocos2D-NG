namespace CocosNG.Tests;
using CocosNG.Core.Nodes;
using CocosNG.Core.Primitives;

[TestClass]
public class CCDrawNodeTests
{
    [TestMethod]
    public void DrawTriangle_AddsThreeVertices()
    {
        var node = new CCDrawNode();
        var red = new CCColor4B(255, 0, 0, 255);
        node.DrawTriangle(new CCPoint(0, 0), red, new CCPoint(10, 0), red, new CCPoint(0, 10), red);
        Assert.AreEqual(3, node.VertexCount);
    }

    /// <summary>A node redrawn every frame clears and refills; it must not keep last frame's vertices.</summary>
    [TestMethod]
    public void Clear_ForgetsEarlierVertices()
    {
        var node = new CCDrawNode();
        var red = new CCColor4B(255, 0, 0, 255);
        for (int i = 0; i < 50; i++)
        {
            node.DrawTriangle(new CCPoint(0, 0), red, new CCPoint(10, 0), red, new CCPoint(0, 10), red);
        }
        node.Clear();
        node.DrawTriangle(new CCPoint(0, 0), red, new CCPoint(10, 0), red, new CCPoint(0, 10), red);
        Assert.AreEqual(3, node.VertexCount);
    }
}
