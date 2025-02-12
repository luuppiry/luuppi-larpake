import React from "react";
import ReactDOM from "react-dom/client";
import Home from "./pages/Home.tsx";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import LarpakePage from "./pages/LarpakePage.tsx";
import OwnStatistics from "./pages/OwnStatistics.tsx";
import Header, { SidePanel } from "./components/Header.tsx";
import { fetchAzurelogin } from "./services/http-client.ts";

const root = ReactDOM.createRoot(
    document.getElementById("root") as HTMLElement
);

const token = await fetchAzurelogin();

root.render(
    <React.StrictMode>
        <div>
            <Header />
            <SidePanel />
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Home />}></Route>
                    <Route path="/larpake" element={<LarpakePage />}></Route>
                    <Route
                        path="/statistics"
                        element={<OwnStatistics />}
                    ></Route>
                </Routes>
            </BrowserRouter>
        </div>
    </React.StrictMode>
);
