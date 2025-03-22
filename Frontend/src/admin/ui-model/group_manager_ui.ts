import { throwIfAnyNull } from "../../helpers";

export default class GroupManagerUI {
    memberContainer: HTMLUListElement;
    addMemberBtn: HTMLButtonElement;
    selectUserDialog: HTMLDialogElement;
    availableUsersContainer: HTMLDialogElement;
    cancelChooseUserBtn: HTMLElement;
    searchField: HTMLInputElement;
    cancelEditUserBtn: HTMLElement;
    editUserDialog: HTMLDialogElement;
    deleteUserBtn: HTMLButtonElement;
    editOkBtn: HTMLButtonElement;
    saveBtn: HTMLButtonElement;
    groupNameInput: HTMLInputElement;
    groupNumberInput: HTMLInputElement;
    larpakeIdInput: HTMLInputElement;

    constructor() {
        this.memberContainer = document.getElementById("user-container") as HTMLUListElement;
        this.addMemberBtn = document.getElementById("add-member-btn") as HTMLButtonElement;
        this.selectUserDialog = document.getElementById("choose-user-dialog") as HTMLDialogElement;
        this.availableUsersContainer = document.getElementById("available-users-container") as HTMLDialogElement;
        this.cancelChooseUserBtn = document.getElementById("cancel-choose-user-btn") as HTMLElement;
        this.searchField = document.getElementById("user-search-term") as HTMLInputElement;
        this.cancelEditUserBtn = document.getElementById("cancel-edit-btn") as HTMLElement;
        this.editUserDialog = document.getElementById("edit-user-dialog") as HTMLDialogElement;
        this.deleteUserBtn = document.getElementById("delete-user-btn") as HTMLButtonElement;
        this.editOkBtn = document.getElementById("edit-ok-btn") as HTMLButtonElement;
        this.saveBtn = document.getElementById("save-btn") as HTMLButtonElement;
        this.groupNameInput = document.getElementById("group-name") as HTMLInputElement;
        this.groupNumberInput = document.getElementById("group-number") as HTMLInputElement;
        this.larpakeIdInput = document.getElementById("larpake-id") as HTMLInputElement;

        throwIfAnyNull([
            this.memberContainer,
            this.addMemberBtn,
            this.selectUserDialog,
            this.availableUsersContainer,
            this.cancelChooseUserBtn,
            this.searchField,
            this.cancelEditUserBtn,
            this.editUserDialog,
            this.deleteUserBtn,
            this.editOkBtn,
            this.saveBtn,
            this.groupNameInput,
            this.groupNumberInput,
            this.larpakeIdInput,
        ]);
    }
}
