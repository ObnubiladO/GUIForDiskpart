using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace GUIForDiskpart.Presentation;

public static class ThemeManager
{
    private static readonly IReadOnlyDictionary<string, (string Light, string Dark)> Colors =
        new Dictionary<string, (string, string)>
        {
            ["ThemeWindowBrush"] = ("#FF8C8C8C", "#FF20242B"),
            ["ThemeTextBrush"] = ("#FF000000", "#FFF0F2F5"),
            ["ThemeButtonBrush"] = ("#FFC3C3C3", "#FF353B46"),
            ["ThemeTileBrush"] = ("#FFA5A4A4", "#FF343A44"),
            ["ThemeBorderBrush"] = ("#FF676767", "#FF66717E"),
            ["ThemeInputBrush"] = ("#FFFFFFFF", "#FF2B313B"),
            ["ThemeMenuBrush"] = ("#FFF0F0F0", "#FF282D36"),
            ["ThemeTableBrush"] = ("#FFFFFFFF", "#FF272D36"),
            ["ThemeTableRowBrush"] = ("#FFC7C7C7", "#FF313845"),
            ["ThemeTableAlternateBrush"] = ("#FFF3F5E0", "#FF39414D"),
            ["ThemeSearchBrush"] = ("#FF3C3C3C", "#FF2B313B"),
            ["ThemeSearchTextBrush"] = ("#FFE4E4E4", "#FFF0F2F5"),
            ["ThemeLogBrush"] = ("#FF252525", "#FF171B20"),
            ["ThemeLogTextBrush"] = ("#FF009409", "#FF42D46A"),
            ["ThemePressedBrush"] = ("#FFC0C0C0", "#FF424B58"),
            ["ThemeBevelLightBrush"] = ("#FFFFFFFF", "#FF596573"),
            ["ThemeBevelShadowBrush"] = ("#FF828282", "#FF20252C"),
            ["ThemeBevelDarkBrush"] = ("#FF000000", "#FF101319"),
            ["ThemePressedHighlightBrush"] = ("#FFDFDFDF", "#FF343D48"),
            ["ThemeDangerBrush"] = ("#FFB90000", "#FFFF6666")
        };

    public static bool IsDarkMode { get; private set; }

    public static void Initialize()
    {
        ApplyPalette(Application.Current.Resources, Properties.Settings.Default.DarkMode);
    }

    public static void SetDarkMode(bool darkMode)
    {
        ApplyPalette(Application.Current.Resources, darkMode);
        Properties.Settings.Default.DarkMode = darkMode;
        Properties.Settings.Default.Save();
    }

    public static void ApplyPalette(ResourceDictionary resources, bool darkMode)
    {
        foreach (var (key, colors) in Colors)
        {
            var color = (Color)ColorConverter.ConvertFromString(darkMode ? colors.Dark : colors.Light);
            resources[key] = new SolidColorBrush(color);
        }
        IsDarkMode = darkMode;
    }
}
