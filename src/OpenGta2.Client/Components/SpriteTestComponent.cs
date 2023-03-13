using Microsoft.Xna.Framework;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;

namespace OpenGta2.Client.Components;

public class SpriteTestComponent : BaseDrawableComponent
{
    private FontRenderer? _fontRenderer;

    public SpriteTestComponent(GtaGame game) : base(game)
    {
    }
    
    protected override void LoadContent()
    {
        _fontRenderer = new FontRenderer(Game.AssetManager, Game.Services.GetService<LevelProvider>());
    }
    
    public override void Draw(GameTime gameTime)
    {
        PerformanceCounters.Drawing.StartMeasurement("DrawSpriteTest");
        
        // font
        // _fontRenderer!.Draw(GraphicsDevice, new Vector2(500, 500), 0, "HELLO WORLD");
        
        PerformanceCounters.Drawing.StopMeasurement();
    }
}