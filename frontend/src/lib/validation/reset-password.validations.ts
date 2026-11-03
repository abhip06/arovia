import { z } from "zod";

export const resetPasswordSchema = z.object({
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
});

export type ResetPasswordFormValues = z.infer<
    typeof resetPasswordSchema
>;
