/* Move html element imports here, because too much stuff in main file */
export const container = document.getElementById("user-container") as HTMLUListElement;
if (container == null) {
    throw new Error("User container is null, check naming");
}

export const userTemplate = document.getElementById("user-template") as HTMLTemplateElement;
if (userTemplate == null) {
    throw new Error("User template is null, check naming");
}

export const addMemberBtn = document.getElementById("add-member-btn") as HTMLButtonElement;
if (addMemberBtn == null) {
    throw new Error("Add member button is null, check naming");
}

export const selectUserDialog = document.getElementById("choose-user-dialog") as HTMLDialogElement;
if (selectUserDialog == null) {
    throw new Error("Choose user dialog is null, check naming");
}

export const availableUsersContainer = document.getElementById("available-users-container") as HTMLDialogElement;
if (availableUsersContainer == null) {
    throw new Error("Available users container is null, check naming");
}

export const cancelChooseUserBtn = document.getElementById("cancel-choose-user-btn") as HTMLElement;
if (cancelChooseUserBtn == null) {
    throw new Error("Cancel dialog btn is null, check naming");
}

export const availableUserTemplate = document.getElementById("available-user-template") as HTMLTemplateElement;
if (availableUserTemplate == null) {
    throw new Error("Search user template is null, check naming");
}

export const searchField = document.getElementById("user-search-term") as HTMLInputElement;
if (searchField == null) {
    throw new Error("Search field is null, check naming");
}

export const cancelEditUserBtn = document.getElementById("cancel-edit-btn") as HTMLElement;
if (cancelEditUserBtn == null){
    throw new Error("Cancel user edit is null, check naming");
}

export const editUserDialog = document.getElementById("edit-user-dialog") as HTMLDialogElement;
if (editUserDialog == null){
    throw new Error("Edit user dialog is null, check naming");
}

export const deleteUserBtn = document.getElementById("delete-user-btn") as HTMLButtonElement;
if (editUserDialog == null){
    throw new Error("Delete user dialog is null, check naming");
}

export const editOkBtn = document.getElementById("edit-ok-btn") as HTMLButtonElement;
if (editUserDialog == null){
    throw new Error("Edit ok button is null, check naming");
}

export const saveBtn = document.getElementById("save-btn") as HTMLButtonElement;
if (saveBtn == null){
    throw new Error("Save button is null, check naming");
}