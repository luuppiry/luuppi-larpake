class FormContainer extends HTMLElement {
    observer: MutationObserver;

    constructor() {
        super();
        // Add mutation observer to track changes to the element
        this.observer = new MutationObserver((_) => this.appendContent());
    }

    connectedCallback() {
        // Add observer to track if element children are changed
        this.observer.observe(this, { childList: true });
    }

    disconnectedCallback() {
        // disconnect when element is not rendered
        this.observer.disconnect();
    }

    appendContent() {
        /* This method should be only called by mutation observer
         * Method reads inner html of the component and moves it to correct palce
         */
        const content = this.innerHTML;

        // Disconnect observer because no later changes should be tracked (causes stack overflow)
        this.observer.disconnect();
        this.innerHTML = `
        <div class="hor-center-content">
            <!-- Default max width is 1400px -->
            <div class="section" style="max-width: 1400px">
                <div class="stripe"></div>
                <div class="section-filler">
                    <!-- Container for elements and correct expansion, do not remove -->
                    <div>
                        <!-- Content -->
                        ${content}
                    </div>
                </div>
            </div>
        </div>`;
    }
}

if ("customElements" in window) {
    customElements.define("form-container", FormContainer);
}
