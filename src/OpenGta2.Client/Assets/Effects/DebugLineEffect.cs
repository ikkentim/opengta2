using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Assets.Effects;

public class DebugLineEffect : Effect
{
    private readonly EffectParameter _matrixParam;
    
    public Matrix TransformMatrix { get; set; }

    public DebugLineEffect(Effect effect)
        : base(effect)
    {
        _matrixParam = Parameters["MatrixTransform"];
    }

    protected override void OnApply()
    {
        _matrixParam.SetValue(TransformMatrix);
    }

    public override Effect Clone()
    {
        return new DebugLineEffect(this);
    }
}