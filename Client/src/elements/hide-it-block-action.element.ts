import type { ManifestBlockAction } from '@umbraco-cms/backoffice/block';
import type { UmbBlockAction } from '@umbraco-cms/backoffice/block';
import type { UmbBlockActionElement } from '@umbraco-cms/backoffice/block';
import type { MetaBlockActionDefaultKind } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Custom block action element for Hide It toggle.
 * Shows icon-ban when hidden, icon-eye when visible.
 * Also applies reduced opacity to the parent block when hidden.
 */
@customElement('hideit-block-action')
export class HideItBlockActionElement
  extends UmbLitElement
  implements UmbBlockActionElement
{
  #api?: UmbBlockAction<MetaBlockActionDefaultKind>;

  @property({ attribute: false })
  public manifest?: ManifestBlockAction<MetaBlockActionDefaultKind>;

  public set api(api: UmbBlockAction<MetaBlockActionDefaultKind> | undefined) {
    this.#api = api;
  }

  @state()
  private _isHidden = false;

  constructor() {
    super();

    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, async (context) => {
      if (!context) return;

      const settingsValuesObservable = await context.settingsValues();
      
      this.observe(
        settingsValuesObservable,
        (values) => {
          const wasHidden = this._isHidden;
          this._isHidden = values !== undefined && values.hideIt === true;
          
          // Apply visual feedback to parent block
          if (wasHidden !== this._isHidden) {
            this.#updateBlockVisualState();
          }
        },
        'observeHideItValue',
      );
    });
  }

  #blockEntryElement?: Element;

  #findBlockEntry(): Element | null {
    // Walk up through shadow DOM boundaries to find block entry
    let node: Node | null = this;
    while (node) {
      // Check if current node matches
      if (node instanceof Element) {
        if (node.tagName.toLowerCase().startsWith('umb-block-') && 
            node.tagName.toLowerCase().endsWith('-entry')) {
          return node;
        }
      }
      // Move to parent, crossing shadow DOM boundaries
      if (node.parentNode) {
        node = node.parentNode;
      } else if (node instanceof ShadowRoot) {
        node = node.host;
      } else {
        break;
      }
    }
    return null;
  }

  #updateBlockVisualState() {
    // Cache the block entry element
    if (!this.#blockEntryElement) {
      this.#blockEntryElement = this.#findBlockEntry() ?? undefined;
    }
    
    if (this.#blockEntryElement) {
      const el = this.#blockEntryElement as HTMLElement;
      // Target the umb-extension-slot which contains content but NOT the action bar
      const content = el.shadowRoot?.querySelector('umb-extension-slot') as HTMLElement;
      if (this._isHidden) {
        this.#blockEntryElement.setAttribute('data-hideit-hidden', '');
        if (content) {
          content.style.opacity = '0.4';
          content.style.display = 'block'; // Ensure opacity applies (not display: contents)
        }
      } else {
        this.#blockEntryElement.removeAttribute('data-hideit-hidden');
        if (content) {
          content.style.opacity = '';
          content.style.display = '';
        }
      }
    }
  }

  async #onClick(event: PointerEvent) {
    event.stopPropagation();

    try {
      await this.#api?.execute();
      this.dispatchEvent(new UmbActionExecutedEvent());
    } catch (error) {
      console.error('Error executing Hide It action:', error);
    }
  }

  // Inline SVG for eye-off icon (not available in Umbraco icon registry)
  #renderEyeOffIcon() {
    return html`
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" class="eye-off-icon">
        <path d="M10.733 5.076a10.744 10.744 0 0 1 11.205 6.575 1 1 0 0 1 0 .696 10.747 10.747 0 0 1-1.444 2.49"/>
        <path d="M14.084 14.158a3 3 0 0 1-4.242-4.242"/>
        <path d="M17.479 17.499a10.75 10.75 0 0 1-15.417-5.151 1 1 0 0 1 0-.696 10.75 10.75 0 0 1 4.446-5.143"/>
        <path d="m2 2 20 20"/>
      </svg>
    `;
  }

  override render() {
    if (!this.manifest) return html``;
    
    // When hidden: show eye icon (to show it), when visible: show eye-off icon (to hide it)
    const label = this._isHidden ? 'Show block' : 'Hide block';
    
    return html`
      <uui-button
        data-mark="block-action:${this.manifest.alias}"
        look="secondary"
        label=${label}
        title=${label}
        @click=${this.#onClick}>
        ${this._isHidden 
          ? html`<uui-icon name="icon-eye"></uui-icon>` 
          : this.#renderEyeOffIcon()}
      </uui-button>
    `;
  }

  static override styles = [
    css`
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
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'hideit-block-action': HideItBlockActionElement;
  }
}

export default HideItBlockActionElement;
