import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

/**
 * The default property alias used when no custom alias is configured.
 */
export const DEFAULT_HIDE_IT_ALIAS = 'hideIt';
const SAFE_ALIAS_PATTERN = /^[A-Za-z][A-Za-z0-9_]*$/;
const UNSAFE_ALIAS_VALUES = new Set(['__proto__', 'constructor', 'prototype']);

let aliasPromise: Promise<string> | undefined;

/**
 * Resolves the property alias used to hide blocks.
 * The alias is fetched once from the Hide It configuration endpoint and cached,
 * falling back to the default "hideIt" alias if the request fails.
 */
export function getHideItPropertyAlias(host: UmbClassInterface): Promise<string> {
  aliasPromise ??= fetchPropertyAlias(host).catch(() => DEFAULT_HIDE_IT_ALIAS);
  return aliasPromise;
}

async function fetchPropertyAlias(host: UmbClassInterface): Promise<string> {
  const authContext = await host.getContext(UMB_AUTH_CONTEXT);
  if (!authContext) {
    return DEFAULT_HIDE_IT_ALIAS;
  }

  const config = authContext.getOpenApiConfiguration();
  const token = await config.token();

  try {
    const response = await fetch(`${config.base}/umbraco/management/api/v1/hideit/configuration`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (!response.ok) {
      return DEFAULT_HIDE_IT_ALIAS;
    }

    const data = (await response.json()) as { propertyAlias?: unknown };
    return normalizeAlias(data.propertyAlias);
  } catch (error) {
    console.error('[HideIt] Error fetching configuration:', error);
    return DEFAULT_HIDE_IT_ALIAS;
  }
}

function normalizeAlias(alias: unknown): string {
  const value = typeof alias === 'string' ? alias.trim() : '';
  if (!value || !SAFE_ALIAS_PATTERN.test(value) || UNSAFE_ALIAS_VALUES.has(value)) {
    return DEFAULT_HIDE_IT_ALIAS;
  }

  return value;
}
