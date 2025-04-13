import { parseAttendanceLink } from "./builders.js";
import { ATTENDANCE_CODE_HEADER, ATTENDANCE_KEY_LENGTH, Q_ATTENDANCE_KEY } from "./constants.js";
import { addSingleCharacterInputBehaviour, parseFromInputRow } from "./services/input_service.js";
import QRCodeService from "./services/qr_code_read_service.js";


function main() {
   
    const videoOutput = document.getElementById("qr-canvas") as HTMLCanvasElement;
    const resultView = document.getElementById("qr-result") as HTMLDivElement;
    const startScanningBtn = document.getElementById("btn-scan-qr") as HTMLAnchorElement;

    const qrService = new QRCodeService(
        videoOutput,
        resultView,
        startScanningBtn
    );
    qrService.addQRCodeScanning();
    qrService.setOnCodeCaptured(qrCodeCaptured);
    document.querySelectorAll("._char").forEach(addSingleCharacterInputBehaviour);

    // Add manual input event handler
    document.getElementById("submit-btn")?.addEventListener("click", (e) => {
        e.preventDefault();

        const first = document.getElementById("first-char-field") as HTMLInputElement;
        const input = parseFromInputRow(first).toUpperCase();
        if (input.length < ATTENDANCE_KEY_LENGTH) {
            return;
        }
        const link = parseAttendanceLink(`${ATTENDANCE_CODE_HEADER}${input}`);
        if (link) {
            window.location.href = link;
        } else {
            alert(`Failed to parse completion link from input '${input}'`);
        }
    });
}

function qrCodeCaptured(result: string) {
    const params = new URLSearchParams(new URL(result).search);
    const unvalidatedKey = params.get(Q_ATTENDANCE_KEY);
    const validatedKey = parseAttendanceLink(unvalidatedKey);

    if (validatedKey) {
        window.location.href = validatedKey;
    } else {
        alert(
            "QR-code information was not in correct syntax. " +
                "Never scan QR-code from people you don't trust! " +
                `QR-code input was:\n${result}`
        );
    }
}


main();
