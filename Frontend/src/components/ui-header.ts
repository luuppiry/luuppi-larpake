class Header extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        this.innerHTML = `
         <header class="header">
          <img
                src="/luuppi.logo.svg"
                onclick="window.location.href='index.html'"
                style="height: 60px; cursor: pointer;"
                alt="Luuppi Logo"
            />
            <h1>LÄRPÄKE</h1>
            <div class="menu-icon" onclick="changeLanguage()" style="display:flex; justify-content: center; align-items: center;"><img class="globle" src="/globle.png" height="30px" width="auto"></img></div>
            <div class="menu-icon" onclick="toggleSidePanelOutsider()" style="display:flex; justify-content: center; align-items: center;"><img class="profile-icon" src="/profile-icon.png" height="30px" width="auto"></img></div>
            <div class="menu-icon" onclick="toggleSidePanelOutsider()">☰</div>
         </header>
         `;
    }

    // Runs when object is disconnected from DOM
    disconnectedCallback() {}

    toggle() {
        toggleSidePanelOutsider();
    }

    changeLanguage() {
        changeLanguage();
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
    return `${before}/fi${after}`;  // after contains separator /
}

function toggleSidePanelOutsider(): void {
    const panel: HTMLElement | null = document.getElementById("sidePanel");
    if (panel != null) {
        panel.classList.toggle("open");
    }
}

if ("customElements" in window) {
    customElements.define("ui-header", Header);
}
