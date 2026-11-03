import { z } from "zod";

export const verifyEmailTokenSchema = z.object({
    token: z
        .string()
        .trim()
        .min(1, "Verification link is missing or invalid."),
});

export type VerifyEmailTokenValues = z.infer<
    typeof verifyEmailTokenSchema
>;