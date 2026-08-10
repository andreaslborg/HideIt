// Copyright (c) Hide It Package.
// MIT Licensed.

using HideIt.ValueConverters;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace HideIt.Composers;

/// <summary>
/// Composer that registers Hide It services.
/// Replaces the built-in block property value converters to automatically filter hidden blocks.
/// </summary>
[UsedImplicitly]
public class HideItComposer : IComposer {
  public void Compose( IUmbracoBuilder builder ) {
    // Bind the "HideIt" section from appsettings so a custom property alias can be configured
    builder.Services.Configure<HideItSettings>( builder.Config.GetSection( HideItSettings.SectionName ) );

    // Apply a custom alias to the static extension methods, if one is configured
    string? customAlias = builder.Config.GetSection( HideItSettings.SectionName )[nameof( HideItSettings.PropertyAlias )];
    if ( !string.IsNullOrWhiteSpace( customAlias ) ) {
      BlockExtensions.SetPropertyAlias( customAlias.Trim() );
    }

    // Replace the built-in converters with our HideIt-aware versions
    // This makes hidden block filtering "magic" - no code changes needed in views
    builder.PropertyValueConverters()
        .Replace<BlockListPropertyValueConverter, HideItBlockListPropertyValueConverter>()
        .Replace<BlockGridPropertyValueConverter, HideItBlockGridPropertyValueConverter>();
  }
}