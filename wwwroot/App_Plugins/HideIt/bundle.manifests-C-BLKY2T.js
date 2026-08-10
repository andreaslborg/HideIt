import { UMB_AUTH_CONTEXT as h } from "@umbraco-cms/backoffice/auth";
import { UmbConditionBase as m } from "@umbraco-cms/backoffice/extension-registry";
import { UMB_BLOCK_ENTRY_CONTEXT as u } from "@umbraco-cms/backoffice/block";
const d = "hideIt";
let r;
function p(i) {
  return r ??= b(i).catch(() => d), r;
}
async function b(i) {
  const e = await i.getContext(h);
  if (!e)
    return d;
  const n = e.getOpenApiConfiguration(), o = await n.token(), a = await fetch(`${n.base}/umbraco/management/api/v1/hideit/configuration`, {
    headers: { Authorization: `Bearer ${o}` }
  });
  if (!a.ok)
    return d;
  const s = await a.json(), t = typeof s.propertyAlias == "string" ? s.propertyAlias.trim() : "";
  return t.length > 0 ? t : d;
}
class y extends m {
  constructor(e, n) {
    super(e, n), this.consumeContext(u, async (o) => {
      if (!o) {
        this.permitted = !1;
        return;
      }
      const a = await p(this), s = await o.settingsValues();
      this.observe(
        s,
        (t) => {
          const l = t !== void 0 && a in t;
          this.permitted = l;
        },
        "observeSettingsForHideIt"
      );
    });
  }
}
const f = [
  {
    type: "condition",
    name: "Hide It Has Property Condition",
    alias: "HideIt.Condition.HasHideItProperty",
    api: y
  }
], g = [
  {
    type: "blockAction",
    kind: "default",
    alias: "HideIt.BlockAction.Toggle",
    name: "Hide It Toggle",
    weight: 1e3,
    // High weight = appears first in action bar
    api: () => import("./hide-it-toggle.action-ljlCOSQa.js"),
    element: () => import("./hide-it-block-action.element-CmxW1NNf.js"),
    meta: {
      icon: "icon-eye",
      label: "Toggle visibility"
    },
    conditions: [
      {
        alias: "HideIt.Condition.HasHideItProperty"
      }
    ]
  }
], H = `
/* Hide It - Visual feedback for hidden blocks */
umb-block-list-entry[data-hideit-hidden],
umb-block-grid-entry[data-hideit-hidden],
umb-block-rte-entry[data-hideit-hidden],
[data-hideit-hidden] {
  opacity: 0.4;
}

umb-block-list-entry[data-hideit-hidden] uui-action-bar,
umb-block-grid-entry[data-hideit-hidden] uui-action-bar,
umb-block-rte-entry[data-hideit-hidden] uui-action-bar,
[data-hideit-hidden] uui-action-bar {
  opacity: 1;
}
`, c = document.createElement("style");
c.textContent = H;
document.head.appendChild(c);
const A = [
  ...f,
  ...g
];
export {
  p as g,
  A as m
};
//# sourceMappingURL=bundle.manifests-C-BLKY2T.js.map
