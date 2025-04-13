import { throwIfAnyNull } from "../../helpers.js";

export default class UserManagerUI {
    container: HTMLElement;
    nextPageBtn: HTMLButtonElement;
    prevPageBtn: HTMLButtonElement;
    fromIndexElem: HTMLSpanElement;
    toIndexElem: HTMLSpanElement;
    maxIndexElem: HTMLSpanElement;
    searchField: HTMLInputElement;
    constructor() {
        this.container = document.getElementById("user-container") as HTMLElement;
        this.prevPageBtn = document.getElementById("prev-btn") as HTMLButtonElement;
        this.nextPageBtn = document.getElementById("next-btn") as HTMLButtonElement;
        this.fromIndexElem = document.getElementById("from-count") as HTMLSpanElement;
        this.toIndexElem = document.getElementById("to-count") as HTMLSpanElement;
        this.maxIndexElem = document.getElementById("out-of") as HTMLSpanElement;
        this.searchField = document.getElementById("search-field") as HTMLInputElement;     
        
        
        throwIfAnyNull([
            this.container,
            this.prevPageBtn,
            this.nextPageBtn,
            this.maxIndexElem,
            this.toIndexElem,
            this.fromIndexElem,
            this.searchField,
        ]);
    }
}