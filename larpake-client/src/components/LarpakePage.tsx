import React from "react";
import "../styles/larpake.css";

const changePage = (add: number) => {};

export default function LarpakePage(changePage: (page: number) => {}) {
    return (
        <div className="container">
            <div id="larpake-button-container">
                <button className="button">
                    <div className="button-text">error</div>
                    <div className="button-image">ERROR: page not found!</div>
                </button>
            </div>
            <div className="pagination">
                <button id="prev-page" onClick={() => changePage(-1)}></button>
                <span id="page-info">1 / 1</span>
                <button id="next-page" onClick={() => changePage(1)}>
                    &gt;
                </button>
            </div>
        </div>
    );
}
