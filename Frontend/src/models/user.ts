import { Point2D } from "./common.ts";

// User, GroupMember and Group are used by group_manager.ts
export type User = {
    id: string;
    entraId: string | null;
    entraUsername: string | null;
    username: string | null;
    firstName: string | null;
    lastName: string | null;
    permissions: number;
    startYear: number | null;
};

export type GroupMember = {
    user: User;
    isHidden: boolean;
    isCompeting: boolean;
};

export type Group = {
    id: number;
    larpakeId: number;
    name: string;
    groupNumber: number | null;
    members: GroupMember[];
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


export type GroupDto = {
    id: number;
    larpakeId: number;
    name: string;
    groupNumber: number;
    //createdAt: Date;
    //updatedAt: Date;
    members: string[];
};







