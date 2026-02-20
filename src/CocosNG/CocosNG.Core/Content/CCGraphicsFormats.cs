using Microsoft.Xna.Framework.Graphics;

namespace CocosNG.Core.Content;

public enum CCTexturePixelFormat
{
    Color,
    Alpha8,
    Bgr565,
    Bgra4444,
    Bgra5551,
    Dxt1,
    Dxt3,
    Dxt5,
    HalfSingle,
    HalfVector2,
    HalfVector4,
    NormalizedByte2,
    NormalizedByte4,
    Rg32,
    Rgba64,
    Rgba1010102,
    Single,
    Vector2,
    Vector4,
}

public enum CCRenderTargetDepthFormat
{
    None,
    Depth16,
    Depth24,
    Depth24Stencil8,
}

public enum CCRenderTargetUsage
{
    DiscardContents,
    PreserveContents,
    PlatformContents,
}

internal static class CCGraphicsFormatConverter
{
    internal static SurfaceFormat ToPlatform(CCTexturePixelFormat format)
    {
        return format switch
        {
            CCTexturePixelFormat.Color => SurfaceFormat.Color,
            CCTexturePixelFormat.Alpha8 => SurfaceFormat.Alpha8,
            CCTexturePixelFormat.Bgr565 => SurfaceFormat.Bgr565,
            CCTexturePixelFormat.Bgra4444 => SurfaceFormat.Bgra4444,
            CCTexturePixelFormat.Bgra5551 => SurfaceFormat.Bgra5551,
            CCTexturePixelFormat.Dxt1 => SurfaceFormat.Dxt1,
            CCTexturePixelFormat.Dxt3 => SurfaceFormat.Dxt3,
            CCTexturePixelFormat.Dxt5 => SurfaceFormat.Dxt5,
            CCTexturePixelFormat.HalfSingle => SurfaceFormat.HalfSingle,
            CCTexturePixelFormat.HalfVector2 => SurfaceFormat.HalfVector2,
            CCTexturePixelFormat.HalfVector4 => SurfaceFormat.HalfVector4,
            CCTexturePixelFormat.NormalizedByte2 => SurfaceFormat.NormalizedByte2,
            CCTexturePixelFormat.NormalizedByte4 => SurfaceFormat.NormalizedByte4,
            CCTexturePixelFormat.Rg32 => SurfaceFormat.Rg32,
            CCTexturePixelFormat.Rgba64 => SurfaceFormat.Rgba64,
            CCTexturePixelFormat.Rgba1010102 => SurfaceFormat.Rgba1010102,
            CCTexturePixelFormat.Single => SurfaceFormat.Single,
            CCTexturePixelFormat.Vector2 => SurfaceFormat.Vector2,
            CCTexturePixelFormat.Vector4 => SurfaceFormat.Vector4,
            _ => SurfaceFormat.Color,
        };
    }

    internal static CCTexturePixelFormat ToCocos(SurfaceFormat format)
    {
        return format switch
        {
            SurfaceFormat.Alpha8 => CCTexturePixelFormat.Alpha8,
            SurfaceFormat.Bgr565 => CCTexturePixelFormat.Bgr565,
            SurfaceFormat.Bgra4444 => CCTexturePixelFormat.Bgra4444,
            SurfaceFormat.Bgra5551 => CCTexturePixelFormat.Bgra5551,
            SurfaceFormat.Dxt1 => CCTexturePixelFormat.Dxt1,
            SurfaceFormat.Dxt3 => CCTexturePixelFormat.Dxt3,
            SurfaceFormat.Dxt5 => CCTexturePixelFormat.Dxt5,
            SurfaceFormat.HalfSingle => CCTexturePixelFormat.HalfSingle,
            SurfaceFormat.HalfVector2 => CCTexturePixelFormat.HalfVector2,
            SurfaceFormat.HalfVector4 => CCTexturePixelFormat.HalfVector4,
            SurfaceFormat.NormalizedByte2 => CCTexturePixelFormat.NormalizedByte2,
            SurfaceFormat.NormalizedByte4 => CCTexturePixelFormat.NormalizedByte4,
            SurfaceFormat.Rg32 => CCTexturePixelFormat.Rg32,
            SurfaceFormat.Rgba64 => CCTexturePixelFormat.Rgba64,
            SurfaceFormat.Rgba1010102 => CCTexturePixelFormat.Rgba1010102,
            SurfaceFormat.Single => CCTexturePixelFormat.Single,
            SurfaceFormat.Vector2 => CCTexturePixelFormat.Vector2,
            SurfaceFormat.Vector4 => CCTexturePixelFormat.Vector4,
            _ => CCTexturePixelFormat.Color,
        };
    }

    internal static DepthFormat ToPlatform(CCRenderTargetDepthFormat format)
    {
        return format switch
        {
            CCRenderTargetDepthFormat.None => DepthFormat.None,
            CCRenderTargetDepthFormat.Depth16 => DepthFormat.Depth16,
            CCRenderTargetDepthFormat.Depth24 => DepthFormat.Depth24,
            CCRenderTargetDepthFormat.Depth24Stencil8 => DepthFormat.Depth24Stencil8,
            _ => DepthFormat.None,
        };
    }

    internal static CCRenderTargetDepthFormat ToCocos(DepthFormat format)
    {
        return format switch
        {
            DepthFormat.Depth16 => CCRenderTargetDepthFormat.Depth16,
            DepthFormat.Depth24 => CCRenderTargetDepthFormat.Depth24,
            DepthFormat.Depth24Stencil8 => CCRenderTargetDepthFormat.Depth24Stencil8,
            _ => CCRenderTargetDepthFormat.None,
        };
    }

    internal static RenderTargetUsage ToPlatform(CCRenderTargetUsage usage)
    {
        return usage switch
        {
            CCRenderTargetUsage.PreserveContents => RenderTargetUsage.PreserveContents,
            CCRenderTargetUsage.PlatformContents => RenderTargetUsage.PlatformContents,
            _ => RenderTargetUsage.DiscardContents,
        };
    }

    internal static CCRenderTargetUsage ToCocos(RenderTargetUsage usage)
    {
        return usage switch
        {
            RenderTargetUsage.PreserveContents => CCRenderTargetUsage.PreserveContents,
            RenderTargetUsage.PlatformContents => CCRenderTargetUsage.PlatformContents,
            _ => CCRenderTargetUsage.DiscardContents,
        };
    }
}
