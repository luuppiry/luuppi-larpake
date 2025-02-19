type ListItem = {
    href: string;
    title: string;
};

class SidePanel extends HTMLElement {
    items: ListItem[];

    constructor() {
        super();

        this.items = [
            { href: "main.html", title: "Home" },
            { href: "larpake.html", title: "Lärpäke" },
            { href: "statistics.html", title: "Own statistics" },
            {
                href: "latest_accomplishment.html",
                title: "Latest accomplishments",
            },
            { href: "common_statistics.html", title: "Common statistics" },
            { href: "upcoming_events.html", title: "Upcoming events" },
            { href: "own_tutors.html", title: "Own tutors" },
            { href: "event_marking.html", title: "Event_marking_fuksi" },
            { href: "tutor_mark_event.html", title: "Event_marking_tutor" },
        ];
    }

    // Runs when element is appended or moved in DOM
    connectedCallback() {
        const lines: string = this.items
            .map((x) => `<li><a href="${x.href}">${x.title}</a></li>`)
            .reduce((acc, x) => `${acc}\n${x}`);

        this.innerHTML = `
         <div class="side-panel" id="sidePanel">
             <span class="close-btn" onclick="toggleSidePanel()">
                 X
             </span>
             <ul>${lines}</ul>
         </div>;
         `;
    }

    // Runs when object is disconnected from DOM
    disconnectedCallback() {}

    toggelSidePanel() {
        toggleSidePanel();
    }
}

function toggleSidePanel(): void {
    const panel: HTMLElement | null = document.getElementById("sidePanel");
    if (panel != null) {
        panel.classList.toggle("open");
    }
}

if ("customElements" in window) {
    customElements.define("side-panel", SidePanel);
}
