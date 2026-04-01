using UnityEngine;

/// <summary>
/// Generates all game sprites programmatically using Texture2D and Sprite.Create().
/// No external art assets are required — everything is drawn at runtime.
/// </summary>
public static class ProceduralGraphics
{
    // ── Sprite size constants ──────────────────────────────────────────────────
    private const int BlackHoleSize  = 128;
    private const int CityObjectSize = 64;
    private const int BuffSize       = 64;
    private const int UISize         = 128;
    private const int MapGroundSize  = 512;

    // ── Black Hole ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a black-hole sprite with a radial gradient from black centre to a
    /// coloured edge plus a glowing ring.
    /// </summary>
    public static Sprite CreateBlackHoleSprite(Color accentColor)
    {
        int size = BlackHoleSize;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float half   = size * 0.5f;
        float radius = half - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half;
                float dy = y - half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                {
                    pixels[y * size + x] = Color.clear;
                    continue;
                }

                float t = dist / radius; // 0 = centre, 1 = edge

                // Core: black → accent colour gradient
                Color core = Color.Lerp(Color.black, accentColor * 0.6f, t * t);

                // Glowing ring near the edge
                float ringT = Mathf.Clamp01((t - 0.75f) / 0.2f);
                Color ring  = Color.Lerp(core, accentColor, ringT);

                // Outer glow fade-out
                float alpha = dist < radius - 4f ? 1f : Mathf.Lerp(1f, 0f, (dist - (radius - 4f)) / 4f);

                pixels[y * size + x] = new Color(ring.r, ring.g, ring.b, alpha);
            }
        }

        // Draw swirling particle dots
        DrawOrbitDots(pixels, size, accentColor, 8, half * 0.85f);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // ── City Objects ──────────────────────────────────────────────────────────

    /// <summary>Small grey/brown building rectangle with yellow window dots.</summary>
    public static Sprite CreateBuildingSmallSprite()
    {
        int w = 24, h = 32;
        return CreateBuildingSprite(w, h, new Color(0.4f, 0.38f, 0.36f), 2, 3);
    }

    /// <summary>Tall skyscraper with more windows and darker colour.</summary>
    public static Sprite CreateBuildingLargeSprite()
    {
        int w = 22, h = 48;
        return CreateBuildingSprite(w, h, new Color(0.28f, 0.3f, 0.35f), 3, 5);
    }

    /// <summary>Coloured car rectangle with two dark wheel circles.</summary>
    public static Sprite CreateCarSprite(Color bodyColor)
    {
        int size = CityObjectSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        // Car body
        FillRect(pixels, size, 4, 12, size - 8, size - 24, bodyColor);

        // Windshield strip (lighter)
        Color glass = new Color(0.6f, 0.8f, 0.9f, 0.8f);
        FillRect(pixels, size, 10, 18, size - 20, 8, glass);

        // Wheels
        DrawFilledCircle(pixels, size, 12, 10, 7, new Color(0.15f, 0.15f, 0.15f));
        DrawFilledCircle(pixels, size, size - 12, 10, 7, new Color(0.15f, 0.15f, 0.15f));

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Green circle crown on a brown trunk.</summary>
    public static Sprite CreateTreeSprite()
    {
        int size = CityObjectSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        // Trunk
        FillRect(pixels, size, size / 2 - 4, 2, 8, 20, new Color(0.45f, 0.28f, 0.1f));

        // Crown
        DrawFilledCircle(pixels, size, size / 2, size / 2 + 8, 20, new Color(0.18f, 0.62f, 0.17f));

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Small brown bench rectangle with two thin legs.</summary>
    public static Sprite CreateBenchSprite()
    {
        int size = CityObjectSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        Color wood = new Color(0.55f, 0.35f, 0.15f);

        // Seat plank
        FillRect(pixels, size, 8, size / 2, size - 16, 6, wood);

        // Left leg
        FillRect(pixels, size, 10, size / 2 - 10, 4, 10, wood);

        // Right leg
        FillRect(pixels, size, size - 14, size / 2 - 10, 4, 10, wood);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Thin grey pole with a yellow light circle on top.</summary>
    public static Sprite CreateStreetLightSprite()
    {
        int size = CityObjectSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        Color pole = new Color(0.55f, 0.55f, 0.55f);

        // Pole
        FillRect(pixels, size, size / 2 - 2, 4, 4, size - 20, pole);

        // Light glow halo
        DrawFilledCircle(pixels, size, size / 2, size - 12, 10, new Color(1f, 0.95f, 0.5f, 0.5f));

        // Light core
        DrawFilledCircle(pixels, size, size / 2, size - 12, 5, new Color(1f, 0.95f, 0.4f));

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Small dark-green irregular circle (bush).</summary>
    public static Sprite CreateBushSprite()
    {
        int size = CityObjectSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        float half   = size * 0.5f;
        float radius = half - 6f;

        // Fixed seed ensures a consistent, reproducible bush shape every run
        System.Random rng = new System.Random(42);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half;
                float dy = y - half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Noisy edge
                float noise = (float)(rng.NextDouble() - 0.5) * 6f;
                if (dist > radius + noise) continue;

                float t = dist / radius;
                Color c = Color.Lerp(new Color(0.1f, 0.45f, 0.1f), new Color(0.2f, 0.65f, 0.2f), 1f - t);
                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Small dark-grey cylinder (rectangle with slightly lighter top).</summary>
    public static Sprite CreateTrashCanSprite()
    {
        int size = CityObjectSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        Color body = new Color(0.3f, 0.3f, 0.3f);
        Color lid  = new Color(0.45f, 0.45f, 0.45f);

        // Body
        FillRect(pixels, size, 14, 6, size - 28, size - 20, body);

        // Lid
        FillRect(pixels, size, 12, size - 18, size - 24, 6, lid);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // ── Map ───────────────────────────────────────────────────────────────────

    /// <summary>Light-grey circular asphalt ground disc.</summary>
    public static Sprite CreateGroundSprite()
    {
        int size = MapGroundSize;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float half   = size * 0.5f;
        float radius = half - 2f;

        Color asphalt = new Color(0.22f, 0.24f, 0.22f);
        Color border  = new Color(0.08f, 0.08f, 0.08f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half;
                float dy = y - half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius) { pixels[y * size + x] = Color.clear; continue; }

                // Dark border ring
                if (dist > radius - 8f)
                    pixels[y * size + x] = border;
                else
                    pixels[y * size + x] = asphalt;
            }
        }

        // Draw grid-like road lines
        DrawRoadLines(pixels, size, radius);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // ── Buffs ─────────────────────────────────────────────────────────────────

    /// <summary>Yellow lightning bolt (speed boost).</summary>
    public static Sprite CreateSpeedBuffSprite()
    {
        int size = BuffSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        Color yellow = new Color(1f, 0.85f, 0.1f);
        DrawFilledCircle(pixels, size, size / 2, size / 2, size / 2 - 2, new Color(1f, 0.85f, 0.1f, 0.25f));
        DrawLightningBolt(pixels, size, yellow);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Red/grey U-shape (magnet).</summary>
    public static Sprite CreateMagnetBuffSprite()
    {
        int size = BuffSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        DrawFilledCircle(pixels, size, size / 2, size / 2, size / 2 - 2, new Color(0.8f, 0.1f, 0.1f, 0.25f));
        DrawMagnetShape(pixels, size, new Color(0.8f, 0.1f, 0.1f), new Color(0.5f, 0.5f, 0.55f));

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Orange circle with "x2" text impression.</summary>
    public static Sprite CreateDoubleMassBuffSprite()
    {
        int size = BuffSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        DrawFilledCircle(pixels, size, size / 2, size / 2, size / 2 - 2, new Color(1f, 0.5f, 0.05f));
        DrawX2Mark(pixels, size, Color.white);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Blue diamond/shield shape.</summary>
    public static Sprite CreateShieldBuffSprite()
    {
        int size = BuffSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        DrawShieldShape(pixels, size, new Color(0.2f, 0.5f, 1f), new Color(0.6f, 0.8f, 1f));

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Purple expanding rings icon (gravity pulse).</summary>
    public static Sprite CreateGravityPulseBuffSprite()
    {
        int size = BuffSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        Color purple = new Color(0.6f, 0.1f, 0.9f);
        DrawExpandingRings(pixels, size, purple);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    /// <summary>Semi-transparent dark circle (joystick background).</summary>
    public static Sprite CreateJoystickBackgroundSprite()
    {
        int size = UISize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        float half   = size * 0.5f;
        float radius = half - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half;
                float dy = y - half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > radius) continue;

                float edge = Mathf.Clamp01((radius - dist) / 6f);
                pixels[y * size + x] = new Color(0.1f, 0.1f, 0.1f, 0.55f * edge);

                if (dist > radius - 4f)
                    pixels[y * size + x] = new Color(0.5f, 0.5f, 0.5f, 0.6f * edge);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Lighter semi-transparent circle (joystick knob).</summary>
    public static Sprite CreateJoystickKnobSprite()
    {
        int size = UISize / 2;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        float half   = size * 0.5f;
        float radius = half - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half;
                float dy = y - half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > radius) continue;

                float t = dist / radius;
                pixels[y * size + x] = new Color(0.7f, 0.7f, 0.75f, 0.85f - t * 0.3f);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Semi-transparent dark rounded rectangle (panel background).</summary>
    public static Sprite CreatePanelSprite()
    {
        int w = 200, h = 100;
        Texture2D tex = NewTexture(w, h);
        Color[] pixels = new Color[w * h];
        Color bg = new Color(0f, 0f, 0f, 0.65f);
        int corner = 8;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool inCorner = false;
                int cx = -1, cy = -1;
                if (x < corner && y < corner)           { cx = corner - x; cy = corner - y; inCorner = true; }
                else if (x > w - corner - 1 && y < corner) { cx = x - (w - corner - 1); cy = corner - y; inCorner = true; }
                else if (x < corner && y > h - corner - 1) { cx = corner - x; cy = y - (h - corner - 1); inCorner = true; }
                else if (x > w - corner - 1 && y > h - corner - 1) { cx = x - (w - corner - 1); cy = y - (h - corner - 1); inCorner = true; }

                if (inCorner && Mathf.Sqrt(cx * cx + cy * cy) > corner)
                    continue;

                pixels[y * w + x] = bg;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>White 1×1 pixel — used as a generic flat-colour sprite source.</summary>
    public static Sprite CreateWhitePixelSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    // ── Territory ─────────────────────────────────────────────────────────────

    /// <summary>1×1 semi-transparent coloured pixel for territory fill.</summary>
    public static Sprite CreateTerritorySprite(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        Color c = color;
        c.a = 0.35f;
        tex.SetPixel(0, 0, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    // ── Internal drawing helpers ──────────────────────────────────────────────

    private static Texture2D NewTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    private static void FillRect(Color[] pixels, int texWidth, int x, int y, int w, int h, Color color)
    {
        for (int py = y; py < y + h; py++)
        {
            for (int px = x; px < x + w; px++)
            {
                int idx = py * texWidth + px;
                if (idx >= 0 && idx < pixels.Length)
                    pixels[idx] = color;
            }
        }
    }

    private static void DrawFilledCircle(Color[] pixels, int texWidth, int cx, int cy, int radius, Color color)
    {
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int idx = y * texWidth + x;
                    if (idx >= 0 && idx < pixels.Length)
                        pixels[idx] = color;
                }
            }
        }
    }

    private static void DrawOrbitDots(Color[] pixels, int texWidth, Color color, int dotCount, float orbitRadius)
    {
        int half = texWidth / 2;
        for (int i = 0; i < dotCount; i++)
        {
            float angle = (float)i / dotCount * Mathf.PI * 2f;
            int cx = half + Mathf.RoundToInt(Mathf.Cos(angle) * orbitRadius);
            int cy = half + Mathf.RoundToInt(Mathf.Sin(angle) * orbitRadius);
            DrawFilledCircle(pixels, texWidth, cx, cy, 3, color);
        }
    }

    private static Sprite CreateBuildingSprite(int w, int h, Color wallColor, int cols, int rows)
    {
        int size = CityObjectSize;
        Texture2D tex = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        int bx = (size - w) / 2;
        int by = (size - h) / 2;

        FillRect(pixels, size, bx, by, w, h, wallColor);

        // Windows
        Color window = new Color(0.95f, 0.9f, 0.4f, 0.9f);
        int padX = 3, padY = 4;
        int cellW = (w - padX * 2) / cols;
        int cellH = (h - padY * 2) / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int wx = bx + padX + col * cellW + 1;
                int wy = by + padY + row * cellH + 1;
                FillRect(pixels, size, wx, wy, Mathf.Max(1, cellW - 2), Mathf.Max(1, cellH - 2), window);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static void DrawRoadLines(Color[] pixels, int texSize, float mapRadius)
    {
        Color road = new Color(0.32f, 0.32f, 0.32f);
        int half  = texSize / 2;
        int thick = Mathf.Max(2, texSize / 64);

        // Horizontal and vertical main roads
        for (int i = -thick; i <= thick; i++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float dx = x - half;
                if (Mathf.Abs(dx) < mapRadius)
                {
                    int idx = (half + i) * texSize + x;
                    if (idx >= 0 && idx < pixels.Length) pixels[idx] = road;
                    idx = x * texSize + (half + i);
                    if (idx >= 0 && idx < pixels.Length) pixels[idx] = road;
                }
            }
        }

        // Diagonal roads
        for (int x = 0; x < texSize; x++)
        {
            for (int dt = -thick; dt <= thick; dt++)
            {
                int y1 = x + dt;
                int y2 = texSize - 1 - x + dt;

                if (y1 >= 0 && y1 < texSize)
                {
                    float dx = x - half; float dy = y1 - half;
                    if (dx * dx + dy * dy < mapRadius * mapRadius)
                    {
                        int idx = y1 * texSize + x;
                        if (idx >= 0 && idx < pixels.Length) pixels[idx] = road;
                    }
                }
                if (y2 >= 0 && y2 < texSize)
                {
                    float dx = x - half; float dy = y2 - half;
                    if (dx * dx + dy * dy < mapRadius * mapRadius)
                    {
                        int idx = y2 * texSize + x;
                        if (idx >= 0 && idx < pixels.Length) pixels[idx] = road;
                    }
                }
            }
        }
    }

    private static void DrawLightningBolt(Color[] pixels, int size, Color color)
    {
        // Simple zigzag polyline representing a bolt
        int[] xs = { size / 2 + 6, size / 2 + 2, size / 2 + 8, size / 2 - 4, size / 2 - 8, size / 2 - 2, size / 2 - 8 };
        int[] ys = { size - 10,    size / 2 + 8, size / 2 + 2, size / 2 - 2, 10,            size / 2 - 8, size / 2 - 2 };

        for (int i = 0; i < xs.Length - 1; i++)
            DrawLine(pixels, size, xs[i], ys[i], xs[i + 1], ys[i + 1], color, 2);
    }

    private static void DrawMagnetShape(Color[] pixels, int size, Color tipColor, Color bodyColor)
    {
        int cx = size / 2;
        int cy = size / 2;
        int r  = size / 2 - 8;
        int thick = 6;

        // U-curve (arc from left leg down and around to right leg)
        for (float a = 0f; a <= Mathf.PI; a += 0.02f)
        {
            for (int t = -thick / 2; t <= thick / 2; t++)
            {
                int x = cx + Mathf.RoundToInt((r + t) * Mathf.Cos(a));
                int y = cy - Mathf.RoundToInt((r + t) * Mathf.Sin(a));
                int idx = y * size + x;
                if (idx >= 0 && idx < pixels.Length)
                    pixels[idx] = bodyColor;
            }
        }

        // Red tips
        FillRect(pixels, size, cx - r - thick / 2, cy - 10, thick + 1, 12, tipColor);
        FillRect(pixels, size, cx + r - thick / 2, cy - 10, thick + 1, 12, tipColor);
    }

    private static void DrawX2Mark(Color[] pixels, int size, Color color)
    {
        // Two crossing diagonal lines ("X")
        DrawLine(pixels, size, size / 4, size / 4, size / 2 - 2, size * 3 / 4, color, 2);
        DrawLine(pixels, size, size / 2 - 2, size / 4, size / 4, size * 3 / 4, color, 2);

        // "2" shape — simplified strokes
        int ox = size / 2 + 4;
        DrawLine(pixels, size, ox,      size * 3 / 4, ox + 12, size * 3 / 4, color, 2);
        DrawLine(pixels, size, ox + 12, size * 3 / 4, ox + 12, size / 2,     color, 2);
        DrawLine(pixels, size, ox,      size / 2,     ox + 12, size / 2,     color, 2);
        DrawLine(pixels, size, ox,      size / 2,     ox,      size / 4,     color, 2);
        DrawLine(pixels, size, ox,      size / 4,     ox + 12, size / 4,     color, 2);
    }

    private static void DrawShieldShape(Color[] pixels, int size, Color fill, Color highlight)
    {
        int cx = size / 2;
        // Shield: hexagonal-ish polygon
        int[] xs = { cx, cx + 18, cx + 22, cx + 18, cx,  cx - 18, cx - 22, cx - 18 };
        int[] ys = { size - 8, size - 14, size / 2, 10, 6, 10, size / 2, size - 14 };

        FillPolygon(pixels, size, xs, ys, fill);

        // Highlight stripe
        for (int y = size / 4; y < size * 3 / 4; y++)
        {
            int idx = y * size + cx - 2;
            if (idx >= 0 && idx < pixels.Length) pixels[idx] = highlight;
            idx = y * size + cx - 1;
            if (idx >= 0 && idx < pixels.Length) pixels[idx] = highlight;
        }
    }

    private static void DrawExpandingRings(Color[] pixels, int size, Color color)
    {
        int cx = size / 2;
        int cy = size / 2;
        int[] radii = { 6, 13, 20, 27 };
        float[] alphas = { 1f, 0.8f, 0.55f, 0.3f };

        for (int r = 0; r < radii.Length; r++)
        {
            Color c = new Color(color.r, color.g, color.b, alphas[r]);
            DrawRing(pixels, size, cx, cy, radii[r], 2, c);
        }

        // Centre dot
        DrawFilledCircle(pixels, size, cx, cy, 4, color);
    }

    private static void DrawRing(Color[] pixels, int size, int cx, int cy, int radius, int thickness, Color color)
    {
        for (float a = 0; a < Mathf.PI * 2f; a += 0.02f)
        {
            for (int t = 0; t < thickness; t++)
            {
                int r = radius + t;
                int x = cx + Mathf.RoundToInt(Mathf.Cos(a) * r);
                int y = cy + Mathf.RoundToInt(Mathf.Sin(a) * r);
                int idx = y * size + x;
                if (idx >= 0 && idx < pixels.Length)
                    pixels[idx] = color;
            }
        }
    }

    private static void DrawLine(Color[] pixels, int texWidth, int x0, int y0, int x1, int y1, Color color, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            for (int tx = -thickness / 2; tx <= thickness / 2; tx++)
            {
                for (int ty = -thickness / 2; ty <= thickness / 2; ty++)
                {
                    int idx = (y0 + ty) * texWidth + (x0 + tx);
                    if (idx >= 0 && idx < pixels.Length)
                        pixels[idx] = color;
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 <  dx) { err += dx; y0 += sy; }
        }
    }

    private static void FillPolygon(Color[] pixels, int size, int[] xs, int[] ys, Color color)
    {
        int minY = int.MaxValue, maxY = int.MinValue;
        for (int i = 0; i < ys.Length; i++) { minY = Mathf.Min(minY, ys[i]); maxY = Mathf.Max(maxY, ys[i]); }
        minY = Mathf.Clamp(minY, 0, size - 1);
        maxY = Mathf.Clamp(maxY, 0, size - 1);

        for (int y = minY; y <= maxY; y++)
        {
            int minX = size, maxX = 0;
            int n = xs.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if ((ys[i] <= y && y < ys[j]) || (ys[j] <= y && y < ys[i]))
                {
                    int ix = xs[i] + (y - ys[i]) * (xs[j] - xs[i]) / (ys[j] - ys[i]);
                    minX = Mathf.Min(minX, ix);
                    maxX = Mathf.Max(maxX, ix);
                }
            }
            for (int x = Mathf.Clamp(minX, 0, size - 1); x <= Mathf.Clamp(maxX, 0, size - 1); x++)
            {
                int idx = y * size + x;
                if (idx >= 0 && idx < pixels.Length)
                    pixels[idx] = color;
            }
        }
    }
}
