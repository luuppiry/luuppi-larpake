import HttpClient from "../api_client/http_client.js";
import { Permissions } from "../constants.js";
import { hasPermissions } from "../helpers.js";

type ListItem = {
    href: string;
    title: { fi: string; en: string };
    submenu?: ListItem[];
};

class SidePanel extends HTMLElement {
    client: HttpClient;
    profilePath: string | null = null;
    adminPath: string | null = null;
    permissions: Permissions | null = null;

    constructor() {
        super();
        this.client = new HttpClient();
    }

    async connectedCallback() {
        this.#addClickListeners();
        const user = await this.client.trySilentLogin();
        let items: ListItem[] = [{ href: "index.html", title: { fi: "Koti", en: "Home" } }];
        if (hasPermissions(user?.permissions ?? null, Permissions.Freshman)) {
            items = [
                { href: "index.html", title: { fi: "Koti", en: "Home" } },
                {
                    href: "larpake.html",
                    title: { fi: "Lärpäke", en: "Lärpäke" },
                    submenu: [
                        {
                            href: "larpake.html?page=0",
                            title: { fi: "Ensi askeleet", en: "Baby steps" },
                        },
                        {
                            href: "larpake.html?page=2",
                            title: { fi: "Pienen pieni luuppilainen", en: "Young Luuppian" },
                        },
                        {
                            href: "larpake.html?page=4",
                            title: { fi: "Pii-Klubilla tapahtuu", en: "Pii-Klubi happenings" },
                        },
                        {
                            href: "larpake.html?page=5",
                            title: { fi: "Normipäivä", en: "Averageday" },
                        },
                        {
                            href: "larpake.html?page=6",
                            title: { fi: "Normipäivä", en: "Normal day" },
                        },
                        {
                            href: "larpake.html?page=7",
                            title: { fi: "Yliopistoelämää", en: "University life" },
                        },
                        {
                            href: "larpake.html?page=9",
                            title: { fi: "Vaikutusvaltaa", en: "Influence" },
                        },
                        {
                            href: "larpake.html?page=11",
                            title: { fi: "Liikunnallista", en: "Exercise - Participate" },
                        },
                        {
                            href: "larpake.html?page=12",
                            title: { fi: "Kaikenlaista", en: "This and that" },
                        },
                        {
                            href: "larpake.html?page=14",
                            title: { fi: "Tanpereella", en: "In Tampere" },
                        },
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
                    href: "own_groups.html",
                    title: { fi: "Ryhmäni", en: "My Group" },
                },
                {
                    href: "join.html",
                    title: { fi: "Liity ryhmään", en: "Join group" },
                },
                {
                    href: "faq.html",
                    title: { fi: "UKK", en: "FAQ" },
                },
                {
                    href: "contact_us.html",
                    title: { fi: "Ota yhteyttä", en: "Contact us" },
                },
            ];
        }

        if (hasPermissions(user?.permissions ?? null, Permissions.Tutor)) {
            items.push({
                href: "read.html",
                title: { fi: "Tuutori - Allekirjoita", en: "Tutor - Sign Task" },
            });
        }

        if (hasPermissions(user?.permissions ?? null, Permissions.Admin)) {
            items.push({
                href: "admin/admin.html",
                title: { fi: "Ylläpito", en: "Admin ONLY IN FI" },
            });
        }

        if (hasPermissions(user?.permissions ?? null, Permissions.Sudo)) {
            items.push({
                href: "admin/admin.html",
                title: { fi: "SUDO Ylläpito", en: "Sudo Admin ONLY IN FI" },
            });
        }

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
                <div class="_close close-btn">
                    <img class="close-x" src="/icons/close-x.png" height="30px" width="auto">
                </div>
                <ul>${menuItems}</ul>
            </nav>
        `;

        this.setupSubmenuToggle();
        this.querySelector<HTMLDivElement>("._close")?.addEventListener("click", (_) => {
            this.toggleSidePanel();
        });
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
