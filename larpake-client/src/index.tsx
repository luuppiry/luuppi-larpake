import React from "react";
import ReactDOM from "react-dom/client";
import Home from "./pages/Home.tsx";
import {
    PublicClientApplication,
    EventType,
    AuthenticationResult,
} from "@azure/msal-browser";
import { loginRequest, msalConfig } from "./authConfig.ts";
import {
    AuthenticatedTemplate,
    MsalProvider,
    UnauthenticatedTemplate,
    useMsal,
} from "@azure/msal-react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import LarpakePage from "./pages/LarpakePage.tsx";
import OwnStatistics from "./pages/OwnStatistics.tsx";
import Header, { SidePanel } from "./components/Header.tsx";

const root = ReactDOM.createRoot(
    document.getElementById("root") as HTMLElement
);

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
