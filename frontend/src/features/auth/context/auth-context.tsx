"use client";

import {
    createContext,
    useContext,
    useCallback,
    useMemo,
    useState,
} from "react";

import type { CurrentUser } from "@/types/user.types";

export interface AuthState {
    isLoading: boolean;
    authStatus: boolean;
    user: CurrentUser | null;
}

interface AuthContextValue extends AuthState {
    setAuthenticatedUser: (user: CurrentUser) => void;
    setUnauthenticated: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(
    undefined,
);

interface AuthProviderProps {
    children: React.ReactNode;
}

export function AuthProvider({
    children,
}: AuthProviderProps) {
    const [state, setState] = useState<AuthState>({
        isLoading: true,
        authStatus: false,
        user: null,
    });

    const setAuthenticatedUser = useCallback((user: CurrentUser) => {
        setState({
            isLoading: false,
            authStatus: true,
            user,
        });
    }, []);

    const setUnauthenticated = useCallback(() => {
        setState({
            isLoading: false,
            authStatus: false,
            user: null,
        });
    }, []);

    const value = useMemo(
        () => ({
            ...state,
            setAuthenticatedUser,
            setUnauthenticated,
        }),
        [state, setAuthenticatedUser, setUnauthenticated],
    );

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error(
            "useAuth must be used within AuthProvider.",
        );
    }

    return context;
}
