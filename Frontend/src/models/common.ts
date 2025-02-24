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

export type Point2D = {
    X: number;
    Y: number;
};