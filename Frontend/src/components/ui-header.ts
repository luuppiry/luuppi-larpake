import HttpClient from "../api_client/http_client";

class Header extends HTMLElement {
    client: HttpClient;
    constructor() {
        super();
        this.client = new HttpClient();
    }

    connectedCallback() {
        const hasLanguageOptions: boolean = this.getAttribute("lang-options") === "false" ? false : true;
        const indexPath = this.#add_path_correction("index.html");
        const profilePath = this.#add_path_correction("profile.html");
        const adminPath = this.#add_path_correction("admin/admin.html");

        const loggedIn = Object.keys(sessionStorage).find((key) => key.includes("msal.account.keys"));
        const currentLang = window.location.href.includes("/en/") ? "en" : "fi";

        const adminBtn = currentLang === "fi" ? `<a href="${adminPath}">Ylläpito</a>` : "";

        const langBtn = hasLanguageOptions
            ? `
                    <div
                        class="menu-icon"
                        id="ui-header-change-language-btn"
                        style="display: flex; justify-content: center; align-items: center">
                        <img class="globle" src="/icons/globle.png" height="30px" width="auto" />
                    </div>`
            : "<!-- no other languages available -->";

        const profileBtn = loggedIn
            ? `<div
                class="menu-icon"
                id="ui-header-profile-btn"
                style="display: flex; justify-content: center; align-items: center">
                <img class="profile-icon" src="/icons/profile-icon.png" height="30px" width="auto" />
                <div class="profile-dropdown" id="profileDropdown">
                    <a href="${profilePath}">Profiili</a>
                    ${adminBtn}
                    <a href="#" style="color: red;">Kirjaudu ulos</a>
                </div>
            </div> `
            : `<div
                    class="menu-icon"
                    id="ui-header-login-btn"
                    style="display: flex; justify-content: center; align-items: center">
                    <img class="login-icon" src="/icons/login-icon.png" height="30px" width="auto" />
                </div> `;

        this.innerHTML = `
         <header class="header">
            <img
                src="/luuppi.logo.svg"
                onclick="window.location.href='${indexPath}'"
                style="height: 60px; cursor: pointer"
                alt="Luuppi Logo"
            />
            <h1>LÄRPÄKE</h1>
            ${langBtn}
            ${profileBtn}
            <div class="menu-icon" id="ui-header-open-menu-btn">☰</div>
        </header>
         `;

        const languageBtn = this.querySelector<HTMLDivElement>("#ui-header-change-language-btn");
        if (languageBtn == null) {
            throw new Error("Language button not found");
        }
        languageBtn.addEventListener("click", (_) => {
            this.changeLanguage();
        });

        if (loggedIn) {
            const profileBtn = this.querySelector<HTMLDivElement>("#ui-header-profile-btn");
            if (profileBtn == null) {
                throw new Error("Profile button not found");
            }
            profileBtn.addEventListener("click", (_) => {
                this.profileDropdown();
            });
            const logoutBtn = this.querySelector<HTMLAnchorElement>(".profile-dropdown a[href='#']");
            if (logoutBtn == null) {
                throw new Error("Logout button not found");
            }
            logoutBtn.addEventListener("click", (event) => {
                event.preventDefault(); // Prevent default anchor behavior
                this.logout();
            });
        }

        if (!loggedIn) {
            const logInBtn = this.querySelector<HTMLDivElement>("#ui-header-login-btn");
            if (logInBtn == null) {
                throw new Error("Login button not found");
            }
            logInBtn.addEventListener("click", (_) => {
                this.login();
            });
        }

        const menuBtn = this.querySelector<HTMLDivElement>("#ui-header-open-menu-btn");
        if (menuBtn == null) {
            throw new Error("Menu button not found");
        }
        menuBtn.addEventListener("click", (_) => {
            this.toggle();
        });
    }

    // Runs when object is disconnected from DOM
    disconnectedCallback() {}

    toggle() {
        const nameOverride = this.getAttribute("side-panel-name");
        toggleSidePanelOutsider(nameOverride);
    }

    changeLanguage() {
        changeLanguage();
    }

    profileDropdown() {
        profileDropdown();
    }

    async login() {
        try {
            const token = await this.client.login();
            if (token) {
                console.log("Login successful.");
                location.reload();
            } else {
                console.log("Login failed.");
            }
        } catch (error) {
            console.error("Login Error:", error);
        }
    }

    async logout() {
        try {
            const token = await this.client.logout();
            if (token) {
                console.log("Access Token:", token);
                location.reload();
            } else {
                console.log("Logout failed.");
            }
        } catch (error) {
            console.error("Logout Error:", error);
        }
    }
    #add_path_correction(path: string): string {
        /* Path depth should be positive number
         * For example 2 = ../../<path>
         */
        const pathDepth = this.getAttribute("path-depth") as number | null;

        // Build correction
        let correction = "";
        if (pathDepth != null && pathDepth > 0) {
            for (let i = 0; i < pathDepth; i++) {
                correction += "../";
            }
        }

        if (correction == "") {
            return path;
        }

        // Add correction
        return correction + path;
    }
}

function profileDropdown(): void {
    const profileDropdown = document.getElementById("profileDropdown");
    if (!profileDropdown) return;

    // Detect current language
    const currentLang = window.location.href.includes("/en/") ? "en" : "fi";

    // Update the menu items based on language
    const profileLink = profileDropdown.querySelector("a[href='profile.html']");
    const adminLink = profileDropdown.querySelector("a[href='admin/admin.html']");
    const logoutLink = profileDropdown.querySelector("a[href='#']");

    if (profileLink) {
        profileLink.textContent = currentLang === "en" ? "Profile" : "Profiili";
    }
    if (adminLink) {
        adminLink.textContent = currentLang === "en" ? "Admin" : "Ylläpito";
    }
    if (logoutLink) {
        logoutLink.textContent = currentLang === "en" ? "Logout" : "Kirjaudu ulos";
    }

    profileDropdown.style.display = profileDropdown.style.display === "block" ? "none" : "block";

    if (profileDropdown.style.display === "block") {
        // Add event listener to detect clicks outside the dropdown
        document.addEventListener("click", closeDropdownOutside);
    }
}

function changeLanguage(): void {
    console.log("change language");

    // Add language if none are defined in url (from proxying)
    const currentUrl = addMissingLanguage(window.location.href);
    let newUrl;

    if (currentUrl.includes("/fi/")) {
        newUrl = currentUrl.replace("/fi/", "/en/");
    } else if (currentUrl.includes("/en/")) {
        newUrl = currentUrl.replace("/en/", "/fi/");
    } else {
        return;
    }
    window.open(newUrl, "_self");
}

function addMissingLanguage(currentUrl: string): string {
    if (currentUrl.includes("/en/") || currentUrl.includes("/fi/")) {
        return currentUrl;
    }

    /* user haven't chosen language, so the language choice
     * was made at proxy level meaning language is fi
     * calculate index where to put '/fi/' and
     *
     * Url is in format http(s)://domain.do/<some>/<stuff>
     * Find first :// and next / for append location
     */

    let sepIndex = currentUrl.indexOf("://");
    if (sepIndex > 0) {
        sepIndex += "://".length;
    } else {
        sepIndex = 0;
    }

    sepIndex = currentUrl.indexOf("/", sepIndex);
    if (sepIndex < 0) {
        // entire url is domain, add to end
        return `${currentUrl}/fi/`;
    }

    const before = currentUrl.slice(0, sepIndex); // take http(s)://domain.do
    const after = currentUrl.slice(sepIndex, currentUrl.length); // take /<some>/<stuff>
    return `${before}/fi${after}`; // after contains separator /
}

function toggleSidePanelOutsider(nameOverride: string | null = null): void {
    // If side panel is not found with default name and name is overridden
    const searchName = nameOverride == null ? "side-panel-element" : nameOverride;

    const panel: HTMLElement | null = document.getElementById(searchName);
    if (panel != null) {
        panel.classList.toggle("open");
    }
}

function closeDropdownOutside(event: MouseEvent): void {
    const profileDropdown = document.getElementById("profileDropdown");
    const profileBtn = document.getElementById("ui-header-profile-btn");

    if (!profileDropdown || !profileBtn) return;

    // Check if the click is outside the dropdown and the profile button
    if (!profileDropdown.contains(event.target as Node) && !profileBtn.contains(event.target as Node)) {
        profileDropdown.style.display = "none";
        document.removeEventListener("click", closeDropdownOutside);
    }
}

if ("customElements" in window) {
    customElements.define("ui-header", Header);
}
