export const storage = {
  getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('jwt');
  },
  setToken(token: string) {
    if (typeof window === 'undefined') return;
    localStorage.setItem('jwt', token);
  },
  clear() {
    if (typeof window === 'undefined') return;
    localStorage.removeItem('jwt');
  }
}

export const apiBase = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';




