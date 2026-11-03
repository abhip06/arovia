import type { Metadata } from "next";

import { ContactPage } from "./_components/contact-page";

export const metadata: Metadata = {
    title: "Contact Us",
    description:
        "Get in touch with Arovia for questions, order support, feedback, and general assistance.",
};

export default function ContactPageRoute() {
    return <ContactPage />;
}