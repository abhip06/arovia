export interface CurrentUser {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phone?: string | null;
    gender?: string | null;
    dateOfBirth?: string | null;
    role: string;
}

export interface UpdateCurrentUserRequest {
    firstName: string;
    lastName: string;
    phone?: string | null;
    gender?: string | null;
    dateOfBirth?: string | null;
}