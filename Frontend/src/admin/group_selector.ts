type Group = {
    id: number;
    groupNumber: number;
    title: string;
    larpakeId: number;
};

let data: Group[] = [
    {
        id: 1,
        groupNumber: 4,
        title: "Superfuksit",
        larpakeId: 1,
    },
    {
        id: 2,
        groupNumber: 11,
        title: "Apollo 11",
        larpakeId: 1,
    },
    {
        id: 3,
        groupNumber: 2,
        title: "G-fuksit",
        larpakeId: 2,
    },
];

const container = document.getElementById("groups-container") as HTMLUListElement;
if (container == null) {
    throw new Error("Group container is null, check naming.");
}

const groupTemplate = document.getElementById("group-template") as HTMLTemplateElement;
if (groupTemplate == null) {
    throw new Error("Group template is null, check naming");
}

const addBtnTemplate = document.getElementById("add-new-template") as HTMLTemplateElement;
if (addBtnTemplate == null) {
    throw new Error("Group template is null, check naming");
}

function main() {
    // Sort by LärpäkeId, then GroupNumber
    data.sort((first, second) => {
        if (first.larpakeId > second.larpakeId) {
            return 1;
        }
        if (first.larpakeId < second.larpakeId) {
            return -1;
        }
        if (first.groupNumber == second.groupNumber) {
            return 0;
        }
        return first.groupNumber < second.groupNumber ? -1 : 1;
    });

    if (data.length > 0) {
        // Remove any existing children
        let child = container.firstElementChild;
        while (child != null) {
            container.removeChild(child);
            child = container.firstElementChild;
        }
    }

    data.forEach(setData);
    appendAddNew();
}

function setData(group: Group) {
    const fragment = document.importNode(groupTemplate.content, true);
    container.appendChild(fragment);
    const node = container.children[container.children.length - 1];

    const link = node.querySelector(".link") as HTMLAnchorElement;
    if (link) {
        link.href = `group_manager.html?groupId=${group.id}`;
    }

    const groupField = node.querySelector("._group-title") as HTMLElement;
    if (groupField) {
        groupField.innerText = group.title;
    }

    const numberField = node.querySelector("._group-number") as HTMLElement;
    if (numberField) {
        const num = group.groupNumber.toString();
        numberField.innerText = `(${num})`;
    }

    const idField = node.querySelector("._group-id") as HTMLElement;
    if (idField) {
        const id = group.id.toString();
        idField.innerText = `Id: ${id}`;
    }

    const larpakeField = node.querySelector("._larpake") as HTMLElement;
    if (larpakeField) {
        const larpake = group.larpakeId.toString();
        larpakeField.innerText = `Lärpäke: ${larpake}`;
    }
}

function appendAddNew() {
    const fragment = document.importNode(addBtnTemplate.content, true);
    container.appendChild(fragment);
}

main();
