import { parseInviteLink } from "./builders.js";
import { INVITE_KEY_LENGTH } from "./constants";

document.querySelectorAll("._char").forEach((x) => {
    const input = x as HTMLInputElement;
    if (!input) {
        return;
    }
    input.addEventListener("input", () => {
        const next = input.nextElementSibling as HTMLElement;
        if (next) {
            next.focus();
        }
    });
    input.addEventListener("keydown", (e) => {
        const invalid = ["Tab", "Control", "Shift"];
        if (invalid.includes(e.key)) {
            return;
        }
        input.value = "";
    });
});

document.querySelector<HTMLButtonElement>("._join")?.addEventListener("click", (_) => {
    let input = "";
    const first = document.getElementById("first-char-field") as HTMLInputElement;
    input += first.value;
    // While loop assignment is done on purpose
    let next: Element | null = first;
    while ((next = next.nextElementSibling)) {
        const elem = next as HTMLInputElement;
        if (!elem) {
            continue;
        }
        input += elem.value;
    }

    input = input.toUpperCase();
    console.log("Given code was:", input);

    const link = parseInviteLink(input);
    if (link) {
        window.location.href = link;
    }
    else {
        alert(`Koodin on oltava ${INVITE_KEY_LENGTH} merkkiä pitkä!`)
    }
});
