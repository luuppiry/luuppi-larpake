import { Container, GuidIdObject, RowsAffected } from "../models/common.js";
import { Group, PermissionCollection, Signature, SvgMetadata, User } from "../models/user.js";
import HttpClient from "./http_client.js";
import RequestEngine from "./request_engine.js";

const FETCH_CHUNK_SIZE = 100;

export class UserClient extends RequestEngine {
    constructor(client: HttpClient | null = null) {
        super(client ?? new HttpClient());
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

    async getSelf(): Promise<User | null> {
        const response = await this.client.get("api/users/me");
        if (!response.ok) {
            console.warn("Failed to fetch user information:", await response.json());
            return null;
        }
        return await response.json();
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

    async getOwnSignatures(): Promise<Signature[] | null> {
        const response = await this.client.get("api/signatures/own");
        if (!response.ok) {
            console.warn("Failed to fetch own signatures:", await response.json());
            return null;
        }
        const signatures: Container<Signature[]> = await response.json();
        return signatures.data;
    }

    async uploadSignature(signature: SvgMetadata): Promise<GuidIdObject | null> {
        return await this.post<GuidIdObject>({
            url: "api/signatures/own",
            body: signature,
            failMessage: "Failed to upload signature",
            isContainerType: false,
        });
    }

    async getPermissionTable(): Promise<PermissionCollection | null> {
        return await this.get<PermissionCollection>({
            url: "api/status/permissions",
            params: null,
            failMessage: "Failed to fetch permissions table",
            isContainerType: false,
        });
    }
    async getById(userId: string): Promise<User | null> {
        return await this.get<User>({
            url: `api/users/${userId}`,
            params: null,
            failMessage: `Failed to fetch user ${userId}`,
            isContainerType: false
        });
    }
}
