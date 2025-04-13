export function addSingleCharacterInputBehaviour(elem: Element) {
    const input = elem as HTMLInputElement;
    if (!input) {
        return;
    }
    input.addEventListener("input", () => {
        next(input);
    });
    input.addEventListener("keydown", (e) => {
        if (e.key === "ArrowLeft") {
            previous(input);
        }
        if (e.key === "ArrowRight") {
            next(input);
        }
        if (isCharacter(e.key)) {
            input.value = "";
        }
    });
    input.addEventListener("paste", (e) => {
        e.preventDefault();
        const pasteData = e.clipboardData?.getData("text");
        if (!pasteData) {
            return;
        }

        // Paste clipboard data into each field
        let index = 0;
        let current = input;
        while (current) {
            const value = pasteData[index];
            if (!value) {
                return;
            }
            current.value = value;
            index++;
            current = current.nextElementSibling as HTMLInputElement;
        }
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

function isCharacter(input: string) {
    return /^[a-zA-Z0-9]$/.test(input);
}

function previous(elem: HTMLElement) {
    const last = elem.previousElementSibling as HTMLElement;
    if (last) {
        last.focus();
    }
}

function next(elem: HTMLElement) {
    const next = elem.nextElementSibling as HTMLElement;
    if (next) {
        next.focus();
    }
}
