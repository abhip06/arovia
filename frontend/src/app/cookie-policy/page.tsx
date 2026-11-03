import type { Metadata } from "next";

import { LegalPage } from "@/components/common/legal-page";

export const metadata: Metadata = {
    title: "Cookie Policy",
    description:
        "Learn how Arovia uses cookies and similar technologies.",
};

const sections = [
    {
        title: "1. What Are Cookies?",
        content: (
            <p>
                Cookies are small text files stored on your device by a
                website. Similar technologies, such as local storage and
                pixels, may perform comparable functions. They can help a
                website remember information, maintain sessions, understand
                usage, and provide a better experience.
            </p>
        ),
    },

    {
        title: "2. How Arovia Uses Cookies",
        content: (
            <p>
                Arovia may use cookies and similar technologies to operate the
                website, maintain authentication sessions, remember preferences,
                understand how the website is used, improve performance,
                protect against fraud and abuse, and support analytics or
                marketing where permitted.
            </p>
        ),
    },

    {
        title: "3. Types of Cookies We May Use",
        content: (
            <>
                <div className="space-y-5">
                    <div>
                        <h3 className="font-semibold text-foreground">
                            Essential cookies
                        </h3>
                        <p className="mt-2">
                            These are necessary for core functionality such as
                            authentication, security, session management, and
                            basic website operation.
                        </p>
                    </div>

                    <div>
                        <h3 className="font-semibold text-foreground">
                            Preference cookies
                        </h3>
                        <p className="mt-2">
                            These remember choices such as language, theme, or
                            other interface preferences.
                        </p>
                    </div>

                    <div>
                        <h3 className="font-semibold text-foreground">
                            Analytics cookies
                        </h3>
                        <p className="mt-2">
                            These may help us understand traffic, feature usage,
                            performance, and user journeys so that we can improve
                            Arovia.
                        </p>
                    </div>

                    <div>
                        <h3 className="font-semibold text-foreground">
                            Marketing cookies
                        </h3>
                        <p className="mt-2">
                            Where applicable and legally permitted, these may
                            be used to understand marketing effectiveness or
                            provide more relevant advertising experiences.
                        </p>
                    </div>
                </div>
            </>
        ),
    },

    {
        title: "4. Authentication and Security",
        content: (
            <p>
                Some cookies or similar technologies are required to maintain
                your signed-in session and protect the security of your account.
                Disabling these technologies may prevent parts of Arovia from
                functioning correctly.
            </p>
        ),
    },

    {
        title: "5. Third-Party Technologies",
        content: (
            <p>
                We may use third-party service providers for analytics,
                payments, authentication, security, customer support, delivery,
                or other services. These providers may use cookies or similar
                technologies in accordance with their own policies and applicable
                law.
            </p>
        ),
    },

    {
        title: "6. Managing Cookies",
        content: (
            <p>
                Most browsers allow you to view, block, delete, or otherwise
                manage cookies through browser settings. Blocking essential
                cookies may affect your ability to sign in, place orders, or
                use certain Arovia features.
            </p>
        ),
    },

    {
        title: "7. Consent and Cookie Choices",
        content: (
            <p>
                Where applicable law requires consent for non-essential cookies
                or similar technologies, Arovia will provide an appropriate
                consent mechanism. Your choices may be remembered through a
                cookie or similar technology so that we do not repeatedly ask
                for the same preference.
            </p>
        ),
    },

    {
        title: "8. Changes to This Cookie Policy",
        content: (
            <p>
                We may update this Cookie Policy when our technology,
                integrations, services, or legal obligations change. The
                current version will always be published on this page together
                with its latest update date.
            </p>
        ),
    },

    {
        title: "9. Contact",
        content: (
            <>
                <p>
                    For questions about cookies or privacy, contact us at:
                </p>

                <div className="rounded-xl border border-border bg-muted/30 p-5">
                    <p className="font-medium text-foreground">
                        [PRIVACY CONTACT NAME]
                    </p>
                    <p>[LEGAL ENTITY NAME]</p>
                    <p>[EMAIL ADDRESS]</p>
                    <p>[REGISTERED ADDRESS]</p>
                </div>
            </>
        ),
    },
];

export default function CookiePolicyPage() {
    return (
        <LegalPage
            title="Cookie Policy"
            description="How Arovia uses cookies and similar technologies to provide, secure, and improve its services."
            lastUpdated="September 8, 2026"
            sections={sections}
        />
    );
}