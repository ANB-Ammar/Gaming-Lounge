document.addEventListener("DOMContentLoaded", () => {
    const body = document.body;
    const sidebar = document.getElementById("adminSidebar");
    const mobileBreakpoint = 992;

    const setSidebarState = open => {
        if (window.innerWidth < mobileBreakpoint) {
            body.classList.toggle("admin-sidebar-open", open);
            return;
        }

        const collapsed = !open;
        body.classList.toggle("admin-sidebar-collapsed", collapsed);
        localStorage.setItem("neon-admin-sidebar-collapsed", String(collapsed));
    };

    if (window.innerWidth >= mobileBreakpoint
        && localStorage.getItem("neon-admin-sidebar-collapsed") === "true") {
        body.classList.add("admin-sidebar-collapsed");
    }

    document.querySelectorAll("[data-sidebar-toggle]").forEach(button => {
        button.addEventListener("click", () => {
            const isMobile = window.innerWidth < mobileBreakpoint;
            const isOpen = isMobile
                ? body.classList.contains("admin-sidebar-open")
                : !body.classList.contains("admin-sidebar-collapsed");
            setSidebarState(!isOpen);
        });
    });

    document.querySelectorAll("[data-sidebar-close]").forEach(button => {
        button.addEventListener("click", () => body.classList.remove("admin-sidebar-open"));
    });

    sidebar?.querySelectorAll("a").forEach(link => {
        link.addEventListener("click", () => body.classList.remove("admin-sidebar-open"));
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth >= mobileBreakpoint) {
            body.classList.remove("admin-sidebar-open");
        }
    });

    const confirmElement = document.getElementById("adminConfirmModal");
    const confirmModal = confirmElement ? bootstrap.Modal.getOrCreateInstance(confirmElement) : null;
    const confirmMessage = confirmElement?.querySelector("[data-confirm-message]");
    const confirmAccept = confirmElement?.querySelector("[data-confirm-accept]");
    let pendingElement = null;

    document.querySelectorAll("[data-confirm]").forEach(element => {
        element.addEventListener("click", event => {
            if (element.dataset.confirmed === "true") {
                delete element.dataset.confirmed;
                return;
            }

            event.preventDefault();
            pendingElement = element;
            if (confirmMessage) {
                confirmMessage.textContent = element.dataset.confirm || "Are you sure you want to continue?";
            }
            confirmModal?.show();
        });
    });

    confirmAccept?.addEventListener("click", () => {
        if (!pendingElement) {
            return;
        }

        const element = pendingElement;
        pendingElement = null;
        confirmModal?.hide();
        element.dataset.confirmed = "true";
        element.click();
    });

    document.querySelectorAll("[data-auto-submit]").forEach(control => {
        control.addEventListener("change", () => control.form?.requestSubmit());
    });

    document.querySelectorAll("[data-image-preview-input]").forEach(input => {
        const preview = document.querySelector(input.dataset.imagePreviewInput);
        const updatePreview = () => {
            if (preview && input.value.trim()) {
                preview.src = input.value.trim();
            }
        };
        input.addEventListener("input", updatePreview);
        updatePreview();
    });

    const slugify = value => value
        .trim()
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, "-")
        .replace(/-{2,}/g, "-")
        .replace(/^-|-$/g, "");

    document.querySelectorAll("[data-slug-source]").forEach(source => {
        const target = document.querySelector(source.dataset.slugTarget);
        if (!target) {
            return;
        }

        const updateSlugPreview = () => {
            target.textContent = slugify(source.value) || target.dataset.slugFallback || "item";
        };

        source.addEventListener("input", updateSlugPreview);
        updateSlugPreview();
    });
});