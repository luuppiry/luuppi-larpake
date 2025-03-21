import { User, Group, GroupMember } from "../models/user";
import {
    addMemberBtn,
    availableUsersContainer,
    availableUserTemplate,
    cancelChooseUserBtn,
    cancelEditUserBtn,
    container,
    deleteUserBtn,
    editOkBtn,
    editUserDialog,
    saveBtn,
    searchField,
    selectUserDialog,
    userTemplate,
} from "./page_extensions/group_manager_imports.ts";

const TUTOR = "Tutor";
const FRESHMAN = "Fuksi";
const FRESHMAN_CLASS = "fuksi";
const TUTOR_CLASS = "tutor";

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

    deleteUserBtn.addEventListener("click", (e) => {
        e.preventDefault();

        const userId = editUserDialog.querySelector<HTMLParagraphElement>("._id")?.id;
        if (userId == null) {
            throw new Error("User id to be deleted is null.");
        }
        removeUser(userId);
        editUserDialog.close();
    });

    searchField.addEventListener("input", (event) => {
        const value = (event.target as HTMLInputElement)?.value;
        updateSearchMatches(value);
    });

    cancelEditUserBtn.addEventListener("click", (_) => {
        editUserDialog.close();
    });

    editOkBtn.addEventListener("click", (_) => {
        changeEdited();
        editUserDialog.close();
    });

    saveBtn.addEventListener("click", (_) => {
        SaveGroupState();
    })
}

function startExistingGroup(groupId: number) {
    const data = fetchData(groupId);

    // Remove no data field
    if (data.members.length > 0) {
        hidNoUsersLabel();
    }

    // Add Group name
    const groupName = document.getElementById("group-name") as HTMLInputElement;
    groupName.value = data.name;

    // Add group number
    const groupNumber = document.getElementById("group-number") as HTMLInputElement;
    groupNumber.value = data.groupNumber?.toString() ?? "";

    // Add larpake id
    const larpakeId = document.getElementById("larpake-id") as HTMLInputElement;
    larpakeId.value = data.larpakeId?.toString() ?? "";

    data.members.sort(sortFunc).forEach(appendNewUser);
}

function appendNewUser(user: GroupMember) {
    // Add new member
    const fragment = document.importNode(userTemplate.content, true);
    container.appendChild(fragment);
    const node = container.children[container.children.length - 1];

    // Set username
    const userName = node.querySelector<HTMLHeadingElement>(".username")!;
    userName.innerText = user.user.entraId ?? "N/A";

    // Set firstname
    const firstName = node.querySelector<HTMLSpanElement>(".first-name")!;
    firstName.innerText = user.user.firstName ?? "N/A";

    // Set lastname
    const lastName = node.querySelector<HTMLSpanElement>(".last-name")!;
    lastName.innerText = user.user.lastName ?? "N/A";

    // Set user status in group
    const status = node.querySelector<HTMLBRElement>("._status")!;
    const isFuksi = user.isCompeting;
    status.classList.add(isFuksi ? FRESHMAN_CLASS : TUTOR_CLASS);
    status.innerText = isFuksi ? FRESHMAN : TUTOR;

    // Set id
    const idField = node.querySelector<HTMLParagraphElement>("._id")!;
    idField.id = user.user.id;

    node.addEventListener("click", (e) => editUser(e.target as HTMLElement));
}

function editUser(target: HTMLElement): void {
    const userId = target.querySelector<HTMLElement>("._id")?.id!;
    const userElem = getFirstMatchingMember(userId);
    if (userElem == null) {
        throw new Error("Selected user not found.");
    }

    editUserDialog.showModal();

    // Read data from group members container
    const status = userElem.querySelector<HTMLElement>("._status")?.innerText;
    const username = userElem.querySelector<HTMLElement>("._username")?.innerText;

    // Set status
    const statusField = editUserDialog.querySelector<HTMLSelectElement>("._status")!;
    statusField.value = status === TUTOR ? "false" : "true";

    // Set username
    const usernameField = editUserDialog.querySelector<HTMLElement>("._username")!;
    usernameField.innerText = username ?? "N/A";

    // Set id
    const idField = editUserDialog.querySelector<HTMLParagraphElement>("._id")!;
    idField.id = userId;
}

function changeEdited() {
    const userId = editUserDialog.querySelector<HTMLParagraphElement>("._id")?.id;
    const status = editUserDialog.querySelector<HTMLSelectElement>("._status")?.value;
    if (userId == null || userId == "") {
        throw new Error("User to be edited cannot be null");
    }
    const user = getFirstMatchingMember(userId);
    if (status != null) {
        const statusField = user?.querySelector<HTMLBRElement>("._status")!;

        const isFuksi = status === "true";
        statusField.classList.remove(!isFuksi ? FRESHMAN_CLASS : TUTOR_CLASS);
        statusField.classList.add(isFuksi ? FRESHMAN_CLASS : TUTOR_CLASS);
        statusField.innerText = isFuksi ? FRESHMAN : TUTOR;
    }
}

function removeUser(userId: string) {
    if (userId == null) {
        throw new Error("Trying to remove null user id");
    }

    const user = getFirstMatchingMember(userId);
    if (user != null) {
        container.removeChild(user);
    }
}

function getFirstMatchingMember(userId: string) {
    for (let i = 0; i < container.children.length; i++) {
        const current = container.children.item(i)!;
        const idField = current?.querySelector<HTMLParagraphElement>("._id");
        if (idField?.id == userId) {
            return current;
        }
    }
}

function sortFunc(first: GroupMember, second: GroupMember): number {
    if (first.isCompeting && !second.isCompeting) {
        return 1;
    }
    if (first.user.entraId == null) {
        return -1;
    }
    if (second.user.entraId == null) {
        return 1;
    }
    return first.user.entraId > second.user.entraId ? 1 : -1;
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
            contains(u.firstName, searchTerm) || contains(u.lastName, searchTerm) || contains(u.entraId, searchTerm)
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
    username.innerText = user.entraId ?? "N/A";

    const firstName = node.querySelector<HTMLParagraphElement>("._first-name")!;
    firstName.innerText = user.firstName ?? "N/A";

    const lastName = node.querySelector<HTMLParagraphElement>("._last-name")!;
    lastName.innerText = user.lastName ?? "N/A";

    node.id = user.id;
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

    const matchingUsers = availableUsers!.filter((x) => x.id === userId);
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

function readMemberData(){
    // Read data from user container
    // Remove duplicate users and use the one with least privilege
    throw new Error("Function not implemented.");
}


function SaveGroupState() {
    // Read common data
    // Read member data
    // Upload all to server

    throw new Error("Function not implemented.");
}





main();


