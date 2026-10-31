using System.Collections.Generic;
using Godot;

/// <summary>
/// Original vector mech preview. It intentionally uses generated shapes rather
/// than copying additional source-game art into this fan project.
/// </summary>
public partial class MechWorkshopPreview : Control
{
    private readonly Dictionary<MechPartSlot, WorkshopPartDefinition> _loadout = new();
    private static readonly Color Background = new(0.055f, 0.075f, 0.095f);
    private static readonly Color Outline = new(0.16f, 0.22f, 0.28f);
    private static readonly Color Highlight = new(0.55f, 0.78f, 0.82f);

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(320, 330);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void SetLoadout(IReadOnlyDictionary<MechPartSlot, WorkshopPartDefinition> loadout)
    {
        _loadout.Clear();

        if (loadout != null)
        {
            foreach (KeyValuePair<MechPartSlot, WorkshopPartDefinition> entry in loadout)
            {
                if (entry.Value != null)
                    _loadout[entry.Key] = entry.Value;
            }
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        float unit = Mathf.Min(Size.X / 390.0f, Size.Y / 390.0f);
        unit = Mathf.Max(0.55f, unit);
        Vector2 center = new(Size.X * 0.5f, Size.Y * 0.47f);

        DrawGrid(unit);
        DrawLine(center + new Vector2(-130, 150) * unit, center + new Vector2(130, 150) * unit, Outline, 2.0f * unit);

        Color frameColor = GetPartColor(MechPartSlot.Frame, new Color(0.30f, 0.43f, 0.55f));
        Color mobilityColor = GetPartColor(MechPartSlot.Mobility, new Color(0.27f, 0.39f, 0.43f));
        Color coreColor = GetPartColor(MechPartSlot.Core, new Color(0.78f, 0.62f, 0.31f));
        Color utilityColor = GetPartColor(MechPartSlot.Utility, new Color(0.51f, 0.43f, 0.62f));

        // Legs and feet.
        DrawRectAt(center, -48, 72, 33, 110, mobilityColor, unit);
        DrawRectAt(center, 15, 72, 33, 110, mobilityColor, unit);
        DrawRectAt(center, -59, 173, 54, 17, Shade(mobilityColor, 0.72f), unit);
        DrawRectAt(center, 5, 173, 54, 17, Shade(mobilityColor, 0.72f), unit);
        DrawCircle(center + new Vector2(-31, 74) * unit, 16 * unit, Outline);
        DrawCircle(center + new Vector2(31, 74) * unit, 16 * unit, Outline);
        DrawCircle(center + new Vector2(-31, 74) * unit, 8 * unit, Highlight);
        DrawCircle(center + new Vector2(31, 74) * unit, 8 * unit, Highlight);

        // Main frame and shoulders.
        DrawRectAt(center, -71, -37, 142, 112, frameColor, unit);
        DrawRectAt(center, -98, -27, 29, 61, Shade(frameColor, 0.82f), unit);
        DrawRectAt(center, 69, -27, 29, 61, Shade(frameColor, 0.82f), unit);
        DrawRectAt(center, -43, -68, 86, 31, Shade(frameColor, 1.18f), unit);
        DrawRectAt(center, -23, -103, 46, 35, Shade(frameColor, 1.28f), unit);

        // Cockpit glass and service details.
        DrawRectAt(center, -17, -94, 34, 20, new Color(0.20f, 0.62f, 0.68f), unit);
        DrawRectAt(center, -47, 5, 94, 11, Outline, unit);
        DrawLine(center + new Vector2(0, -52) * unit, center + new Vector2(0, 50) * unit, Outline, 2 * unit);
        DrawCircle(center + new Vector2(0, -13) * unit, 10 * unit, coreColor);
        DrawCircle(center + new Vector2(0, -13) * unit, 4 * unit, Background);

        // Optional support modules are shown as side-mounted blocks.
        if (_loadout.ContainsKey(MechPartSlot.Core))
        {
            DrawRectAt(center, -116, 15, 24, 45, coreColor, unit);
            DrawRectAt(center, 92, 15, 24, 45, coreColor, unit);
        }

        if (_loadout.ContainsKey(MechPartSlot.Utility))
        {
            DrawRectAt(center, -81, 44, 21, 26, utilityColor, unit);
            DrawRectAt(center, 60, 44, 21, 26, utilityColor, unit);
        }

        DrawLine(center + new Vector2(-145, 198) * unit, center + new Vector2(145, 198) * unit, Outline, 1.2f * unit);
        DrawString(ThemeDB.FallbackFont, center + new Vector2(-95, 225) * unit, "WORKSHOP / FRAME PREVIEW",
            HorizontalAlignment.Left, -1, Mathf.Max(10, (int)(11 * unit)), Highlight);
    }

    private void DrawGrid(float unit)
    {
        Color grid = new(0.11f, 0.15f, 0.18f, 0.55f);
        for (int x = 20; x < Size.X; x += Mathf.Max(12, (int)(26 * unit)))
            DrawLine(new Vector2(x, 0), new Vector2(x, Size.Y), grid, 1.0f);
        for (int y = 20; y < Size.Y; y += Mathf.Max(12, (int)(26 * unit)))
            DrawLine(new Vector2(0, y), new Vector2(Size.X, y), grid, 1.0f);
    }

    private void DrawRectAt(Vector2 center, float x, float y, float width, float height, Color color, float unit)
    {
        var rect = new Rect2(
            center + new Vector2(x, y) * unit,
            new Vector2(width, height) * unit);
        DrawRect(rect, Outline);
        DrawRect(new Rect2(rect.Position + new Vector2(2, 2) * unit, rect.Size - new Vector2(4, 4) * unit), color);
    }

    private Color GetPartColor(MechPartSlot slot, Color fallback)
    {
        if (!_loadout.TryGetValue(slot, out WorkshopPartDefinition part) ||
            string.IsNullOrWhiteSpace(part.PreviewColor))
            return fallback;

        return Color.FromHtml(part.PreviewColor);
    }

    private static Color Shade(Color color, float factor)
    {
        return new Color(
            Mathf.Clamp(color.R * factor, 0, 1),
            Mathf.Clamp(color.G * factor, 0, 1),
            Mathf.Clamp(color.B * factor, 0, 1),
            color.A);
    }
}
