import React from "react";
import ReactDOM from "react-dom/client";
import Home from "./pages/Home.tsx";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import LarpakePage from "./pages/LarpakePage.tsx";
import OwnStatistics from "./pages/OwnStatistics.tsx";
import Header, { MetaTags, SidePanel } from "./components/Header.tsx";
import { MsalProvider, useMsal } from "@azure/msal-react";
import createInstance from "./auth.ts";

const root = ReactDOM.createRoot(
    document.getElementById("root") as HTMLElement
);


// Meta tags contain react <Helmet> that causes errors in React.StrictMode
root.render(
    <MsalProvider instance={createInstance()}>
        <MetaTags />
        <React.StrictMode>
            <div>
                <Header />
                <SidePanel />
                <BrowserRouter>
                    <Routes>
                        <Route path="/" element={<Home />}></Route>
                        <Route
                            path="/larpake"
                            element={<LarpakePage />}
                        ></Route>
                        <Route
                            path="/statistics"
                            element={<OwnStatistics />}
                        ></Route>
                    </Routes>
                </BrowserRouter>
            </div>
        </React.StrictMode>
    </MsalProvider>
);
