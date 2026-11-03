export const productTypes = [
    {
        slug: "medicine",
        apiType: "Medicine",
        label: "Medicines",
        description: "Everyday medicines and essentials, all in one place.",
    },
    {
        slug: "medical-device",
        apiType: "MedicalDevice",
        label: "Medical devices",
        description: "Trusted devices and equipment for care at home.",
    },
    {
        slug: "diagnostic",
        apiType: "Diagnostic",
        label: "Diagnostics",
        description: "Diagnostic essentials to support your health routine.",
    },
    {
        slug: "wellness",
        apiType: "Wellness",
        label: "Wellness",
        description: "Thoughtful picks for everyday health and wellbeing.",
    },
    {
        slug: "personal-care",
        apiType: "PersonalCare",
        label: "Personal care",
        description: "Personal care products for the routines you rely on.",
    },
    {
        slug: "healthcare-accessory",
        apiType: "HealthcareAccessory",
        label: "Healthcare accessories",
        description: "Useful accessories for everyday healthcare needs.",
    },
] as const;

export type ProductTypeSlug = (typeof productTypes)[number]["slug"];
export type ProductTypeApi = (typeof productTypes)[number]["apiType"];
export type CatalogSort = "relevance" | "price-low" | "price-high";
export type CatalogRouteType = ProductTypeSlug | "search";

export interface CatalogProduct {
    productVariantId: string;
    productName: string;
    productSlug: string;
    brandName: string | null;
    sku: string;
    packageQuantity: number;
    packageUnit: string;
    netQuantity: number | null;
    netQuantityUnit: string | null;
    mrp: number | null;
    sellingPrice: number;
    imageUrl: string | null;
}

export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface CatalogOption {
    slug: string;
    name: string;
}

export interface CatalogCategory extends CatalogOption {
    productType: string;
    children: CatalogCategory[];
}

export interface CatalogProductParams {
    page: number;
    pincode?: string;
    type?: ProductTypeApi;
    brand?: string;
    manufacturer?: string;
    category?: string;
    search?: string;
}

export interface ProductDetail {
    product: {
        id: string;
        name: string;
        slug: string;
        type: string;
        requiresPrescription: boolean;
        brand: CatalogOption | null;
        manufacturer: CatalogOption;
        returnable: boolean;
        returnWindowDays: number;
        categories: Array<{ name: string; slug: string }>;
        ingredients: Array<{
            name: string;
            slug: string;
            strengthValue: number;
            strengthUnit: string;
            role: string;
        }>;
    };
    variant: {
        id: string;
        sku: string;
        barcode: string | null;
        packageQuantity: number;
        packageUnit: string;
        netQuantity: number | null;
        netQuantityUnit: string | null;
        sellingPrice: number;
    };
    information: {
        description: string | null;
        highlights: string[];
        directions: string | null;
        warnings: string[];
        storageInstructions: string | null;
        sideEffects: string[];
        howToUse: string | null;
        specifications: Array<{ key: string; value: string }>;
        faqs: Array<{ question: string; answer: string }>;
    } | null;
    availability: {
        pincode: string;
        isServiceable: boolean;
        isAvailable: boolean;
        availableQuantity: number;
        mrp: number | null;
    } | null;
    images: Array<{
        altText: string | null;
        sortOrder: number;
        isPrimary: boolean;
        url: string;
    }>;
}

export interface CatalogFilters {
    brand?: string;
    manufacturer?: string;
    category?: string;
    sort: CatalogSort;
}

export function getProductType(slug: string) {
    return productTypes.find((productType) => productType.slug === slug);
}
