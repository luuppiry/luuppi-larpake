import React, { useEffect } from "react";
import "../styles/main.css";
import { EntraId } from "../services/auth-client.ts";
import { IMsalContext } from "@azure/msal-react";
import useAuthenticatedClient, { HttpClient } from "../services/http-client.ts";

async function makeApiRequest(apiClient: HttpClient) {
    const reponse = await apiClient.fetch("api/larpakkeet/own")

    console.log(reponse);
}



export default function Home() {
    // useMsal() can only be called inside react component
    const apiClient = useAuthenticatedClient();
    
    useEffect(() => {
        // Clear the URL hash to prevent state mismatch errors
        if (window.location.hash) {
            window.location.hash = "";
        }
    }, []);

    

    // onClick={() => authenticate(ctx)}
    return (
        <div>
            <button onClick={() => makeApiRequest(apiClient)}>Press me!</button>
            <div className="container">
                <div className="image-section">
                    <img src="/kiasa.png" alt="Kiasa" />
                    <h2>TÄSTÄ LÄRPÄKKEEN IHMEELLISEEN MAAILMAAN</h2>
                    <a href="larpake.html" className="main-site-button">
                        ⭢
                    </a>
                </div>
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
