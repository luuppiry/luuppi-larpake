const dialog = document.getElementById("signature-dialog") as HTMLDialogElement;
const showDialogBtn = document.getElementById("show-dialog-btn") as HTMLButtonElement;
const closeBtn = document.getElementById("close-dialog-btn") as HTMLButtonElement;
const submitBtn = document.getElementById("submit-dialog-btn") as HTMLButtonElement;

showDialogBtn.addEventListener("click", () => {
    dialog.showModal();
});

closeBtn.addEventListener("click", () => {
    dialog.close();
});

submitBtn.addEventListener("click", () => {
    console.log("submitted");
    dialog.close();
});
