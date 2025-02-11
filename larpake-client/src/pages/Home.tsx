import React from "react";
import Header, { SidePanel } from "../components/Header.tsx";
import "../styles/main.css";
import kiasaImage from "../assets/kiasa.png";
import { PublicClientApplication } from "@azure/msal-browser";
import { loginRequest } from "../authConfig.ts";
import {
    AuthenticatedTemplate,
    UnauthenticatedTemplate,
    useMsal,
} from "@azure/msal-react";

export default function Home() {
    const { instance } = useMsal();

    const login = () => {
        instance
            .loginRedirect(loginRequest)
            .catch((error) => console.log(error));
    };

    const logout = () => {
        instance.logoutRedirect().catch((error) => console.log());
    };

    return (
        <div>
            <Header />
            <SidePanel />

            <div className="container">
                <div className="image-section">
                    <img src={kiasaImage} alt="Kiasa" />
                    <h2>TÄSTÄ LÄRPÄKKEEN IHMEELLISEEN MAAILMAAN</h2>
                    <a href="larpake.html" className="main-site-button">
                        ⭢
                    </a>
                </div>
            </div>
            <div>
                <AuthenticatedTemplate>
                    <button onClick={logout}>Logout</button>
                </AuthenticatedTemplate>
                <UnauthenticatedTemplate>
                    <button onClick={login}>Login</button>
                </UnauthenticatedTemplate>
            </div>
            <div className="container">
                <div className="events-section">
                    <h2>TULEVAT LÄRPÄKETAPAHTUMAT</h2>
                    <div className="event">
                        <h3>Keskiviikko 27. marras. - Kv-pikkujoulut</h3>
                        <p>17:00–21:00</p>
                        <p>
                            Tapahtuma on tarkoitettu kansainvälisille
                            opiskelijoille, mutta kaikki opiskelijat ovat myös
                            tervetulleita! Tapahtuman kieli on englanti.
                        </p>
                    </div>
                    <div className="event">
                        <h3>
                            Torstai 28. marras. - Luonnontieteilijöiden
                            jouluristeily
                        </h3>
                        <p>To 28.11 klo 17:00 - Pe 29.11 klo 12:00</p>
                        <p>
                            Liput tälle matkalle myydään Hervannan kampuksella
                            Sähkötalon aulassa ja Kaupin kampuksella
                            Lääketieteen aulassa.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
