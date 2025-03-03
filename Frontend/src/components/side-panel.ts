type ListItem = {
    href: string;
    title: { fi: string; en: string };
    submenu?: ListItem[];
};

class SidePanel extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        this.#addClickListeners();

        const items: ListItem[] = [
            { href: "index.html", title: { fi: "Koti", en: "Home" } },
            {
                href: "larpake.html",
                title: { fi: "Lärpäke", en: "Lärpäke" },
                submenu: [
                    { href: "larpake.html?page=0", title: { fi: "Ensi askeleet", en: "Baby steps" } },
                    {
                        href: "larpake.html?page=2",
                        title: { fi: "Pienen pieni luuppilainen", en: "Youngster luuppi's member" },
                    },
                    { href: "larpake.html?page=4", title: { fi: "Pii-Klubilla tapahtuu", en: "Pii-Klubi happenings" } },
                    { href: "larpake.html?page=5", title: { fi: "Normipäivä", en: "Averageday" } },
                    { href: "larpake.html?page=6", title: { fi: "Yliopistoelämää", en: "University life" } },
                    { href: "larpake.html?page=8", title: { fi: "Vaikutusvaltaa", en: "Power usage" } },
                    { href: "larpake.html?page=10", title: { fi: "Liikunnallista", en: "Sporty" } },
                    { href: "larpake.html?page=11", title: { fi: "Kaikenlaista", en: "Miscellaneous" } },
                    { href: "larpake.html?page=13", title: { fi: "Tanpereella", en: "Welcome to Tampere" } },
                ],
            },
            {
                href: "statistics.html",
                title: { fi: "Omat tilastot", en: "My Statistics" },
            },
            {
                href: "latest_completion.html",
                title: { fi: "Viimeisimmät suoritukset", en: "Latest Achievements" },
            },
            {
                href: "common_statistics.html",
                title: { fi: "Yhteiset tilastot", en: "Common Statistics" },
            },
            {
                href: "upcoming_events.html",
                title: { fi: "Tulevat tapahtumat", en: "Upcoming Events" },
            },
            {
                href: "own_tutors.html",
                title: { fi: "Omat tuutorit", en: "My Tutors" },
            },
            {
                href: "event_marking.html",
                title: { fi: "DEMO Kirjaa osallistuminen - Fuksi", en: "DEMO Log Attendance - Freshman" },
            },
            {
                href: "tutor_mark_event.html",
                title: { fi: "DEMO Kirjaa osallistuminen - Tuutori", en: "DEMO Log Attendance - Tutor" },
            },
        ];

        const correctedItems = this.#addPathCorrection(items);
        this.render(correctedItems);
    }

    render(items: ListItem[]) {
        const language = this.getAttribute("lang") !== "en" ? "fi" : "en";

        const menuItems = items
            .map((item) => {
                if (item.submenu) {
                    const submenuItems = item.submenu
                        .map((sub) => `<ul><a href="${sub.href}">${sub.title[language]}</a></ul>`)
                        .join("\n");
                    return `
                    <li class="submenu-toggle">${item.title[language]}</li>
                    <li class="active" id="larpakeSubMenu" style="display: none;">
                        ${submenuItems}
                    </li>`;
                } else {
                    return `<li><a href="${item.href}">${item.title[language]}</a></li>`;
                }
            })
            .join("\n");

        this.innerHTML = `
            <nav class="side-panel" id="side-panel-element">
                <div class="close-btn" onclick="toggleSidePanel()" style="display:flex; justify-content: center; align-items: center;">
                    <img class="close-x" src="/icons/close-x.png" height="30px" width="auto">
                </div>
                <ul>${menuItems}</ul>
            </nav>
        `;

        this.setupSubmenuToggle();
    }

    setupSubmenuToggle() {
        this.querySelectorAll(".submenu-toggle").forEach((toggle) => {
            toggle.addEventListener("click", (event) => {
                event.preventDefault();
                const submenu = toggle.nextElementSibling as HTMLElement;
                if (submenu.style.display === "none" || submenu.style.display === "") {
                    submenu.style.display = "block";
                } else {
                    window.open("larpake.html", "_self");
                }
            });
        });
    }

    #addPathCorrection(items: ListItem[]) {
        const stringPathDepth = this.getAttribute("path-depth") ?? "0";
        const pathDepth = parseInt(stringPathDepth) || 0;
        let correction = "";
        for (let i = 0; i < pathDepth; i++) {
            correction += "../";
        }
        items.forEach((item) => {
            item.href = correction + item.href;
            if (item.submenu) {
                item.submenu.forEach((sub) => (sub.href = correction + sub.href));
            }
        });
        return items;
    }

    #addClickListeners() {
        window.addEventListener(
            "click",
            (event: MouseEvent) => {
                if (!this.firstElementChild?.classList.contains("open")) {
                    // Side panel is already closed
                    return;
                }

                // Get clicked UI element
                const target = event.target as HTMLElement;

                if (document.querySelector<HTMLElement>("ui-header")?.contains(target)) {
                    /* This is little bit janky but works
                     * If side ui header is not filtered out,
                     * this would immidiately close the menu when menu button pressed
                     */
                    return;
                }

                if (this.contains(target) == true) {
                    // Clicked inside sidepanel
                    return;
                }
                this.toggleSidePanel();
            },
            false
        );
    }

    toggleSidePanel() {
        toggleSidePanel();
    }
}

function toggleSidePanel(): void {
    const panel: HTMLElement | null = document.getElementById("side-panel-element");
    if (panel != null) {
        panel.classList.toggle("open");
    }
}

if ("customElements" in window) {
    customElements.define("side-panel", SidePanel);
}
