using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Assets.Effects;

public class ScreenspaceSpriteEffect : Effect
{
    private readonly EffectParameter _matrixParam;
    private readonly EffectParameter _textureParam;

    private Viewport _lastViewport;
    private Matrix _projection;

    public Texture2D? Texture { get; set; }
    public Matrix? TransformMatrix { get; set; }

    public ScreenspaceSpriteEffect(Effect effect)
        : base(effect)
    {
        _matrixParam = Parameters["MatrixTransform"];
        _textureParam = Parameters["Texture"];
    }

    protected override void OnApply()
    {
        var vp = GraphicsDevice.Viewport;
        if (vp.Width != _lastViewport.Width || vp.Height != _lastViewport.Height)
        {
            Matrix.CreateOrthographicOffCenter(0, vp.Width, 0, vp.Height, -1, 1, out _projection);

            _projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -10);

            if (GraphicsDevice.UseHalfPixelOffset)
            {
                _projection.M41 += -0.5f * _projection.M11;
                _projection.M42 += -0.5f * _projection.M22;
            }

            _lastViewport = vp;
        }

        _textureParam?.SetValue(Texture);

        if (TransformMatrix.HasValue)
            _matrixParam.SetValue(TransformMatrix.GetValueOrDefault() * _projection);
        else
            _matrixParam.SetValue(_projection);
    }

    public override Effect Clone()
    {
        return new ScreenspaceSpriteEffect(this);
    }
}