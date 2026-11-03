import { z } from "zod";

export const signInSchema = z.object({
    email: z
        .email("Enter a valid email address")
        .trim()
        .max(
            320,
            "Email address must be 320 characters or less",
        ),

    password: z
        .string()
        .min(1, "Password is required")
        .min(
            8,
            "Password must contain at least 8 characters",
        )
        .max(
            128,
            "Password must contain 128 characters or less",
        ),
});

export type SignInFormValues = z.infer<typeof signInSchema>;