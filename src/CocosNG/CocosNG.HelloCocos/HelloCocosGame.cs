using CocosNG.Core.Nodes;
using CocosNG.Core.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CocosNG.HelloCocos;

public sealed class HelloCocosGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly CCNode _rootNode;
    private readonly CCAnimation _animation;

    public HelloCocosGame()
    {
        _graphics = new GraphicsDeviceManager(this)
            .WithPreferredBackBufferSize(1280, 720)
            .WithIsFullScreen(false)
            .WithSynchronizeWithVerticalRetrace(true);

        this
            .WithIsMouseVisible(true)
            .WithIsFixedTimeStep(true);

        _rootNode = new CCNode()
            .WithPosition(new CCPoint(64f, 64f))
            .WithScale(1f)
            .WithVisible(true);

        _animation = new CCAnimation()
            .WithDelayPerUnit(0.25f)
            .WithLoops(0)
            .WithRestoreOriginalFrame(true);
    }

    protected override void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _rootNode.WithRotation(_rootNode.Rotation + (45f * dt));
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        base.Draw(gameTime);
    }
}
