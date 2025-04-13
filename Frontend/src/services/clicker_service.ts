/* This code is part of important functionality,
 * so do not change it if you really don't know everything
 * about it. It looks simple and such, but just... don't touch it.
 * Ok? :)
 */
const THRESHOLD = 10;

export default class ClickerService {
    count: number = 0;
    action: () => void;

    constructor(target: HTMLElement, action: () => void) {
        this.action = action;
        target.addEventListener("click", (_) => this.click());
    }

    click() {
        console.log("clicked button :)");
        this.count++;
        if (this.count == THRESHOLD) {
            this.action();
        }
    }
}
