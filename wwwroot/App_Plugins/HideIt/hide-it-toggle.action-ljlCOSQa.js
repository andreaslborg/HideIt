import { UmbBlockActionBase as l, UMB_BLOCK_ENTRY_CONTEXT as r, UMB_BLOCK_MANAGER_CONTEXT as c } from "@umbraco-cms/backoffice/block";
import { g } from "./bundle.manifests-C-BLKY2T.js";
class p extends l {
  #t;
  #e;
  constructor(e, n) {
    super(e, n), this.consumeContext(r, (t) => {
      this.#t = t;
    }), this.consumeContext(c, (t) => {
      this.#e = t;
    });
  }
  async execute() {
    if (!this.#t || !this.#e)
      return;
    const e = this.#t.getSettings();
    if (!e)
      return;
    const n = await g(this), t = e.values ?? [], s = t.findIndex((u) => u.alias === n), a = s >= 0 ? t[s].value === !0 : !1, i = [...t];
    s >= 0 ? i[s] = { ...i[s], value: !a } : i.push({
      alias: n,
      value: !a,
      culture: null,
      segment: null,
      editorAlias: "Umbraco.TrueFalse"
    });
    const o = {
      ...e,
      values: i
    };
    this.#e.setOneSettings(o);
  }
}
export {
  p as HideItToggleAction,
  p as api
};
//# sourceMappingURL=hide-it-toggle.action-ljlCOSQa.js.map
