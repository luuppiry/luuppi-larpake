type ListItem = {
    href: string;
    title: { [key: string]: string }; // Support multiple languages
};

class SidePanel extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        const language = this.getAttribute("lang") !== "en" ? "fi" : "en";
        const items: ListItem[] = [
            {
                href: "index.html",
                title: { fi: "Koti", en: "Home" },
            },
            {
                href: "larpake.html",
                title: { fi: "Lärpäke", en: "Widget" },
            },
            {
                href: "statistics.html",
                title: { fi: "Oma statistiikka", en: "My Statistics" },
            },
            {
                href: "latest_completion.html",
                title: { fi: "Viimeisimmät suoritukset", en: "Latest Achievements" },
            },
            {
                href: "common_statistics.html",
                title: { fi: "Yhteiset statistiikat", en: "Shared Statistics" },
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
            {
                href: "profile.html",
                title: { fi: "DEMO Profiili", en: "DEMO Profile" },
            },
        ];

        if (language === "fi") {
            items.push({
                href: "admin/admin.html",
                title: { fi: "DEMO Ylläpito", en: "Admin" },
            });
        }

        const correctedItems = this.#add_path_correction(items);

        this.render(correctedItems);

        window.addEventListener(
            "click",
            (event: MouseEvent) => {
                if (!this.firstElementChild?.classList.contains("open")) {
                    // Side bar was not opened
                    return;
                }

                const target = event.target as HTMLElement;

                /* This is little bit janky but works
                 * If side ui header is not filtered out,
                 * this would immidiately close the menu when menu button pressed
                 */
                if (document.querySelector<HTMLElement>("ui-header")?.contains(target)) {
                    return;
                }

                if (this.contains(target) == true) {
                    // Clicked inside sidebar
                    return;
                }
                this.toggleSidePanel();
            },
            false
        ); // False to sen event listener onwards from here
    }

    render(items: ListItem[]) {
        const language = this.getAttribute("lang") !== "en" ? "fi" : "en";
        const menuItems = items.map((item) => `<li><a href="${item.href}">${item.title[language]}</a></li>`).join("\n");

        this.innerHTML = `
         <div class="side-panel" id="side-panel-element">
             <div class="close-btn" onclick="toggleSidePanel()" style="display:flex; justify-content: center; align-items: center;">
                <img class="close-x" id="side-panel-close-btn" src="/icons/close-x.png" height="30px" width="auto"></img>
             </div>
             <ul>${menuItems}</ul>
         </div>
         `;
    }

    #add_path_correction(items: ListItem[]): ListItem[] {
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
            return items;
        }

        // Add correction
        for (let j = 0; j < items.length; j++) {
            items[j].href = correction + items[j].href;
        }
        return items;
    }

    disconnectedCallback() {}

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
