namespace CocosNG.Tests;
using CocosNG.Core.Nodes;
using CocosNG.Core.Primitives;

[TestClass]
public class CCNodeTest
{
    [TestMethod]
    public void TestCtor()
    {
        var x = new CCNode();
    }

    [TestMethod]
    public void FluentMethods_ReturnSameInstance()
    {
        var node = new CCNode();
        var chained = node
            .WithPosition(new CCPoint(4f, 9f))
            .WithZ(3f)
            .WithRotation(12f)
            .WithScale(1.5f)
            .WithVisible(false);

        Assert.AreSame(node, chained);
        Assert.AreEqual(4f, node.Position.X, 0.0001f);
        Assert.AreEqual(9f, node.Position.Y, 0.0001f);
        Assert.AreEqual(3f, node.Z, 0.0001f);
        Assert.AreEqual(12f, node.Rotation, 0.0001f);
        Assert.AreEqual(1.5f, node.Scale, 0.0001f);
        Assert.IsFalse(node.Visible);
    }
}
