export default function mapChildren<T>(children: HTMLCollection, func: (elem: Element) => T | null): T[]{
    /* Generic method to map children */
    const result: T[] = []; 
    for (let i = 0; i < children.length; i++){
        const current = children.item(i);
        if (current == null){
            continue;
        }
        const mapped = func(current);
        if (mapped == null){
            continue;
        }
        result.push(mapped!);
    }
    return result;
}

export function isEmpty(val: string | null | undefined) {
    return val == undefined || val == null || val == "";
}
