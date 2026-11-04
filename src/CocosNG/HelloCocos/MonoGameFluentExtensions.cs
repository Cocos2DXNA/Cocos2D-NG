using Microsoft.Xna.Framework;

namespace HelloCocos;

// Fluent setup helpers for MonoGame types (previously provided by XnaNG).
internal static class MonoGameFluentExtensions
{
    public static GraphicsDeviceManager WithPreferredBackBufferSize(this GraphicsDeviceManager graphics, int width, int height)
    {
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        return graphics;
    }

    public static GraphicsDeviceManager WithIsFullScreen(this GraphicsDeviceManager graphics, bool isFullScreen)
    {
        graphics.IsFullScreen = isFullScreen;
        return graphics;
    }

    public static GraphicsDeviceManager WithSynchronizeWithVerticalRetrace(this GraphicsDeviceManager graphics, bool vsync)
    {
        graphics.SynchronizeWithVerticalRetrace = vsync;
        return graphics;
    }

    public static T WithIsMouseVisible<T>(this T game, bool isMouseVisible) where T : Game
    {
        game.IsMouseVisible = isMouseVisible;
        return game;
    }

    public static T WithIsFixedTimeStep<T>(this T game, bool isFixedTimeStep) where T : Game
    {
        game.IsFixedTimeStep = isFixedTimeStep;
        return game;
    }
}
