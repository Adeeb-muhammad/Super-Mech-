/// <summary>
/// Responsive breakpoint rules for the workshop UI. Kept pure so the desktop
/// and compact layout decisions can be checked with ordinary unit tests.
/// </summary>
public static class WorkshopLayoutRules
{
    public const float CompactBreakpoint = 1024.0f;

    public static bool UseCompactLayout(float viewportWidth)
    {
        return viewportWidth < CompactBreakpoint;
    }

    public static int HorizontalMargin(float viewportWidth)
    {
        return viewportWidth < 600.0f ? 10 : 16;
    }
}
