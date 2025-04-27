import HttpClient from "../api_client/http_client.js";
import { Permissions } from "../constants.js";
import { hasPermissions } from "../helpers.js";
import { UserInfo } from "../models/common.js";
import {
    adminPages,
    baseCollection,
    ListItem,
    sudoPages,
    tutorPages,
    userPages,
} from "./side-panel-data.js";

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
        const items = this.createMenuItems(user);
        this.render(items);
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
                    const listItem = document.createElement("li");
                    const link = document.createElement("a");
                    listItem.appendChild(link);
                    link.href = item.href;
                    link.textContent = item.title[language];
                    if (item.href.includes("index.html")) {
                        link.classList.add("._home-redirect");
                    }
                    return listItem.outerHTML;
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

    createMenuItems(user: UserInfo | null) {
        let items: ListItem[] = baseCollection;

        if (hasPermissions(user?.permissions, Permissions.Freshman)) {
            items.push(...userPages);
        }

        if (hasPermissions(user?.permissions, Permissions.Tutor)) {
            items.push(...tutorPages);
        }

        if (hasPermissions(user?.permissions, Permissions.Admin)) {
            items.push(...adminPages);
        }

        if (hasPermissions(user?.permissions, Permissions.Sudo)) {
            items.push(...sudoPages);
        }

        const correctedItems = this.#addPathCorrection(items);
        return correctedItems;
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
