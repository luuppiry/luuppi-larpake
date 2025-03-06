import { User, Group, GroupMember } from "../models/user";
import {
    addMemberBtn,
    availableUsersContainer,
    availableUserTemplate,
    cancelChooseUserBtn,
    cancelEditUserBtn,
    container,
    editUserDialog,
    searchField,
    selectUserDialog,
    userTemplate,
} from "./page_extensions/group_manager_imports.ts";

// Section DATA
const availableMembers = [
    {
        user: {
            userId: "guid-1",
            username: "user123",
            firstName: "Jaakko",
            lastName: "User",
            permissions: 987132,
        },
        isHidden: false,
        isCompeting: false,
    },
    {
        user: {
            userId: "guid-2",
            username: "jorma987",
            firstName: "Jorma",
            lastName: "Jurnukka",
            permissions: 546,
        },
        isHidden: false,
        isCompeting: true,
    },
    {
        user: {
            userId: "guid-3",
            username: "third",
            firstName: "Veeti",
            lastName: "Koivunen",
            permissions: 106,
        },
        isHidden: false,
        isCompeting: true,
    },
];

let availableUsers: User[] | null = null;

function fetchData(groupId: number): Group {
    console.log(`Fetching group ${groupId}.`);
    return {
        id: 1,
        larpakeId: 4,
        name: "Superfuksit",
        groupNumber: 12,
        members: availableMembers,
    };
}

// Section LOGIC

function fetchAllUsers() {
    return availableMembers.map((x) => x.user);
}

function fetchUser(userId: string): GroupMember | null {
    const matching = availableMembers.filter((x) => x.user.userId == userId);
    return matching.length > 0 ? matching[0] : null;
}

function main() {
    const params = new URLSearchParams(window.location.search);
    const groupId: number = parseInt(params.get("groupId") ?? "");

    if (!Number.isNaN(groupId)) {
        startExistingGroup(groupId);
    }

    addMemberBtn.addEventListener("click", (_) => {
        selectUserDialog.showModal();
        updateSearchMatches(null);
    });

    cancelChooseUserBtn.addEventListener("click", (e) => {
        e.preventDefault();
        selectUserDialog.close();
    });
    
    cancelEditUserBtn.addEventListener("click", (e) => {
        e.preventDefault();
        editUserDialog.close();
    });

    searchField.addEventListener("input", (event) => {
        const value = (event.target as HTMLInputElement)?.value;
        updateSearchMatches(value);
    });

    cancelEditUserBtn.addEventListener("click", (_) => {
        editUserDialog.close();
    });
}

function startExistingGroup(groupId: number) {
    const data = fetchData(groupId);

    // Remove no data field
    if (data.members.length > 0) {
        hidNoUsersLabel();
    }

    // Add Group name
    const groupName = document.getElementById("group-name") as HTMLInputElement;
    if (groupName) {
        groupName.value = data.name;
    }

    // Add group number
    const groupNumber = document.getElementById("group-number") as HTMLInputElement;
    if (groupNumber) {
        groupNumber.value = data.groupNumber?.toString() ?? "";
    }

    // Add larpake id
    const larpakeId = document.getElementById("larpake-id") as HTMLInputElement;
    if (larpakeId) {
        larpakeId.value = data.larpakeId?.toString() ?? "";
    }

    data.members.sort(sortFunc).forEach(appendNewUser);
}

function appendNewUser(user: GroupMember) {
    // Add new member
    const fragment = document.importNode(userTemplate.content, true);
    container.appendChild(fragment);
    const node = container.children[container.children.length - 1];

    // Set username
    const userName = node.querySelector<HTMLHeadingElement>(".username");
    if (userName) {
        userName.innerText = user.user.username ?? "N/A";
    }

    // Set firstname
    const firstName = node.querySelector<HTMLSpanElement>(".first-name");
    if (firstName) {
        firstName.innerText = user.user.firstName ?? "N/A";
    }

    // Set lastname
    const lastName = node.querySelector<HTMLSpanElement>(".last-name");
    if (lastName) {
        lastName.innerText = user.user.lastName ?? "N/A";
    }

    // Set user status in group
    const status = node.querySelector<HTMLBRElement>(".status");
    if (status) {
        const isFuksi = user.isCompeting;
        status.classList.add(isFuksi ? "fuksi" : "tutor");
        status.innerText = isFuksi ? "Fuksi" : "Tutor";
    }

    node.id = user.user.userId;
    node.addEventListener("click", (e) => editUser(e.target as HTMLElement));
}

function editUser(target: HTMLElement): void {
    const user = fetchUser(target.id);
    if (user == null) {
        throw new Error("Selected user not found.");
    }

    editUserDialog.showModal();
    const selector = editUserDialog.querySelector<HTMLSelectElement>("._status")!;
    selector.value = user.isCompeting ? "true" : "false";
}

function sortFunc(first: GroupMember, second: GroupMember): number {
    if (first.isCompeting && !second.isCompeting) {
        return 1;
    }
    if (first.user.username == null) {
        return -1;
    }
    if (second.user.username == null) {
        return 1;
    }
    return first.user.username > second.user.username ? 1 : -1;
}

function hidNoUsersLabel() {
    const label = document.getElementById("no-members-label");
    if (label == null) {
        throw new Error("No members label is null, check naming");
    }
    if (!label.classList.contains("hidden")) {
        label.classList.add("hidden");
    }
}

function updateSearchMatches(searchTerm: string | null) {
    // Empty container
    while (availableUsersContainer.children.length > 0) {
        const target = availableUsersContainer.children[0];
        availableUsersContainer.removeChild(target);
    }
    if (availableUsers == null) {
        availableUsers = fetchAllUsers();
    }

    // No search, show all
    if (searchTerm == null || searchTerm == "") {
        availableUsers.forEach((x) => appendSearchListUser(x));
        return;
    }

    const predicate = (u: User) => {
        return (
            contains(u.firstName, searchTerm) || contains(u.lastName, searchTerm) || contains(u.username, searchTerm)
        );
    };

    availableUsers.filter(predicate).map(appendSearchListUser);
}

function appendSearchListUser(user: User) {
    if (availableUsersContainer.children.length > 20) {
        return;
    }

    // Add new member
    const fragment = document.importNode(availableUserTemplate.content, true);
    availableUsersContainer.appendChild(fragment);
    const node = availableUsersContainer.children[availableUsersContainer.children.length - 1] as HTMLElement;

    const username = node.querySelector<HTMLParagraphElement>("._username")!;
    username.innerText = user.username ?? "N/A";

    const firstName = node.querySelector<HTMLParagraphElement>("._first-name")!;
    firstName.innerText = user.firstName ?? "N/A";

    const lastName = node.querySelector<HTMLParagraphElement>("._last-name")!;
    lastName.innerText = user.lastName ?? "N/A";

    node.id = user.userId;
    node.addEventListener("click", (e) => selectUser(e));
}

function selectUser(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const userId = target.id;
    if (userId == null) {
        throw new Error("Selected target does not containe user id.");
    }
    if (userId == "") {
        return;
    }

    if (availableUsers == null) {
        availableUsers = fetchAllUsers();
    }

    const matchingUsers = availableUsers!.filter((x) => x.userId === userId);
    const user: User | null = matchingUsers.length > 0 ? matchingUsers[0] : null;
    if (user == null) {
        throw new Error(`Selected user ${userId} does not exist.`);
    }

    hidNoUsersLabel();
    appendNewUser({
        user: user,
        isHidden: false,
        isCompeting: true,
    });

    selectUserDialog.close();
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

main();
