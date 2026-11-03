import type { Metadata } from "next";

import { LegalPage } from "@/components/common/legal-page";

export const metadata: Metadata = {
    title: "Terms of Service",
    description:
        "The terms that apply when you access or use Arovia.",
};

const sections = [
    {
        title: "1. About These Terms",
        content: (
            <>
                <p>
                    These Terms of Service (“Terms”) govern your access to and
                    use of Arovia, including our website, applications,
                    products, services, and related features.
                </p>

                <p>
                    Arovia is operated by{" "}
                    <strong className="font-medium text-foreground">
                        [LEGAL ENTITY NAME]
                    </strong>
                    . By accessing or using Arovia, you agree to these Terms
                    and our Privacy Policy and Cookie Policy.
                </p>
            </>
        ),
    },

    {
        title: "2. Eligibility and Accounts",
        content: (
            <>
                <p>
                    You must provide accurate information when creating an
                    account and must keep your information reasonably current.
                </p>

                <p>
                    You are responsible for protecting your account credentials
                    and for activity performed through your account, except
                    where unauthorized use results from circumstances for which
                    you are not responsible under applicable law.
                </p>

                <p>
                    You must not create an account using another person&apos;s
                    identity, impersonate another person, or provide
                    information that is knowingly false or misleading.
                </p>
            </>
        ),
    },

    {
        title: "3. Arovia Services",
        content: (
            <>
                <p>
                    Arovia provides an online platform through which users may
                    discover healthcare, wellness, personal-care, and related
                    products and services, subject to availability and
                    applicable law.
                </p>

                <p>
                    Product availability, descriptions, images, pricing,
                    offers, delivery estimates, and other information may
                    change from time to time.
                </p>
            </>
        ),
    },

    {
        title: "4. Healthcare Information and Medical Disclaimer",
        content: (
            <>
                <p>
                    Information presented on Arovia is intended to help you
                    understand products and services and is not a substitute
                    for professional medical advice, diagnosis, or treatment.
                </p>

                <p>
                    Arovia does not replace your doctor, pharmacist, or other
                    qualified healthcare professional. You should seek
                    professional medical advice regarding symptoms, diagnosis,
                    treatment, medicines, allergies, interactions, pregnancy,
                    or other health-related concerns.
                </p>

                <p>
                    Do not rely solely on information on Arovia when making
                    decisions about a medical condition or treatment.
                </p>
            </>
        ),
    },

    {
        title: "5. Regulated and Prescription Products",
        content: (
            <>
                <p>
                    Where Arovia makes regulated healthcare products available,
                    their sale, supply, prescription requirements, eligibility,
                    quantity restrictions, and fulfilment may be subject to
                    applicable laws and regulatory requirements.
                </p>

                <p>
                    We may require a valid prescription, additional
                    verification, or other information before fulfilling
                    certain orders.
                </p>

                <p>
                    We may refuse, cancel, limit, or modify an order where
                    required by law or where fulfilment would otherwise be
                    unlawful or unsafe.
                </p>
            </>
        ),
    },

    {
        title: "6. Product Information",
        content: (
            <p>
                We aim to provide accurate and useful product information.
                However, product packaging, labels, ingredients, specifications,
                availability, and other details may change. You should verify
                information on the product packaging and consult an appropriate
                healthcare professional where necessary.
            </p>
        ),
    },

    {
        title: "7. Orders and Contract Formation",
        content: (
            <>
                <p>
                    Placing an order constitutes an offer to purchase the
                    selected products. An order is not necessarily accepted
                    merely because you receive an order acknowledgement.
                </p>

                <p>
                    We may accept, reject, cancel, or limit an order for reasons
                    including product availability, pricing or listing errors,
                    payment issues, delivery restrictions, suspected fraud,
                    legal requirements, or other legitimate operational
                    reasons.
                </p>

                <p>
                    Where we cancel an order after payment has been received,
                    any eligible refund will be processed according to our
                    applicable refund and cancellation procedures.
                </p>
            </>
        ),
    },

    {
        title: "8. Pricing and Payments",
        content: (
            <>
                <p>
                    Prices displayed on Arovia are generally shown inclusive
                    or exclusive of applicable taxes as indicated during
                    checkout.
                </p>

                <p>
                    We may correct pricing, tax, availability, or listing
                    errors. Where such an error materially affects an order,
                    we may contact you before fulfilment or cancel the affected
                    order and provide an appropriate refund where applicable.
                </p>

                <p>
                    Payments may be processed through third-party payment
                    providers. You agree to provide valid payment information
                    and authorize the applicable transaction.
                </p>
            </>
        ),
    },

    {
        title: "9. Delivery",
        content: (
            <p>
                Delivery estimates are provided for convenience and may be
                affected by availability, address accuracy, logistics
                providers, weather, public holidays, regulatory requirements,
                and events outside our reasonable control. You are responsible
                for providing a valid and serviceable delivery address.
            </p>
        ),
    },

    {
        title: "10. Returns, Cancellations, and Refunds",
        content: (
            <>
                <p>
                    Returns, replacements, cancellations, and refunds are
                    subject to the applicable Arovia return and refund policy,
                    product-specific restrictions, and applicable law.
                </p>

                <p>
                    Certain healthcare products may not be eligible for return
                    for health, safety, hygiene, regulatory, or other legitimate
                    reasons. Any such restriction will be communicated where
                    applicable.
                </p>
            </>
        ),
    },

    {
        title: "11. Acceptable Use",
        content: (
            <>
                <p>You must not:</p>

                <ul className="list-disc space-y-2 pl-5">
                    <li>use Arovia for unlawful purposes;</li>
                    <li>
                        attempt to gain unauthorized access to our systems or
                        another user&apos;s account;
                    </li>
                    <li>
                        interfere with the operation, availability, or security
                        of the platform;
                    </li>
                    <li>
                        use automated systems to scrape or collect information
                        without authorization;
                    </li>
                    <li>
                        upload malware or other harmful code;
                    </li>
                    <li>
                        submit misleading, fraudulent, abusive, or unlawful
                        content; or
                    </li>
                    <li>
                        misuse discounts, promotions, referral programs, or
                        other offers.
                    </li>
                </ul>
            </>
        ),
    },

    {
        title: "12. Reviews and User Content",
        content: (
            <p>
                Where Arovia permits reviews, comments, questions, or other
                user-generated content, you are responsible for the content you
                submit. Content must be accurate, lawful, relevant, and must
                not violate another person&apos;s rights. We may remove or restrict
                content that violates these Terms or applicable law.
            </p>
        ),
    },

    {
        title: "13. Intellectual Property",
        content: (
            <p>
                Arovia and its content, including its software, design,
                branding, logos, text, graphics, interfaces, and other
                materials, are owned by or licensed to Arovia and are protected
                by applicable intellectual-property laws. Except as expressly
                permitted, you may not copy, reproduce, modify, distribute,
                reverse engineer, publish, sell, or exploit our content or
                software without authorization.
            </p>
        ),
    },

    {
        title: "14. Third-Party Services",
        content: (
            <p>
                Arovia may integrate with or link to third-party services,
                websites, payment providers, logistics providers, authentication
                providers, or other external services. Their availability and
                operation may be subject to their own terms and policies.
                Arovia is not responsible for third-party services except to
                the extent required by applicable law.
            </p>
        ),
    },

    {
        title: "15. Suspension and Termination",
        content: (
            <p>
                We may suspend or terminate access to an account or service
                where reasonably necessary to protect users, investigate abuse
                or fraud, comply with law, enforce these Terms, or protect the
                security and integrity of the platform. Where appropriate and
                legally permissible, we will provide reasonable notice or an
                opportunity to resolve the issue.
            </p>
        ),
    },

    {
        title: "16. Disclaimers",
        content: (
            <p>
                To the extent permitted by applicable law, Arovia is provided
                on an “as available” basis. We do not guarantee that the
                platform will always be uninterrupted, error-free, completely
                secure, or continuously available. Nothing in these Terms
                excludes any warranty, right, or remedy that cannot lawfully be
                excluded.
            </p>
        ),
    },

    {
        title: "17. Limitation of Liability",
        content: (
            <p>
                To the maximum extent permitted by applicable law, Arovia and
                its officers, employees, affiliates, and service providers
                will not be liable for indirect, incidental, special, or
                consequential loss arising from your use of the platform.
                Nothing in these Terms limits liability that cannot legally be
                limited or excluded.
            </p>
        ),
    },

    {
        title: "18. Indemnity",
        content: (
            <p>
                To the extent permitted by applicable law, you agree to
                indemnify and hold harmless Arovia and its affiliates,
                officers, employees, and service providers from claims,
                losses, liabilities, and expenses arising from your unlawful
                use of the platform, violation of these Terms, or violation of
                another person&apos;s rights.
            </p>
        ),
    },

    {
        title: "19. Changes to These Terms",
        content: (
            <p>
                We may update these Terms to reflect changes in our services,
                technology, legal requirements, or business practices. Updated
                Terms will be published on this page with a revised date.
                Continued use of Arovia after the effective date constitutes
                acceptance of the updated Terms to the extent permitted by law.
            </p>
        ),
    },

    {
        title: "20. Governing Law and Jurisdiction",
        content: (
            <p>
                These Terms are governed by the laws of India, subject to
                applicable consumer-protection and other mandatory rights. The
                courts at{" "}
                <strong className="font-medium text-foreground">
                    [CITY, STATE]
                </strong>{" "}
                will have jurisdiction, subject to applicable law.
            </p>
        ),
    },

    {
        title: "21. Contact and Grievance Redressal",
        content: (
            <>
                <p>
                    Questions, complaints, or notices may be sent to:
                </p>

                <div className="rounded-xl border border-border bg-muted/30 p-5">
                    <p className="font-medium text-foreground">
                        [GRIEVANCE OFFICER NAME]
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

export default function TermsOfServicePage() {
    return (
        <LegalPage
            title="Terms of Service"
            description="The rules and conditions that apply when you access or use Arovia."
            lastUpdated="September 8, 2026"
            sections={sections}
        />
    );
}
