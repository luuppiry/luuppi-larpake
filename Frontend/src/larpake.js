// Parse query string to get page number
const urlParams = new URLSearchParams(window.location.search);
const pageNum = urlParams.get("page");

const pages = [
    {
        header: "Lärpäke / Ensi askeleet",
        buttons: [
            "ENSI ASKELEET - OTSIKKO",
            "TUTUSTUMISILTA 19.8 5P",
            "KAUPUNKIKÄVELY 3P",
            "TUTUSTUMISSAUNA 20.8 5P",
            "PUISTOHENGAILU 3P - PERUUTETTU",
            "KAMPUSKIERROS 3P"
        ]
    },
    {
        header: "Lärpäke / Ensi askeleet",
        buttons: [
            "KÄY PÄÄAINEESI ALOITUSLUENNOLLA 2P",
            "OSALLISTU TREY:N FUKSISUUNNISTUKSEEN 5P",
            "LÄRPÄKKEEN PÄÄLLYSTYS TAI KORISTELU 2P",
            "KERÄÄ VIIDELTÄ TUUTORILTA ALLEKIRJOITUS 5P",
            "FUKSIRYHMÄN TAPAHTUMA 5P",
            "KUMMIRYHMÄN TAPAHTUMA 5P"
        ]
    },
    {
        header: "Lärpäke / Ensi askeleet",
        buttons: [
            "PUBIRUNDI 3P",
            "LAULA KARAOKESSA 2P",
            "FUKSIKEILAUS 4P",
            "PIENEN PIENI LUUPPILAINEN - OTSIKKO",
            "LIITY LUUPIN JÄSENEKSI 20P",
            "TILAA HAALARIT 10P"
        ]
    },
    {
        header: "Lärpäke / Pienen pieni luuppilainen",
        buttons: [
            "OSTA LUUPIN HAALARIMERKKI 2P",
            "JUTTELE KYYKÄSTÄ DDRNV:N JÄSENEN KANSSA TAI OSTA HAALARIMERKKI 2P",
            "OSTA LUUPPI-TUOTE (pl. Luuppi-haalarimekki) 3P",
            "OMPELE 5 HAALARIMERKKIÄ 6P",
            "OSALLISTU KOPO-TAPAHTUMAAN 5P",
            "KIERTOAJELU 5P"
        ]
    },
    {
        header: "Lärpäke / Pii-Klubilla tapahtuu",
        buttons: [
            "KASTAJAISET 5P",
            "PII-KLUBILLA TAPAHTUU - OTSIKKO",
            "KÄY TOIMISTOLLA 2P",
            "KEITÄ PANNULLINEN KAHVIA 3P",
            "PULLAPÄIVÄ 2P",
            "OSALLISTU TALKOOPÄIVÄÄN 3P"
        ]
    },
    {
        header: "Lärpäke / Normipäivä",
        buttons: [
            "NORMIPÄIVÄ - OTSIKKO",
            "ENNAKKOLIPPU KOLMIOILLE 3P",
            "OSALLISTU KOLMIOIDEN VIRALLISILLE ETKOILLE 26.9 3P",
            "KOTIBILEET/ETKOT (OMAT/LUUPPILAISEN) 2P",
            "SITSIT 3P",
            "PELI-ILTA 3P"
        ]
    },
    {
        header: "Lärpäke / Normipäivä",
        buttons: [
            "LAUTAPELI-ILTA 3P",
            "LANIT 2P",
            "MUU TAPAHTUMA 5P",
            "YLIOPISTOELÄMÄÄ - OTSIKKO",
            "SELVITÄ KOULUTUSASIANTUNTIJASI NIMI 2P",
            "LUO OMA HOPS 5P"
        ]
    },
    {
        header: "Lärpäke / Yliopistoelämää",
        buttons: [
            "SIVUAINEPANEELI 5P",
            "MTT-TIE-KAHVIT 3P",
            "KÄY LASKUTUVASSA TAI LUUPIN OPINTOPIIRISSÄ 3P",
            "KÄY SYÖMÄSSÄ 4 ERI OPISKELIJARAVINTOLASSA 4P",
            "VIERAILE TOISEN AINEJÄRJESTÖN/KILLAN/KERHON TILOISSA 2P",
            "HANKI TOISEN TOISEN AINEJÄRJESTÖN/KILLAN/KERHON HAALARIMERKKI 2P"
        ]
    },
    {
        header: "Lärpäke / Yliopistoelämää",
        buttons: [
            "OSALLISTU YLIOPISTON TUTKIMUKSEEN 6P",
            "VAIKUTUSVALTAA - OTSIKKO",
            "KYSY SPONSORIA HAALAREIHIN 5P",
            "SAA SPONSORI HAALAREIHIN 15P",
            "NAKKEILE TAPAHTUMASSA 5P",
            "OSALLISTU LUUPIN HALLITUKSEN KOKOUKSEN 5P"
        ]
    },
    {
        header: "Lärpäke / Vaikutusvaltaa",
        buttons: [
            "JUTTU LUUPPISANOMIIN 7P",
            "LUUPIN HAALARIMERKKIKISA 2P",
            "OSALLISTU HALLITUSINFO-TAPAHTUMAAN 5P",
            "OSALLISTU HALLITUSPANEELLIIN 3P",
            "SÄÄNTÖMÄÄRÄINEN VUOSIKOKOUS 5P",
            "ÄÄNESTÄ TREY:N EDUSTAJISTOVAALEISSA 2P"
        ]
    },
    {
        header: "Lärpäke / Liikunnallista",
        buttons: [
            "LIIKUNNALLISTA - OTSIKKO",
            "FUKSIKYYKKÄ (ALKULOHKOT) 5P",
            "FUKSIKYYKKÄ (FINAALI) 5P",
            "LUUPIN LIIKUNTAVUORO 3P",
            "LASERTAISTELU 3P",
            "METSÄRETKI 3P"
        ]
    },
    {
        header: "Lärpäke / Kaikenlaista",
        buttons: [
            "LIIKUNTAHAASTE 3P",
            "MUU LIIKUNTATAPAHTUMA 3P",
            "KAIKENLAISTA - OTSIKKO",
            "TEK-INFO 2P",
            "LOIMU-INFO 2P",
            "SEURAA LUUPIN SOMEA (TG/IG/TT) 2P"
        ]
    },
    {
        header: "Lärpäke / Kaikenlaista",
        buttons: [
            "OSALLISTU ATK-YTP:LLE 5P",
            "OSALLISTU INTEGRAATIOFESTEILLE 5P",
            "KULTTUURITAPAHTUMA 4P",
            "POIKKITIETEELLINEN TAPAHTUMA TAMPEREELLA 3P",
            "LUUPIN PELITURNAUS 3P",
            "YRITYSTAPAHTUMA 3P"
        ]
    },
    {
        header: "Lärpäke / Tanpereella",
        buttons: [
            "KESKUSTAEXCU 3P",
            "SELVITÄ LUUPIN KIASA-NORSUN TAUSTATARINA 1P",
            "TANPEREELLA - OTSIKKO",
            "KÄY PYYNIKIN NÄKÖTORNILLA 2P",
            "KÄY HERVANNAN VESITORNILLA 2P",
            "SYÖ SIIPIÄ, VEGESIIPIÄ, MUSTAMAKKARAA TAI PYYNIKIN MUNKKEJA 2P"
        ]
    },
    {
        header: "Lärpäke / Tanpereella",
        buttons: [
            "KÄY MUSEOSSA TAMPEREELLA 2P",
            "KÄY HOTELLI TORNIN HUIPULLA 2P",
            "KÄY NÄSINNEULALLA 2P",
            "RATIKKA-AJELU 5P",
            "KÄY JOKAISELLA YLIOPISTON KAMPUKSELLA 3P",
            "- TYHJÄ"
        ]
    }
];


let currentPage = 0;
if (Number(pageNum)) {
    currentPage = Number(pageNum);
}

function renderPage() {
    if (currentPage === pages.length){
        window.open("statistics.html", "_self");
    }

    const header = document.getElementById("larpake-page-name");
    const buttonContainer = document.getElementById("larpake-button-container");
    const pageInfo = document.getElementById("page-info");
    const prevPage = document.getElementById("prev-page");
    const nextPage = document.getElementById("next-page");

    // Update header
    header.textContent = pages[currentPage].header;

    // Update buttons
    buttonContainer.innerHTML = ""; // Clear previous buttons
    pages[currentPage].buttons.forEach(buttonText => {
        const buttonWrapper = document.createElement("div"); // Wrapper for text and image
        buttonWrapper.className = "button-wrapper";

        const button = document.createElement("button");
        button.className = "button";
        const textContainer = document.createElement("div"); // Container for text
        textContainer.className = "button-text";
        

        // Add disabled style for specific cases
        if (buttonText.includes("PERUUTETTU")) {
            textContainer.textContent = buttonText.split(" - ")[0];
            button.classList.add("disabled");
            const imageContainer = document.createElement("div");
            imageContainer.className = "button-image";
            imageContainer.textContent = "PERUUTETTU";
            button.appendChild(textContainer);
            button.appendChild(imageContainer);
        } else if (buttonText.includes("OTSIKKO")) {
            textContainer.textContent = buttonText.split(" - ")[0];
            button.style.fontSize = "25px";
            button.appendChild(textContainer);
        } else if (buttonText.includes("TYHJÄ")) {
            textContainer.textContent = "";
            button.style.visibility = "hidden";
            button.appendChild(textContainer);
        } else {
            textContainer.textContent = buttonText;    
            const imageContainer = document.createElement("div"); // Container for signature
            imageContainer.className = "button-image";
            const img = document.createElement("img");
            img.src = getSignatureImage();
            imageContainer.appendChild(img);
            button.addEventListener("click", () => {
                window.location.href = 'event_marking.html?eventId=1';
            });
            button.appendChild(textContainer);
            button.appendChild(imageContainer);
        }

        buttonWrapper.appendChild(button);
        buttonContainer.appendChild(buttonWrapper);
    });

    // Update pagination info
    pageInfo.textContent = `${currentPage + 1} / ${pages.length + 1}`;

    // Update button states
    prevPage.disabled = currentPage === 0;
    //nextPage.disabled = currentPage === pages.length - 1;
}

function getSignatureImage() {
    const signature_list = ["", "", "", "img/signatures/test.png", "img/signatures/signature_HV_2.png", "img/signatures/signature_HV_3.png", "img/signatures/signature_JK_2.png", "img/signatures/signature_JK_1.png", "img/signatures/signature_JK_3.png"];
    return signature_list[Math.floor(Math.random()*signature_list.length)];
}

function changePage(direction) {
    currentPage += direction;
    renderPage();
}

function goToPage(index) {
    currentPage = index;
    renderPage();
}

function toggleSidePanel() {
    const panel = document.getElementById('sidePanel');
    panel.classList.toggle('open');
}

// Initialize the first page
renderPage();
