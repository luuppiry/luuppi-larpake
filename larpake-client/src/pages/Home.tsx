import React from "react";
import "../styles/main.css";
import { EntraId } from "../services/auth-client.ts";
import { useMsal } from "@azure/msal-react";
import { AccountInfo, IPublicClientApplication } from "@azure/msal-browser";

async function authenticate(msal: IPublicClientApplication, accounts: AccountInfo[]) {
    const client = new EntraId(msal, accounts)
    await client.fetchAzurelogin();
}

export default function Home() {
    const { instance, accounts} = useMsal();
    return (
        <div>
            <div>
                <button onClick={() => authenticate(instance, accounts)}>Hello</button>{" "}
            </div>
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
