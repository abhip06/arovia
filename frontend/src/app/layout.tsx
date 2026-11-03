import type { Metadata } from "next";
import { Plus_Jakarta_Sans } from "next/font/google";

import "./globals.css";
import { ThemeProvider } from "@/providers/theme-provider";
import { Providers } from "./providers";

import { Toaster } from "sonner";
import { ApplicationFrame } from "@/components/navigation/application-frame";

const plusJakartaSans = Plus_Jakarta_Sans({
  subsets: ["latin"],
  variable: "--font-sans",
  display: "swap",
});

export const metadata: Metadata = {
  title: {
    default: "Arovia - Online Healthcare & Wellness Store | Medicines, Health Products & More",
    template: "%s | Arovia",
  },
  description:
    "A simpler way to discover medicines, wellness essentials, personal care products, healthcare devices, and more.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body
        className={`${plusJakartaSans.variable} min-h-screen bg-background font-sans antialiased`}
      >
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <Providers>
            <ApplicationFrame>{children}</ApplicationFrame>
          </Providers>
          <Toaster
            theme="dark"
            position="bottom-right"
            closeButton
            richColors
          />
        </ThemeProvider>
      </body>
    </html>
  );
}
