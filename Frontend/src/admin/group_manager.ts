import GroupClient from "../api_client/group_client.js";
import { UserClient } from "../api_client/user_client.js";
import { parseInviteLink } from "../builders.js";
import { Q_GROUP_ID } from "../constants.js";
import {
    appendTemplateElement,
    isEmpty,
    pushUrlState,
    removeChildren,
    showOkDialog,
    ToOverwriteDictionary,
} from "../helpers.js";
import { User, Group, GroupMember } from "../models/user";
import { groupMemberSortFunc } from "../sortFunctions.js";
import GroupManagerUI from "./ui-model/group_manager_ui.js";

const TUTOR = "Tutor";
const FRESHMAN = "Fuksi";
const FRESHMAN_CLASS = "fuksi";
const TUTOR_CLASS = "tutor";

const MAX_SEARCH_RESULTS = 20;
const DEBOUNCH_TIMEOUT = 500;

// Section DATA
const groupClient = new GroupClient();
const userClient = new UserClient(groupClient.client);

class GroupManager extends GroupManagerUI {
    allUsers: Map<string, User>;
    group: Group;
    debounchTimerId: NodeJS.Timeout | null = null;
    groupInviteCode: string | null = null;

    inviteLink: string | null;
    uploadFunc: (group: Group) => Promise<boolean>;
    inviteRefreshFunc: (groupId: number) => Promise<string>;

    constructor(
        group: Group | null,
        users: User[],
        invitelink: string | null,
        uploadFunc: (group: Group) => Promise<boolean>,
        inviteRefreshFunc: (groupId: number) => Promise<string>
    ) {
        super();

        this.group = group ?? {
            id: -1,
            larpakeId: -1,
            name: "",
            groupNumber: null,
            members: [],
        };
        this.inviteLink = invitelink;
        this.allUsers = ToOverwriteDictionary(users, (x) => x.id);

        this.uploadFunc = uploadFunc;
        this.inviteRefreshFunc = inviteRefreshFunc;
    }

    render() {
        this.groupNameInput.value = this.group.name ?? "";
        this.groupNumberInput.value = this.group.groupNumber?.toString() ?? "";
        this.larpakeIdInput.value = this.group.larpakeId?.toString() ?? "";
        this.group.members?.sort(groupMemberSortFunc).forEach((x) => this.#appendNewUser(x));
        this.#addEventListeners();

        this.#renderInviteKey();
    }

    #appendNewUser(member: GroupMember) {
        const user = this.allUsers.get(member.userId);

        // Add new member
        const elem = appendTemplateElement<HTMLElement>("member-template", this.memberContainer);
        elem.querySelector<HTMLHeadingElement>("._username")!.innerText =
            user?.entraUsername ?? member.userId ?? "N/A";
        elem.querySelector<HTMLSpanElement>("._first-name")!.innerText = user?.firstName ?? "N/A";
        elem.querySelector<HTMLSpanElement>("._last-name")!.innerText = user?.lastName ?? "N/A";
        elem.querySelector<HTMLParagraphElement>("._id")!.innerText = member.userId;

        // Set user status in group
        const status = elem.querySelector<HTMLBRElement>("._status")!;
        const isFuksi = member.isCompeting;
        status.classList.add(isFuksi ? FRESHMAN_CLASS : TUTOR_CLASS);
        status.innerText = isFuksi ? FRESHMAN : TUTOR;

        elem.addEventListener("click", (e) => this.#editUser(e.target as HTMLElement));
    }

    #editUser(target: HTMLElement): void {
        const userId = target.querySelector<HTMLElement>("._id")?.innerText!;
        const userElem = this.#getFirstMatchingMember(userId);
        if (userElem == null) {
            throw new Error("Selected user not found.");
        }

        this.editUserDialog.showModal();

        // Read data from group members container
        const status = userElem.querySelector<HTMLElement>("._status")?.innerText;
        const username = userElem.querySelector<HTMLElement>("._username")?.innerText;

        this.editUserDialog.querySelector<HTMLSelectElement>("._status")!.value =
            status === TUTOR ? "false" : "true";
        this.editUserDialog.querySelector<HTMLElement>("._username")!.innerText = username ?? "N/A";
        this.editUserDialog.querySelector<HTMLParagraphElement>("._id")!.innerText = userId;
    }

    #changeEdited() {
        const userId = this.editUserDialog.querySelector<HTMLParagraphElement>("._id")?.innerText;
        const status = this.editUserDialog.querySelector<HTMLSelectElement>("._status")?.value;
        if (userId == null || userId == "") {
            throw new Error("User to be edited cannot be null");
        }
        const user = this.#getFirstMatchingMember(userId);
        if (status != null) {
            const statusField = user?.querySelector<HTMLBRElement>("._status")!;

            const isFuksi = status === "true";
            statusField.classList.remove(!isFuksi ? FRESHMAN_CLASS : TUTOR_CLASS);
            statusField.classList.add(isFuksi ? FRESHMAN_CLASS : TUTOR_CLASS);
            statusField.innerText = isFuksi ? FRESHMAN : TUTOR;
        }
    }

    removeUser(userId: string) {
        if (!userId) {
            throw new Error("Trying to remove null user id");
        }

        const user = this.#getFirstMatchingMember(userId);
        if (user) {
            this.memberContainer.removeChild(user);
        }
    }

    #updateSearchMatches(searchTerm: string | null) {
        removeChildren(this.availableUsersContainer);

        // No search, show all
        if (isEmpty(searchTerm)) {
            this.#getAllUsers()
                .slice(0, MAX_SEARCH_RESULTS)
                .forEach((x) => this.#appendSearchListUser(x));
            return;
        }

        const predicate = (u: User) =>
            contains(u.firstName, searchTerm) ||
            contains(u.lastName, searchTerm) ||
            contains(u.entraId, searchTerm) ||
            contains(u.entraUsername, searchTerm) ||
            contains(u.startYear?.toString() ?? "", searchTerm) ||
            contains(u.username, searchTerm) ||
            contains(u.id, searchTerm) ||
            contains(u.permissions.toString(), searchTerm);

        this.#getAllUsers()
            .filter(predicate)
            .slice(0, MAX_SEARCH_RESULTS)
            .map((x) => this.#appendSearchListUser(x));
    }

    #appendSearchListUser(user: User) {
        const elem = appendTemplateElement<HTMLElement>(
            "available-user-template",
            this.availableUsersContainer
        );

        elem.querySelector<HTMLParagraphElement>("._id")!.innerText = user.id;
        elem.querySelector<HTMLParagraphElement>("._email")!.innerText =
            user.entraUsername ?? "N/A";
        elem.querySelector<HTMLParagraphElement>("._username")!.innerText = user.username ?? "N/A";
        elem.querySelector<HTMLParagraphElement>("._first-name")!.innerText =
            user.firstName ?? "N/A";
        elem.querySelector<HTMLParagraphElement>("._last-name")!.innerText = user.lastName ?? "N/A";

        elem.addEventListener("click", (e) => this.selectUser(e));
    }

    selectUser(event: MouseEvent) {
        const target = event.target as HTMLElement;
        const userId = target.querySelector<HTMLElement>("._id")!.innerText;
        if (userId == null) {
            throw new Error("Selected target does not containe user id.");
        }
        if (userId == "") {
            return;
        }

        const matchingUsers = this.#getAllUsers().filter((x) => x.id === userId);
        const user: User | null = matchingUsers.length > 0 ? matchingUsers[0] : null;
        if (user == null) {
            throw new Error(`Selected user ${userId} does not exist.`);
        }

        this.#appendNewUser({
            userId: user.id,
            isHidden: false,
            isCompeting: true,
        });

        this.selectUserDialog.close();
    }

    async SaveGroupState() {
        // Read common data
        const groupName = this.groupNameInput.value;
        if (isEmpty(groupName)) {
            this.#showUploadErrorDialog("Group name cannot be empty (minlength 3).");
            return;
        }
        const larpakeId = parseInt(this.larpakeIdInput.value);
        if (Number.isNaN(larpakeId)) {
            this.#showUploadErrorDialog(`'${this.larpakeIdInput.value}' is not valid larpake id.`);
            return;
        }
        let groupNumber: number | null = parseInt(this.groupNumberInput.value);
        groupNumber = Number.isNaN(groupNumber) ? null : groupNumber;

        // Read member data
        const members: GroupMember[] = [];
        for (const child of this.memberContainer.children) {
            if (!child.classList.contains("_member")) {
                // This is probably not correct ui elemnt, so skip it
                continue;
            }
            const id = child.querySelector<HTMLParagraphElement>("._id")!.innerText;
            const status = child.querySelector<HTMLParagraphElement>("._status")!.innerText;
            if (!id) {
                throw new Error(`Parsed invalid user id '${id}'`);
            }

            members.push({
                isHidden: false,
                isCompeting: status !== TUTOR,
                userId: id,
            });
        }

        // Upload all to server
        const success = await this.uploadFunc({
            id: this.group.id,
            larpakeId: larpakeId,
            name: groupName,
            groupNumber: groupNumber,
            members: members,
        });
        if (success) {
            showOkDialog("success-dialog", () => {
                window.location.reload();
            });
        } else {
            this.#showUploadErrorDialog();
        }
    }

    #renderInviteKey() {
        this.inviteLink ??= "";
        this.inviteLinkField.value = this.inviteLink;
    }

    #addEventListeners() {
        this.addMemberBtn.addEventListener("click", (_) => {
            this.selectUserDialog.showModal();
            this.#updateSearchMatches(null);
        });

        this.cancelChooseUserBtn.addEventListener("click", (e) => {
            e.preventDefault();
            this.selectUserDialog.close();
        });

        this.cancelEditUserBtn.addEventListener("click", (e) => {
            e.preventDefault();
            this.editUserDialog.close();
        });

        this.deleteUserBtn.addEventListener("click", (e) => {
            e.preventDefault();

            const userId =
                this.editUserDialog.querySelector<HTMLParagraphElement>("._id")?.innerText;
            if (userId == null) {
                throw new Error("User id to be deleted is null.");
            }
            this.removeUser(userId);
            this.editUserDialog.close();
        });

        this.searchField.addEventListener("input", (event) => {
            clearTimeout(this.debounchTimerId ?? undefined);

            this.debounchTimerId = setTimeout(() => {
                const value = (event.target as HTMLInputElement)?.value;
                this.#updateSearchMatches(value);
            }, DEBOUNCH_TIMEOUT);
        });

        this.cancelEditUserBtn.addEventListener("click", (_) => {
            this.editUserDialog.close();
        });

        this.editOkBtn.addEventListener("click", (_) => {
            this.#changeEdited();
            this.editUserDialog.close();
        });

        this.saveBtn.addEventListener("click", async (_) => {
            await this.SaveGroupState();
        });

        this.copyInviteLinkBtn.addEventListener("click", (_) => {
            if (!this.inviteLink) {
                alert("Failed to retrieve invite link, try to refresh and try again.");
                return;
            }
            navigator.clipboard.writeText(this.inviteLink);
        });

        this.showInviteQrCodeBtn.addEventListener("click", (_) => {
            showOkDialog("qr-code-dialog");
        });

        this.refreshInviteLinkBtn.addEventListener("click", async (_) => {
            if (!this.group.id) {
                alert("Group id not defined! Have you ever saved the group?");
                return;
            }

            const key = await this.inviteRefreshFunc(this.group.id);
            this.inviteLink = parseInviteLink(key, this.group.id);
            this.#renderInviteKey();
        });
    }

    #getFirstMatchingMember(userId: string): Element | null {
        for (const child of this.memberContainer.children) {
            const idField = child?.querySelector<HTMLParagraphElement>("._id");
            if (idField?.innerText == userId) {
                return child;
            }
        }
        return null;
    }

    #getAllUsers() {
        return Array.from(this.allUsers.values());
    }

    #showUploadErrorDialog(message: string | null = null) {
        const dialog = document.getElementById("upload-error-dialog") as HTMLDialogElement;
        dialog.showModal();

        if (message) {
            dialog.querySelector<HTMLParagraphElement>("._msg")!.innerText = message;
        }

        dialog.querySelector<HTMLButtonElement>("._ok")?.addEventListener("click", (_) => {
            dialog.close();
        });
    }
}

async function main() {
    const params = new URLSearchParams(window.location.search);
    const groupId: number = parseInt(params.get(Q_GROUP_ID) ?? "");

    const allUsers = await userClient.getAllUnpaged();
    if (!allUsers) {
        throw new Error("Could not fetch all available users.");
    }

    let group = null;
    let inviteKey = null;
    if (!Number.isNaN(groupId)) {
        group = await groupClient.getGroupById(groupId);
        if (!group) {
            throw new Error(`Could not load group ${groupId} from the server.`);
        }

        // Get invite key
        const inviteResponse = await groupClient.getInviteKey(group.id);
        inviteKey = inviteResponse?.inviteKey ?? null;
        if (!inviteKey) {
            console.error("Failed to fetch invite key. You can try to refresh it to fix problem.");
        }
    }

    const inviteLink = parseInviteLink(inviteKey, group?.id);
    const manager = new GroupManager(group, allUsers, inviteLink, uploadGroup, refreshInviteLink);
    manager.render();
}

function contains(value: string | null, searchValue: string | null) {
    if (value == null) {
        return false;
    }
    if (searchValue == null || searchValue == "") {
        return true;
    }
    return value.toLowerCase().includes(searchValue.toLowerCase());
}

async function refreshInviteLink(groupId: number): Promise<string> {
    const code = await groupClient.refreshInviteLink(groupId);
    if (!code) {
        throw new Error("Failed to refresh group invite key.");
    }
    return code.inviteKey;
}

async function uploadGroup(group: Group): Promise<boolean> {
    if (group.larpakeId <= 0) {
        console.error("Group must belong to valid larpake id.");
        return false;
    }
    const id = await groupClient.uploadGroup(group);
    if (!id) {
        console.error("Failed to upload group into database, see errors (warnings) above");
        return false;
    }

    pushUrlState((params) => {
        params.set(Q_GROUP_ID, id.toString());
    });
    return true;
}

main();
