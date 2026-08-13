import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

/**
 * The default property alias used when no custom alias is configured.
 */
export const DEFAULT_HIDE_IT_ALIAS = 'hideIt';
const SAFE_ALIAS_PATTERN = /^[A-Za-z][A-Za-z0-9_]*$/;
const UNSAFE_ALIAS_VALUES = new Set(['__proto__', 'constructor', 'prototype']);
const CUSTOM_STYLESHEET_ID = 'hideit-custom-stylesheet';
const CUSTOM_SHADOW_STYLESHEET_MARKER = 'hideit-custom-shadow-stylesheet';

type HideItConfigurationResponse = {
  propertyAlias?: unknown;
  cssPath?: unknown;
};

export type HideItConfiguration = {
  propertyAlias: string;
  cssPath: string | null;
};

let configurationPromise: Promise<HideItConfiguration> | undefined;
let customStylesheetPath: string | null = null;
let customShadowStylesheetPath: string | null = null;
let customShadowCssPromise: Promise<string> | undefined;
let customShadowConstructedStylesheetPromise: Promise<CSSStyleSheet> | undefined;

/**
 * Resolves the property alias used to hide blocks.
 * The alias is fetched once from the Hide It configuration endpoint and cached,
 * falling back to the default "hideIt" alias if the request fails.
 */
export function getHideItPropertyAlias(host: UmbClassInterface): Promise<string> {
  return getHideItConfiguration(host).then((configuration) => configuration.propertyAlias);
}

export function getHideItConfiguration(host: UmbClassInterface): Promise<HideItConfiguration> {
  configurationPromise ??= fetchConfiguration(host).catch(() => ({
    propertyAlias: DEFAULT_HIDE_IT_ALIAS,
    cssPath: null,
  }));

  return configurationPromise;
}

export function ensureHideItCustomStylesheet(cssPath: string): void {
  const normalizedCssPath = normalizeCssPath(cssPath);
  if (!normalizedCssPath || customStylesheetPath === normalizedCssPath) {
    return;
  }

  const existing = document.getElementById(CUSTOM_STYLESHEET_ID);
  if (existing instanceof HTMLLinkElement) {
    existing.href = normalizedCssPath;
    customStylesheetPath = normalizedCssPath;
    return;
  }

  const stylesheet = document.createElement('link');
  stylesheet.id = CUSTOM_STYLESHEET_ID;
  stylesheet.rel = 'stylesheet';
  stylesheet.href = normalizedCssPath;
  document.head.appendChild(stylesheet);
  customStylesheetPath = normalizedCssPath;
}

export async function applyHideItCustomShadowStylesheet(
  blockEntryElement: Element,
  cssPath: string,
): Promise<void> {
  const normalizedCssPath = normalizeCssPath(cssPath);
  const blockEntry = blockEntryElement as HTMLElement;
  const shadowRoot = blockEntry.shadowRoot;

  if (!normalizedCssPath || !shadowRoot) {
    return;
  }

  resetCustomShadowStylesheetCacheIfPathChanged(normalizedCssPath);

  try {
    const constructedStylesheet = await getOrCreateCustomShadowConstructedStylesheet(normalizedCssPath);
    if (!shadowRoot.adoptedStyleSheets.includes(constructedStylesheet)) {
      shadowRoot.adoptedStyleSheets = [...shadowRoot.adoptedStyleSheets, constructedStylesheet];
    }
  } catch (error) {
    console.error('[HideIt] Error applying custom shadow stylesheet:', error);
    await applyCustomShadowStyleElement(shadowRoot, normalizedCssPath);
  }
}

async function fetchConfiguration(host: UmbClassInterface): Promise<HideItConfiguration> {
  const authContext = await host.getContext(UMB_AUTH_CONTEXT);
  if (!authContext) {
    return {
      propertyAlias: DEFAULT_HIDE_IT_ALIAS,
      cssPath: null,
    };
  }

  const config = authContext.getOpenApiConfiguration();
  const token = await config.token();

  try {
    const response = await fetch(`${config.base}/umbraco/management/api/v1/hideit/configuration`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      return {
        propertyAlias: DEFAULT_HIDE_IT_ALIAS,
        cssPath: null,
      };
    }

    const data = (await response.json()) as HideItConfigurationResponse;
    return {
      propertyAlias: normalizeAlias(data.propertyAlias),
      cssPath: normalizeCssPath(data.cssPath),
    };
  } catch (error) {
    console.error('[HideIt] Error fetching configuration:', error);
    return {
      propertyAlias: DEFAULT_HIDE_IT_ALIAS,
      cssPath: null,
    };
  }
}

function normalizeAlias(alias: unknown): string {
  const value = typeof alias === 'string' ? alias.trim() : '';
  if (!value || !SAFE_ALIAS_PATTERN.test(value) || UNSAFE_ALIAS_VALUES.has(value)) {
    return DEFAULT_HIDE_IT_ALIAS;
  }

  return value;
}

function normalizeCssPath(cssPath: unknown): string | null {
  const value = typeof cssPath === 'string' ? cssPath.trim() : '';
  return value.length > 0 ? value : null;
}

function resetCustomShadowStylesheetCacheIfPathChanged(normalizedCssPath: string): void {
  if (customShadowStylesheetPath === normalizedCssPath) {
    return;
  }

  customShadowStylesheetPath = normalizedCssPath;
  customShadowCssPromise = undefined;
  customShadowConstructedStylesheetPromise = undefined;
}

function getOrCreateCustomShadowCss(cssPath: string): Promise<string> {
  customShadowCssPromise ??= fetch(cssPath, { credentials: 'same-origin' }).then(async (response) => {
    if (!response.ok) {
      throw new Error(`Failed to load stylesheet "${cssPath}" (${response.status})`);
    }

    const cssText = await response.text();
    return toShadowHostScopedCss(cssText);
  });

  return customShadowCssPromise;
}

function getOrCreateCustomShadowConstructedStylesheet(cssPath: string): Promise<CSSStyleSheet> {
  customShadowConstructedStylesheetPromise ??= getOrCreateCustomShadowCss(cssPath).then((cssText) => {
    const stylesheet = new CSSStyleSheet();
    stylesheet.replaceSync(cssText);
    return stylesheet;
  });

  return customShadowConstructedStylesheetPromise;
}

async function applyCustomShadowStyleElement(shadowRoot: ShadowRoot, cssPath: string): Promise<void> {
  const cssText = await getOrCreateCustomShadowCss(cssPath);
  let styleElement = shadowRoot.querySelector(`style[data-${CUSTOM_SHADOW_STYLESHEET_MARKER}]`) as HTMLStyleElement | null;

  if (!styleElement) {
    styleElement = document.createElement('style');
    styleElement.setAttribute(`data-${CUSTOM_SHADOW_STYLESHEET_MARKER}`, '');
    shadowRoot.appendChild(styleElement);
  }

  if (styleElement.textContent !== cssText) {
    styleElement.textContent = cssText;
  }
}

function toShadowHostScopedCss(cssText: string): string {
  const hostHiddenToken = '__HIDEIT_HOST_HIDDEN__';
  const hostHiddenClassToken = '__HIDEIT_HOST_HIDDEN_CLASS__';
  const hostVisibleClassToken = '__HIDEIT_HOST_VISIBLE_CLASS__';

  return cssText
    .replace(/umb-block-list-entry\[data-hideit-hidden\]/g, hostHiddenToken)
    .replace(/umb-block-grid-entry\[data-hideit-hidden\]/g, hostHiddenToken)
    .replace(/umb-block-rte-entry\[data-hideit-hidden\]/g, hostHiddenToken)
    .replace(/\[data-hideit-hidden\]/g, hostHiddenToken)
    .replace(/\.hideit-block--hidden/g, hostHiddenClassToken)
    .replace(/\.hideit-block--visible/g, hostVisibleClassToken)
    .replace(new RegExp(hostHiddenToken, 'g'), ':host([data-hideit-hidden])')
    .replace(new RegExp(hostHiddenClassToken, 'g'), ':host(.hideit-block--hidden)')
    .replace(new RegExp(hostVisibleClassToken, 'g'), ':host(.hideit-block--visible)');
}
