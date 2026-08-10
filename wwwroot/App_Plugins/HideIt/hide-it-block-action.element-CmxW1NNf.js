import { UMB_BLOCK_ENTRY_CONTEXT as E } from "@umbraco-cms/backoffice/block";
import { html as c, css as H, property as C, state as x, customElement as A } from "@umbraco-cms/backoffice/external/lit";
import { UmbActionExecutedEvent as B } from "@umbraco-cms/backoffice/event";
import { UmbLitElement as M } from "@umbraco-cms/backoffice/lit-element";
import { g as O } from "./bundle.manifests-C-BLKY2T.js";
var I = Object.defineProperty, S = Object.getOwnPropertyDescriptor, v = (t) => {
  throw TypeError(t);
}, b = (t, e, i, a) => {
  for (var o = a > 1 ? void 0 : a ? S(e, i) : e, h = t.length - 1, f; h >= 0; h--)
    (f = t[h]) && (o = (a ? f(e, i, o) : f(o)) || o);
  return a && o && I(e, i, o), o;
}, m = (t, e, i) => e.has(t) || v("Cannot " + i), r = (t, e, i) => (m(t, e, "read from private field"), e.get(t)), p = (t, e, i) => e.has(t) ? v("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, i), _ = (t, e, i, a) => (m(t, e, "write to private field"), e.set(t, i), i), l = (t, e, i) => (m(t, e, "access private method"), i), u, n, s, g, y, w, k;
let d = class extends M {
  constructor() {
    super(), p(this, s), p(this, u), this._isHidden = !1, p(this, n), this.consumeContext(E, async (t) => {
      if (!t) return;
      const e = await O(this), i = await t.settingsValues();
      this.observe(
        i,
        (a) => {
          const o = this._isHidden;
          this._isHidden = a !== void 0 && a[e] === !0, o !== this._isHidden && l(this, s, y).call(this);
        },
        "observeHideItValue"
      );
    });
  }
  set api(t) {
    _(this, u, t);
  }
  render() {
    if (!this.manifest) return c``;
    const t = this._isHidden ? "Show block" : "Hide block";
    return c`
      <uui-button
        data-mark="block-action:${this.manifest.alias}"
        look="secondary"
        label=${t}
        title=${t}
        @click=${l(this, s, w)}>
        ${this._isHidden ? c`<uui-icon name="icon-eye"></uui-icon>` : l(this, s, k).call(this)}
      </uui-button>
    `;
  }
};
u = /* @__PURE__ */ new WeakMap();
n = /* @__PURE__ */ new WeakMap();
s = /* @__PURE__ */ new WeakSet();
g = function() {
  let t = this;
  for (; t; ) {
    if (t instanceof Element && t.tagName.toLowerCase().startsWith("umb-block-") && t.tagName.toLowerCase().endsWith("-entry"))
      return t;
    if (t.parentNode)
      t = t.parentNode;
    else if (t instanceof ShadowRoot)
      t = t.host;
    else
      break;
  }
  return null;
};
y = function() {
  if (r(this, n) || _(this, n, l(this, s, g).call(this) ?? void 0), r(this, n)) {
    const e = r(this, n).shadowRoot?.querySelector("umb-extension-slot");
    this._isHidden ? (r(this, n).setAttribute("data-hideit-hidden", ""), e && (e.style.opacity = "0.4", e.style.display = "block")) : (r(this, n).removeAttribute("data-hideit-hidden"), e && (e.style.opacity = "", e.style.display = ""));
  }
};
w = async function(t) {
  t.stopPropagation();
  try {
    await r(this, u)?.execute(), this.dispatchEvent(new B());
  } catch (e) {
    console.error("Error executing Hide It action:", e);
  }
};
k = function() {
  return c`
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" class="eye-off-icon">
        <path d="M10.733 5.076a10.744 10.744 0 0 1 11.205 6.575 1 1 0 0 1 0 .696 10.747 10.747 0 0 1-1.444 2.49"/>
        <path d="M14.084 14.158a3 3 0 0 1-4.242-4.242"/>
        <path d="M17.479 17.499a10.75 10.75 0 0 1-15.417-5.151 1 1 0 0 1 0-.696 10.75 10.75 0 0 1 4.446-5.143"/>
        <path d="m2 2 20 20"/>
      </svg>
    `;
};
d.styles = [
  H`
      :host {
        --umb-button-border-radius: var(--uui-button-border-radius);
        --umb-button-padding-left-factor: var(--uui-button-padding-left-factor);
        --umb-button-padding-right-factor: var(--uui-button-padding-right-factor);
      }

      uui-button {
        --uui-button-border-radius: var(--umb-button-border-radius);
        --uui-button-padding-left-factor: var(--umb-button-padding-left-factor);
        --uui-button-padding-right-factor: var(--umb-button-padding-right-factor);
      }

      /* Match uui-icon sizing */
      .eye-off-icon {
        width: 1.1em;
        height: 1.1em;
        vertical-align: middle;
      }
    `
];
b([
  C({ attribute: !1 })
], d.prototype, "manifest", 2);
b([
  x()
], d.prototype, "_isHidden", 2);
d = b([
  A("hideit-block-action")
], d);
const T = d;
export {
  d as HideItBlockActionElement,
  T as default
};
//# sourceMappingURL=hide-it-block-action.element-CmxW1NNf.js.map
