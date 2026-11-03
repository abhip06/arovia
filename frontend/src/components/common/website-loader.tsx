"use client";

import { useIsFetching } from "@tanstack/react-query";
import { AnimatePresence, motion, useReducedMotion } from "framer-motion";

import { useAuth } from "@/features/auth/context/auth-context";
import { Logo } from "../brand/logo";

export function WebsiteLoader() {
    const isFetching = useIsFetching() > 0;
    const { isLoading: isCheckingSession } = useAuth();
    const busy = isFetching || isCheckingSession;
    const visible = busy;
    const reduceMotion = useReducedMotion();

    return (
        <AnimatePresence>
            {visible && (
                <motion.div
                    role="status"
                    aria-live="polite"
                    aria-label="Loading Arovia"
                    initial={reduceMotion ? false : { opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    transition={{ duration: reduceMotion ? 0 : 0.2 }}
                    className="fixed inset-0 z-[100] flex flex-col items-center justify-center bg-background/95 px-6 text-center backdrop-blur-md"
                >
                    <Logo
                        textClassName="text-4xl md:text-5xl"
                        imageSize={64}
                    />
                    <p className="mt-1 text-sm text-muted-foreground">Bringing your care essentials together…</p>
                    <div aria-hidden="true" className="mt-6 h-1 w-36 overflow-hidden rounded-full bg-muted">
                        <motion.div
                            className="h-full rounded-full bg-primary"
                            initial={{ x: "-100%" }}
                            animate={{ x: reduceMotion ? "0%" : "100%" }}
                            transition={reduceMotion ? { duration: 0 } : { duration: 1.1, repeat: Infinity, ease: "easeInOut" }}
                        />
                    </div>
                </motion.div>
            )}
        </AnimatePresence>
    );
}
