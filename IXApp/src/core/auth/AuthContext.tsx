import React, { createContext, useState } from 'react';
import type { UserProfile } from './types';
import { authService } from './authService';
import { userHasPermission } from '@core/permissions/permissionService';

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
  const [user, setUser] = useState<UserProfile | null>(() => authService.getInitialUser());
  const [isLoading, setIsLoading] = useState(false);

  const login = async (username: string, _password?: string): Promise<void> => {
    setIsLoading(true);
    try {
      const response = await authService.login(username);
      setUser(response.user);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = (): void => {
    setUser(null);
    void authService.logout();
  };

  const hasPermission = (permissionCode: string): boolean => {
    return userHasPermission(user, permissionCode);
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
