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
/// so the block action and condition can use a custom property alias.
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
  /// <returns>The configured property alias, falling back to the default when not customized.</returns>
  [HttpGet( "configuration" )]
  [ProducesResponseType( typeof( HideItConfigurationResponseModel ), StatusCodes.Status200OK )]
  public IActionResult Configuration() {
    string alias = _settings.Value.PropertyAlias;
    if ( string.IsNullOrWhiteSpace( alias ) ) {
      alias = HideItSettings.DefaultPropertyAlias;
    }

    return Ok( new HideItConfigurationResponseModel { PropertyAlias = alias.Trim() } );
  }
}

/// <summary>
/// Response model for the Hide It configuration endpoint.
/// </summary>
public class HideItConfigurationResponseModel {
  /// <summary>
  /// The property alias used to determine if a block should be hidden.
  /// </summary>
  public string PropertyAlias { get; set; } = HideItSettings.DefaultPropertyAlias;
}
