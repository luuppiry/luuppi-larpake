class FormContainer extends HTMLElement {
    observer: MutationObserver;

    constructor() {
        super();
        this.observer = new MutationObserver((_) => this.appendContent());
    }

    connectedCallback() {
        this.observer.observe(this, { childList: true });
    }

    disconnectedCallback() {
        this.observer.disconnect();
    }

    appendContent() {
        const content = this.innerHTML;

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
