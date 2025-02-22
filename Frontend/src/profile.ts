import Draw from "./services/signature_provider.ts";

const dialog = document.getElementById("signature-dialog") as HTMLDialogElement;
const showDialogBtn = document.getElementById("show-dialog-btn") as HTMLButtonElement;
const closeBtn = document.getElementById("close-dialog-btn") as HTMLButtonElement;
const submitBtn = document.getElementById("submit-dialog-btn") as HTMLButtonElement;
const signatureCanvas = document.getElementById("signature-canvas") as HTMLCanvasElement;

let signatureProvider = new Draw(signatureCanvas);

showDialogBtn.addEventListener("click", () => {
    dialog.showModal();
});

closeBtn.addEventListener("click", () => {
    signatureProvider.refresh();
    dialog.close();
});

submitBtn.addEventListener("click", () => {
    const output = signatureProvider.get_flattened();
    console.log("submitted signature: ", output);
    signatureProvider.refresh();
    dialog.close();
});
