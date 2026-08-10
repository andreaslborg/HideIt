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
  /// The property alias used to determine if a block should be hidden.
  /// Defaults to "hideIt".
  /// </summary>
  public string PropertyAlias { get; } = DefaultPropertyAlias;

  public HideItSettings() {
  }

  public HideItSettings( string? propertyAlias ) {
    PropertyAlias = NormalizePropertyAlias( propertyAlias );
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

  [System.Text.RegularExpressions.GeneratedRegex( "^[A-Za-z][A-Za-z0-9_]*$", System.Text.RegularExpressions.RegexOptions.CultureInvariant )]
  private static partial System.Text.RegularExpressions.Regex MyRegex();
}
