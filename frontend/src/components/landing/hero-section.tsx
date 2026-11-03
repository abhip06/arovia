"use client";

import { useEffect, useRef, useState } from "react";

import { motion, AnimatePresence } from "framer-motion";

import {
    ArrowRight,
    FlaskConical,
    Pill,
    Truck,
} from "lucide-react";

import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type HeroScene = {
    id: "products" | "lab-tests" | "delivery";
    start: number;
    end: number;
    eyebrow: string;
    title: string;
    description: string;
    ctaLabel: string;
    icon: typeof Pill;
    href: string;
};

const heroScenes: HeroScene[] = [
    {
        id: "products",
        start: 0,
        end: 15,
        eyebrow: "Everyday healthcare",
        title: "Authentic healthcare products.",
        description:
            "Discover medicines, wellness essentials, and healthcare products selected to make everyday care simpler and more convenient.",
        ctaLabel: "Explore products",
        icon: Pill,
        href: "/otc/medicine",
    },
    {
        id: "lab-tests",
        start: 15,
        end: 30,
        eyebrow: "Know your health",
        title: "Trusted lab testing.",
        description:
            "Access convenient diagnostic testing designed to help you understand your health with greater clarity and confidence.",
        ctaLabel: "Explore lab tests",
        icon: FlaskConical,
        href: "/lab-tests",
    },
    {
        id: "delivery",
        start: 30,
        end: 39,
        eyebrow: "Delivered with care",
        title: "Healthcare at your doorstep.",
        description:
            "From essential products to everyday healthcare needs, Arovia helps make getting what you need feel easier.",
        ctaLabel: "Explore Arovia",
        icon: Truck,
        href: "/",
    },
];

function getHeroScene(currentTime: number) {
    return (
        heroScenes.find(
            (scene) =>
                currentTime >= scene.start &&
                currentTime < scene.end,
        ) ?? heroScenes[0]
    );
}

export function HeroSection() {

    const router = useRouter();
    const videoRef = useRef<HTMLVideoElement | null>(null);

    const [currentTime, setCurrentTime] = useState(0);
    const [videoReady, setVideoReady] = useState(false);

    const currentScene = getHeroScene(currentTime);
    const CurrentSceneIcon = currentScene.icon;

    useEffect(() => {
        const video = videoRef.current;

        if (!video) {
            return;
        }

        const handleVisibilityChange = () => {
            if (document.visibilityState === "visible") {
                void video.play().catch(() => { });
            } else {
                video.pause();
            }
        };

        document.addEventListener(
            "visibilitychange",
            handleVisibilityChange,
        );

        return () => {
            document.removeEventListener(
                "visibilitychange",
                handleVisibilityChange,
            );
        };
    }, []);

    return (
        <section className="relative isolate min-h-screen overflow-hidden bg-black" >
            <video
                ref={videoRef}
                src="/videos/arovia-about-hero.mp4"
                autoPlay
                muted
                loop
                playsInline
                preload="auto"
                aria-hidden="true"
                onLoadedData={() => {
                    setVideoReady(true);
                }}
                onTimeUpdate={(event) => {
                    setCurrentTime(
                        event.currentTarget.currentTime,
                    );
                }}
                className="absolute inset-0 size-full object-cover"
            />

            {/* Video readability */}
            <div
                aria-hidden="true"
                className="absolute inset-0 bg-black/30"
            />

            {/* Subtle brand atmosphere */}
            <div
                aria-hidden="true"
                className="pointer-events-none absolute -left-40 top-1/4 size-[28rem] rounded-full bg-primary/15 blur-[120px]"
            />

            {/* Loading surface */}
            <AnimatePresence>
                {!videoReady && (
                    <motion.div
                        initial={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        transition={{
                            duration: 0.6,
                        }}
                        className="absolute inset-0 z-30 bg-background"
                    />
                )}
            </AnimatePresence>

            {/* Hero content */}
            <div className="relative z-20 flex min-h-screen items-center">
                <div className="container mx-auto px-4 py-16 sm:px-6 sm:py-20 lg:px-8">
                    <div className="max-w-3xl">
                        <AnimatePresence mode="wait">
                            <motion.div
                                key={currentScene.id}
                                initial={{
                                    opacity: 0,
                                    y: 18,
                                }}
                                animate={{
                                    opacity: 1,
                                    y: 0,
                                }}
                                exit={{
                                    opacity: 0,
                                    y: -12,
                                }}
                                transition={{
                                    duration: 0.45,
                                    ease: [
                                        0.22,
                                        1,
                                        0.36,
                                        1,
                                    ],
                                }}
                                className="flex flex-col items-start"
                            >
                                <div className="flex size-12 items-center justify-center rounded-2xl border border-white/15 bg-white/10 text-white backdrop-blur-md">
                                    <CurrentSceneIcon className="size-5" />
                                </div>

                                <div className="mt-7 flex flex-col items-start gap-4">
                                    <p className="text-xs font-semibold uppercase tracking-[0.14em] text-white/75 sm:text-sm">
                                        {currentScene.eyebrow}
                                    </p>

                                    <h1 className="max-w-4xl text-balance text-5xl font-semibold text-white lg:text-6xl xl:text-7xl">
                                        {currentScene.title}
                                    </h1>

                                    <p className="max-w-xl text-pretty tracking-wide text-sm text-white/75 lg:text-base">
                                        {currentScene.description}
                                    </p>
                                </div>

                                <div className="mt-8 flex w-full flex-col gap-3 sm:w-auto sm:flex-row">
                                    <Button
                                        type="button"
                                        onClick={() =>
                                            router.push(
                                                currentScene.href,
                                            )
                                        }
                                        className="group h-11 rounded-xl px-6 cursor-pointer"
                                    >
                                        {currentScene.ctaLabel}

                                        <ArrowRight className="ml-2 size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
                                    </Button>

                                    <Button
                                        type="button"
                                        variant="outline"
                                        onClick={() =>
                                            router.push(
                                                "/contact",
                                            )
                                        }
                                        className="h-11 rounded-xl border-white/20 bg-white/5 px-6 text-white backdrop-blur-md hover:bg-white/20 hover:text-white cursor-pointer"
                                    >
                                        Talk to us
                                    </Button>
                                </div>
                            </motion.div>
                        </AnimatePresence>
                    </div>
                </div>
            </div>

            {/* Timeline */}
            <div className="absolute bottom-7 left-4 right-4 z-20 sm:left-6 sm:right-6 lg:left-8 lg:right-8">
                <div className="container mx-auto">
                    <div className="flex items-center justify-between gap-6">
                        <div className="flex items-center gap-2">
                            {heroScenes.map((scene) => {
                                const isActive =
                                    scene.id ===
                                    currentScene.id;

                                const progress = Math.min(
                                    Math.max(
                                        ((currentTime -
                                            scene.start) /
                                            (scene.end -
                                                scene.start)) *
                                        100,
                                        0,
                                    ),
                                    100,
                                );

                                return (
                                    <div
                                        key={scene.id}
                                        className={cn(
                                            "h-1 overflow-hidden rounded-full bg-white/20 transition-[width] duration-300",
                                            isActive
                                                ? "w-16 sm:w-20"
                                                : "w-7",
                                        )}
                                    >
                                        <div
                                            className="h-full rounded-full bg-white"
                                            style={{
                                                width: isActive
                                                    ? `${progress}%`
                                                    : currentTime >=
                                                        scene.end
                                                        ? "100%"
                                                        : "0%",
                                            }}
                                        />
                                    </div>
                                );
                            })}
                        </div>

                        <p className="hidden text-xs text-white/60 sm:block">
                            Better Health. Made Simple.
                        </p>
                    </div>
                </div>
            </div>

            {/* Scroll cue */}
            <motion.div
                initial={{
                    opacity: 0,
                    y: -6,
                }}
                animate={{
                    opacity: 1,
                    y: 0,
                }}
                transition={{
                    delay: 1,
                    duration: 0.5,
                }}
                className="absolute bottom-20 left-1/2 z-20 hidden -translate-x-1/2 flex-col items-center gap-2 text-white/70 lg:flex"
            >
                <span className="text-[11px] font-medium uppercase tracking-[0.14em]">
                    Discover Arovia
                </span>

                <motion.div
                    animate={{
                        y: [0, 5, 0],
                    }}
                    transition={{
                        duration: 1.5,
                        repeat: Infinity,
                        ease: "easeInOut",
                    }}
                >
                    <ArrowRight className="size-4 rotate-90" />
                </motion.div>
            </motion.div>
        </section>

    )
}
