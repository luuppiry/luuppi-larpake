import GroupClient from "./api_client/group_client.js";
import { appendTemplateElement, ToOverwriteDictionary } from "./helpers.js";
import { GroupMemberCollection } from "./models/user.js";

const groupClient = new GroupClient();

async function main() {
    const groupContainer = document.getElementById("group-container") as HTMLUListElement;

    const groups = await groupClient.getOwnGroups(false);
    if (!groups) {
        throw new Error("Failed to fecth own groups");
    }


    const ids = groups.map(x => x.id);
    const groupMembers = await groupClient.getGroupMembersByGroupIds(ids);
    if (!groupMembers){
        alert("Fetching members failed");
    }

    const lookup = ToOverwriteDictionary<number, GroupMemberCollection>(groupMembers!, x => x.groupId)



    for (const group of groups) {
        const elem = appendTemplateElement<HTMLElement>("group-template", groupContainer);

        elem.querySelector<HTMLElement>("._group-name")!.innerText = group.name;
        elem.querySelector<HTMLElement>("._group-number")!.innerText =
            group.groupNumber?.toString() ?? "N/A";


        const members = lookup.get(group.id);
        if (members){
            const freshmanContainer = elem.querySelector<HTMLElement>("._freshmen")!
            const tutorContainer = elem.querySelector<HTMLElement>("._tutors")!
            renderGroupMembers(members, tutorContainer, freshmanContainer);
        }
    }
}

function renderGroupMembers(members: GroupMemberCollection, tutors: HTMLElement, freshmen: HTMLElement){
    for (const tutor of members.tutors){
        const elem = appendTemplateElement<HTMLElement>("tutor-template", tutors)!

        elem.querySelector<HTMLSpanElement>("._first-name")!.innerText = tutor.firstName ?? "N/A"
        elem.querySelector<HTMLSpanElement>("._last-name")!.innerText = tutor.lastName ?? "N/A"
        elem.querySelector<HTMLSpanElement>("._username")!.innerText = tutor.username ?? "???"
        elem.querySelector<HTMLSpanElement>("._start-year")!.innerText = tutor.startYear?.toString() ?? "???"
    }
    for (const freshman of members.members){
        const elem = appendTemplateElement<HTMLElement>("freshman-template", freshmen)!
        elem.querySelector<HTMLSpanElement>("._first-name")!.innerText = freshman.firstName ?? "N/A"
        elem.querySelector<HTMLSpanElement>("._last-name")!.innerText = freshman.lastName ?? "N/A"
        elem.querySelector<HTMLSpanElement>("._username")!.innerText = freshman.username ?? "???"
    }
    
   
}

main();

 
