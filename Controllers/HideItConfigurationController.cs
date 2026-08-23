// Copyright (c) Hide It Package.
// MIT Licensed.

using Asp.Versioning;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;

namespace HideIt.Controllers;

/// <summary>
/// Backoffice API controller exposing the Hide It configuration to the client,
/// so block actions and conditions can use configured Hide It options.
/// </summary>
[ApiVersion( "1.0" )]
[VersionedApiBackOfficeRoute( "hideit" )]
[ApiExplorerSettings( GroupName = "Hide It" )]
[UsedImplicitly]
public class HideItConfigurationController : ManagementApiControllerBase {
  private readonly IOptions<HideItSettings> _settings;

  public HideItConfigurationController( IOptions<HideItSettings> settings ) {
    _settings = settings;
  }

  /// <summary>
  /// Gets the Hide It configuration.
  /// </summary>
  /// <returns>The configured Hide It client options.</returns>
  [HttpGet( "configuration" )]
  [ProducesResponseType( typeof( HideItConfigurationResponseModel ), StatusCodes.Status200OK )]
  public IActionResult Configuration() {
    string alias = HideItSettings.NormalizePropertyAlias( _settings.Value.PropertyAlias );
    string? cssPath = HideItSettings.NormalizeCssPath( _settings.Value.CssPath );
    string visibleIcon = HideItSettings.NormalizeIconPath( _settings.Value.VisibleIcon, HideItSettings.DefaultVisibleIcon );
    string hiddenIcon = HideItSettings.NormalizeIconPath( _settings.Value.HiddenIcon, HideItSettings.DefaultHiddenIcon );

    return Ok( new HideItConfigurationResponseModel {
      PropertyAlias = alias,
      CssPath = cssPath,
      VisibleIcon = visibleIcon,
      HiddenIcon = hiddenIcon
    } );
  }
}

/// <summary>
/// Response model for the Hide It configuration endpoint.
/// </summary>
public class HideItConfigurationResponseModel {
  /// <summary>
  /// The property alias used to determine if a block should be hidden.
  /// </summary>
  public string PropertyAlias { get; init; } = HideItSettings.DefaultPropertyAlias;

  /// <summary>
  /// Optional custom stylesheet path for block state styling.
  /// </summary>
  public string? CssPath { get; init; }

  /// <summary>
  /// The icon shown when a block is visible.
  /// </summary>
  public string VisibleIcon { get; init; } = HideItSettings.DefaultVisibleIcon;

  /// <summary>
  /// The icon shown when a block is hidden.
  /// </summary>
  public string HiddenIcon { get; init; } = HideItSettings.DefaultHiddenIcon;
}
