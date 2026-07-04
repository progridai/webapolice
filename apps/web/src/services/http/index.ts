/**
 * services/http/index.ts
 *
 * Barrel export do módulo HTTP.
 */
export { httpClient, setTokenProvider, HttpApiError } from './httpClient';
export { normalizeError, flattenValidationErrors } from './httpError';
export { API_CONFIG } from './apiConfig';
export type { ApiError, ValidationErrors, HttpOptions, HttpResponse } from './http.types';
