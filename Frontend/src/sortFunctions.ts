import { LarpakeTask, Section } from "./models/larpake";
import { GroupMember } from "./models/user";

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
    if (!first.user?.entraId) {
        return -1;
    }
    if (!second.user?.entraId) {
        return 1;
    }
    return first.user.entraId > second.user.entraId ? 1 : -1;
}