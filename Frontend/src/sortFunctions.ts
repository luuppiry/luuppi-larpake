import { LarpakeTask, Section } from "./models/larpake";
import { Group, GroupMember } from "./models/user";

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

export function groupMemberSortFunc(first: GroupMember, second: GroupMember){
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