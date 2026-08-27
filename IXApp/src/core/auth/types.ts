export interface UserProfile {
  id: string;
  username: string;
  email: string;
  displayName: string;
  roles: string[];
  permissions: string[];
  allowedCompanies?: string[];
  avatarUrl?: string;
  defaultCompany?: string;
}

export interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  token: string | null;
}

export interface LoginResponse {
  user: UserProfile;
  token: string;
  expiresInSeconds: number;
}

export interface AuthAdapter {
  login: (username: string, password: string) => Promise<LoginResponse>;
  getCurrentUser: () => Promise<UserProfile>;
  refreshToken: () => Promise<string>;
  logout: () => Promise<void>;
}
