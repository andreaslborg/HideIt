import { UmbConditionBase as s } from "@umbraco-cms/backoffice/extension-registry";
import { UMB_BLOCK_ENTRY_CONTEXT as r } from "@umbraco-cms/backoffice/block";
class c extends s {
  constructor(d, n) {
    super(d, n), this.consumeContext(r, async (t) => {
      if (!t) {
        this.permitted = !1;
        return;
      }
      const o = await t.settingsValues();
      this.observe(
        o,
        (i) => {
          const a = i !== void 0 && "hideIt" in i;
          this.permitted = a;
        },
        "observeSettingsForHideIt"
      );
    });
  }
}
const l = [
  {
    type: "condition",
    name: "Hide It Has Property Condition",
    alias: "HideIt.Condition.HasHideItProperty",
    api: c
  }
], h = [
  {
    type: "blockAction",
    kind: "default",
    alias: "HideIt.BlockAction.Toggle",
    name: "Hide It Toggle",
    weight: 1e3,
    // High weight = appears first in action bar
    api: () => import("./hide-it-toggle.action-sVuZ5Gcr.js"),
    element: () => import("./hide-it-block-action.element-Dnez-a2n.js"),
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
], b = `
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
`, e = document.createElement("style");
e.textContent = b;
document.head.appendChild(e);
const y = [
  ...l,
  ...h
];
export {
  y as manifests
};
//# sourceMappingURL=hideit.js.map
