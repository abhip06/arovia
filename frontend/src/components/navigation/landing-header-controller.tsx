"use client";

import { AnimatePresence, motion } from "framer-motion";
import { useEffect, useState } from "react";

import { LandingHeader } from "./landing-header";
import { SiteHeader } from "./site-header";

export function LandingHeaderController() {
    const [showStickyHeader, setShowStickyHeader] =
        useState(false);

    useEffect(() => {
        const handleScroll = () => {
            setShowStickyHeader(
                window.scrollY >= window.innerHeight,
            );
        };

        handleScroll();

        window.addEventListener(
            "scroll",
            handleScroll,
            { passive: true },
        );

        return () => {
            window.removeEventListener(
                "scroll",
                handleScroll,
            );
        };
    }, []);

    return (
        <>
            <AnimatePresence initial={false}>
                {!showStickyHeader && (
                    <motion.div
                        key="landing-header"
                        initial={false}
                        animate={{
                            opacity: 1,
                            y: 0,
                        }}
                        exit={{
                            opacity: 0,
                            y: -8,
                        }}
                        transition={{
                            duration: 0.22,
                            ease: "easeOut",
                        }}
                    >
                        <LandingHeader />
                    </motion.div>
                )}
            </AnimatePresence>

            <AnimatePresence initial={false}>
                {showStickyHeader && (
                    <motion.div
                        key="site-header"
                        initial={{
                            opacity: 0,
                            y: -10,
                        }}
                        animate={{
                            opacity: 1,
                            y: 0,
                        }}
                        transition={{
                            duration: 0.25,
                            ease: "easeOut",
                        }}
                    >
                        <SiteHeader />
                    </motion.div>
                )}
            </AnimatePresence>
        </>
    );
}