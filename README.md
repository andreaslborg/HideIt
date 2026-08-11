# Hide It

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![NuGet Version](https://img.shields.io/nuget/v/Our.Umbraco.HideIt.svg)](https://www.nuget.org/packages/Our.Umbraco.HideIt)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Our.Umbraco.HideIt.svg)](https://www.nuget.org/packages/Our.Umbraco.HideIt)

**The "now you see me, now you don't" package for Umbraco Block List and Block Grid!**

Ever wanted to temporarily hide a block without deleting it? Maybe it's a seasonal promo, a work-in-progress section, or that testimonial from your ex-client. Whatever the reason - Hide It has your back.

Works with both **Block List** and **Block Grid** editors.

## What's in the Box?

- **One-Click Toggle** — Eye icon right there in the block action bar. Click it. Done.
- **Visual Feedback** — Hidden blocks get dimmed so you know what's hiding
- **Zero View Changes** — Hidden blocks vanish from the frontend *automagically*
- **Nested Support** — BlockGrid areas? Yep, filters all the way down

<img width="1220" height="874" alt="Hide It" src="https://github.com/user-attachments/assets/2ef439b4-ddc3-46ed-b751-25a90f748816" />


## Installation

```bash
dotnet add package Our.Umbraco.HideIt
```

## Setup (It's Stupid Simple)

1. Add a **True/False** property to your block's **Settings** element type
2. Give it the alias `hideIt`
3. There is no step 3

The toggle appears. The magic happens. Your frontend stays clean.

<img width="876" height="288" alt="toggle-button" src="https://github.com/user-attachments/assets/ce41eeb2-c2c9-4066-a959-055bfb69e0f9" />

### Want a Custom Alias?

Maybe `hideIt` isn't your style, or you're migrating a site that already has its own "hide this" property. Point Hide It at any alias in `appsettings.json`:

```json
{
  "HideIt": {
    "PropertyAlias": "hideFromSite"
  }
}
```

Both the backoffice toggle and the frontend filtering pick up the custom alias. Leave the setting out and the default `hideIt` keeps working.

## How It Works

### On the Frontend
Nothing! That's the point. Hidden blocks just... aren't there. The package intercepts Umbraco's property converters and filters them out before your views even see them.

### Want Manual Control?

Don't trust the magic? Fair. Extension methods are available:

```csharp
using HideIt;

// Filter manually
var visibleBlocks = Model.Blocks.WhereVisible();
var visibleGrid = Model.Grid.WhereVisible();

// Check a single block
if (!block.IsBlockHidden())
{
    // This block is ready for its close-up
}
```

## Requirements

- Umbraco **17.5+** _(that's when [block actions](https://releases.umbraco.com/release/umbraco/Umbraco-CMS/17.5.0) became a thing)_ or **18.x**
- .NET 10.0

## Versions

| Package Version | Umbraco Version |
|-----------------|-----------------|
| 18.x            | 18.0.0 - 18.x   |
| 17.x            | 17.5.0 - 17.x   |

## Contributing

### Local Umbraco test site

An in-repo Umbraco 18 site is available under `HideIt.Test`.

- Open `HideIt.slnx`
- Start `HideIt.Test` with **IIS Express** or the **Umbraco.Web.UI** profile
- The site uses the local `HideIt.csproj` via project reference, so backoffice/frontend changes are picked up automatically
- The sample is intentionally a single-page homepage test surface focused on **Content Rows** (Block List) and **Content Grid** (Block Grid)
- The sample ships with a seeded SQLite database and media, so you can start testing immediately
- Backoffice login: `admin@example.com` / `1234567890`

Found a bug? Create an issue [here](https://github.com/andreaslborg/HideIt/issues).
Got an idea? PRs welcome!

## License

MIT
