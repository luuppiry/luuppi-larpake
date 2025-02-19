type ListItem = {
    href: string;
    title: { [key: string]: string }; // Support multiple languages
};

class SidePanel extends HTMLElement {
    items: ListItem[];


    constructor() {
        super();
        

        this.items = [
            { 
                href: "index.html", 
                title: { fi: "Koti", en: "Home" } 
            },
            { 
                href: "larpake.html", 
                title: { fi: "Lärpäke", en: "Widget" } 
            },
            { 
                href: "statistics.html", 
                title: { fi: "Oma statistiikka", en: "My Statistics" } 
            },
            {
                href: "latest_accomplishment.html",
                title: { fi: "Viimeisimmät suoritukset", en: "Latest Achievements" },
            },
            { 
                href: "common_statistics.html", 
                title: { fi: "Yhteiset statistiikat", en: "Shared Statistics" } 
            },
            { 
                href: "upcoming_events.html", 
                title: { fi: "Tulevat tapahtumat", en: "Upcoming Events" } 
            },
            { 
                href: "own_tutors.html", 
                title: { fi: "Omat tutorit", en: "My Tutors" } 
            },
            { 
                href: "event_marking.html", 
                title: { fi: "Kirjaa osallistuminen - Fuksi", en: "Log Attendance - Freshman" } 
            },
            { 
                href: "tutor_mark_event.html", 
                title: { fi: "Kirjaa osallistuminen - Tuutori", en: "Log Attendance - Tutor" } 
            },
        ];
    }

    connectedCallback() {
        this.render();
    }

  

    render() {
        const language = this.getAttribute("lang") !==  "en" ?  "fi" : "en";
        const menuItems = this.items
            .map((item) => `<li><a href="${item.href}">${item.title[language]}</a></li>`)
            .join("\n");

        this.innerHTML = `
         <div class="side-panel" id="sidePanel">
             <span class="close-btn" onclick="toggleSidePanel()">X</span>
             <ul>${menuItems}</ul>
         </div>
         `;
    }

    disconnectedCallback() {}

    toggleSidePanel() {
        toggleSidePanel();
    }
}

function toggleSidePanel(): void {
    const panel: HTMLElement | null = document.getElementById("sidePanel");
    if (panel) {
        panel.classList.toggle("open");
    }
}

if ("customElements" in window) {
    customElements.define("side-panel", SidePanel);
}
