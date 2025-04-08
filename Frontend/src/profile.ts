import DrawView from "./services/signature_provider.js";
import { GuidIdObject, Point2D } from "./models/common.js";
import SignatureRenderer, { SvgOptions } from "./services/signature_renderer.js";
import { UserClient } from "./api_client/user_client.js";
import {
    appendTemplateElement,
    hasRole,
    isEmpty,
    removeChildren,
    Role,
    showOkDialog,
    ThrowIfNull,
    toRole,
} from "./helpers.js";
import GroupClient from "./api_client/group_client.js";
import { PermissionCollection, User } from "./models/user.js";
import { MAX_SIGNATURES, SIGNATURE_INFO } from "./constants.js";

type PointCluster = {
    id: string | null;
    points: Point2D[][];
};

class SignatureHandler {
    container: HTMLElement;
    signatures: PointCluster[];

    constructor() {
        this.container = document.getElementById("signature-container") as HTMLElement;
        ThrowIfNull(this.container);
        this.signatures = [];

        document.getElementById("save-signatures-btn")?.addEventListener("click", async (_) => {
            await this.save();
        });
    }

    async save() {
        const isFull = this.signatures.length > MAX_SIGNATURES;

        const existingSignatures = this.signatures.filter((x) => !isEmpty(x.id));
        const newSignatures = this.signatures
            .filter((x) => isEmpty(x.id))
            .slice(0, MAX_SIGNATURES - existingSignatures.length);

        for (const signature of newSignatures) {
            const success = await uploadSignature(signature.points);
            if (!success) {
                showOkDialog("upload-failed-dialog");
                throw new Error("Failed to upload signature. Possible error details above.");
            }
        }

        showOkDialog("saved-dialog", () => {
            window.location.reload();
        });

        if (isFull) {
            showOkDialog("max-signatures-dialog", () => {
                console.log("Max signatures reached");
            });
        }
    }

    addSignature(
        signature: Point2D[][],
        signatureId: string | null,
        options: SvgOptions | null = null
    ) {
        this.signatures.push({
            id: signatureId,
            points: signature,
        });

        const item = appendTemplateElement<HTMLLIElement>("signature-template", this.container);
        item.querySelector<HTMLParagraphElement>("._id")!.innerText = signatureId ?? "";
        const svg = item.querySelector<HTMLElement>("._svg")!;

        // Render
        const renderer = new SignatureRenderer(signature, options);
        renderer.renderTo(svg);
    }
}

const userClient = new UserClient();
const groupClient = new GroupClient(userClient.client);
const signatureHandler = new SignatureHandler();

async function main() {
    await loadProfile();
    addEventListeners();
}

async function loadProfile() {
    const user = await userClient.getSelf();
    if (!user) {
        throw new Error("Failed to fetch user information");
    }

    const permissionsTable = await userClient.getPermissionTable();

    const root = document.getElementById("profile-container")!;
    root.querySelector<HTMLParagraphElement>("._email")!.innerText = user?.entraUsername ?? "N/A";
    root.querySelector<HTMLParagraphElement>("._first-name")!.innerText = user?.firstName ?? "N/A";
    root.querySelector<HTMLParagraphElement>("._last-name")!.innerText = user?.lastName ?? "N/A";
    root.querySelector<HTMLParagraphElement>("._username")!.innerText = user?.username ?? "N/A";
    root.querySelector<HTMLParagraphElement>("._role")!.innerText = toRole(
        user?.permissions,
        permissionsTable
    );

    await loadGroups(root);
    await loadSignatures(user, permissionsTable);
}

async function loadSignatures(user: User, permissionsTable: PermissionCollection | null) {
    if (!hasRole(user.permissions, Role.Tutor, permissionsTable)) {
        document.getElementById("signature-section")!.style.display = "none";
        return;
    }

    const signatures = await userClient.getOwnSignatures();
    if (!signatures) {
        throw new Error("Failed to fetch user signatures");
    }

    signatures
        .sort((first, second) => (first.id < second.id ? -1 : 1))
        .forEach((x) =>
            signatureHandler.addSignature(x.signature.data, x.id, {
                stroke: "black",
                fill: "none",
                strokeWidth: x.signature.lineWidth,
                strokeLinecap: x.signature.lineCap,
            })
        );
}

async function loadGroups(root: HTMLElement) {
    const groups = await groupClient.getOwnGroups(true);
    if (!groups || groups.length === 0) {
        return;
    }
    const groupContainer = root.querySelector<HTMLUListElement>("._groups")!;
    removeChildren(groupContainer);
    for (const group of groups) {
        const item = document.createElement("li");
        groupContainer.appendChild(item);
        item.innerText = group.name;
    }
}

async function uploadSignature(value: Point2D[][]): Promise<GuidIdObject | null> {
    return await userClient.uploadSignature({
        height: SIGNATURE_INFO.HEIGTH,
        width: SIGNATURE_INFO.WIDTH,
        data: value,
        lineWidth: SIGNATURE_INFO.LINE_WIDTH,
        strokeStyle: SIGNATURE_INFO.STROKE_STYLE,
        lineCap: SIGNATURE_INFO.LINE_CAP,
    });
}

function addEventListeners() {
    const dialog = document.getElementById("signature-dialog") as HTMLDialogElement;
    const signatureCanvas = document.getElementById("signature-canvas") as HTMLCanvasElement;
    let signatureProvider = new DrawView(signatureCanvas);

    document.getElementById("show-dialog-btn")?.addEventListener("click", () => {
        dialog.showModal();
    });
    document.getElementById("close-dialog-btn")?.addEventListener("click", () => {
        signatureProvider.refresh();
        dialog.close();
    });

    document.getElementById("submit-dialog-btn")?.addEventListener("click", () => {
        // Read new signature
        const output = signatureProvider.getFlattened();
        signatureProvider.refresh();
        dialog.close();

        // Validate signature is not empty
        if (output.length == 0) {
            console.log("No signature path data found.");
            return;
        }

        // Update UI
        signatureHandler.addSignature(output, null);
    });
}

main();
