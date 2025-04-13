import { appendSearchArray } from "../helpers.js";
import { Container, IdObject } from "../models/common.js";
import { Group, GroupInfo, GroupInvite, GroupMember, GroupMemberCollection } from "../models/user.js";
import HttpClient from "./http_client.js";
import RequestEngine from "./request_engine.js";

const MIN_SEARCH_LENGTH = 3;
const MAX_SEARCH_LENGTH = 30;

type NonCompetingGroupMember = {
    userId: string;
    isHidden: boolean;
};

type MemberIds = {
    members: string[];
};

export default class GroupClient extends RequestEngine {
    
    constructor(client: HttpClient | null = null) {
        super(client );
    }



    async getGroupJoinInformation(key: string): Promise<GroupInfo | 404 | null> {
        const response = await this.client.get(`api/groups/${key}/join`);
        if (response.status === 404) {
            console.warn("Group not found, link might be expired.");
            return 404;
        }
        if (!response.ok) {
            console.warn(`Failed to fetch group ${key}:`, await response.json());
            return null;
        }
        const group: GroupInfo = await response.json();
        return group;
    }

    async join(key: string): Promise<boolean | 404> {
        const response = await this.client.post(`api/groups/${key}/join`);
        if (response.status === 404) {
            return 404;
        }
        if (!response.ok) {
            console.warn(`Failed to join group '${key}'`, await response.json());
            return false;
        }
        return true;
    }

    async getOwnGroups(minimize: boolean): Promise<Group[] | null> {
        const params = new URLSearchParams();
        params.set("DoMinimize", minimize ? "true" : "false");

        return await this.get<Group[]>({
            url: "api/groups/own",
            params: params,
            failMessage: "Failed to fetch own groups:",
            isContainerType: true,
        });
    }

    async getGroupsPaged(
        minimize: boolean = true,
        search: string | null = null,
        pageSize: number = 20,
        pageOffset: number = 0,
        isOrQuery: boolean = true
    ): Promise<Container<Group[]> | null> {
        const query = new URLSearchParams();
        query.append("DoMinimize", minimize ? "true" : "false");
        query.append("IncludeHiddenMembers", "true");
        query.append("PageSize", pageSize.toString());
        query.append("PageOffset", pageOffset.toString());
        // query.append("ContainsUser", <string>)
        // query.append("IncludeHiddenMembers", <boolean>)
        // query.append("IsSearchMemberCompeting", <boolean>)

        if (search && search.length >= MIN_SEARCH_LENGTH && search.length <= MAX_SEARCH_LENGTH) {
            query.append("GroupName", search);
        }
        if (search) {
            query.append("IsORQuery", isOrQuery ? "true" : "false");
            const numeric = parseInt(search);
            if (!Number.isNaN(numeric)) {
                query.append("StartYear", numeric.toString());
                query.append("LarpakeId", numeric.toString());
                query.append("GroupNumber", numeric.toString());
            }
        }

        return await this.get<Container<Group[]>>({
            url: "api/groups",
            params: query,
            failMessage: "Failed to fetch groups",
            isContainerType: false,
        });
    }

    async getGroups(
        minimize: boolean = true,
        search: string | null = null,
        pageSize: number = 20,
        pageOffset: number = 0,
        isOrQuery: boolean = true
    ): Promise<Group[] | null> {
        const paged = await this.getGroupsPaged(minimize, search, pageSize, pageOffset, isOrQuery);
        if (paged) {
            return paged.data;
        }
        return null;
    }

    async getGroupById(groupId: number): Promise<Group | null> {
        /* Group is 'minimized' */
        return await this.get<Group>({
            url: `api/groups/${groupId}`,
            params: null,
            failMessage: `Failed to fetch group ${groupId}`,
            isContainerType: false,
        });
    }

    async getGroupMembers(groupId: number): Promise<null | string[]> {
        const response = await this.client.get(`api/groups/${groupId}/members`);
        if (!response.ok) {
            console.warn(`Failed to fetch group ${groupId} members`, await response.json());
            return null;
        }
        const ids: MemberIds = await response.json();
        return ids.members;
    }

    async uploadGroup(group: Group): Promise<number | null> {
        if (group.id <= 0) {
            return this.#createGroup(group);
        }

        const response = await this.#updateGroup(group);
        if (!response.ok && response.status === 404) {
            return this.#createGroup(group);
        }
        if (!response.ok) {
            console.warn(`Failed to update existing group ${group.id}`, await response.json());
            return null;
        }
        return group.id;
    }

    async getInviteKey(groupId: number): Promise<GroupInvite | null> {
        return await this.get<GroupInvite>({
            url: `api/groups/${groupId}/invite`,
            params: null,
            failMessage: "Failed to get current group invite key.",
            isContainerType: false,
        });
    }

    async refreshInviteLink(groupId: number): Promise<GroupInvite | null> {
        return await this.post<GroupInvite>({
            url: `api/groups/${groupId}/invite/refresh`,
            body: null,
            failMessage: "Failed to refresh group invite key to new one.",
            isContainerType: false,
        });
    }


    async getGroupMembersByGroupIds(groupIds: number[]) {
        const params = new URLSearchParams();
        appendSearchArray(params, "GroupIds", groupIds.map(x => x.toString()))
        
        return await this.get<GroupMemberCollection[]>({
            url: "api/groups/members",
            params: params,
            failMessage: "Failed to fetch group members from multiple groups",
            isContainerType: true
        })
    }

    async #updateGroup(group: Group): Promise<Response> {
        const response = await this.client.put(`api/groups/${group.id}`, group);
        if (!response.ok) {
            return response;
        }

        const userResponse = await this.#setGroupMembers(group.id, group.members);

        // Null means success (no error)
        return userResponse === null ? response : userResponse!;
    }

    async #createGroup(group: Group): Promise<number | null> {
        /* Null means no error in here, I know it is stupid, but get over it */
        const response = await this.client.post("api/groups", group);
        if (!response.ok) {
            console.warn("Failed to create group body.", await response.json());
            return null;
        }
        const idObject: IdObject = await response.json();
        const id = idObject.id;

        const userResult = await this.#setGroupMembers(id, group.members);
        if (userResult == null) {
            return id;
        }
        return null;
    }

    async #setGroupMembers(groupId: number, members: GroupMember[]): Promise<Response | null> {
        const ids = members.map((x) => x.userId);
        this.#removeNotIncludedMembers(groupId, ids);

        const competitors = members.filter((x) => x.isCompeting).map((x) => x.userId);
        if (competitors.length > 0) {
            const competitorResponse = await this.client.post(`api/groups/${groupId}/members`, {
                memberIds: competitors,
            });

            if (!competitorResponse.ok) {
                console.warn(
                    "Failed to upload competitor group members",
                    await competitorResponse.json()
                );
                return competitorResponse;
            }
        }

        const tutors: NonCompetingGroupMember[] = members
            .filter((x) => !x.isCompeting)
            .map((x) => {
                return {
                    userId: x.userId,
                    isHidden: x.isHidden,
                };
            });
        if (tutors.length > 0) {
            const tutorResponse = await this.client.post(
                `api/groups/${groupId}/members/non-competing`,
                {
                    members: tutors,
                }
            );
            if (!tutorResponse.ok) {
                console.warn("Failed to upload tutor group members", await tutorResponse.json());
                return tutorResponse;
            }
        }
        return null;
    }

    async #removeNotIncludedMembers(groupId: number, memberIds: string[]) {
        const oldMembersRequest = await this.getGroupMembers(groupId);
        if (oldMembersRequest) {
            const membersToBeRemoved = oldMembersRequest.filter((x) => !memberIds.includes(x));
            if (membersToBeRemoved.length <= 0) {
                return;
            }
            const deleteResponse = await this.client.delete(`api/groups/${groupId}/members`, {
                memberIds: membersToBeRemoved,
            });

            // Don't really care if this fails actually
            if (!deleteResponse.ok) {
                console.warn(
                    `Failed to remove members: `,
                    membersToBeRemoved,
                    " from group",
                    groupId,
                    "because of",
                    await deleteResponse.json()
                );
            }
        }
    }
}
