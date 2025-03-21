import { UserClient } from "../api_client/user_client";
import { Q_PAGE_OFFSET, Q_PAGE_SIZE, Q_SEARCH } from "../constants";
import { appendTemplateElement, isEmpty, removeChildren, throwIfAnyNull } from "../helpers";
import { User } from "../models/user";

const VISIBLE_ID_LENGTH = 6;
const DEFAULT_PAGE_SIZE = 25;
const DEBOUNCH_TIMEOUT = 500;

type Paging = {
    pageSize: number;
    pageOffset: number;
    searchValue: string | null;
};

const data: User[] = [
    {
        id: "01951002-aaa9-7175-a89b-153eff018ea7",
        entraId: "cffd39ef-6d20-459d-8c34-bf94fbffe02c",
        firstName: "Henri",
        lastName: "Vainio",
        permissions: 2147483647,
        entraUsername: "henri.m.vainio@gmail.com",
        username: "henrivain",
        startYear: 2024,
    },
    {
        id: "0194ad9a-c487-7f0d-b0be-a9b12e6d44d3",
        entraId: "0194ad9a-c487-705e-90c0-2a046022a0c0",
        firstName: "Juliusz",
        lastName: "Kotelba",
        permissions: 2147483647,
        entraUsername: "juliusz.kotelba@gmail.com",
        username: "julle",
        startYear: 2024,
    },
];

class UserBrowser {
    allUsers: User[] = [];
    filtered: User[] = [];
    container: HTMLElement;
    nextPageBtn: HTMLButtonElement;
    prevPageBtn: HTMLButtonElement;
    offset: number;
    pageSize: number;
    fromIndexElem: HTMLSpanElement;
    toIndexElem: HTMLSpanElement;
    maxIndexElem: HTMLSpanElement;
    searchField: HTMLInputElement;
    filter: string | null = null;
    debounchTimerId: number | null = null;

    constructor(options: Paging) {
        this.offset = options.pageOffset;
        this.pageSize = options.pageSize;
        this.container = document.getElementById("user-container") as HTMLElement;
        this.prevPageBtn = document.getElementById("prev-btn") as HTMLButtonElement;
        this.nextPageBtn = document.getElementById("next-btn") as HTMLButtonElement;
        this.fromIndexElem = document.getElementById("from-count") as HTMLSpanElement;
        this.toIndexElem = document.getElementById("to-count") as HTMLSpanElement;
        this.maxIndexElem = document.getElementById("out-of") as HTMLSpanElement;
        this.searchField = document.getElementById("search-field") as HTMLInputElement;

        throwIfAnyNull([
            this.container,
            this.prevPageBtn,
            this.nextPageBtn,
            this.maxIndexElem,
            this.toIndexElem,
            this.fromIndexElem,
            this.searchField,
        ]);

        // Bind function to point to correct "this" in event handlers
        this.renderFiltered = this.renderFiltered.bind(this);
        this.renderPrevious = this.renderPrevious.bind(this);
        this.renderFiltered = this.renderFiltered.bind(this);

        // Add event handlers
        this.nextPageBtn.addEventListener("click", this.renderNext);
        this.prevPageBtn.addEventListener("click", this.renderPrevious);
        this.searchField.addEventListener("input", this.renderFiltered);

        if (options.searchValue) {
            this.searchField.value = options.searchValue;
        }
    }

    setAvailableUsers(users: User[]) {
        this.allUsers = users;
        this.#filter();
    }

    renderNext() {
        this.#render(this.offset + this.pageSize, this.filtered);
    }

    renderPrevious() {
        this.#render(this.offset - this.pageSize, this.filtered);
    }

    renderCurrent() {
        this.#render(this.offset, this.filtered);
    }

    renderFiltered() {
        const func = () => {
            pushSearchValue(this.searchField.value);
            this.#filter();
            this.renderCurrent();
        };

        clearTimeout(this.debounchTimerId ?? undefined);

        this.debounchTimerId = setTimeout(func, DEBOUNCH_TIMEOUT);
    }

    #render(offset: number, allUsers: User[]) {
        if (offset > allUsers.length) {
            offset = largestMultipleLessThanOrEqualTo(this.pageSize, allUsers.length);
        }
        if (offset < 0) {
            offset = 0;
        }

        const endIndex = Math.min(allUsers.length, offset + this.pageSize);
        this.#renderUsers(allUsers.slice(offset, endIndex));

        pushPageQuery(this.pageSize, offset);

        this.offset = offset;
        this.#updateIndexes(offset, Math.min(allUsers.length, offset + this.pageSize), allUsers.length);
    }

    #renderUsers(users: User[]) {
        removeChildren(this.container, (child) => !child.classList.contains("column-titles"));

        for (const user of users) {
            const elem = appendTemplateElement<HTMLTableRowElement>("user-template", this.container);

            elem.querySelector<HTMLTableCellElement>("._email")!.innerText = user.entraUsername ?? "N/A";
            elem.querySelector<HTMLTableCellElement>("._username")!.innerText = user.username ?? "-";
            elem.querySelector<HTMLTableCellElement>("._first-name")!.innerText = user.firstName ?? "-";
            elem.querySelector<HTMLTableCellElement>("._last-name")!.innerText = user.lastName ?? "-";
            elem.querySelector<HTMLTableCellElement>("._permissions")!.innerText = user.permissions.toString() ?? "-";
            elem.querySelector<HTMLTableCellElement>("._startYear")!.innerText = user.startYear?.toString() ?? "-";
            elem.querySelector<HTMLTableCellElement>("._id")!.innerText = truncateId(user.id);
            elem.querySelector<HTMLTableCellElement>("._entra-id")!.innerText = truncateId(user.entraId);
            elem.querySelector<HTMLTableCellElement>("._id-full")!.innerText = user.id ?? "";
            elem.querySelector<HTMLTableCellElement>("._entra-id-full")!.innerText = user.entraId ?? "";
        }
    }

    #updateIndexes(offset: number, count: number, max: number) {
        // Button *enablement*
        this.prevPageBtn.disabled = offset <= 0;
        this.nextPageBtn.disabled = offset + count >= max;

        // Numeric indexes
        this.fromIndexElem.innerText = offset.toString();
        this.toIndexElem.innerText = Math.min(offset + count, max).toString();
        this.maxIndexElem.innerText = max.toString();
    }

    #filter() {
        const value = this.searchField.value;
        if (isEmpty(value)) {
            this.filtered = this.allUsers;
            return;
        }
        this.filtered = this.allUsers.filter(
            (u) =>
                u.username?.includes(value) ||
                u.entraId?.includes(value) ||
                u.id.includes(value) ||
                u.firstName?.includes(value) ||
                u.lastName?.includes(value) ||
                u.permissions.toString().includes(value) ||
                u.entraUsername?.includes(value) ||
                u.startYear?.toString().includes(value)
        );
    }
}

async function main() {
    const allUsers = await fetchUsers();

    const options = getOptions();
    const browser = new UserBrowser(options);
    browser.setAvailableUsers(allUsers);
    browser.renderCurrent();
}

function pushPageQuery(size: number, offset: number) {
    const url = new URL(window.location.href);
    const params = new URLSearchParams(url.search);
    params.set(Q_PAGE_SIZE, size.toString());
    params.set(Q_PAGE_OFFSET, offset.toString());
    url.search = params.toString();
    window.history.pushState({}, "", url);
}

function pushSearchValue(value: string | null) {
    const url = new URL(window.location.href);
    const params = new URLSearchParams(url.search);
    if (isEmpty(value)) {
        params.delete(Q_SEARCH);
    } else {
        params.set(Q_SEARCH, value);
    }
    url.search = params.toString();
    window.history.pushState({}, "", url);
}

function getOptions(): Paging {
    const params = new URLSearchParams(new URL(window.location.href).search);
    const offset = parseInt(params.get(Q_PAGE_OFFSET) ?? "");
    const size = parseInt(params.get(Q_PAGE_SIZE) ?? "");
    const searchValue = params.get(Q_SEARCH);
    return {
        pageOffset: Number.isNaN(offset) ? 0 : offset,
        pageSize: Number.isNaN(size) ? DEFAULT_PAGE_SIZE : size,
        searchValue: searchValue,
    };
}

function truncateId(id: string | null): string {
    if (isEmpty(id)) {
        return "N/A";
    }
    if (id.length < VISIBLE_ID_LENGTH) {
        return `...${id}`;
    }
    // Take from the back
    return `...${id.slice(-VISIBLE_ID_LENGTH)}`;
}

async function fetchUsers(): Promise<User[]> {
    return data;
}

function largestMultipleLessThanOrEqualTo(threshold: number, num: number) {
    if (num === 0) {
        // Avoid division by zero
        return 0;
    }
    return Math.floor(threshold / num) * num;
}

main();
