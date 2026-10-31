namespace OpenGta2.Client.Diagnostics;

/// <summary>
/// How the tile textures are mapped on a block (u and v are the axes of the tile texture, after flip and rotation).
/// </summary>
public enum TileSpan
{
    Full,
    /// <summary>The edges of a block are the centres of the first and last texel, so neighbouring tiles share their edge texel; only for flat (transparent) faces.</summary>
    SharedEdgeFlat,
    /// <summary>As <see cref="SharedEdgeFlat"/>, for solid faces too.</summary>
    SharedEdgeAll
}

/// <summary>
/// Debug switches that influence the game; changed from the debug screens.
/// </summary>
public class DebugSettings
{
    /// <summary>
    /// Noon lighting (full ambient light, point lights off) instead of the dusk lighting the original game shows.
    /// </summary>
    public bool Noon { get; set; }

    /// <summary>
    /// Flat (transparent) faces as an alpha test at 0.5 with opaque pixels, instead of blending every pixel with an alpha above 5%.
    /// With bilinear filtering the alpha test gives thin, smooth lines for things like power lines.
    /// </summary>
    public bool FlatAlphaTest { get; set; }

    /// <summary>
    /// Draw the collision debug boxes (player box, block under the player).
    /// </summary>
    public bool ShowHighlights { get; set; }

    /// <summary>
    /// Sample the map tiles without filtering (crisp pixels) instead of bilinear. Bilinear filtering bends diagonal lines a little
    /// where one tile ends and the next begins.
    /// </summary>
    public bool PointSampling { get; set; }

    /// <summary>
    /// Sharp bilinear filtering of the map tiles: crisp texels like point sampling, blended only over about one screen pixel at texel
    /// boundaries. Needs bilinear sampling (point sampling off).
    /// </summary>
    public bool SharpTexels { get; set; }

    /// <summary>
    /// Wrap the map tile coordinates instead of clamping them (what the original game probably does): with bilinear filtering the
    /// edge of a tile then blends in the opposite edge of the same tile.
    /// </summary>
    public bool WrapSampling { get; set; }

    /// <summary>
    /// How tile textures are mapped on a block. The default (shared edge on all faces) puts the edges of a block on the centres of the
    /// first and last texel, so neighbouring tiles share their edge texel; the art of the power line tiles is drawn for that.
    /// </summary>
    public TileSpan TileSpan { get; set; } = TileSpan.SharedEdgeAll;

    /// <summary>
    /// Draw the outlines (in magenta, on top of everything) of the flat faces: the lids and walls with transparent pixels such as power lines.
    /// </summary>
    public bool ShowFlatOutlines { get; set; }

    /// <summary>
    /// Move the player at a tenth of the speed, to step through collision.
    /// </summary>
    public bool SlowPlayer { get; set; }
}
