import type { Metadata } from "next";

import { LegalPage } from "@/components/common/legal-page";

export const metadata: Metadata = {
    title: "Privacy Policy",
    description:
        "Learn how Arovia collects, uses, protects, and manages personal information.",
};

const sections = [
    {
        title: "1. Who We Are",
        content: (
            <>
                <p>
                    Arovia is an online healthcare and wellness platform
                    operated by{" "}
                    <strong className="font-medium text-foreground">
                        [LEGAL ENTITY NAME]
                    </strong>
                    , having its registered office at{" "}
                    <strong className="font-medium text-foreground">
                        [REGISTERED ADDRESS]
                    </strong>
                    .
                </p>

                <p>
                    In this Privacy Policy, “Arovia”, “we”, “us”, and “our”
                    refer to the operator of the Arovia platform. “You” and
                    “your” refer to a person accessing or using our website,
                    applications, products, or services.
                </p>
            </>
        ),
    },

    {
        title: "2. Scope of This Policy",
        content: (
            <>
                <p>
                    This Privacy Policy explains how we collect, use, disclose,
                    store, and otherwise process personal information when you
                    use Arovia, create an account, browse products, place an
                    order, contact us, or otherwise interact with our services.
                </p>

                <p>
                    This policy applies to information collected through our
                    website, applications, communications, customer support
                    channels, and other services that link to this policy.
                </p>
            </>
        ),
    },

    {
        title: "3. Information We Collect",
        content: (
            <>
                <p>
                    Depending on how you use Arovia, we may collect information
                    such as your name, email address, mobile number, account
                    credentials, delivery information, order information,
                    payment-related references, customer support communications,
                    and preferences.
                </p>

                <p>
                    We may also collect technical and usage information such as
                    IP address, browser type, device information, operating
                    system, approximate location derived from technical data,
                    pages visited, interactions with the website, and diagnostic
                    information.
                </p>

                <p>
                    We only seek information that is reasonably necessary for
                    the relevant purpose. We do not ask you to provide
                    unnecessary sensitive information simply to browse Arovia.
                </p>
            </>
        ),
    },

    {
        title: "4. Information You Provide",
        content: (
            <>
                <p>
                    You may provide personal information when you create an
                    account, sign in, request verification, place an order,
                    communicate with customer support, submit a review, or
                    otherwise interact with Arovia.
                </p>

                <p>
                    You are responsible for ensuring that information provided
                    to us is accurate and up to date. We may request additional
                    information where reasonably necessary to provide a service,
                    comply with applicable law, prevent fraud, or resolve an
                    issue.
                </p>
            </>
        ),
    },

    {
        title: "5. How We Use Your Information",
        content: (
            <>
                <p>We may use personal information to:</p>

                <ul className="list-disc space-y-2 pl-5">
                    <li>create and manage your account;</li>
                    <li>authenticate users and secure accounts;</li>
                    <li>process and fulfil orders;</li>
                    <li>communicate about orders, accounts, and services;</li>
                    <li>provide customer support;</li>
                    <li>prevent fraud, abuse, and unauthorized activity;</li>
                    <li>improve our website, products, and services;</li>
                    <li>analyse usage and technical performance;</li>
                    <li>send promotional communications where permitted;</li>
                    <li>
                        comply with applicable legal, regulatory, tax, and
                        accounting obligations; and
                    </li>
                    <li>
                        protect the rights, security, and property of Arovia,
                        our users, and others.
                    </li>
                </ul>
            </>
        ),
    },

    {
        title: "6. Legal Basis and Consent",
        content: (
            <>
                <p>
                    Where consent is required by applicable law, we will seek
                    consent before processing personal information for the
                    relevant purpose.
                </p>

                <p>
                    Certain processing may also be necessary to provide a
                    service requested by you, perform a transaction, comply
                    with a legal obligation, protect against fraud or security
                    threats, or otherwise process information as permitted by
                    applicable law.
                </p>

                <p>
                    Where processing is based on consent, you may withdraw that
                    consent in accordance with applicable law. Withdrawal of
                    consent does not affect processing that occurred lawfully
                    before the withdrawal.
                </p>
            </>
        ),
    },

    {
        title: "7. Payments",
        content: (
            <>
                <p>
                    Payment transactions may be processed through third-party
                    payment service providers. Arovia does not need to store
                    complete payment card numbers or card security codes in
                    order to provide ordinary payment functionality.
                </p>

                <p>
                    Payment providers may process payment information under
                    their own privacy policies and applicable security
                    requirements. We may receive payment confirmations,
                    transaction identifiers, payment status, and limited
                    payment-method information necessary to manage your order.
                </p>
            </>
        ),
    },

    {
        title: "8. Service Providers and Third Parties",
        content: (
            <>
                <p>
                    We may use trusted third-party service providers to operate
                    parts of Arovia, such as hosting, cloud infrastructure,
                    authentication, email delivery, analytics, payment
                    processing, shipping, customer support, security, and
                    monitoring.
                </p>

                <p>
                    These providers may process information on our behalf and
                    are expected to use it only for the services they provide
                    to us, subject to applicable contractual and legal
                    requirements.
                </p>
            </>
        ),
    },

    {
        title: "9. Cookies and Similar Technologies",
        content: (
            <p>
                Arovia may use cookies, local storage, and similar technologies
                to maintain sessions, remember preferences, provide essential
                functionality, understand usage, improve performance, and,
                where permitted, support analytics or marketing activities.
                More information is provided in our{" "}
                <a
                    href="/cookie-policy"
                    className="font-medium text-primary underline-offset-4 hover:underline"
                >
                    Cookie Policy
                </a>
                .
            </p>
        ),
    },

    {
        title: "10. Data Security",
        content: (
            <>
                <p>
                    We use reasonable technical and organizational safeguards
                    designed to protect personal information against
                    unauthorized access, alteration, disclosure, loss, misuse,
                    or destruction.
                </p>

                <p>
                    Security measures may include access controls,
                    authentication mechanisms, encryption where appropriate,
                    logging, monitoring, backups, and security reviews.
                </p>

                <p>
                    No internet-based service can guarantee absolute security.
                    You should also protect your account credentials and notify
                    us promptly if you believe your account has been
                    compromised.
                </p>
            </>
        ),
    },

    {
        title: "11. Data Retention",
        content: (
            <p>
                We retain personal information only for as long as reasonably
                necessary for the purposes described in this policy, including
                providing services, maintaining records, resolving disputes,
                preventing fraud, enforcing agreements, and complying with
                applicable legal, tax, accounting, and regulatory requirements.
                When information is no longer required, we may delete it,
                anonymize it, or otherwise dispose of it in accordance with
                applicable law.
            </p>
        ),
    },

    {
        title: "12. Your Rights",
        content: (
            <>
                <p>
                    Subject to applicable law, you may have rights relating to
                    your personal information, including the right to obtain
                    information about processing, request correction or
                    updating, request deletion where legally permitted,
                    withdraw consent where applicable, and raise a grievance.
                </p>

                <p>
                    Requests may be submitted through{" "}
                    <strong className="font-medium text-foreground">
                        [PRIVACY / GRIEVANCE EMAIL]
                    </strong>
                    . We may need to verify your identity before processing
                    certain requests.
                </p>
            </>
        ),
    },

    {
        title: "13. Children's Privacy",
        content: (
            <p>
                Arovia is not intended to be used by children independently.
                Where applicable law requires parental or guardian consent for
                processing a child&apos;s personal information, we will follow the
                applicable requirements. If you believe a child has provided
                personal information to us in circumstances where such
                collection was not permitted, please contact us.
            </p>
        ),
    },

    {
        title: "14. International Data Transfers",
        content: (
            <p>
                Some service providers or infrastructure used by Arovia may
                process information outside your state or country. Where
                personal information is transferred across jurisdictions, we
                will do so in accordance with applicable law and use appropriate
                safeguards where required.
            </p>
        ),
    },

    {
        title: "15. Changes to This Privacy Policy",
        content: (
            <p>
                We may update this Privacy Policy from time to time to reflect
                changes in our services, technology, legal requirements, or
                privacy practices. The updated version will be published on
                this page with a revised “Last updated” date. Where required,
                we will provide additional notice or obtain consent.
            </p>
        ),
    },

    {
        title: "16. Contact and Grievance Redressal",
        content: (
            <>
                <p>
                    For privacy questions, requests, or complaints, contact:
                </p>

                <div className="rounded-xl border border-border bg-muted/30 p-5">
                    <p className="font-medium text-foreground">
                        [GRIEVANCE OFFICER / PRIVACY CONTACT NAME]
                    </p>
                    <p>[DESIGNATION]</p>
                    <p>[LEGAL ENTITY NAME]</p>
                    <p>[REGISTERED ADDRESS]</p>
                    <p>[EMAIL ADDRESS]</p>
                    <p>[PHONE NUMBER, IF APPLICABLE]</p>
                </div>
            </>
        ),
    },
];

export default function PrivacyPolicyPage() {
    return (
        <LegalPage
            title="Privacy Policy"
            description="How Arovia collects, uses, protects, and manages personal information."
            lastUpdated="September 8, 2026"
            sections={sections}
        />
    );
}
