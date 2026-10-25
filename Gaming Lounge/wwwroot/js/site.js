document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-confirm]").forEach(element => {
        element.addEventListener("click", event => {
            if (!window.confirm(element.dataset.confirm)) {
                event.preventDefault();
            }
        });
    });

    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll(".navbar .nav-link").forEach(link => {
        const path = new URL(link.href, window.location.origin).pathname.toLowerCase();
        if (path !== "/" && currentPath.startsWith(path)) {
            link.classList.add("active");
        } else if (path === "/" && currentPath === "/") {
            link.classList.add("active");
        }
    });

    const header = document.querySelector(".site-header");
    const updateHeader = () => header?.classList.toggle("scrolled", window.scrollY > 12);
    updateHeader();
    window.addEventListener("scroll", updateHeader, { passive: true });
});
