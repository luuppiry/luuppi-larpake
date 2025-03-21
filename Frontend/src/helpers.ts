import { LarpakeTask, Section } from "./models/larpake";

export const LANG_FI = "fi";
export const LANG_EN = "en";
export const LANG_ATTRIBUTE_NAME = "lang";

export default function mapChildren<T>(children: HTMLCollection, func: (elem: Element) => T | null): T[] {
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

export function ToDictionary<TKey, TValue>(values: TValue[], selector: (value: TValue) => TKey): Map<TKey, TValue[]> {
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

export function SectionSortFunc(first: Section, second: Section): number {
    /* Sort by
     * - bigger ordering weight
     * - bigger id
     */

    if (first.orderingWeightNumber > second.orderingWeightNumber) {
        return -1;
    }
    if (second.orderingWeightNumber < first.orderingWeightNumber) {
        return 1;
    }
    return first.id > second.id ? 1 : -1;
}

export function TaskSortFunc(first: LarpakeTask, second: LarpakeTask): number {
    /* Sort by
     * - bigger ordering weight
     * - bigger id
     */

    if (first.orderingWeightNumber > second.orderingWeightNumber) {
        return -1;
    }
    if (second.orderingWeightNumber < first.orderingWeightNumber) {
        return 1;
    }
    return first.id > second.id ? 1 : -1;
}

export function getInputNumericByDocId(fieldName: string) {
    return parseInt((document.getElementById(fieldName) as HTMLInputElement)?.value);
}

export function overwriteQueryParam(name: string, value: string) {
    const pieced = window.location.href.split("?");
    const url = pieced[0];

    const params = new URLSearchParams();
    params.append(name, value);

    // Change page url without reloading. Good for changes in query parameters
    window.history.pushState(
        {
            change: `update page url with query param  '${name}': '${value}'`,
        },
        "",
        `${url}?${params}`
    );
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

export function removeChildren(elem: HTMLElement) {
    const children = [...elem.children];
    for (const child of children) {
        elem.removeChild(child);
    }
}

export function throwIfAnyNull(elems: HTMLElement[]) {
    for (const elem of elems) {
        if (elem == null) {
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
    return array.map((x) => `${key}=${encodeURIComponent(x)}`).join("&");
}

export function getSearchParams(){
    return new URLSearchParams(new URL(window.location.href).search);
}