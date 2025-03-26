import { Point2D } from "../models/common.js";

const svgNamespace = "http://www.w3.org/2000/svg";

/* Compile point data into svg 
 * and render to (svg) html element. 
 */
export type SvgOptions = {
    stroke: string;
    fill: string;
    strokeWidth: number;
    strokeLinecap: string;
};

export default class SignatureRenderer {
    rawData: Point2D[][];
    compiledSvg: SVGPathElement[];
    options: SvgOptions;

    constructor(pointData: Point2D[][], options: SvgOptions | null = null) {
        this.rawData = pointData;
        // Use options or default if null
        this.options = options ?? {
            stroke: "black",
            fill: "none",
            strokeWidth: 2,
            strokeLinecap: "round",
        } ;
        this.compiledSvg = this.compile();
    }

    compile(): SVGPathElement[] {
        this.compiledSvg = [];

        const result: SVGPathElement[] = [];
        const data = this.rawData;

        for (let i = 0; i < data.length; i++) {
            // New path segment
            let path: SVGPathElement = document.createElementNS(svgNamespace, "path");
            const pointGroup = data[i];

            // Build single drawn line
            let line = "";
            for (let j = 0; j < pointGroup.length; j++) {
                // Get current point
                const point: Point2D = pointGroup[j];

                // Add line from point to another
                if (j === 0) {
                    line += `M ${point.X} ${point.Y}`;
                } else {
                    line += `L ${point.X} ${point.Y}`;
                }
            }

            // Set styles
            path.setAttributeNS(null, "stroke", this.options.stroke);
            path.setAttributeNS(null, "fill", this.options.fill);
            path.setAttributeNS(null, "stroke-width", this.options.strokeWidth.toString());
            path.setAttributeNS(null, "stroke-linecap", this.options.strokeLinecap);
            path.setAttributeNS(null, "d", line);

            result.push(path);
        }
        return result;
    }

    renderTo(svgElement: HTMLElement) {
        for (let i = 0; i < this.compiledSvg.length; i++) {
            svgElement.appendChild(this.compiledSvg[i]);
        }
    }
}