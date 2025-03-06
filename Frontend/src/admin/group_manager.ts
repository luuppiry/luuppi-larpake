type User = {
    userId: string;
    username: string | null;
    firstName: string | null;
    lastName: string | null;
    permissions: number;
};

type Member = {
    user: User;
    isHidden: boolean;
    isCompeting: boolean;
};

type Group = {
    id: number;
    larpakeId: number;
    name: string;
    groupNumber: number | null;
    members: Member[];
};

function fetchData(groupId: number): Group {
    console.log(`Fetching group ${groupId}.`);
    return {
        id: 1,
        larpakeId: 4,
        name: "Superfuksit",
        groupNumber: 12,
        members: [
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
        ],
    };
}

const container = document.getElementById("user-container") as HTMLUListElement;
if (container == null) {
    throw new Error("User container is null, check naming");
}

const userTemplate = document.getElementById("user-template") as HTMLTemplateElement;
if (userTemplate == null) {
    throw new Error("User template is null, check naming");
}

const addMemberBtn = document.getElementById("add-member-btn") as HTMLButtonElement;
if (addMemberBtn == null) {
    throw new Error("Add member button is null, check naming");
}

const params = new URLSearchParams(window.location.search);
const groupId: number = parseInt(params.get("groupId") ?? "");
if (!Number.isNaN(groupId)) {
    startExistingGroup(groupId);
}

function startExistingGroup(groupId: number) {
    const data = fetchData(groupId);
    // Remove no data fields
    if (data.members.length > 0) {
        for (let i = 0; i < container.children.length; i++) {
            const child = container.children.item(0);
            if (child != null) {
                container.removeChild(child);
            }
        }
    }

    // Add common data
    const groupName = document.getElementById("group-name") as HTMLInputElement;
    if (groupName) {
        groupName.value = data.name;
    }
    const groupNumber = document.getElementById("group-number") as HTMLInputElement;
    if (groupNumber) {
        groupNumber.value = data.groupNumber?.toString() ?? "";
    }
    const larpakeId = document.getElementById("larpake-id") as HTMLInputElement;
    if (larpakeId) {
        larpakeId.value = data.larpakeId?.toString() ?? "";
    }

    data.members.sort(sortFunc).forEach(addToUserContainer);
}

function addToUserContainer(user: Member) {
    const fragment = document.importNode(userTemplate.content, true);
    container.appendChild(fragment);
    const node = container.children[container.children.length - 1];

    const userName = node.querySelector<HTMLHeadingElement>(".username");
    if (userName) {
        userName.innerText = user.user.username ?? "N/A";
    }

    const firstName = node.querySelector<HTMLSpanElement>(".first-name");
    if (firstName) {
        firstName.innerText = user.user.firstName ?? "N/A";
    }

    const lastName = node.querySelector<HTMLSpanElement>(".last-name");
    if (lastName) {
        lastName.innerText = user.user.lastName ?? "N/A";
    }

    const status = node.querySelector<HTMLBRElement>(".status");
    if (status) {
        const isFuksi = user.isCompeting;
        status.classList.add(isFuksi ? "fuksi" : "tutor");
        status.innerText = isFuksi ? "Fuksi" : "Tutor";
    }
}

function sortFunc(first: Member, second: Member): number {
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
