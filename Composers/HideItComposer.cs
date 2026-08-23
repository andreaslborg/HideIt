// Copyright (c) Hide It Package.
// MIT Licensed.

using HideIt.Filters;
using HideIt.PropertyEditors;
using HideIt.ValueConverters;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace HideIt.Composers;

/// <summary>
/// Composer that registers Hide It services.
/// Replaces the built-in block property value converters and registers the Hide It filter pipeline.
/// </summary>
[UsedImplicitly]
public class HideItComposer : IComposer {
  public void Compose( IUmbracoBuilder builder ) {
    try {
      // Read and normalize alias from configuration once, then register immutable settings.
      string? configuredAlias = builder.Config.GetSection( HideItSettings.SectionName )["PropertyAlias"];
      string? configuredCssPath = builder.Config.GetSection( HideItSettings.SectionName )["CssPath"];
      string? configuredVisibleIcon = builder.Config.GetSection( HideItSettings.SectionName )["VisibleIcon"];
      string? configuredHiddenIcon = builder.Config.GetSection( HideItSettings.SectionName )["HiddenIcon"];
      HideItSettings settings = new( configuredAlias, configuredCssPath, configuredVisibleIcon, configuredHiddenIcon );

      builder.Services.AddSingleton( Options.Create( settings ) );
      builder.Services.AddSingleton<IHideItBlockListFilter, HideItVisibilityBlockListFilter>();
      builder.Services.AddSingleton<IHideItBlockGridFilter, HideItVisibilityBlockGridFilter>();
      builder.Services.Replace( ServiceDescriptor.Singleton<IBlockValuePropertyIndexValueFactory, HideItBlockValuePropertyIndexValueFactory>() );
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
