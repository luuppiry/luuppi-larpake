import { Container, IdObject, RowsAffected } from "../models/common.ts";
import { Group, GroupMember, PermissionData, Signature, User } from "../models/user.ts";
import HttpClient from "./http_client.ts";

const FETCH_CHUNK_SIZE = 100;

type NonCompetingGroupMember = {
    userId: string;
    isHidden: boolean;
};

type MemberIds = {
    members: string[];
};

export class UserClient {
    client: HttpClient;

    constructor() {
        this.client = new HttpClient();
    }

    async getAllUnpaged(): Promise<User[] | null> {
        const result: User[] = [];

        const query = new URLSearchParams();

        // query.append("StartedBefore", <date>)
        // query.append("StartedAfter", <date>)
        // query.append("Permissions", <number>)
        // query.append("IsORQuery", <boolean>)
        // query.append("UserIds", <id array>)
        // query.append("EntraIds", <id array>)
        // query.append("EntraUsername", <guid>)
        query.append("PageSize", FETCH_CHUNK_SIZE.toString());

        while (true) {
            let offset = 0;
            query.set("PageOffset", offset.toString());
            const response = await this.client.get("api/users", query);
            if (!response.ok) {
                console.warn("Failed to fetch users from offset", offset, await response.json());
                return null;
            }

            const users: Container<User[]> = await response.json();

            result.push(...users.data);

            offset = users.nextPage;
            if (!offset || offset <= 0) {
                return result;
            }
        }
    }

    async updateUser(userId: string, startYear: null | number): Promise<boolean> {
        const response = await this.client.put(`api/users/${userId}`, {
            startYear: startYear,
        });

        if (!response.ok) {
            console.warn(`Failed to update user ${userId} start year.`, await response.json());
            return false;
        }
        const result: RowsAffected = await response.json();
        if (result.rowsAffected <= 0) {
            console.warn(`User ${userId} not found`);
            return false;
        }
        return true;
    }

    async setPermissions(userId: string, permissions: number) {
        const response = await this.client.put(`api/users/${userId}/permissions`, {
            permissions: permissions,
        });

        if (!response.ok) {
            console.warn(`Failed to update user ${userId} permissions.`, await response.json());
            return false;
        }
        const result: RowsAffected = await response.json();
        if (result.rowsAffected <= 0) {
            console.warn(`User ${userId} not found`);
            return false;
        }
        return true;
    }

    async getOwnGroups(): Promise<Group[] | null> {
        const query = new URLSearchParams();
        query.append("doMinimize", "false");

        const response = await this.client.get("api/groups/own", query);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        const groups: Container<Group[]> = await response.json();
        return groups.data;
    }

    async getSignature(id: string): Promise<Signature | null> {
        if (id == null) {
            throw new Error("Signature id cannot be null.");
        }

        const response = await this.client.get(`api/signatures/${id}`);
        if (!response.ok) {
            console.warn(response);
            return null;
        }
        return await response.json();
    }

    async getPermissionMetadata(): Promise<PermissionData | null> {
        const response = await this.client.get("api/status/permissions");
        if (!response.ok) {
            console.warn("Failed to fetch permission metadata", await response.json());
            return null;
        }
        return await response.json();
    }

    async getGroups(minimize: boolean = true): Promise<Group[] | null> {
        const query = new URLSearchParams();
        query.append("DoMinimize", minimize ? "true" : "false");
        query.append("IncludeHiddenMembers", "true");

        // query.append("GroupName", <string>)
        // query.append("ContainsUser", <string>)
        // query.append("StartYear", <number>)
        // query.append("LarpakeId", <number>)
        // query.append("IsCompeting", <boolean>)
        query.append("PageSize", "100")
        // query.append("PageOffset", <number>)

        const response = await this.client.get("api/groups", query);
        if (!response.ok) {
            console.warn("Failed to fetch groups");
            return null;
        }

        const groups: Container<Group[]> = await response.json();
        return groups.data;
    }

    async getGroupById(groupId: number): Promise<Group | null> {
        /* Group is 'minimized' */
        const response = await this.client.get(`api/groups/${groupId}`);
        if (!response.ok) {
            console.warn(`Failed to fetch group ${groupId}`, await response.json());
            return null;
        }
        return await response.json();
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
                console.warn("Failed to upload competitor group members", await competitorResponse.json());
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
            const tutorResponse = await this.client.post(`api/groups/${groupId}/members/non-competing`, {
                members: tutors,
            });
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
