export const INDEX_LINK_ID = "_index-link";

export interface ListItem {
    href: string;
    title: { fi: string; en: string };
    submenu?: ListItem[];
    queryId?: string;
}

export const baseCollection: ListItem[] = [
    { href: "index.html", title: { fi: "Koti", en: "Home" }, queryId: INDEX_LINK_ID },
];

export const userPages: ListItem[] = [
    {
        href: "larpake.html",
        title: { fi: "Lärpäke", en: "Lärpäke" },
        submenu: [
            {
                href: "larpake.html?page=0",
                title: { fi: "Ensi askeleet", en: "Baby steps" },
            },
            {
                href: "larpake.html?page=2",
                title: { fi: "Pienen pieni luuppilainen", en: "Young Luuppian" },
            },
            {
                href: "larpake.html?page=4",
                title: { fi: "Pii-Klubilla tapahtuu", en: "Pii-Klubi happenings" },
            },
            {
                href: "larpake.html?page=5",
                title: { fi: "Normipäivä", en: "Averageday" },
            },
            {
                href: "larpake.html?page=6",
                title: { fi: "Normipäivä", en: "Normal day" },
            },
            {
                href: "larpake.html?page=7",
                title: { fi: "Yliopistoelämää", en: "University life" },
            },
            {
                href: "larpake.html?page=9",
                title: { fi: "Vaikutusvaltaa", en: "Influence" },
            },
            {
                href: "larpake.html?page=11",
                title: { fi: "Liikunnallista", en: "Exercise - Participate" },
            },
            {
                href: "larpake.html?page=12",
                title: { fi: "Kaikenlaista", en: "This and that" },
            },
            {
                href: "larpake.html?page=14",
                title: { fi: "Tanpereella", en: "In Tampere" },
            },
        ],
    },
    {
        href: "statistics.html",
        title: { fi: "Omat tilastot", en: "My Statistics" },
    },
    {
        href: "latest_completion.html",
        title: { fi: "Viimeisimmät suoritukset", en: "Latest Achievements" },
    },
    {
        href: "common_statistics.html",
        title: { fi: "Yhteiset tilastot", en: "Common Statistics" },
    },
    {
        href: "upcoming_events.html",
        title: { fi: "Tulevat tapahtumat", en: "Upcoming Events" },
    },
    {
        href: "own_groups.html",
        title: { fi: "Ryhmäni", en: "My Group" },
    },
    {
        href: "join.html",
        title: { fi: "Liity ryhmään", en: "Join group" },
    },
    {
        href: "faq.html",
        title: { fi: "UKK", en: "FAQ" },
    },
    {
        href: "contact_us.html",
        title: { fi: "Ota yhteyttä", en: "Contact us" },
    },
];

export const tutorPages: ListItem[] = [
    {
        href: "read.html",
        title: { fi: "Tuutori - Allekirjoita", en: "Tutor - Sign Task" },
    },
];

export const adminPages: ListItem[] = [
    {
        href: "admin/admin.html",
        title: { fi: "Ylläpito", en: "Admin ONLY IN FI" },
    },
];

export const sudoPages: ListItem[] = [
    {
        href: "admin/admin.html",
        title: { fi: "SUDO Ylläpito", en: "Sudo Admin ONLY IN FI" },
    },
];
