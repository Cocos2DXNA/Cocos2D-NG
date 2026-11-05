using System.Reflection;
using CocosNG.Core.Actions;
using CocosNG.Core.Actions.Interval;
using CocosNG.Core.Nodes;

namespace CocosNG.Tests.Nodes;

/// <summary>
/// Nodes and action managers must be safe to drop and collect without a director, and must not
/// do work on the GC's finalizer thread.
/// </summary>
[TestClass]
public class CCNodeLifetimeTests
{
    private const BindingFlags DeclaredInstance = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    [TestMethod]
    public void CCNode_HasNoFinalizer()
    {
        Assert.IsNull(typeof(CCNode).GetMethod("Finalize", DeclaredInstance), "a node finalizer runs Cleanup on the GC thread");
    }

    [TestMethod]
    public void CCActionManager_HasNoFinalizer()
    {
        Assert.IsNull(typeof(CCActionManager).GetMethod("Finalize", DeclaredInstance));
    }

    [TestMethod]
    public void DroppedNodes_AreCollectedWithoutADirector()
    {
        // With a throwing finalizer this kills the test process.
        MakeGarbageNodes(2000);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static void MakeGarbageNodes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var node = new CCNode();
            node.AddChild(new CCNode());
        }
    }

    [TestMethod]
    public void CleanupAndActionCalls_DoNothingWithoutADirector()
    {
        var node = new CCNode();
        node.AddChild(new CCNode());
        node.ActionManager = null;
        node.Scheduler = null;

        node.StopAllActions();
        node.StopActionByTag(1);
        node.UnscheduleUpdate();
        node.UnscheduleAllSelectors();
        node.PauseSchedulerAndActions();
        node.ResumeSchedulerAndActions();
        node.Cleanup();

        Assert.IsNull(node.GetActionByTag(1));
        Assert.AreEqual(0, node.NumberOfRunningActions());
    }

    [TestMethod]
    public void RemoveAllActions_HandlesManyTargets()
    {
        // More targets than twice the manager's starting key buffer (128).
        var manager = new CCActionManager();
        var targets = new List<CCNode>();
        for (int i = 0; i < 300; i++)
        {
            var target = new CCNode();
            targets.Add(target);
            manager.AddAction(new CCDelayTime(10f), target, true);
        }

        manager.RemoveAllActions();

        foreach (var target in targets)
        {
            Assert.AreEqual(0, manager.NumberOfRunningActionsInTarget(target));
        }
    }
}
