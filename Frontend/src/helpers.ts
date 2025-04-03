import { PermissionCollection } from "./models/user.js";

export const LANG_FI = "fi";
export const LANG_EN = "en";
export const LANG_ATTRIBUTE_NAME = "lang";

export default function mapChildren<T>(
    children: HTMLCollection,
    func: (elem: Element) => T | null
): T[] {
    /* Generic method to map children */
    const result: T[] = [];
    for (let i = 0; i < children.length; i++) {
        const current = children.item(i);
        if (current == null) {
            continue;
        }
        const mapped = func(current);
        if (mapped == null) {
            continue;
        }
        result.push(mapped!);
    }
    return result;
}

export function isEmpty(val: string | null | undefined) {
    return val == undefined || val == null || val == "";
}

export function ThrowIfNull<T>(value: T) {
    if (value == null || value == undefined) {
        throw new Error("Value cannot be null");
    }
}

export function ToDictionary<TKey, TValue>(
    values: TValue[],
    selector: (value: TValue) => TKey
): Map<TKey, TValue[]> {
    const result = new Map<TKey, TValue[]>();
    values.forEach((x) => {
        const key = selector(x);
        if (!result.get(key)) {
            result.set(key, []);
        }
        result.get(key)!.push(x);
    });
    return result;
}

export function ToOverwriteDictionary<TKey, TValue>(
    values: TValue[],
    selector: (value: TValue) => TKey
): Map<TKey, TValue> {
    const result = new Map<TKey, TValue>();
    values.forEach((x) => {
        const key = selector(x);
        result.set(key, x);
    });
    return result;
}

export function getInputNumericByDocId(fieldName: string) {
    return parseInt((document.getElementById(fieldName) as HTMLInputElement)?.value);
}

export function pushUrlState(manipulate: (params: URLSearchParams) => void) {
    const url = new URL(window.location.href);
    const params = new URLSearchParams(url.search);

    manipulate(params);

    url.search = params.toString();
    window.history.pushState({}, "", url);
}

export function formatDate(date: Date) {
    if (!date) {
        return "";
    }
    if (typeof date === "string") {
        date = new Date(date);
    }
    const year = date.getFullYear();
    const month = date.getMonth() + 1;
    const day = date.getDate();
    return `${day}.${month}.${year}`;
}

export function formatDateTime(date: Date) {
    const day = formatTime(date);
    const time = formatTime(date);
    return `${day} ${time}`;
}

export function formatTime(date: Date) {
    if (!date) {
        return "";
    }
    if (typeof date === "string") {
        date = new Date(date);
    }

    const hours = date.getHours().toString().padStart(2, "0");
    const minutes = date.getMinutes().toString().padStart(2, "0");
    return `${hours}:${minutes}`;
}

export function getDocumentLangCode() {
    // Default to finnish
    return document.documentElement.lang == LANG_EN ? LANG_EN : LANG_FI;
}

type LangObject = {
    languageCode: string;
}

export function getMatchingLangObject<T>(textData: LangObject[] | null): T | null {
    if (!textData) {
        return null;
    }
    const lang = getDocumentLangCode();

    const matching = textData.filter(x => x.languageCode === lang)[0]
    if (matching) {
        return matching as T;
    }
    const finnish = textData.filter(x => x.languageCode === LANG_FI)[0];
    if (finnish) {
        return finnish as T;
    }
    return textData[0] as T;
}

export function removeChildren(
    elem: HTMLElement,
    predicate: null | ((elem: Element) => boolean) = null
) {
    const children = [...elem.children];
    for (const child of children) {
        if (predicate == null || predicate(child)) {
            elem.removeChild(child);
        }
    }
}

export function throwIfAnyNull(elems: (HTMLElement | null)[]) {
    for (const elem of elems) {
        if (!elem) {
            throw new Error("Element cannot be null");
        }
    }
}

export function appendTemplateElement<TAppended>(id: string, container: HTMLElement) {
    const template = document.getElementById(id) as HTMLTemplateElement;
    const node = document.importNode(template.content, true);
    container.appendChild(node);
    return container.children[container.children.length - 1] as TAppended;
}

export function encodeArrayToQueryString(key: string, array: string[]): string {
    return array.map((x) => `${key}[]=${encodeURIComponent(x)}`).join("&");
}

export function getSearchParams() {
    return new URLSearchParams(new URL(window.location.href).search);
}

export function showOkDialog(
    id: string,
    afterClose: null | (() => void) = null
): HTMLDialogElement {
    const dialog = document.getElementById(id) as HTMLDialogElement;
    dialog.showModal();
    dialog.querySelector<HTMLButtonElement>("._ok")?.addEventListener("click", (_) => {
        dialog.close();
        if (afterClose) {
            afterClose();
        }
    });
    return dialog;
}

export function toRole(
    permissions: number,
    table: PermissionCollection | null,
    lang: string | null = null
): string {
    table ??= getDefaultPermissionsTable();
    lang ??= getDocumentLangCode();
    const isFinnish = lang !== LANG_EN;

    if (permissions >= table.roles.sudo) {
        return "Sudo";
    }
    if (permissions >= table.roles.admin) {
        return "Admin";
    }
    if (permissions >= table.roles.tutor) {
        return "Tutor";
    }
    if (permissions >= table.roles.freshman) {
        return isFinnish ? "Fuksi" : "Freshman";
    }
    return isFinnish ? "Ei roolia" : "No role";
}

export enum Role {
    Freshman = 6,
    Tutor = 766,
    Admin = 4194302,
    Sudo = 2147483647,
}

export function hasRole(
    permissions: number,
    role: Role,
    table: PermissionCollection | null = null
) {
    table ??= getDefaultPermissionsTable();
    if (role === Role.Sudo) {
        return permissions >= table.roles.sudo;
    }
    if (role === Role.Admin) {
        return permissions >= table.roles.admin;
    }
    if (role === Role.Tutor) {
        return permissions >= table.roles.tutor;
    }
    if (role === Role.Freshman) {
        return permissions >= table.roles.freshman;
    }
    throw new Error("Invalid role type");
}

function getDefaultPermissionsTable(): PermissionCollection {
    return {
        roles: {
            freshman: Role.Freshman as number,
            tutor: Role.Tutor as number,
            admin: Role.Admin as number,
            sudo: Role.Sudo as number,
        },
    };
}
