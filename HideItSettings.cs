// Copyright (c) Hide It Package.
// MIT Licensed.

namespace HideIt;

/// <summary>
/// Typed configuration for the Hide It package, bound from the "HideIt" section in appsettings.
/// </summary>
public partial class HideItSettings {
  private static readonly System.Text.RegularExpressions.Regex _propertyAliasPattern = MyRegex();

  private static readonly HashSet<string> _unsafeJavascriptPropertyAliases = new( StringComparer.Ordinal ) {
    "__proto__",
    "constructor",
    "prototype"
  };

  /// <summary>
  /// The configuration section name.
  /// </summary>
  public const string SectionName = "HideIt";

  /// <summary>
  /// The default property alias used when no custom alias is configured.
  /// </summary>
  public const string DefaultPropertyAlias = "hideIt";

  /// <summary>
  /// The default icon shown when a block is visible.
  /// </summary>
  public const string DefaultVisibleIcon = "/App_Plugins/HideIt/icons/eye.svg";

  /// <summary>
  /// The default icon shown when a block is hidden.
  /// </summary>
  public const string DefaultHiddenIcon = "/App_Plugins/HideIt/icons/eye-off.svg";

  /// <summary>
  /// The property alias used to determine if a block should be hidden.
  /// Defaults to "hideIt".
  /// </summary>
  public string PropertyAlias { get; } = DefaultPropertyAlias;

  /// <summary>
  /// Optional custom stylesheet path for block state styling.
  /// When set, Hide It skips its default visual styling.
  /// </summary>
  public string? CssPath { get; }

  /// <summary>
  /// The icon shown when a block is visible.
  /// </summary>
  public string VisibleIcon { get; } = DefaultVisibleIcon;

  /// <summary>
  /// The icon shown when a block is hidden.
  /// </summary>
  public string HiddenIcon { get; } = DefaultHiddenIcon;

  public HideItSettings() {
  }

  public HideItSettings( string? propertyAlias, string? cssPath = null, string? visibleIcon = null, string? hiddenIcon = null ) {
    PropertyAlias = NormalizePropertyAlias( propertyAlias );
    CssPath = NormalizeCssPath( cssPath );
    VisibleIcon = NormalizeIconPath( visibleIcon, DefaultVisibleIcon );
    HiddenIcon = NormalizeIconPath( hiddenIcon, DefaultHiddenIcon );
  }

  /// <summary>
  /// Normalizes a configured alias to a safe value.
  /// Falls back to the default alias when missing, invalid, or unsafe.
  /// </summary>
  public static string NormalizePropertyAlias( string? alias ) {
    string normalizedAlias = alias?.Trim() ?? string.Empty;

    if ( string.IsNullOrWhiteSpace( normalizedAlias ) || !_propertyAliasPattern.IsMatch( normalizedAlias ) || _unsafeJavascriptPropertyAliases.Contains( normalizedAlias ) ) {
      return DefaultPropertyAlias;
    }

    return normalizedAlias;
  }

  /// <summary>
  /// Normalizes a configured stylesheet path.
  /// Returns null when not configured.
  /// </summary>
  public static string? NormalizeCssPath( string? cssPath ) {
    string normalizedCssPath = cssPath?.Trim() ?? string.Empty;
    return string.IsNullOrWhiteSpace( normalizedCssPath ) ? null : normalizedCssPath;
  }

  /// <summary>
  /// Normalizes a configured icon path.
  /// Falls back to the provided default when missing or invalid.
  /// </summary>
  public static string NormalizeIconPath( string? iconPath, string defaultValue ) {
    string normalizedIconPath = iconPath?.Trim() ?? string.Empty;
    return IconPathRegex().IsMatch( normalizedIconPath ) ? normalizedIconPath : defaultValue;
  }

  [System.Text.RegularExpressions.GeneratedRegex( "^[A-Za-z][A-Za-z0-9_]*$", System.Text.RegularExpressions.RegexOptions.CultureInvariant )]
  private static partial System.Text.RegularExpressions.Regex MyRegex();

  [System.Text.RegularExpressions.GeneratedRegex( @"^(\/|\.\/|\.\.\/).+\.svg([?#].*)?$", System.Text.RegularExpressions.RegexOptions.CultureInvariant | System.Text.RegularExpressions.RegexOptions.IgnoreCase )]
  private static partial System.Text.RegularExpressions.Regex IconPathRegex();
}
