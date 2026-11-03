export interface Address {
    id: string;
    label: string;
    recipientName: string;
    recipientPhone: string | null;
    addressLine1: string;
    addressLine2: string | null;
    city: string;
    state: string;
    postalCode: string;
    country: string;
    isPrimary: boolean;
}

export interface AddressRequest {
    label: string;
    recipientName: string;
    recipientPhone: string | null;
    addressLine1: string;
    addressLine2: string | null;
    city: string;
    state: string;
    postalCode: string;
    country: string;
}

export type CreateAddressRequest = AddressRequest;
export type UpdateAddressRequest = AddressRequest;