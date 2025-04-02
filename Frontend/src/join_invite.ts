import GroupClient from "./api_client/group_client.js";
import { parseValidInviteCodeToKey } from "./builders.js";
import { Q_INVITE_KEY } from "./constants.js";
import { getSearchParams, showOkDialog, throwIfAnyNull } from "./helpers.js";

const client = new GroupClient();

const nameField = document.getElementById("group-name") as HTMLParagraphElement;
const groupNumberField = document.getElementById("group-number") as HTMLSpanElement;
const joinBtn = document.getElementById("join-btn") as HTMLButtonElement;
throwIfAnyNull([nameField, groupNumberField, joinBtn]);

async function main() {
    const params = getSearchParams();
    const inviteCode = params.get(Q_INVITE_KEY);
    if (!inviteCode) {
        window.location.href = "join.html";
        return;
    }

    const validKey = parseValidInviteCodeToKey(inviteCode);



    if (!validKey) {
        handleMalformed();
        return;
    }

    const group = await client.getGroupJoinInformation(validKey);
    if (group === 404) {
        handleExpired();
        return;
    }
    if (!group) {
        handleMalformed();
        return;
    }

    // Add group information
    nameField.innerText = group.name.toString();
    groupNumberField.innerText = group.groupNumber.toString();

    // Handle join pressed
    joinBtn?.addEventListener("click", async (_) => {
        const joined = await client.join(validKey);
        switch (joined) {
            case true:
                window.location.href = "own_groups.html";
                return;
            case 404:
                handleExpired();
                return;

            default:
                handleMalformed();
                return;
        }
    });
}

function handleMalformed() {
    showOkDialog("link-expired-dialog", () => {
        window.location.href = "join.html";
    });
}

function handleExpired() {
    showOkDialog("join-failed-dialog", () => {
        window.location.href = "join.html";
    });
}

main();
