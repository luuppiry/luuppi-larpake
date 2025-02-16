export type User = {};

export type Group = {
    id: number;
    larpakeId: number;
    name: string;
    groupNumber: number;
    createdAt: Date;
    updatedAt: Date;
    members: string[];
};

export type Point2D = {
    x: number;
    y: number;
};

export type Signature = {
    id: string;
    ownerId: string;
    createdAt: Date;
    signature: {
        height: number;
        width: number;
        data: Point2D[][];
        lineWidth: number;
        strokeStyle: string;
        lineCap: string;
    };
};
