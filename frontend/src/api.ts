import type { Hold, SchoolClass } from './types';

declare global {
  interface Window {
    APP_CONFIG?: { apiUrl?: string };
  }
}

// Backend URL: runtime config (Docker) -> VITE_API_URL (dev) -> default
export const API_URL =
  window.APP_CONFIG?.apiUrl || import.meta.env.VITE_API_URL || 'http://localhost:8080';

// Error with a message that can be shown to the user directly
export class ApiError extends Error {
  status: number;
  code: string;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.status = status;
    this.code = code;
  }
}

async function request<T>(method: string, path: string, studentId?: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = {};
  if (studentId) headers['X-Student-Id'] = studentId;
  if (body !== undefined) headers['Content-Type'] = 'application/json';

  let response: Response;
  try {
    response = await fetch(API_URL + path, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(0, 'NETWORK_ERROR', 'Cannot reach the server. Please check your connection or try again in a moment.');
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    const message = data?.message || `Something went wrong (error ${response.status}). Please try again.`;
    throw new ApiError(response.status, data?.error || 'UNKNOWN', message);
  }

  return data as T;
}

export const api = {
  getClasses: () => request<SchoolClass[]>('GET', '/classes'),
  getClass: (classId: string) => request<SchoolClass>('GET', `/classes/${classId}`),
  createClass: (name: string, totalSeats: number) =>
    request<SchoolClass>('POST', '/classes', undefined, { name, totalSeats }),

  apply: (classId: string, studentId: string) => request<Hold>('POST', `/classes/${classId}/holds`, studentId),
  getHold: (holdId: string) => request<Hold>('GET', `/holds/${holdId}`),
  confirm: (holdId: string, studentId: string) => request<{ holdId: string; status: string }>('POST', `/holds/${holdId}/confirm`, studentId),
  cancel: (holdId: string, studentId: string) => request<void>('DELETE', `/holds/${holdId}`, studentId),
  getMyHolds: (studentId: string) => request<Hold[]>('GET', '/me/holds', studentId),
};

export function errorMessage(error: unknown): string {
  if (error instanceof Error) return error.message;
  return 'Something went wrong. Please try again.';
}
