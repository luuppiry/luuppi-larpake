import { UserClient } from "../api_client/user_client";
import { Q_GROUP_ID } from "../constants";
import { appendTemplateElement, removeChildren } from "../helpers";
import { Group } from "../models/user";

const userClient = new UserClient();

async function main() {
    const container = document.getElementById("groups-container") as HTMLUListElement;
    if (container == null) {
        throw new Error("Group container is null, check naming.");
    }

    const groups = await userClient.getGroups();
    if (!groups) {
        throw new Error("Failed to fetch groups");
    }

    if (groups.length > 0) {
        // Remove any existing children
        removeChildren(container, (x) => !x.classList.contains("_no-remove"));
    }

    groups.sort(groupSortFunc).forEach((x) => setData(x, container));
}

function setData(group: Group, container: HTMLElement) {
    const elem = appendTemplateElement<Element>("group-template", container);

    elem.querySelector<HTMLAnchorElement>(".link")!.href = `group_manager.html?${Q_GROUP_ID}=${group.id}`;
    elem.querySelector<HTMLElement>("._group-name")!.innerText = group.name;
    elem.querySelector<HTMLElement>("._group-number")!.innerText = `(${group.groupNumber})`;
    elem.querySelector<HTMLElement>("._group-id")!.innerText = `Id: ${group.id}`;
    elem.querySelector<HTMLElement>("._larpake")!.innerText = `Lärpäke: ${group.larpakeId}`;
}

function groupSortFunc(first: Group, second: Group): number {
    if (first.larpakeId < second.larpakeId) {
        return -1;
    }
    if (first.larpakeId > second.larpakeId) {
        return 1;
    }
    if (!second.groupNumber) {
        return -1;
    }
    if (!first.groupNumber) {
        return 1;
    }
    if (first.groupNumber < second.groupNumber) {
        return -1;
    }
    if (first.groupNumber > second.groupNumber) {
        return 1;
    }
    return first.id < second.id ? -1 : 1;
}

main();
