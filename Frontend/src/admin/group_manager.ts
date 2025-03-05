type User = {
    userId: string;
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

const params = new URLSearchParams(window.location.search);
const groupId: number = parseInt(params.get("groupId") ?? "");

if (Number.isNaN(groupId)) {
    startNewGroup();
} else {
    startExistingGroup(groupId);
}

function fetchData(groupId: number) {
    return;
}

function startNewGroup() {}

function startExistingGroup(groupId: number) {}
