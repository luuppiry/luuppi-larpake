import { addTaskEventListeners } from "../components/task-editor";
import SectionEditor, {func} from "../components/section-editor";

document.getElementById("commonCancelBtn")?.addEventListener("click", (_) => {
    // Cancel to default values
});

document.getElementById("commonSaveBtn")?.addEventListener("click", (_) => {
    // Validate and send new values to server
});



function render(){

    

    addTaskEventListeners();
}

render();
