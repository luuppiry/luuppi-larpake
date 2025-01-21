// Parse query string to get event ID
const urlParams = new URLSearchParams(window.location.search);
const eventId = urlParams.get("eventId");

const eventDatabase = {
  1: {
    title: "KAUPUNKIKÄVELY (3P)",
    path: "Lärpäke / Ensi askeleet / Kaupunkikävely",
    updated: "19.01.2024 klo 12.00",
    description: "Osallistu orientaatioviikon kaupunkikävelyyn.",
    code: "MOIKKA69",
    date: "Tiistai 20. elok.",
    time: "15:00-19:00",
    location: "Tampereen keskusta",
  },
  2: {
    title: "POIKKITIETEELLINEN TAPAHTUMA TAMPEREELLA 3P",
    path: "Lärpäke / Kaikenlaista / Poikkitieteellinen tapahtuma Tampereella",
    updated: "19.01.2024 klo 12.00",
    description: "Osallistu (Luupin ja) toisen ainejärjestön kanssa järjestettävään tapahtumaan. Kolmiobileiden kaltaiset opiskelijabileet eivät käy poikkitieteellisestä tapahtumasta. Valmistaudu todistamaan osallistumisesi kuvatodistein tai haalarimerkein!",
    code: "WXYZ6789",
    date: "XX.YY.ZZZZ",
    time: "XX:XX-YY:YY",
    location: "Tampere",
  },
  3: {
    title: "SYÖ SIIPIÄ, VEGESIIPIÄ, MUSTAMAKKARAA TAI PYYNIKIN MUNKKEJA 2P",
    path: "Lärpäke / Tanpereella / Syö siipiä, vegesiipiä, mustamakkaraa tai Pyynikin munkkeja",
    updated: "19.01.2024 klo 12.00",
    description: "Syö tamperelaista perinneruokaa ravintolassa. Todistukseksi kelpaa tuore kuva tai tuutorin läsnäolo.",
    code: "HGER41J9",
    date: "XX.YY.ZZZZ",
    time: "XX:XX-YY:YY",
    location: "Pirkanmaa",
  },
};

// Fuksi section

// Populate event details
if (eventId && eventDatabase[eventId]) {
  const event = eventDatabase[eventId];
  document.getElementById("larpake-page-name").textContent = event.path;
  document.getElementById("event-title").textContent = event.title;
  document.getElementById("event-updated").textContent = `Päivitetty: ${event.updated}`;
  document.getElementById("event-description").textContent = event.description;
  document.getElementById("qr-section-text-1").textContent = "Näytä tämä QR-koodi tuutorille kirjataksesi osallistumisesi"
  document.getElementById("qr-section-text-2").textContent = "tai käytä seuraavanlaista koodia"
  document.getElementById("qr-code").src = `https://api.qrserver.com/v1/create-qr-code/?data=${event.code}&amp;size=250x250`;
  document.getElementById("event-code").textContent = event.code;
  document.getElementById("event-date").textContent = `Päivämäärä: ${event.date} - ${event.title}`;
  document.getElementById("event-time").textContent = `Aika: ${event.time}`;
  document.getElementById("event-location").textContent = `Sijainti: ${event.location}`;
} else {
  document.getElementById("event-title").textContent = "Error: Event not found!";
  document.getElementById("event-updated").textContent = "Päivitetty: ";
  document.getElementById("event-description").textContent = "";
  document.getElementById("qr-code").src = "";
  document.getElementById("event-code").textContent = "404";
  document.getElementById("event-date").textContent = "Päivämäärä: ";
  document.getElementById("event-time").textContent = "Aika: ";
  document.getElementById("event-location").textContent = "Sijainti: ";
}

function toggleSidePanel() {
    const panel = document.getElementById('sidePanel');
    panel.classList.toggle('open');
}
