using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Rendering.Effects;

public class SpriteEffect : Effect
{
    private readonly EffectParameter _matrixParam;
    private readonly EffectParameter _spritesParam;

    private Viewport _lastViewport;
    private Matrix _projection;

    public Texture2D? Texture { get; set; }
    public Matrix? TransformMatrix { get; set; }

    public SpriteEffect(Effect effect)
        : base(effect)
    {
        _matrixParam = Parameters["MatrixTransform"];
        _spritesParam = Parameters["Sprites"];
    }

    protected override void OnApply()
    {
        var vp = GraphicsDevice.Viewport;
        if (vp.Width != _lastViewport.Width || vp.Height != _lastViewport.Height)
        {
            Matrix.CreateOrthographicOffCenter(0, vp.Width, 0, vp.Height, -1, 1, out _projection);

            _projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -10);
            _lastViewport = vp;
        }

        _spritesParam?.SetValue(Texture);

        if (TransformMatrix.HasValue)
            _matrixParam.SetValue(TransformMatrix.GetValueOrDefault() * _projection);
        else
            _matrixParam.SetValue(_projection);
    }

    public override Effect Clone()
    {
        return new SpriteEffect(this);
    }
}