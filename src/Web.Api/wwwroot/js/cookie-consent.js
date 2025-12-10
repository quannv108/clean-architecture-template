/**
 * Cookie Consent Banner
 * Implements GDPR-compliant cookie consent functionality
 */

class CookieConsentBanner {
    constructor(options = {}) {
        this.options = {
            bannerId: 'cookie-consent-banner',
            apiEndpoint: '/consent', // API endpoint to save consent
            cookieName: 'cookie-consent',
            expiryDays: 365,
            ...options
        };

        this.init();
    }

    init() {
        // Check if consent has already been given
        const consent = this.getConsent();
        if (!consent) {
            this.showBanner();
        } else {
            // If consent was previously given, ensure analytics/tracking is enabled
            this.enableBasedOnConsent(consent);
        }
    }

    showBanner() {
        // Create the banner HTML
        const banner = document.createElement('div');
        banner.id = this.options.bannerId;
        banner.className = 'cookie-consent-banner';
        banner.innerHTML = `
            <div class="cookie-consent-content">
                <div class="cookie-consent-text">
                    <p>We use cookies to improve your experience, analyze traffic, and provide marketing communications. 
                    You can manage your preferences below.</p>
                </div>
                <div class="cookie-consent-options">
                    <div class="cookie-option">
                        <label>
                            <input type="checkbox" id="essential-consent" checked disabled>
                            <span>Essential (always on)</span>
                        </label>
                    </div>
                    <div class="cookie-option">
                        <label>
                            <input type="checkbox" id="analytics-consent">
                            <span>Analytics (track how you use our site)</span>
                        </label>
                    </div>
                    <div class="cookie-option">
                        <label>
                            <input type="checkbox" id="marketing-consent">
                            <span>Marketing (show relevant ads)</span>
                        </label>
                    </div>
                </div>
                <div class="cookie-consent-buttons">
                    <button id="accept-all" class="btn btn-primary">Accept All</button>
                    <button id="accept-selected" class="btn btn-secondary">Accept Selected</button>
                    <button id="reject-all" class="btn btn-link">Reject</button>
                </div>
            </div>
        `;

        document.body.appendChild(banner);

        // Add CSS for the banner
        this.addStyles();

        // Add event listeners
        this.addEventListeners();
    }

    addStyles() {
        const style = document.createElement('style');
        style.id = 'cookie-consent-styles';
        style.textContent = `
            .cookie-consent-banner {
                position: fixed;
                bottom: 0;
                left: 0;
                right: 0;
                background: #ffffff;
                border-top: 1px solid #e0e0e0;
                padding: 1rem;
                z-index: 10000;
                box-shadow: 0 -2px 10px rgba(0,0,0,0.1);
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            }
            
            .cookie-consent-content {
                max-width: 1200px;
                margin: 0 auto;
                display: flex;
                flex-direction: column;
                gap: 1rem;
            }
            
            .cookie-consent-text p {
                margin: 0;
                color: #333;
            }
            
            .cookie-consent-options {
                display: flex;
                gap: 1rem;
                flex-wrap: wrap;
            }
            
            .cookie-option {
                display: flex;
                align-items: center;
            }
            
            .cookie-option label {
                display: flex;
                align-items: center;
                gap: 0.5rem;
                cursor: pointer;
                font-size: 0.9rem;
            }
            
            .cookie-consent-buttons {
                display: flex;
                gap: 0.5rem;
                justify-content: flex-end;
            }
            
            .btn {
                padding: 0.5rem 1rem;
                border: none;
                border-radius: 0.25rem;
                cursor: pointer;
                font-size: 0.9rem;
            }
            
            .btn-primary {
                background: #007bff;
                color: white;
            }
            
            .btn-primary:hover {
                background: #0056b3;
            }
            
            .btn-secondary {
                background: #6c757d;
                color: white;
            }
            
            .btn-secondary:hover {
                background: #545b62;
            }
            
            .btn-link {
                background: transparent;
                color: #007bff;
                text-decoration: underline;
            }
            
            .btn-link:hover {
                color: #0056b3;
            }
            
            @media (max-width: 768px) {
                .cookie-consent-options {
                    flex-direction: column;
                    gap: 0.5rem;
                }
                
                .cookie-consent-buttons {
                    flex-direction: column;
                }
                
                .btn {
                    width: 100%;
                    margin-bottom: 0.25rem;
                }
            }
        `;

        document.head.appendChild(style);
    }

    addEventListeners() {
        // Accept all
        document.getElementById('accept-all').addEventListener('click', () => {
            this.setAllConsents(true);
            this.saveAndHide();
        });

        // Accept selected
        document.getElementById('accept-selected').addEventListener('click', () => {
            this.saveAndHide();
        });

        // Reject all (except essential)
        document.getElementById('reject-all').addEventListener('click', () => {
            this.setAllConsents(false);
            document.getElementById('essential-consent').checked = true; // Essential remains checked
            this.saveAndHide();
        });
    }

    setAllConsents(checked) {
        document.getElementById('analytics-consent').checked = checked;
        document.getElementById('marketing-consent').checked = checked;
    }

    async saveAndHide() {
        const consent = {
            analytics: document.getElementById('analytics-consent').checked,
            marketing: document.getElementById('marketing-consent').checked,
            functional: true, // Essential cookies are always allowed
            timestamp: new Date().toISOString()
        };

        try {
            // Save consent to backend API
            await this.saveConsentToApi(consent);

            // Save consent to cookie
            this.setConsent(consent);

            // Enable/disable services based on consent
            this.enableBasedOnConsent(consent);

            // Remove banner
            const banner = document.getElementById(this.options.bannerId);
            if (banner) {
                banner.remove();
            }

            // Remove styles
            const style = document.getElementById('cookie-consent-styles');
            if (style) {
                style.remove();
            }

            // Dispatch consent event
            window.dispatchEvent(new CustomEvent('cookieConsent', {
                detail: consent
            }));
        } catch (error) {
            console.error('Failed to save consent:', error);
        }
    }

    async saveConsentToApi(consent) {
        // This would typically require user authentication to identify the user
        // For now, we'll just make a request to the consent API
        const consentData = {
            consentType: 'Analytics',
            isGranted: consent.analytics,
            source: 'CookieBanner'
        };

        try {
            // In a real implementation, you would send the full consent preferences
            // and ensure the user is authenticated
            await fetch(this.options.apiEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    // Authorization header would be needed in real implementation
                },
                body: JSON.stringify(consentData)
            });
        } catch (error) {
            console.error('Failed to save consent to API:', error);
        }
    }

    enableBasedOnConsent(consent) {
        if (consent.analytics) {
            // Enable analytics here - e.g., load Google Analytics script
            this.enableAnalytics();
        } else {
            // Disable analytics
            this.disableAnalytics();
        }

        if (consent.marketing) {
            // Enable marketing tracking
            this.enableMarketing();
        } else {
            // Disable marketing tracking
            this.disableMarketing();
        }
    }

    enableAnalytics() {
        // Example: Load Google Analytics or other analytics script
        console.log('Analytics enabled based on consent');
        // In a real implementation, you would load the analytics script here
    }

    disableAnalytics() {
        // Example: Disable analytics tracking
        console.log('Analytics disabled based on consent');
        // In a real implementation, you would disable tracking here
    }

    enableMarketing() {
        console.log('Marketing tracking enabled based on consent');
    }

    disableMarketing() {
        console.log('Marketing tracking disabled based on consent');
    }

    setConsent(consent) {
        const expiry = new Date();
        expiry.setDate(expiry.getDate() + this.options.expiryDays);

        document.cookie = `${this.options.cookieName}=${JSON.stringify(consent)};expires=${expiry.toUTCString()};path=/;SameSite=Strict`;
    }

    getConsent() {
        const name = this.options.cookieName + "=";
        const decodedCookie = decodeURIComponent(document.cookie);
        const ca = decodedCookie.split(';');

        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) === 0) {
                return JSON.parse(c.substring(name.length, c.length));
            }
        }
        return null;
    }
}

// Initialize the cookie consent banner when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Check if this is a page that should show the banner
    // (Not on admin pages or pages that don't need tracking)
    if (window.location.pathname !== '/admin') {
        window.cookieConsent = new CookieConsentBanner();
    }
});