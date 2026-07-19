// Central API utility – handles auth token and base path consistently
// Token is stored under the key 'token' by AuthContext

export const getToken = () => localStorage.getItem('token');

export const authHeaders = () => ({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${getToken()}`,
});

export async function apiFetch(path, options = {}) {
  const res = await fetch(path, {
    ...options,
    headers: {
      ...authHeaders(),
      ...(options.headers || {}),
    },
  });

  let data;
  try {
    data = await res.json();
  } catch {
    data = { success: false, message: `HTTP ${res.status}` };
  }

  return data;
}

export const apiGet = (path) => apiFetch(path);

export const apiPost = (path, body) =>
  apiFetch(path, { method: 'POST', body: JSON.stringify(body) });

export const apiPut = (path, body) =>
  apiFetch(path, { method: 'PUT', body: JSON.stringify(body) });

export const apiDelete = (path) =>
  apiFetch(path, { method: 'DELETE' });
