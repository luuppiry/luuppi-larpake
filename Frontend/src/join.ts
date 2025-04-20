import { parseInviteLink } from "./builders.js";
import { INVITE_KEY_LENGTH } from "./constants.js";
import { addSingleCharacterInputBehaviour, parseFromInputRow } from "./services/input_service.js";

document.querySelectorAll("._char").forEach(addSingleCharacterInputBehaviour);

document.querySelector<HTMLButtonElement>("._join")?.addEventListener("click", (_) => {
    const first = document.getElementById("first-char-field") as HTMLInputElement;
    const input = parseFromInputRow(first);
    console.log("Given code was:", input);

    const link = parseInviteLink(input);
    if (link) {
        window.location.href = link;
    } else {
        alert(`Koodin on oltava ${INVITE_KEY_LENGTH} merkkiä pitkä!`);
    }
});
