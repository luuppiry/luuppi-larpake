export function addSingleCharacterInputBehaviour(elem: Element) {
    const input = elem as HTMLInputElement;
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
}

export function parseFromInputRow(first: HTMLInputElement): string {
    let input = "";
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
    return input;
}
