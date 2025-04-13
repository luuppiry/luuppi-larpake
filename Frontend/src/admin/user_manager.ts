import { UserClient } from "../api_client/user_client.js";
import { Q_PAGE_OFFSET, Q_PAGE_SIZE, Q_SEARCH } from "../constants.js";
import {
    appendTemplateElement,
    getDocumentLangCode,
    isEmpty,
    LANG_EN,
    pushUrlState,
    removeChildren,
    replaceUrlState,
} from "../helpers.js";
import { User } from "../models/user.js";
import { userSortFunc } from "../sortFunctions.js";
import UserManagerUI from "./ui-model/user_manager_ui.js";

const VISIBLE_ID_LENGTH = 6;
const DEFAULT_PAGE_SIZE = 25;
const DEBOUNCH_TIMEOUT = 500;

const userClient = new UserClient();

type ChangableUserData = {
    permissions: number;
    startYear: number | null;
};

type Paging = {
    pageSize: number;
    pageOffset: number;
    searchValue: string | null;
};

class UserBrowser extends UserManagerUI {
    allUsers: User[] = [];
    filtered: User[] = [];
    offset: number;
    pageSize: number;
    filter: string | null = null;
    debounchTimerId: NodeJS.Timeout | null = null;
    isInitialPage: boolean = true;

    constructor(options: Paging) {
        super();
        this.offset = options.pageOffset;
        this.pageSize = options.pageSize;

        // Add event handlers
        this.nextPageBtn.addEventListener("click", (_) => this.renderNext());
        this.prevPageBtn.addEventListener("click", (_) => this.renderPrevious());
        this.searchField.addEventListener("input", (_) => this.renderFiltered());

        if (options.searchValue) {
            this.searchField.value = options.searchValue;
        }
    }

    setAvailableUsers(users: User[]) {
        this.allUsers = users.sort(userSortFunc);
        this.#filter();
    }

    renderFirst() {
        this.#render(0, this.filtered);
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
            this.renderFirst();
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

        const stateFunc = (params: URLSearchParams) => {
            params.set(Q_PAGE_SIZE, this.pageSize.toString());
            params.set(Q_PAGE_OFFSET, offset.toString());
        };
        if (this.isInitialPage) {
            replaceUrlState(stateFunc);
            this.isInitialPage = false;
        } else {
            pushUrlState(stateFunc);
        }

        this.offset = offset;
        this.#updateIndexes(
            offset,
            Math.min(allUsers.length, offset + this.pageSize),
            allUsers.length
        );
    }

    #renderUsers(users: User[]) {
        removeChildren(this.container, (child) => !child.classList.contains("column-titles"));

        for (const user of users) {
            const elem = appendTemplateElement<HTMLTableRowElement>(
                "user-template",
                this.container
            );
            setUserInformation(user, elem, true);
            addUserDialogListener(user, elem);
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

    loadPermissionValues();
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
    const users = await userClient.getAllUnpaged();
    if (!users) {
        throw new Error("Failed to fetch users from server.");
    }
    return users;
}

function largestMultipleLessThanOrEqualTo(threshold: number, num: number) {
    if (num === 0) {
        // Avoid division by zero
        return 0;
    }
    return Math.floor(threshold / num) * num;
}

function setUserInformation(user: User, rootElem: HTMLElement, truncateIds: boolean) {
    rootElem.querySelector<HTMLTableCellElement>("._email")!.innerText =
        user.entraUsername ?? "N/A";
    rootElem.querySelector<HTMLTableCellElement>("._username")!.innerText = user.username ?? "-";
    rootElem.querySelector<HTMLTableCellElement>("._first-name")!.innerText = user.firstName ?? "-";
    rootElem.querySelector<HTMLTableCellElement>("._last-name")!.innerText = user.lastName ?? "-";
    const permissions = rootElem.querySelector<HTMLTableCellElement>("._permissions");
    if (permissions instanceof HTMLInputElement) {
        (permissions as HTMLInputElement).value = user.permissions.toString() ?? "-";
    } else {
        permissions!.innerText = user.permissions.toString() ?? "-";
    }

    const startYear = rootElem.querySelector<HTMLElement>("._start-year");
    if (startYear instanceof HTMLInputElement) {
        (startYear as HTMLInputElement).value = user.startYear?.toString() ?? "";
    } else {
        startYear!.innerText = user.startYear?.toString() ?? "";
    }

    rootElem.querySelector<HTMLInputElement>("._start-year")!.value =
        user.startYear?.toString() ?? "";
    rootElem.querySelector<HTMLTableCellElement>("._id")!.innerText = truncateIds
        ? truncateId(user.id)
        : user.id ?? "N/A";
    rootElem.querySelector<HTMLTableCellElement>("._entra-id")!.innerText = truncateIds
        ? truncateId(user.entraId)
        : user.entraId ?? "N/A";
}

function readUserData(rootElem: HTMLElement): ChangableUserData {
    const permissions = parseInt(rootElem.querySelector<HTMLInputElement>("._permissions")!.value);
    if (Number.isNaN(permissions)) {
        throw new Error("Invalid permissions value");
    }

    let startYear: null | number = null;
    const startYearStr = rootElem.querySelector<HTMLInputElement>("._start-year")!.value;
    if (startYearStr) {
        startYear = parseInt(startYearStr);
        if (Number.isNaN(startYear)) {
            throw new Error("Invalid start year value");
        }
    }

    return {
        permissions: permissions,
        startYear: startYear,
    };
}

function addUserDialogListener(user: User, elem: HTMLElement) {
    elem.addEventListener("click", (_) => {
        const dialog = document.getElementById("user-dialog") as HTMLDialogElement;

        dialog.showModal();
        document.body.classList.add("no-scroll");

        setUserInformation(user, dialog, false);

        dialog.addEventListener("close", (_) => {
            document.body.classList.remove("no-scroll");
        });
        dialog.querySelector<HTMLButtonElement>("._ok")?.addEventListener("click", async (e) => {
            // Do somehting
            e.preventDefault();

            const data = readUserData(dialog);
            console.log(
                `User ${user.id} updated from`,
                { permissions: user.permissions, startYear: user.startYear },
                " to ",
                data
            );
            dialog.close();

            const setStartYear = data.startYear != user.startYear;
            if (setStartYear) {
                const success = await userClient.updateUser(user.id, data.startYear);
                if (!success) {
                    showDialog(
                        "error-dialog",
                        "Aloitusvuoden päivittäminen epäonnistui, katso konsoli (F12 -> console)"
                    );
                    return;
                }
            }

            const setPermissions = data.permissions != user.permissions;
            if (setPermissions) {
                const success = await userClient.setPermissions(user.id, data.permissions);
                if (!success) {
                    showDialog(
                        "error-dialog",
                        "Oikeuksien päivittäminen epäonnistui, katso konsoli (F12 -> console)"
                    );
                    return;
                }
            }

            if (setPermissions || setStartYear) {
                showDialog(
                    "success-dialog",
                    `Päivitettiin onnistuneesti käyttäjän tiedot: ${
                        setPermissions ? "oikeudet" : ""
                    } ${setStartYear ? "aloitusvuosi" : ""}`
                );
            }

            elem.querySelector<HTMLElement>("._permissions")!.innerText =
                data.permissions.toString();
            elem.querySelector<HTMLElement>("._start-year")!.innerText =
                data.startYear?.toString() ?? "";

            user.startYear = data.permissions;
            user.permissions = data.permissions;
        });
        dialog.querySelector<HTMLButtonElement>("._cancel")?.addEventListener("click", (e) => {
            e.preventDefault();
            dialog.close();
        });
    });
}

function showDialog(dialogName: string, message: string) {
    const successDialog = document.getElementById(dialogName) as HTMLDialogElement;
    successDialog.showModal();
    successDialog.querySelector<HTMLParagraphElement>("._message")!.innerText = message;

    successDialog.querySelector<HTMLButtonElement>("._ok")!.addEventListener("click", (_) => {
        successDialog.close();
    });
}

async function loadPermissionValues() {
    const lang = getDocumentLangCode();
    const permissions = await userClient.getPermissionTable();
    if (!permissions) {
        console.error(
            "Failed to fetch permission values. If this keeps happening, ask you service maintainer for instructions."
        );
        return;
    }

    const container = document.getElementById("permissions-container") as HTMLElement;

    removeChildren(container, (x) => !x.classList.contains("column-titles"));

    addRole("Fuksi", "Freshman", permissions.roles.freshman);
    addRole("Tutor", "Tutor", permissions.roles.tutor);
    addRole("Admin", "Admin", permissions.roles.admin);
    addRole(
        "Sudo (Täytyy asettaa muualla)",
        "Sudo (Set on separate config)",
        permissions.roles.sudo
    );

    function addRole(keyFi: string, keyEn: string, value: number) {
        const freshman = appendTemplateElement<HTMLElement>("role-template", container);
        freshman.querySelector<HTMLElement>("._role")!.innerText = lang === LANG_EN ? keyEn : keyFi;
        freshman.querySelector<HTMLElement>("._value")!.innerText = value.toString();
    }
}

main();
