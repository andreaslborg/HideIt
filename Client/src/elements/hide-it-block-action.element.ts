import type { ManifestBlockAction } from '@umbraco-cms/backoffice/block';
import type { UmbBlockAction } from '@umbraco-cms/backoffice/block';
import type { UmbBlockActionElement } from '@umbraco-cms/backoffice/block';
import type { MetaBlockActionDefaultKind } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
  DEFAULT_HIDDEN_ICON,
  DEFAULT_VISIBLE_ICON,
  applyHideItCustomShadowStylesheet,
  ensureHideItCustomStylesheet,
  getHideItConfiguration
} from '../hide-it-config.js';

const SVG_FILE_PATTERN = /\.svg(?:[?#].*)?$/i;

/**
 * Custom block action element for Hide It toggle.
 * Shows a state icon: hidden icon when hidden, visible icon when visible.
 * Also applies a hidden/visible state marker to the parent block.
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
    this.requestUpdate();
  }

  @state()
  private _isHidden = false;
  private _useDefaultStyling = true;
  private _hasAppliedVisualState = false;
  private _customCssPath: string | null = null;
  @state()
  private _visibleIcon = DEFAULT_VISIBLE_ICON;
  @state()
  private _hiddenIcon = DEFAULT_HIDDEN_ICON;

  constructor() {
    super();

    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, async (context) => {
      this.#blockEntryElement = undefined;
      this._hasAppliedVisualState = false;

      if (!context) return;

      const configuration = await getHideItConfiguration(this);
      const alias = configuration.propertyAlias;
      this._customCssPath = configuration.cssPath;
      this._visibleIcon = configuration.visibleIcon;
      this._hiddenIcon = configuration.hiddenIcon;
      this._useDefaultStyling = this._customCssPath === null;
      this.requestUpdate();

      if (this._customCssPath !== null) {
        ensureHideItCustomStylesheet(this._customCssPath);
      }

      const settingsValuesObservable = await context.settingsValues();

      this.observe(
        settingsValuesObservable,
        (values) => {
          const wasHidden = this._isHidden;
          this._isHidden = values !== undefined && values[alias] === true;
          
          // Apply visual feedback to parent block
          if (!this._hasAppliedVisualState || wasHidden !== this._isHidden) {
            this.#updateBlockVisualState();
            this._hasAppliedVisualState = true;
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
      el.classList.toggle('hideit-block--hidden', this._isHidden);
      el.classList.toggle('hideit-block--visible', !this._isHidden);

      if (this._customCssPath !== null) {
        void applyHideItCustomShadowStylesheet(el, this._customCssPath);
      }

      // Target the umb-extension-slot which contains content but NOT the action bar
      const content = el.shadowRoot?.querySelector('umb-extension-slot') as HTMLElement;
      if (this._isHidden) {
        this.#blockEntryElement.setAttribute('data-hideit-hidden', '');
        if (content && this._useDefaultStyling) {
          content.style.opacity = '0.4';
          content.style.display = 'block'; // Ensure opacity applies (not display: contents)
        }
      } else {
        this.#blockEntryElement.removeAttribute('data-hideit-hidden');
      }

      if (content && (!this._isHidden || !this._useDefaultStyling)) {
        content.style.opacity = '';
        content.style.display = '';
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

  #isSvgIconPath(iconName: string): boolean {
    const value = iconName.trim();
    if (!SVG_FILE_PATTERN.test(value)) {
      return false;
    }

    return value.startsWith('/') || value.startsWith('./') || value.startsWith('../');
  }

  #renderIcon(iconPath: string, fallbackIconPath: string) {
    const normalizedIconPath = iconPath.trim();
    const resolvedIconPath = this.#isSvgIconPath(normalizedIconPath) ? normalizedIconPath : fallbackIconPath;
    return html`<img src=${resolvedIconPath} alt="" aria-hidden="true" class="hideit-icon-image">`;
  }

  override render() {
    const actionAlias = this.manifest?.alias ?? 'HideIt.BlockAction.Toggle';
    const label = this._isHidden ? 'Show block' : 'Hide block';
    const iconName = this._isHidden ? this._hiddenIcon : this._visibleIcon;
    const fallbackIconPath = this._isHidden ? DEFAULT_HIDDEN_ICON : DEFAULT_VISIBLE_ICON;
    
    return html`
      <uui-button
        data-mark="block-action:${actionAlias}"
        look="secondary"
        label=${label}
        title=${label}
        @click=${this.#onClick}>
        ${this.#renderIcon(iconName, fallbackIconPath)}
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

      .hideit-icon-image {
        width: 1.1em;
        height: 1.1em;
        display: inline-block;
        object-fit: contain;
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
