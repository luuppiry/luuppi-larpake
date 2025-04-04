import { Point2D } from "./common.js";

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
    isHidden: boolean;
    isCompeting: boolean;
    userId: string;
};

export type Group = {
    id: number;
    larpakeId: number;
    name: string;
    groupNumber: number | null;
    members: GroupMember[];
};

export type GroupInvite = {
    inviteKey: string;
    message: string;
}

export type PermissionCollection = {
    roles: {
        freshman: number;
        tutor: number;
        admin: number;
        sudo: number;
    };
};

export type SvgMetadata = {
    height: number;
    width: number;
    data: Point2D[][];
    lineWidth: number;
    strokeStyle: string;
    lineCap: string;
};

export type Signature = {
    id: string;
    ownerId: string;
    createdAt: Date;
    signature: SvgMetadata;
};

export type GroupDto = {
    id: number;
    larpakeId: number;
    name: string;
    groupNumber: number;
    createdAt: Date;
    updatedAt: Date;
    members: string[];
};

export type GroupInfo = {
    larpakeId: number;
    name: string;
    groupNumber: number;
};


export type GroupMemberCollection = {
    groupId: number;
    members: User[];
    tutors: User[];   
}