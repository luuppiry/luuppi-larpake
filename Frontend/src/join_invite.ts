import GroupClient from "./api_client/group_client.js";
import { INVITE_KEY_HEADER, INVITE_KEY_LENGTH, Q_INVITE_KEY } from "./constants.js";
import { getSearchParams, showOkDialog, throwIfAnyNull } from "./helpers.js";

const client = new GroupClient();

const nameField = document.getElementById("group-name") as HTMLParagraphElement;
const groupNumberField = document.getElementById("group-number") as HTMLSpanElement;
const joinBtn = document.getElementById("join-btn") as HTMLButtonElement;
throwIfAnyNull([nameField, groupNumberField, joinBtn]);

async function main() {
    const params = getSearchParams();
    const inviteKey = params.get(Q_INVITE_KEY);
    if (!inviteKey) {
        window.location.href = "join.html";
        return;
    }

    const key = inviteKey.slice(INVITE_KEY_HEADER.length, -1);
    const isKeyMalformed =
        !inviteKey?.startsWith(INVITE_KEY_HEADER) || key.length !== INVITE_KEY_LENGTH;

    if (isKeyMalformed) {
        handleMalformed();
        return;
    }

    const group = await client.getGroupJoinInformation(key);
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
        const joined = await client.join(key);
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
