export type ApiAction = {
    description: string;
    method: string;
    href: string;
};

export type Container<T> = {
    data: T;
    details: string[] | null;
    actions: ApiAction[] | null;
    nextPage: number;
};

export type Point2D = {
    x: number;
    y: number;
};

export type IdObject = {
    id: number;
};

export type GuidIdObject = {
    id: string;
}

export type RowsAffected = {
    rowsAffected: number;
};
