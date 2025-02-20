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
            <span class="menu-icon" onclick="toggleSidePanel()" style="display:block;"><img class="globle" src="/globle.png" height="30px" width="auto"></img></span>
            <span class="menu-icon" onclick="toggleSidePanel()">L</span>
            <span class="menu-icon" onclick="toggleSidePanelOutsider()">☰</span>
         </header>
         `;
    }

    // Runs when object is disconnected from DOM
    disconnectedCallback() {}

    toggelSidePanel() {
        toggleSidePanelOutsider();
    }
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
