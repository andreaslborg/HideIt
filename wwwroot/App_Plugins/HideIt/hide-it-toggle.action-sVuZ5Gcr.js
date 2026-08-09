import { UmbBlockActionBase as l, UMB_BLOCK_ENTRY_CONTEXT as o, UMB_BLOCK_MANAGER_CONTEXT as c } from "@umbraco-cms/backoffice/block";
class d extends l {
  #e;
  #t;
  constructor(t, s) {
    super(t, s), this.consumeContext(o, (e) => {
      this.#e = e;
    }), this.consumeContext(c, (e) => {
      this.#t = e;
    });
  }
  async execute() {
    if (!this.#e || !this.#t)
      return;
    const t = this.#e.getSettings();
    if (!t)
      return;
    const s = t.values ?? [], e = s.findIndex((u) => u.alias === "hideIt"), i = e >= 0 ? s[e].value === !0 : !1, n = [...s];
    e >= 0 ? n[e] = { ...n[e], value: !i } : n.push({
      alias: "hideIt",
      value: !i,
      culture: null,
      segment: null,
      editorAlias: "Umbraco.TrueFalse"
    });
    const a = {
      ...t,
      values: n
    };
    this.#t.setOneSettings(a);
  }
}
export {
  d as HideItToggleAction,
  d as api
};
//# sourceMappingURL=hide-it-toggle.action-sVuZ5Gcr.js.map
