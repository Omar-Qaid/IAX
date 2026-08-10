import React, { createContext, useCallback, useEffect, useState } from 'react';
import { queryClient } from '@core/api/queryClient';
import type { UserProfile } from './types';
import { authService } from './authService';
import { authEvents } from './authEvents';
import { userHasPermission } from '@core/permissions/permissionService';

export interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: UserProfile | null;
  login: (username: string, password?: string) => Promise<void>;
  logout: () => Promise<void>;
  hasPermission: (permissionCode: string) => boolean;
  hasRole: (role: string) => boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const initialUser = authService.getInitialUser();
  const [user, setUser] = useState<UserProfile | null>(initialUser);
  const [isLoading, setIsLoading] = useState(!initialUser && authService.hasSession());
  const clearSession = useCallback(() => {
    setUser(null);
    queryClient.clear();
  }, []);

  useEffect(
    () =>
      authEvents.subscribe((event) => {
        if (event === 'session-expired') clearSession();
      }),
    [clearSession]
  );

  useEffect(() => {
    if (initialUser || !authService.hasSession()) return;
    let active = true;
    authService
      .getCurrentUser()
      .then((currentUser) => {
        if (active) setUser(currentUser);
      })
      .catch(() => {
        if (active) clearSession();
      })
      .finally(() => {
        if (active) setIsLoading(false);
      });
    return () => {
      active = false;
    };
  }, [clearSession, initialUser]);

  const login = async (username: string, password = ''): Promise<void> => {
    setIsLoading(true);
    try {
      const response = await authService.login(username, password);
      setUser(response.user);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async (): Promise<void> => {
    clearSession();
    await authService.logout();
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
