import Draw from "./services/signature_provider.ts";
import { Point2D } from "./models/common.ts";
import SignatureRenderer from "./services/signature_renderer.ts";

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
    // Read new signature
    const output = signatureProvider.get_flattened();
    signatureProvider.refresh();
    dialog.close();

    // Validate signature is not empty
    if (output.length == 0) {
        console.log("No signature path data found.");
        return;
    }

    // upload new signature
    upLoadSignature(output);

    // Draw svg new signature to UI
    const svg = document.getElementById("signature-svg") as HTMLElement;
    const renderer = new SignatureRenderer(output);
    renderer.renderTo(svg);
});

function upLoadSignature(value: Point2D[][]) {
    console.log("uploading new signature...");
    console.log(value);
}
