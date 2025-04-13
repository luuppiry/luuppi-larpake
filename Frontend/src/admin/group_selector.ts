import GroupClient from "../api_client/group_client.js";
import { Q_GROUP_ID, Q_PAGE_OFFSET, Q_PAGE_SIZE, Q_SEARCH } from "../constants.js";
import {
    appendTemplateElement,
    getSearchParams,
    isEmpty,
    pushUrlState,
    removeChildren,
    throwIfAnyNull,
} from "../helpers.js";
import { Group } from "../models/user.js";
import { groupSortFunc } from "../sortFunctions.js";

const PAGE_SIZE = 15;
const DEBOUNCH_TIMEOUT = 500;

const groupClient = new GroupClient();

class GroupSelector {
    prevBtn: HTMLButtonElement;
    nextBtn: HTMLButtonElement;
    container: HTMLUListElement;
    searchField: HTMLInputElement;
    offset: number;
    pageSize: number;
    search: string | null;
    isLastPage: boolean = true;
    debounchTimerId: NodeJS.Timeout | null = null;

    constructor(offset: number, size: number, search: string | null) {
        this.prevBtn = document.getElementById("prev-btn") as HTMLButtonElement;
        this.nextBtn = document.getElementById("next-btn") as HTMLButtonElement;
        this.container = document.getElementById("groups-container") as HTMLUListElement;
        this.searchField = document.getElementById("search") as HTMLInputElement;
        throwIfAnyNull([this.prevBtn, this.nextBtn, this.container]);

        this.offset = offset;
        this.pageSize = size;
        this.search = search;

        this.nextBtn.addEventListener("click", (_) => {
            this.next();
        });

        this.prevBtn.addEventListener("click", (_) => {
            this.previous();
        });

        this.searchField.addEventListener("input", (_) => {
            clearTimeout(this.debounchTimerId ?? undefined);

            this.debounchTimerId = setTimeout(() => {
                this.search = this.searchField.value;
                this.render();
            }, DEBOUNCH_TIMEOUT);
        });
    }

    async next() {
        if (this.isLastPage) {
            return;
        }

        this.offset = this.offset + this.pageSize;
        this.render();
    }

    async previous() {
        this.offset = this.offset - this.pageSize;
        this.render();
    }

    async render() {
        const groups = await groupClient.getGroupsPaged(
            false,
            isEmpty(this.search) ? null : this.search,
            this.pageSize,
            this.offset,
            true
        );
        if (!groups) {
            throw new Error("Failed to fetch groups");
        }

        if (groups.data.length > 0) {
            // Remove any existing children
            removeChildren(this.container, (x) => !x.classList.contains("_no-remove"));
        }

        removeChildren(this.container, (x) => !x.classList.contains("_no-remove"));
        groups.data.sort(groupSortFunc).forEach((x) => this.#setData(x, this.container));

        this.isLastPage = groups.nextPage < 0;
        this.prevBtn.disabled = !(this.offset > 0);
        this.nextBtn.disabled = this.isLastPage;

        pushUrlState((x) => {
            x.set(Q_PAGE_OFFSET, this.offset.toString());
            x.set(Q_PAGE_SIZE, this.pageSize.toString());
            if (this.search) {
                x.set(Q_SEARCH, this.search);
            } else {
                x.delete(Q_SEARCH);
            }
        });
    }

    #setData(group: Group, container: HTMLElement) {
        const elem = appendTemplateElement<Element>("group-template", container);

        elem.querySelector<HTMLAnchorElement>(".link")!.href = `group_manager.html?${Q_GROUP_ID}=${group.id}`;
        elem.querySelector<HTMLElement>("._group-name")!.innerText = group.name;
        elem.querySelector<HTMLElement>("._group-number")!.innerText = `(${group.groupNumber})`;
        elem.querySelector<HTMLElement>("._group-id")!.innerText = `Id: ${group.id}`;
        elem.querySelector<HTMLElement>("._larpake")!.innerText = `Lärpäke: ${group.larpakeId}`;
    }
}

async function main() {
    const params = getSearchParams();
    const offset = parseInt(params.get(Q_PAGE_OFFSET) ?? "0");
    const size = parseInt(params.get(Q_PAGE_SIZE) ?? PAGE_SIZE.toString());
    const search = params.get(Q_SEARCH);

    const selector = new GroupSelector(offset, size, search);
    selector.render();
}



main();
