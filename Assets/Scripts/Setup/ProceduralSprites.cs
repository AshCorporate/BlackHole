using UnityEngine;

/// <summary>
/// Helper class for creating simple procedural sprites at runtime.
/// Used by GameBootstrapper to build the entire game scene without external art assets.
/// </summary>
public static class ProceduralSprites
{
    // Default pixels-per-unit for all generated sprites
    private const float PixelsPerUnit = 100f;

    /// <summary>
    /// Creates a filled circle sprite of the given texture size and color.
    /// </summary>
    /// <param name="size">Texture width and height in pixels (e.g. 128).</param>
    /// <param name="color">Fill color of the circle.</param>
    /// <returns>A Unity Sprite backed by a generated Texture2D.</returns>
    public static Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        float radius = size * 0.5f;
        float cx = radius;
        float cy = radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx + 0.5f;
                float dy = y - cy + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= radius)
                {
                    // Soft edge anti-aliasing
                    float alpha = Mathf.Clamp01(radius - dist);
                    Color c = color;
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
    }

    /// <summary>
    /// Creates a filled square (rectangle) sprite of the given texture size and color.
    /// </summary>
    /// <param name="size">Texture width and height in pixels.</param>
    /// <param name="color">Fill color of the square.</param>
    /// <returns>A Unity Sprite backed by a generated Texture2D.</returns>
    public static Sprite CreateSquareSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
    }

    /// <summary>
    /// Creates a gradient circle sprite that is bright at the rim and dark at the center,
    /// suitable for black hole visuals.
    /// </summary>
    /// <param name="size">Texture size in pixels.</param>
    /// <param name="innerColor">Color at the center of the circle.</param>
    /// <param name="outerColor">Color at the rim of the circle.</param>
    /// <returns>A Unity Sprite.</returns>
    public static Sprite CreateGradientCircleSprite(int size, Color innerColor, Color outerColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        float radius = size * 0.5f;
        float cx = radius;
        float cy = radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx + 0.5f;
                float dy = y - cy + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= radius)
                {
                    float t = dist / radius;
                    float alpha = Mathf.Clamp01(radius - dist);
                    Color c = Color.Lerp(innerColor, outerColor, t);
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
    }

    /// <summary>
    /// Creates a thin ring (hollow circle outline) sprite.
    /// Useful for player outline circles and indicators.
    /// </summary>
    /// <param name="size">Texture width and height in pixels (e.g. 128).</param>
    /// <param name="color">Ring color.</param>
    /// <param name="ringWidthFraction">Ring stroke width as a fraction of the radius (default 0.1 = 10%).</param>
    /// <returns>A Unity Sprite backed by a generated Texture2D.</returns>
    public static Sprite CreateRingSprite(int size, Color color, float ringWidthFraction = 0.1f)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        float outerRadius = size * 0.5f;
        float innerRadius = outerRadius * (1f - ringWidthFraction);
        float cx = outerRadius;
        float cy = outerRadius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx + 0.5f;
                float dy = y - cy + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Soft anti-aliased ring between inner and outer radius
                float outerAlpha = Mathf.Clamp01(outerRadius - dist);
                float innerAlpha = Mathf.Clamp01(dist - innerRadius);
                float alpha      = outerAlpha * innerAlpha;

                Color c = color;
                c.a = alpha;
                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
    }

    /// <summary>
    /// Creates a solid white 1×1 pixel sprite, useful as a base for tinted UI Images.
    /// </summary>
    public static Sprite CreateWhiteSquare()
    {
        return CreateSquareSprite(4, Color.white);
    }
}
