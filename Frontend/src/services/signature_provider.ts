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
        /* Flatten - simplify
         * Remove points that are close to one another from given array.
         * For example threshold of 3 means that y and x coordinates
         * must have at least 3 pixels between abs(last.X - current.X) <= 3
         */
        let result: Point2D[][] = [];
        let workingArray: Point2D[] = [];

        const input = this.points;

        for (let i = 0; i < input.length; i++) {
            const currentPoint = input[i];

            // Separator or end of array found
            const isLastIndex = i === input.length - 1;
            if (currentPoint === null || isLastIndex) {
                // No items between nulls
                if (workingArray.length === 0) {
                    continue;
                }
                // Always include last point
                if (i > 1 && input[i - 1] != null) {
                    workingArray.push(input[i - 1]!);
                }
                // Always include at least two poins in span
                if (workingArray.length === 1) {
                    workingArray.push(workingArray[0]);
                }

                // Push current span to result
                result.push(workingArray);
                workingArray = [];
                continue;
            }

            // Always include first point
            if (workingArray.length === 0) {
                workingArray.push(currentPoint);
            }

            // I think 3 might be good value for threshold

            // Do not include points that are
            // within a threshold from last point
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

    encodeSVG(): string {
        /* Encode poinst array to svg format,
         * Note that this is unsafe and all point
         * Coordinates should be validated to be numbers
         *
         * Probably it is more usable to store as points array,
         * so it takes less space and also provide height and width.
         * */

        let svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${this.canvas.width} ${this.canvas.height}">`;
        const data = this.get_flattened();
        for (let i = 0; i < data.length; i++) {
            // Build one path segment (one stroke)
            svg += `\n\t<path d="`;
            const pathData = data[i];
            for (let j = 0; j < pathData.length; j++) {
                // Add single point
                const currentPoint = pathData[j];
                if (j === 0) {
                    // First point
                    svg += `M ${currentPoint.X} ${currentPoint.Y}`;
                } else {
                    // Other points
                    svg += `L ${currentPoint.X} ${currentPoint.Y}`;
                }
            }
            svg += `" stroke="black" fill="none" stroke-width="2" stroke-linecap="round"/>`;
        }
        svg += "\n</svg>";
        return svg;
    }
}
