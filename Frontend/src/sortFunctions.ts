import { LarpakeTask, Section } from "./models/larpake.js";
import { Group, GroupMember, User } from "./models/user.js";

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

export function groupMemberSortFunc(first: GroupMember, second: GroupMember) {
    if (first.isCompeting && !second.isCompeting) {
        return 1;
    }

    if (!first.userId) {
        return -1;
    }
    if (!second.userId) {
        return 1;
    }
    return first.userId > second.userId ? 1 : -1;
}

export function groupSortFunc(first: Group, second: Group): number {
    if (first.larpakeId < second.larpakeId) {
        return -1;
    }
    if (first.larpakeId > second.larpakeId) {
        return 1;
    }
    if (!second.groupNumber) {
        return -1;
    }
    if (!first.groupNumber) {
        return 1;
    }
    if (first.groupNumber < second.groupNumber) {
        return -1;
    }
    if (first.groupNumber > second.groupNumber) {
        return 1;
    }
    return first.id < second.id ? -1 : 1;
}

export function userSortFunc(first: User, second: User): number {
    if (first.permissions !== second.permissions) {
        // Higher permissions first
        return second.permissions - first.permissions;
    }

    // Nulls names last
    if (!second.lastName) {
        return -1;
    }
    if (!first.lastName) {
        return 1;
    }
    if (first.lastName !== second.lastName) {
        return first.lastName < second.lastName ? -1 : 1;
    }
    if (!second.firstName) {
        return -1;
    }
    if (!first.firstName) {
        return 1;
    }
    if (first.firstName !== second.firstName) {
        return first.firstName < second.firstName ? -1 : 1;
    }
    return first.id < second.id ? -1 : 1;
}
