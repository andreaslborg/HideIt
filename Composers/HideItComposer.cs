// Copyright (c) Hide It Package.
// MIT Licensed.

using HideIt.ValueConverters;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    try {
      // Read and normalize alias from configuration once, then register immutable settings.
      string? configuredAlias = builder.Config.GetSection( HideItSettings.SectionName )["PropertyAlias"];
      HideItSettings settings = new( configuredAlias );

      builder.Services.AddSingleton( Options.Create( settings ) );
      BlockExtensions.SetPropertyAlias( settings.PropertyAlias );

      // Replace the built-in converters with our HideIt-aware versions
      // This makes hidden block filtering "magic" - no code changes needed in views
      builder.PropertyValueConverters()
        .Replace<BlockListPropertyValueConverter, HideItBlockListPropertyValueConverter>()
        .Replace<BlockGridPropertyValueConverter, HideItBlockGridPropertyValueConverter>();
    } catch ( Exception ex ) {
      Serilog.Log.Error( ex, "HideIt: Error during composer initialization" );
      throw;
    }
  }
}
