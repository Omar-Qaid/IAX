import React, { createContext, useState, useEffect } from 'react';
import type { UserProfile } from './types';
import { STORAGE_KEYS } from '@core/constants/appConstants';

export const DEFAULT_USER_PROFILE: UserProfile = {
  id: 'usr-admin-1',
  username: 'admin',
  email: 'admin@contoso.com',
  displayName: 'Enterprise Admin',
  roles: ['SystemAdmin', 'FinanceManager'],
  permissions: ['*'],
  defaultCompany: 'USMF',
};

export interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: UserProfile | null;
  login: (username: string, password?: string) => Promise<void>;
  logout: () => void;
  hasPermission: (permissionCode: string) => boolean;
  hasRole: (role: string) => boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserProfile | null>(DEFAULT_USER_PROFILE);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (user) {
      localStorage.setItem(STORAGE_KEYS.AUTH_TOKEN, 'mock_jwt_token_123');
    } else {
      localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
    }
  }, [user]);

  const login = async (username: string): Promise<void> => {
    setIsLoading(true);
    try {
      const authenticatedUser: UserProfile = {
        ...DEFAULT_USER_PROFILE,
        username,
        displayName: username || 'Enterprise User',
      };
      setUser(authenticatedUser);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = (): void => {
    setUser(null);
  };

  const hasPermission = (permissionCode: string): boolean => {
    if (!user) return false;
    if (user.roles.includes('SystemAdmin') || user.permissions.includes('*')) return true;
    return user.permissions.includes(permissionCode);
  };

  const hasRole = (role: string): boolean => {
    if (!user) return false;
    if (user.roles.includes('SystemAdmin')) return true;
    return user.roles.includes(role);
  };

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated: !!user,
        isLoading,
        user,
        login,
        logout,
        hasPermission,
        hasRole,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
