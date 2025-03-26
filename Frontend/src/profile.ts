import Draw from "./services/signature_provider.js";
import { Point2D } from "./models/common.js";
import SignatureRenderer from "./services/signature_renderer.js";

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

    // upload new signature and update UI
    uploadSignature(output);
    appendToFirstEmpty(output);
});

function uploadSignature(value: Point2D[][]) {
    console.log("uploading new signature...");
    console.log(value);
}

function appendToFirstEmpty(signature: Point2D[][]) {
    // Draw svg new signature to UI
    const svgContainer = document.getElementById("default-signature") as HTMLLIElement;

    // Choose where to write new signature
    let svg: HTMLElement;
    if (svgContainer.classList.contains("signature-unused")) {
        // Use default svg container
        svgContainer.classList.remove("signature-unused");
        svg = svgContainer.firstElementChild as HTMLElement;
    } else {
        // svg container is already in use, clone new
        const destination = document.getElementById("signature-container") as HTMLElement;
        svg = cloneNewSignatureNode(svgContainer, destination);
    }
    
    // Render
    const renderer = new SignatureRenderer(signature);
    renderer.renderTo(svg);
}

function cloneNewSignatureNode(template: HTMLElement, destination: HTMLElement): HTMLElement {
    // Get template child (svg)
    const templateChild = template.firstElementChild as HTMLElement;

    // Clone root (div/li) and child (svg)
    const rootClone = template.cloneNode(false);
    const childClone = templateChild.cloneNode(false) as HTMLElement;

    // Append to destination structure
    rootClone.appendChild(childClone);
    destination.appendChild(rootClone);

    // return child (svg)
    return childClone;
}
