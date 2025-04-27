import HttpClient from "../api_client/http_client.js";
import { Permissions, SIDE_PANEL_ID } from "../constants.js";
import { hasPermissions, removeChildren } from "../helpers.js";
import { UserInfo } from "../models/common.js";
import {
    adminPages,
    baseCollection,
    ListItem,
    sudoPages,
    tutorPages,
    userPages,
} from "./side-panel-data.js";

export default class SidePanel extends HTMLElement {
    client: HttpClient;
    profilePath: string | null = null;
    adminPath: string | null = null;
    permissions: Permissions | null = null;
    isLoaded: boolean = false;
    loaded: (panel: SidePanel) => void;

    constructor() {
        super();
        this.id = SIDE_PANEL_ID;
        this.loaded = (_) => {};
        this.client = new HttpClient();
    }

    async connectedCallback() {
        this.#addClickListeners();
        let user: UserInfo | null = null;
        try {
            user = await this.client.trySilentLogin();
        } catch {}
        const items = this.#createMenuItems(user);
        this.render(items);
        this.loaded(this);
    }

    render(items: ListItem[]) {
        const language: "fi" | "en" = this.getAttribute("lang") !== "en" ? "fi" : "en";

        const nav = document.createElement("nav");
        nav.className = "_nav side-panel";
        nav.id = SIDE_PANEL_ID;

        const closeBtn = document.createElement("div");
        closeBtn.className = "_close close-btn";
        nav.appendChild(closeBtn);

        const closeIcon = document.createElement("img");
        closeIcon.className = "close-x";
        closeIcon.src = "/icons/close-x.png";
        closeIcon.height = 30;
        closeIcon.width = 30;
        closeBtn.appendChild(closeIcon);

        const itemContainer = document.createElement("ul");
        itemContainer.className = "_container";
        nav.appendChild(itemContainer);

        const menuItems = items.map((x) => this.#createMenuItem(x, language));
        menuItems.forEach((x) => itemContainer.appendChild(x));

        removeChildren(this);
        this.appendChild(nav);

        this.#setupSubmenuToggle();
        this.querySelector<HTMLDivElement>("._close")?.addEventListener("click", (_) => {
            this.toggleSidePanel();
        });
    }

    setLoaded(loaded: (panel: SidePanel) => void) {
        /* Run action to side panel after panel
         * fully loaded. If already loaded, runs immidiately.
         * Does not overwrite already set actions, but combines
         * them.
         * */

        // Invoke if already loaded
        if (this.isLoaded) {
            loaded(this);
        }

        const oldLoaded = this.loaded;
        // Combine multiple possible existing actions
        this.loaded = (_) => {
            oldLoaded(this);
            loaded(this);
        };
    }

    queryMenuItems(query: string) {
        return this.querySelectorAll(query);
    }

    #createMenuItems(user: UserInfo | null) {
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

    #createMenuItem(item: ListItem, lang: "fi" | "en"): HTMLElement {
        const listItem = document.createElement("li");
        if (item.queryId) {
            listItem.classList.add(item.queryId);
        }

        if (item.submenu) {
            // Create menu item
            listItem.classList.add("_toggleable");
            listItem.classList.add("submenu-toggle");
            listItem.innerText = item.title[lang];

            // Create submenu container
            const childContainer = document.createElement("ul");
            childContainer.className = "_submenu active";
            childContainer.style.display = "none";
            listItem.appendChild(childContainer);

            // Add submenu items
            const children = this.#createSubmenu(item, lang);
            children.forEach((child) => childContainer.appendChild(child));

            return listItem;
        }

        // Create non-submenu item
        const link = document.createElement("a");
        listItem.appendChild(link);
        link.href = item.href;
        link.textContent = item.title[lang];
        return listItem;
    }

    #createSubmenu(parentItem: ListItem, lang: "fi" | "en"): HTMLElement[] {
        if (!parentItem.submenu) {
            throw new Error("Invalid operation, submenu null");
        }
        return parentItem.submenu.map((item) => {
            const li = document.createElement("li");
            const link = document.createElement("a");
            link.href = item.href;
            link.innerText = item.title[lang] ?? "";
            li.appendChild(link);
            return li;
        });
    }

    #setupSubmenuToggle() {
        this.querySelectorAll("._toggleable").forEach((toggle) => {
            toggle.addEventListener("click", (event) => {
                event.preventDefault();
                const submenu = toggle.querySelector("._submenu") as HTMLElement;
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
    const panel = document.getElementById(SIDE_PANEL_ID);
    const area = panel?.querySelector<HTMLElement>("._nav");

    if (area != null) {
        area.classList.toggle("open");
    }
}

if ("customElements" in window) {
    customElements.define("side-panel", SidePanel);
}
