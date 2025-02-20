class Header extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        this.innerHTML = `
         <header class="header">
          <img
                src="/luuppi.logo.svg"
                onclick="window.location.href='main.html'"
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
}

function changeLanguage(): void {
    const currentUrl = window.location.href;
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

function toggleSidePanelOutsider(): void {
    const panel: HTMLElement | null = document.getElementById("sidePanel");
    if (panel != null) {
        panel.classList.toggle("open");
    }
}

if ("customElements" in window) {
    customElements.define("ui-header", Header);
}
