export type ApiAction = {
    description: string;
    method: string;
    href: string;
}

export type Container<T> = {
    data: T;
    details: string[] | null;
    actions: ApiAction[] | null;
    nextPage: number;
}
