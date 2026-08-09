# Hide It

**The "now you see me, now you don't" package for Umbraco blocks!**

Ever wanted to temporarily hide a block without deleting it? Maybe it's a seasonal promo, a work-in-progress section, or that testimonial from your ex-client. Whatever the reason — Hide It has your back.

## What's in the Box?

- **One-Click Toggle** — Eye icon right there in the block action bar. Click it. Done.
- **Visual Feedback** — Hidden blocks get dimmed so you know what's hiding
- **Zero View Changes** — Hidden blocks vanish from the frontend *automagically*
- **Nested Support** — BlockGrid areas? Yep, filters all the way down

## Installation

```bash
dotnet add package Our.Umbraco.HideIt
```

## Setup (It's Stupid Simple)

1. Add a **True/False** property to your block's **Settings** element type
2. Give it the alias `hideIt`
3. There is no step 3

The toggle appears. The magic happens. Your frontend stays clean.

## How It Works

### In the Backoffice
| Icon | Meaning |
|------|---------|
| <img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/eye.svg" width="20" height="20" /> | Block is visible |
| <img src="https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/eye-off.svg" width="20" height="20" /> | Block is hidden |

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

- Umbraco **17.5+** (that's when block actions became a thing)
- .NET 10.0

## Contributing

Found a bug? Got an idea? PRs welcome!

## License

MIT — Go wild.

---

*Made with coffee by the Umbraco community*
