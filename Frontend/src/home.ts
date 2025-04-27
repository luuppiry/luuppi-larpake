import { Q_NO_TOKEN, UI_HEADER_ID } from "./constants.js";
import Header from "./components/ui-header.js";
import { navigateTo } from "./helpers.js";

const header = document.getElementById(UI_HEADER_ID) as Header;
header.setNavigateHome(() => {
    navigateTo(`index.html?${Q_NO_TOKEN}=false`);
});

console.log(header);
