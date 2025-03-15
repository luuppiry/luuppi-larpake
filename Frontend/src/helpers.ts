import { Section } from "./models/larpake";

export const LANG_FI = "fi";
export const LANG_EN = "en";

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

export function SectionSortFunc(first: Section, second: Section): number {
    /* Sort by
     * - bigger ordering weight
     * - bigger id
     */

    if (first.orderingweightNumber > second.orderingweightNumber) {
        return -1;
    }
    if (second.orderingweightNumber < first.orderingweightNumber) {
        return 1;
    }
    return first.id > second.id ? -1 : 1;
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
