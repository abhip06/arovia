import { z } from "zod";

export const signUpSchema = z.object({
    firstName: z
        .string()
        .trim()
        .min(2, "First name must contain at least 2 characters")
        .max(
            100,
            "First name must be 100 characters or less",
        ),

    lastName: z
        .string()
        .trim()
        .min(2, "Last name must contain at least 2 characters")
        .max(
            100,
            "Last name must be 100 characters or less",
        ),

    email: z
        .email("Enter a valid email address")
        .trim()
        .max(
            320,
            "Email address must be 320 characters or less",
        ),

    password: z
        .string()
        .min(
            8,
            "Password must contain at least 8 characters",
        )
        .max(
            128,
            "Password must contain 128 characters or less",
        )
        .refine(
            (value) => /[A-Z]/.test(value),
            "Password must contain at least one uppercase letter",
        )
        .refine(
            (value) => /[a-z]/.test(value),
            "Password must contain at least one lowercase letter",
        )
        .refine(
            (value) => /\d/.test(value),
            "Password must contain at least one number",
        ),

    terms: z
        .boolean()
        .refine((value) => value === true, {
            message:
                "You must agree to the Terms of Service and Privacy Policy",
        }),
});

export type SignUpFormValues = z.infer<typeof signUpSchema>;