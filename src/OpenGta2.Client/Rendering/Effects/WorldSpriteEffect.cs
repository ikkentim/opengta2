using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Rendering.Effects;

public class WorldSpriteEffect : Effect
{
    private readonly EffectParameter _matrixParam;
    private readonly EffectParameter _textureParam;
    private readonly EffectParameter _colorParam;
    
    public Texture2D? Texture { get; set; }
    public Matrix TransformMatrix { get; set; }
    public Color Color { get; set; } = Color.White;

    public WorldSpriteEffect(Effect effect)
        : base(effect)
    {
        _matrixParam = Parameters["MatrixTransform"];
        _textureParam = Parameters["Texture"];
        _colorParam = Parameters["Color"];
    }

    protected override void OnApply()
    {
        _textureParam?.SetValue(Texture);
        _matrixParam.SetValue(TransformMatrix);
        _colorParam.SetValue(Color.ToVector4());
    }

    public override Effect Clone()
    {
        return new WorldSpriteEffect(this);
    }
}