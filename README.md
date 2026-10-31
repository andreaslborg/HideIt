# Hide It

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![NuGet Version](https://img.shields.io/nuget/vpre/Our.Umbraco.HideIt.svg)](https://www.nuget.org/packages/Our.Umbraco.HideIt)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Our.Umbraco.HideIt.svg)](https://www.nuget.org/packages/Our.Umbraco.HideIt)

Hide It adds block-level visibility control to Umbraco, letting editors toggle blocks on or off without deleting them. It works with both **Block List** and **Block Grid** editors, and requires no changes to your views.

## Features

- **One-click toggle**: An eye icon in the block action bar switches a block between visible and hidden
- **Visual feedback**: Hidden blocks are visually dimmed in the backoffice
- **No view changes required**: Hidden blocks are automatically excluded from frontend rendering
- **Search-safe**: Hidden block content is excluded from Umbraco search indexing
- **Nested support**: Filtering applies recursively within Block Grid areas

<img width="1220" height="874" alt="Hide It" src="https://github.com/user-attachments/assets/2ef439b4-ddc3-46ed-b751-25a90f748816" />

## Installation

```bash
dotnet add package Our.Umbraco.HideIt
```

## Setup

1. Add a **True/False** property to your block's **Settings** element type
2. Set its alias to `hideIt`

The toggle appears automatically in the block action bar.

<img width="876" height="288" alt="toggle-button" src="https://github.com/user-attachments/assets/ce41eeb2-c2c9-4066-a959-055bfb69e0f9" />

See **Advanced / Extensibility** below for configuration options.

## How It Works

Hide It replaces Umbraco's built-in block property value converters. Hidden blocks are filtered out before reaching your views, so no template changes are required.

## Requirements

- Umbraco **17.5+** _(that's when [block actions](https://releases.umbraco.com/release/umbraco/Umbraco-CMS/17.5.0) became a thing)_, **18.x**, or **19.x** (pre-release)
- .NET 10.0 (.NET 11 for Umbraco 19)

## Versions

| Package Version        | Umbraco Version    |
|-------------------------|---------------------|
| 19.x _(pre-release)_   | 19.0.0-beta1+       |
| 18.x                    | 18.0.0 - 18.x       |
| 17.x                    | 17.5.0 - 17.x       |

> **Note:** `19.x` tracks Umbraco 19, which is currently in **beta**. These releases are published as GitHub/NuGet pre-releases and may contain breaking changes until Umbraco 19 reaches a stable (GA) release.

## Advanced / Extensibility

### Custom Property Alias

To use an existing property instead of `hideIt`, set a custom alias in `appsettings.json`:

```json
{
  "HideIt": {
    "PropertyAlias": "hideFromSite"
  }
}
```

Both the backoffice toggle and the frontend filtering use this alias. If unset, `hideIt` is used.

### Custom Styling

To use your own styling instead of the default dimmed appearance, set `HideIt:CssPath`:

```json
{
  "HideIt": {
    "CssPath": "/css/hideit-custom.css"
  }
}
```

- If unset, Hide It applies its default dimmed styling to hidden blocks.
- If set, Hide It's default styling is skipped and your stylesheet is loaded instead (including inside block-entry shadow roots).

Hide It applies these markers to each block entry, which can be targeted in custom CSS:

- Hidden state: `data-hideit-hidden` attribute and `.hideit-block--hidden` class
- Visible state: `.hideit-block--visible` class

### Custom Action Icons

Override the action icon for each state in `appsettings.json`:

```json
{
  "HideIt": {
    "VisibleIcon": "/icons/power.svg",
    "HiddenIcon": "/icons/power-off.svg"
  }
}
```

- `VisibleIcon` is shown when the block is visible.
- `HiddenIcon` is shown when the block is hidden.
- Values must be paths to SVG files.

### Manual Filtering

Extension methods are available for filtering outside the automatic pipeline:

```csharp
using HideIt;

var visibleBlocks = Model.Blocks.WhereVisible();
var visibleGrid = Model.Grid.WhereVisible();

if (!block.IsBlockHidden())
{
    // Block is visible
}
```

### Custom Block Conversion Pipeline

If you have replaced `BlockListPropertyValueConverter` or `BlockGridPropertyValueConverter` with your own implementation, use Hide It's filter extension points instead of chaining converter `Replace<>()` calls:

```csharp
using HideIt;
using Umbraco.Cms.Core.Models.Blocks;

public class MyCustomBlockListFilter : IHideItBlockListFilter {
    public BlockListModel Filter(BlockListModel model) {
        // Custom filtering/transformation
        return model;
    }
}

public class MyCustomBlockGridFilter : IHideItBlockGridFilter {
    public BlockGridModel Filter(BlockGridModel model) {
        // Custom filtering/transformation
        return model;
    }
}

builder.Services.AddSingleton<IHideItBlockListFilter, MyCustomBlockListFilter>();
builder.Services.AddSingleton<IHideItBlockGridFilter, MyCustomBlockGridFilter>();
```

This keeps Hide It's built-in filtering and your custom logic in the same pipeline.

## Contributing

### Local Umbraco Test Site

An in-repo Umbraco 19 (pre-release) site is available under `HideIt.Test`.

- Open `HideIt.slnx`
- Start `HideIt.Test` with **IIS Express** or the **Umbraco.Web.UI** profile
- The site references `HideIt.csproj` directly, so backoffice/frontend changes are picked up automatically
- The sample is a single-page homepage focused on **Content Rows** (Block List) and **Content Grid** (Block Grid)
- A seeded SQLite database is included, so you can start testing immediately
- Backoffice login: `admin@example.com` / `1234567890`

Found a bug or have a feature request? Open an issue [here](https://github.com/andreaslborg/HideIt/issues). Pull requests are welcome.

## License

MIT
