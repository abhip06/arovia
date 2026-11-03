export interface ContactRequest {
    firstName: string;
    lastName: string;
    email: string;
    subject: string;
    message: string;
}

export interface ContactResponse {
    message: string;
}