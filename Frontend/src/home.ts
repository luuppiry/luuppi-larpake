import { Q_NO_TOKEN, SIDE_PANEL_ID, UI_HEADER_ID } from "./constants.js";
import Header from "./components/ui-header.js";
import { navigateTo } from "./helpers.js";
import SidePanel from "./components/side-panel.js";
import { INDEX_LINK_ID } from "./components/side-panel-data.js";

// Set header index page redirect to not redirect back to home
const header = document.getElementById(UI_HEADER_ID) as Header;
header.setNavigateHome(() => {
    navigateTo(`index.html?${Q_NO_TOKEN}=false`);
});

// Set side panel page redirect to not redirect back to home
const sidePanel = document.getElementById(SIDE_PANEL_ID) as SidePanel;
sidePanel.setLoaded((p) => {
    const items = p.queryMenuItems(`.${INDEX_LINK_ID}`);

    items.forEach((x) => {
        for (const child of x.children) {
            const link = child as HTMLAnchorElement;
            if (link) {
                link.href = `index.html?${Q_NO_TOKEN}=false`;
            }
        }
    });
});
