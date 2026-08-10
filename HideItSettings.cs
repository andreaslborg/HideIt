// Copyright (c) Hide It Package.
// MIT Licensed.

namespace HideIt;

/// <summary>
/// Typed configuration for the Hide It package, bound from the "HideIt" section in appsettings.
/// </summary>
public class HideItSettings {
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
  public string PropertyAlias { get; set; } = DefaultPropertyAlias;
}
