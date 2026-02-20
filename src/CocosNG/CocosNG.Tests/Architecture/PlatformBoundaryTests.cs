using System.Reflection;
using CocosNG.Core;
using CocosNG.Core.Content;
using CocosNG.Core.Nodes;
using CocosNG.Core.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CocosNG.Tests.Architecture;

[TestClass]
public class PlatformBoundaryTests
{
    [TestMethod]
    public void CCTexture2D_PublicSurface_DoesNotExposeTexture2D()
    {
        var type = typeof(CCTexture2D);
        var platformType = typeof(Texture2D);
        var violations = new List<string>();

        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var property in type.GetProperties(flags))
        {
            if (property.PropertyType == platformType)
            {
                violations.Add($"Property: {property.Name}");
            }
        }

        foreach (var field in type.GetFields(flags))
        {
            if (field.FieldType == platformType)
            {
                violations.Add($"Field: {field.Name}");
            }
        }

        foreach (var method in type.GetMethods(flags))
        {
            if (method.IsSpecialName)
            {
                continue;
            }

            if (method.ReturnType == platformType)
            {
                violations.Add($"Method return: {method.Name}");
            }

            foreach (var parameter in method.GetParameters())
            {
                if (parameter.ParameterType == platformType)
                {
                    violations.Add($"Method parameter: {method.Name}({parameter.Name})");
                }
            }
        }

        Assert.AreEqual(0, violations.Count, string.Join(Environment.NewLine, violations));
    }

    [TestMethod]
    public void CCTexture2D_ExposesPlatformNeutralTextureId()
    {
        var textureId = typeof(CCTexture2D).GetProperty("TextureId", BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(textureId);
        Assert.AreEqual(typeof(int), textureId.PropertyType);
    }

    [TestMethod]
    public void CCContentManager_IsNotPublic()
    {
        var type = typeof(CCTexture2D).Assembly.GetType("CocosNG.Core.Content.CCContentManager", throwOnError: true)!;

        Assert.IsFalse(type.IsPublic);
    }

    [TestMethod]
    public void CCDrawManager_PublicSurface_DoesNotExposeCorePlatformTypes()
    {
        AssertNoPlatformTypeLeaks(typeof(CCDrawManager));
    }

    [TestMethod]
    public void CCTextureCache_PublicSurface_DoesNotExposeCorePlatformTypes()
    {
        AssertNoPlatformTypeLeaks(typeof(CCTextureCache));
    }

    [TestMethod]
    public void CCRenderTexture_PublicSurface_DoesNotExposeCorePlatformTypes()
    {
        AssertNoPlatformTypeLeaks(typeof(CCRenderTexture));
    }

    [TestMethod]
    public void FluentMethods_ReturnSameInstance()
    {
        var animation = new CCAnimation();
        var chained = animation
            .WithDelayPerUnit(0.2f)
            .WithLoops(3)
            .WithRestoreOriginalFrame(true);

        Assert.AreSame(animation, chained);
    }

    [TestMethod]
    public void CCPoint_RoundTripsThroughVector2()
    {
        var source = new CCPoint(12.5f, -6.25f);
        Vector2 vector = source;
        CCPoint roundTrip = vector;

        Assert.AreEqual(source.X, roundTrip.X, 0.0001f);
        Assert.AreEqual(source.Y, roundTrip.Y, 0.0001f);
    }

    private static void AssertNoPlatformTypeLeaks(Type type)
    {
        var platformTypes = new HashSet<Type>
        {
            typeof(Texture2D),
            typeof(RenderTarget2D),
            typeof(GraphicsDevice),
            typeof(SpriteBatch),
            typeof(Game),
            typeof(GameTime),
            typeof(Color),
            typeof(DisplayOrientation),
            typeof(Matrix),
            typeof(ClearOptions),
            typeof(SurfaceFormat),
            typeof(DepthFormat),
            typeof(RenderTargetUsage)
        };

        var violations = new List<string>();
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var property in type.GetProperties(flags))
        {
            if (platformTypes.Contains(property.PropertyType))
            {
                violations.Add($"Property: {property.Name}");
            }
        }

        foreach (var field in type.GetFields(flags))
        {
            if (platformTypes.Contains(field.FieldType))
            {
                violations.Add($"Field: {field.Name}");
            }
        }

        foreach (var method in type.GetMethods(flags))
        {
            if (method.IsSpecialName)
            {
                continue;
            }

            if (platformTypes.Contains(method.ReturnType))
            {
                violations.Add($"Method return: {method.Name}");
            }

            foreach (var parameter in method.GetParameters())
            {
                if (platformTypes.Contains(parameter.ParameterType))
                {
                    violations.Add($"Method parameter: {method.Name}({parameter.Name})");
                }
            }
        }

        Assert.AreEqual(0, violations.Count, string.Join(Environment.NewLine, violations));
    }
}
