import HttpClient, { AUTHENTICATED_EVENT_NAME } from "../api_client/http_client.js";
import { Permissions, UI_HEADER_ID } from "../constants.js";
import { getDocumentLangCode, hasPermissions, LANG_EN, removeChildren } from "../helpers.js";
import { UserAuthenticatedEvent } from "../models/common.js";

class Header extends HTMLElement {
    client: HttpClient;
    profilePath: string | null = null;
    adminPath: string | null = null;
    permissions: Permissions | null = null;

    constructor() {
        super();
        this.client = new HttpClient();
    }

    async connectedCallback() {
        this.id = UI_HEADER_ID;

        const hasLanguageOptions: boolean =
            this.getAttribute("lang-options") === "false" ? false : true;
        const indexPath = this.#add_path_correction("index.html");

        const langBtn = hasLanguageOptions
            ? ` <div class="menu-icon header-lang-btn"
                    id="ui-header-change-language-btn"
                    >
                 <img class="globle" src="/icons/globle.png" height="30px" width="auto" />
                </div>`
            : "<!-- no other languages available -->";

        this.innerHTML = `
         <header class="header">
            <img class="header-logo"
                src="/luuppi.logo.svg"
                onclick="window.location.href='${indexPath}'"
                alt="Luuppi Logo"
            />
            <h1>LÄRPÄKE</h1>
            ${langBtn}
            <div class="_profile-container">
                <!-- Profile buttons -->
            </div>
            <div id="ui-header-open-menu-btn" class="menu-icon">☰</div>
        </header>
         `;

        const user = await this.client.trySilentLogin();
        document.addEventListener(AUTHENTICATED_EVENT_NAME, (e) => {
            const user = e as UserAuthenticatedEvent;
            if (user) {
                const permissions = user.detail.permissions;
                // Does new auth have more permissions
                if (hasPermissions(this.permissions, permissions)) {
                    this.permissions = permissions;
                    this.resetProfileBtn(permissions);
                }
            }
        });

        this.resetProfileBtn(user?.permissions ?? null);

        const languageBtn = this.querySelector<HTMLDivElement>("#ui-header-change-language-btn");
        if (languageBtn == null) {
            throw new Error("Language button not found");
        }
        languageBtn.addEventListener("click", (_) => {
            this.changeLanguage();
        });

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

    toggleProfileDropdown() {
        profileDropdown();
    }

    resetProfileBtn(permissions: Permissions | null) {
        const container = this.querySelector<HTMLElement>("._profile-container");
        if (!container) {
            console.log("Failed to update header profile button");
            return;
        }

        const btn = this.#getProfileBtn(permissions);
        removeChildren(container);
        container.appendChild(btn);
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

    getHttpClient(): HttpClient{
        return this.client;
    }

    setHttpClient(client: HttpClient) {
        this.client = client;
    }

    #getProfileBtn(permissions: Permissions | null): HTMLElement {
        if (!permissions) {
            // Permissions are null, so user is logged out
            const loginBtn = document.createElement("div");
            loginBtn.id = "ui-header-login-btn";
            loginBtn.className = "menu-icon header-profile-btn";

            const loggedOutIcon = document.createElement("img");
            loggedOutIcon.className = "header-icon";
            loggedOutIcon.src = "/icons/login-icon.png";
            loggedOutIcon.height = 30;

            loginBtn.appendChild(loggedOutIcon);

            loginBtn.addEventListener("click", (e) => {
                e.preventDefault();
                this.login();
            });

            document.addEventListener(AUTHENTICATED_EVENT_NAME, (e) => {
                console.log("Auth handled");
                const typed = e as UserAuthenticatedEvent;
                if (typed) {
                    this.resetProfileBtn(typed.detail.permissions);
                }
            });
            return loginBtn;
        }

        // User is logged in
        const isFinnish = getDocumentLangCode() !== LANG_EN;

        // Create base structure
        const profileDropdown = document.createElement("div");
        profileDropdown.className = "menu-icon header-profile-btn";
        profileDropdown.id = "ui-header-profile-btn";

        const profileIcon = document.createElement("img");
        profileIcon.className = "header-icon";
        profileIcon.src = "/icons/profile-icon.png";
        profileIcon.height = 30;

        const dropdown = document.createElement("div");
        dropdown.id = "profile-dropdown";
        dropdown.className = "_btn-container profile-dropdown";

        // Create buttons
        const profileLink = document.createElement("a");
        profileLink.href = this.#getProfilePath();
        profileLink.innerText = isFinnish ? "Profiili" : "Profile";

        const adminLink = this.#getAdminBtn(permissions, isFinnish);

        const logoutLink = document.createElement("a");
        logoutLink.href = "#";
        logoutLink.className = "color-red";
        logoutLink.innerText = isFinnish ? "Kirjaudu ulos" : "Logout";

        profileDropdown.appendChild(profileIcon);
        profileDropdown.appendChild(dropdown);

        dropdown.appendChild(profileLink);
        if (adminLink) {
            dropdown.appendChild(adminLink);
        }
        dropdown.appendChild(logoutLink);

        profileDropdown.addEventListener("click", (_) => {
            this.toggleProfileDropdown();
        });

        document.addEventListener("click", (e) => {
            const target = e.target as Node;
            if (!target) {
                return;
            }
            if (dropdown.style.display === "none") {
                return;
            }
            if (!profileDropdown.contains(target)) {
                dropdown.style.display = "none";
            }
        });

        logoutLink.addEventListener("click", (e) => {
            e.preventDefault();
            this.logout();
        });
        return profileDropdown;
    }

    #getProfilePath(): string {
        if (this.profilePath) {
            return this.profilePath;
        }
        return this.#add_path_correction("profile.html");
    }

    #getAdminPath(): string {
        if (this.adminPath) {
            return this.adminPath;
        }
        return this.#add_path_correction("admin/admin.html");
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

    #getAdminBtn(permissions: number, isFinnish: boolean): HTMLAnchorElement | null {
        if (!hasPermissions(permissions, Permissions.Admin)) {
            return null;
        }
        if (!isFinnish) {
            return null;
        }
        const adminPath = this.#getAdminPath();

        // Create button
        const result = document.createElement("a");
        result.href = adminPath;
        result.innerText = isFinnish ? "Ylläpito" : "Administration";
        return result;
    }
}

function profileDropdown(): void {
    const profileDropdown = document.getElementById("profile-dropdown");
    if (!profileDropdown) {
        throw new Error("Profile dropdown not found");
    }

    // Detect current language
    profileDropdown.style.display = profileDropdown.style.display === "block" ? "none" : "block";
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

if ("customElements" in window) {
    customElements.define("ui-header", Header);
}
