declare const qrcode: any;

/* Remember to import next line in your html file
 * <script src="https://rawgit.com/sitepoint-editors/jsqrcode/master/src/qr_packed.js"></script>
 */

export default class QRCodeService {
    scanning: boolean = false;
    videoSource: HTMLVideoElement;
    videoOutput: HTMLCanvasElement;
    renderCtx: CanvasRenderingContext2D;
    resultView: HTMLDivElement;
    startScanningBtn: HTMLAnchorElement;
    onCodeCaptured: null | ((value: string) => void) = null;

    constructor(
   
        videoOutput: HTMLCanvasElement,
        resultView: HTMLDivElement,
        startScanningBtn: HTMLAnchorElement
    ) {
        this.videoSource = document.createElement("video");
        this.videoOutput = videoOutput;
        this.renderCtx = videoOutput.getContext("2d")!;
        this.resultView = resultView;
        this.startScanningBtn = startScanningBtn;
    }

    addQRCodeScanning() {
        qrcode.callback = (result: string) => {
            this.#onCapturedInternal(result);
        };

        this.startScanningBtn.onclick = () => {
            navigator.mediaDevices
                .getUserMedia({ video: { facingMode: "environment" } })
                .then((stream: MediaStream) => {
                    this.scanning = true;
                    this.setResultVisibility(true);
                    this.videoSource.setAttribute("playsinline", "true"); // required to tell iOS safari we don't want fullscreen
                    this.videoSource.srcObject = stream;
                    this.videoSource.play();
                    this.tick();
                    this.scan();
                });
        };
    }

    setResultVisibility(isVisible: boolean) {
        this.resultView.hidden = isVisible;
        this.startScanningBtn.hidden = isVisible;
        this.videoOutput.hidden = !isVisible;
    }

    tick() {
        this.videoOutput.height = this.videoSource.videoHeight;
        this.videoOutput.width = this.videoSource.videoWidth;
        if (this.renderCtx) {
            this.renderCtx.drawImage(
                this.videoSource,
                0,
                0,
                this.videoOutput.width,
                this.videoOutput.height
            );
        }
        if (this.scanning) {
            requestAnimationFrame(() => this.tick());
        }
    }

    scan() {
        try {
            qrcode.decode();
        } catch (e) {
            setTimeout(() => this.scan(), 300);
        }
    }

    setOnCodeCaptured(action: (value: string) => void) {
        this.onCodeCaptured = action;
    }

    #onCapturedInternal(result: string) {
        if (result) {
            console.log("Found QR-code value:", result);

            // Call given method with given input
            if (this.onCodeCaptured) {
                this.onCodeCaptured(result);
            }

            // Stop scanning for now
            this.scanning = false;
            const stream = this.videoSource.srcObject as MediaStream;
            stream.getTracks()?.forEach((track) => {
                track.stop();
            });

            this.setResultVisibility(false);
        }
    }
}
