declare const qrcode: any;

const video = document.createElement("video");
const canvasElement = document.getElementById("qr-canvas") as HTMLCanvasElement;
const canvas = canvasElement.getContext("2d");

const qrResult = document.getElementById("qr-result") as HTMLDivElement;
const outputData = document.getElementById("code-input") as HTMLInputElement;
const btnScanQR = document.getElementById("btn-scan-qr") as HTMLAnchorElement;

let scanning = false;

qrcode.callback = (res: any) => {
    if (res) {
        outputData.value = res;
        scanning = false;

        const stream = video.srcObject as MediaStream;
        stream.getTracks()?.forEach((track) => {
            track.stop();
        });

        qrResult.hidden = false;
        canvasElement.hidden = true;
        btnScanQR.hidden = false;
    }
};

btnScanQR.onclick = () => {
    navigator.mediaDevices
        .getUserMedia({ video: { facingMode: "environment" } })
        .then((stream: MediaStream) => {
            scanning = true;
            qrResult.hidden = true;
            btnScanQR.hidden = true;
            canvasElement.hidden = false;
            video.setAttribute("playsinline", "true"); // required to tell iOS safari we don't want fullscreen
            video.srcObject = stream;
            video.play();
            tick();
            scan();
        });
};

function tick() {
    canvasElement.height = video.videoHeight;
    canvasElement.width = video.videoWidth;
    if (canvas) {
        canvas.drawImage(video, 0, 0, canvasElement.width, canvasElement.height);
    }
    if (scanning) {
        requestAnimationFrame(tick);
    }
}

function scan() {
    try {
        qrcode.decode();
    } catch (e) {
        setTimeout(scan, 300);
    }
}
