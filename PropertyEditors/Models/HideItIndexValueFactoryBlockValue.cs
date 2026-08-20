// Copyright (c) Hide It Package.
// MIT Licensed.

using JetBrains.Annotations;
using Umbraco.Cms.Core.Models.Blocks;

namespace HideIt.PropertyEditors.Models;

[UsedImplicitly( ImplicitUseTargetFlags.WithMembers )]
internal sealed class HideItIndexValueFactoryBlockValue {
  public Dictionary<string, List<HideItLayoutItem>> Layout { get; init; } = [];
  public List<BlockItemData> ContentData { get; init; } = [];
  public List<BlockItemData> SettingsData { get; init; } = [];
  public IList<BlockItemVariation> Expose { get; init; } = [];
}

[UsedImplicitly( ImplicitUseTargetFlags.WithMembers )]
internal sealed class HideItLayoutItem {
  public Guid? ContentKey { get; init; }
  public string? ContentUdi { get; init; }
  public Guid? SettingsKey { get; init; }
  public string? SettingsUdi { get; init; }
  public List<HideItLayoutArea> Areas { get; init; } = [];
}

[UsedImplicitly( ImplicitUseTargetFlags.WithMembers )]
internal sealed class HideItLayoutArea {
  public List<HideItLayoutItem> Items { get; init; } = [];
}