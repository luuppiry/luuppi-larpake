export type Point2D = {
    X: number;
    Y: number;
};

export default class Draw {
    flattenThreshold: number;
    canvas: HTMLCanvasElement;
    isDrawing: boolean;
    context: CanvasRenderingContext2D;
    points: (Point2D | null)[];

    constructor(canvas: HTMLCanvasElement) {
        this.flattenThreshold = 5;
        this.canvas = canvas;
        this.context = canvas.getContext("2d")!;
        this.isDrawing = false;
        this.points = [];

        // Do some binding to fix instance calls with event handlers
        this.startDrawing = this.startDrawing.bind(this);
        this.draw = this.draw.bind(this);
        this.stopDrawing = this.stopDrawing.bind(this);

        this.canvas.addEventListener("mousedown", this.startDrawing);
        this.canvas.addEventListener("mousemove", this.draw);
        this.canvas.addEventListener("mouseup", (_) => this.stopDrawing());
        this.canvas.addEventListener("mouseout", (_) => this.stopDrawing());
    }

    startDrawing(event: MouseEvent) {
        this.isDrawing = true;
        this.points.push({ X: event.offsetX, Y: event.offsetY });
    }

    draw(event: MouseEvent) {
        if (this.isDrawing === false) {
            return;
        }

        this.context.lineWidth = 2;
        this.context.lineCap = "round";
        this.context.strokeStyle = "black";
        this.context.beginPath();

        const lastPoint = this.points[this.points.length - 1];
        const currentPoint = { X: event.offsetX, Y: event.offsetY };
        if (lastPoint == null) {
            return;
        }

        this.context.moveTo(lastPoint.X, lastPoint.Y);
        this.context.lineTo(currentPoint.X, currentPoint.Y);
        this.context.stroke();

        this.points.push(currentPoint);
    }

    stopDrawing() {
        this.isDrawing = false;
        this.points.push(null);
    }

    get_flattened(): Point2D[][] {
        let result: Point2D[][] = [];
        let workingArray: Point2D[] = [];

        const input = this.points;

        for (let i = 0; i < input.length; i++) {
            const currentPoint = input[i];

            const isLastIndex = i === input.length - 1;
            if (currentPoint === null || isLastIndex) {
                if (workingArray.length === 0) {
                    continue;
                }
                if (i > 1 && input[i - 1] != null) {
                    workingArray.push(input[i - 1]!);
                }
                if (workingArray.length === 1) {
                    workingArray.push(workingArray[0]);
                }

                result.push(workingArray);
                workingArray = [];
                continue;
            }

            if (workingArray.length === 0) {
                workingArray.push(currentPoint);
            }

            const lastPoint = workingArray[workingArray.length - 1];
            const diffX = Math.abs(currentPoint.X - lastPoint.X);
            const diffY = Math.abs(currentPoint.Y - lastPoint.Y);
            if (diffX <= this.flattenThreshold && diffY <= this.flattenThreshold) {
                continue;
            }
            workingArray.push(currentPoint);
        }
        return result;
    }

    refresh() {
        this.points = [];
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }
}
