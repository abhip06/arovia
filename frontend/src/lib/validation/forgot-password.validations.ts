import { z } from "zod";

export const forgotPasswordSchema = z.object({
    email: z
        .email("Enter a valid email address")
        .trim()
        .max(320, "Email address must be 320 characters or less"),
});

export type ForgotPasswordFormValues = z.infer<
    typeof forgotPasswordSchema
>;
