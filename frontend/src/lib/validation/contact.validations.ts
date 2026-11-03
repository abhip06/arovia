import { z } from "zod";

export const contactSchema = z.object({
    firstName: z
        .string()
        .trim()
        .min(1, "First name is required")
        .max(50, "First name must be 50 characters or less"),

    lastName: z
        .string()
        .trim()
        .min(1, "Last name is required")
        .max(50, "Last name must be 50 characters or less"),

    email: z
        .email("Enter a valid email address")
        .trim(),

    subject: z
        .string()
        .trim()
        .min(3, "Subject must be at least 3 characters")
        .max(120, "Subject must be 120 characters or less"),

    message: z
        .string()
        .trim()
        .min(10, "Message must be at least 10 characters")
        .max(2000, "Message must be 2000 characters or less"),
});

export type ContactFormValues = z.infer<typeof contactSchema>;