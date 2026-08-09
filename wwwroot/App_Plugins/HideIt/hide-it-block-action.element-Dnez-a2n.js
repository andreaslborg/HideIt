import { UMB_BLOCK_ENTRY_CONTEXT as E } from "@umbraco-cms/backoffice/block";
import { html as c, css as H, property as C, state as x, customElement as B } from "@umbraco-cms/backoffice/external/lit";
import { UmbActionExecutedEvent as M } from "@umbraco-cms/backoffice/event";
import { UmbLitElement as O } from "@umbraco-cms/backoffice/lit-element";
var A = Object.defineProperty, I = Object.getOwnPropertyDescriptor, _ = (t) => {
  throw TypeError(t);
}, b = (t, e, i, n) => {
  for (var a = n > 1 ? void 0 : n ? I(e, i) : e, h = t.length - 1, f; h >= 0; h--)
    (f = t[h]) && (a = (n ? f(e, i, a) : f(a)) || a);
  return n && a && A(e, i, a), a;
}, v = (t, e, i) => e.has(t) || _("Cannot " + i), r = (t, e, i) => (v(t, e, "read from private field"), e.get(t)), p = (t, e, i) => e.has(t) ? _("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, i), m = (t, e, i, n) => (v(t, e, "write to private field"), e.set(t, i), i), u = (t, e, i) => (v(t, e, "access private method"), i), l, o, s, g, y, w, k;
let d = class extends O {
  constructor() {
    super(), p(this, s), p(this, l), this._isHidden = !1, p(this, o), this.consumeContext(E, async (t) => {
      if (!t) return;
      const e = await t.settingsValues();
      this.observe(
        e,
        (i) => {
          const n = this._isHidden;
          this._isHidden = i !== void 0 && i.hideIt === !0, n !== this._isHidden && u(this, s, y).call(this);
        },
        "observeHideItValue"
      );
    });
  }
  set api(t) {
    m(this, l, t);
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
        @click=${u(this, s, w)}>
        ${this._isHidden ? c`<uui-icon name="icon-eye"></uui-icon>` : u(this, s, k).call(this)}
      </uui-button>
    `;
  }
};
l = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakMap();
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
  if (r(this, o) || m(this, o, u(this, s, g).call(this) ?? void 0), r(this, o)) {
    const e = r(this, o).shadowRoot?.querySelector("umb-extension-slot");
    this._isHidden ? (r(this, o).setAttribute("data-hideit-hidden", ""), e && (e.style.opacity = "0.4", e.style.display = "block")) : (r(this, o).removeAttribute("data-hideit-hidden"), e && (e.style.opacity = "", e.style.display = ""));
  }
};
w = async function(t) {
  t.stopPropagation();
  try {
    await r(this, l)?.execute(), this.dispatchEvent(new M());
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
  B("hideit-block-action")
], d);
const $ = d;
export {
  d as HideItBlockActionElement,
  $ as default
};
//# sourceMappingURL=hide-it-block-action.element-Dnez-a2n.js.map
