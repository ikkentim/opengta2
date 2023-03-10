using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Utilities;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Components;

public class SpriteTestComponent : DrawableGameComponent
{
    private Rendering.Effects.SpriteEffect? _sb;
    private readonly LevelProvider _levelProvider;
    private FontRenderer? _fontRenderer;
    public SpriteTestComponent(GtaGame game) : base(game)
    {
        _levelProvider = game.Services.GetService<LevelProvider>();
    }

    private new GtaGame Game => (GtaGame)base.Game;
    protected override void LoadContent()
    {
        _sb = new Rendering.Effects.SpriteEffect(Game.Content.Load<Effect>("SpriteEffect"));

        _fontRenderer = new FontRenderer(Game.AssetManager, Game.Services.GetService<LevelProvider>());
    }

    private float _time;

    private int _pedSprite = 0;
    private int _pedRemap = -1;

    public override void Update(GameTime gameTime)
    {
        _time += gameTime.GetDelta();
        
        if (_time >= 0.15)
        {
            _time -= 0.15f;
            _pedSprite++;

            if (_pedSprite >= 158)
            {
                _pedSprite = 0;
                _pedRemap++;

                if (_pedRemap >= _levelProvider.Style.PaletteBase.PedRemap)
                    _pedRemap = -1;
            }
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        PerformanceCounters.Drawing.StartMeasurement("DrawSpriteTest");
        
        // font
        _fontRenderer!.Draw(GraphicsDevice, new Vector2(500, 500), 0, "HELLO WORLD");
        
        // ped

        _sb!.Texture = _levelProvider.Textures.GetSpriteTexture(SpriteKind.Ped, (ushort)_pedSprite, _pedRemap);
        _sb.CurrentTechnique.Passes[0].Apply();
        
        var point = new Vector2(250);
        var size = new Vector2(_sb.Texture.Width, _sb.Texture.Height);
        
        QuadRenderer.Render(GraphicsDevice, point, point + size * 4);
 
        PerformanceCounters.Drawing.StopMeasurement();
    }
}